using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private MovementSettings settings;
        [SerializeField] private Transform cameraYawReference;

        private CharacterController _controller;
        private IPlayerInput _input;
        private HealthSystem _health;
        private RagdollController _ragdoll;
        private PlayerStateMachine _stateMachine = new PlayerStateMachine();

        private float _verticalVelocity;
        private float _timeSinceGrounded = 99f;
        private float _timeSinceJumpPressed = 99f;
        private bool _wasGrounded = true;
        private bool _isCrouching;
        private float _currentPlanarSpeed;

        public event Action<LandingTier, float> Landed;

        public MovementSettings Settings
        {
            get => settings;
            set => settings = value;
        }

        public PlayerStateMachine StateMachine => _stateMachine;
        public IPlayerInput Input => _input;
        public float CurrentSpeed => _currentPlanarSpeed;
        public float VerticalVelocity => _verticalVelocity;
        public bool IsCrouching => _isCrouching;
        public bool IsSprinting => _stateMachine.Current == PlayerState.Sprint;

        // Landmine #11: Awake MUST self-resolve GetComponent<IPlayerInput>() so scenes loaded from disk move.
        public void Awake()
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<MovementSettings>();
            _controller = GetComponent<CharacterController>();
            if (_controller == null)
                _controller = gameObject.AddComponent<CharacterController>();
            _input = GetComponent<IPlayerInput>();
            _health = GetComponent<HealthSystem>();
            _ragdoll = GetComponent<RagdollController>();
            if (_ragdoll != null)
                _ragdoll.Bind(_stateMachine);
            ApplyColliderHeight(false);
        }

        public void Bind(IPlayerInput input, MovementSettings moveSettings = null, Transform yawReference = null)
        {
            if (input != null) _input = input;
            if (moveSettings != null) settings = moveSettings;
            if (yawReference != null) cameraYawReference = yawReference;
        }

        public void Update()
        {
            int maxSim = (_ragdoll != null && _ragdoll.Settings != null) ? _ragdoll.Settings.maxFullSimRagdolls : 12;
            float simRadius = (_ragdoll != null && _ragdoll.Settings != null) ? _ragdoll.Settings.fullSimRadius : 55f;
            RagdollBudget.Tick(transform.position, maxSim, simRadius, Time.deltaTime, _ragdoll);
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (_input == null)
                _input = GetComponent<IPlayerInput>();

            if (_ragdoll != null && _ragdoll.IsActiveRagdoll)
            {
                _ragdoll.Tick(deltaTime);
                return;
            }

            if (!_stateMachine.IsLocomotionAllowed || _controller == null || !_controller.enabled)
            {
                _currentPlanarSpeed = 0f;
                return;
            }

            bool grounded = _controller.isGrounded;
            if (grounded)
            {
                if (!_wasGrounded)
                {
                    HandleLanding(_verticalVelocity);
                    if (_ragdoll != null && _ragdoll.IsActiveRagdoll)
                        return;
                }
                _timeSinceGrounded = 0f;
            }
            else
            {
                _timeSinceGrounded += deltaTime;
            }
            _wasGrounded = grounded;

            Vector2 rawMove = _input != null ? _input.Move : Vector2.zero;
            bool sprintHeld = _input != null && _input.SprintHeld;

            if (_input != null && _input.CrouchPressed)
            {
                _isCrouching = !_isCrouching;
                ApplyColliderHeight(_isCrouching);
            }
            if (sprintHeld && _isCrouching)
            {
                _isCrouching = false;
                ApplyColliderHeight(false);
            }

            if (_input != null && _input.JumpPressed)
                _timeSinceJumpPressed = 0f;
            else
                _timeSinceJumpPressed += deltaTime;

            Vector2 filtered = LocomotionMath.ApplyDeadZone(rawMove, settings.deadZone);
            _currentPlanarSpeed = LocomotionMath.ComputeTargetSpeed(filtered, sprintHeld, _isCrouching, settings);

            Vector3 forward = cameraYawReference != null ? cameraYawReference.forward : transform.forward;
            Vector3 right = cameraYawReference != null ? cameraYawReference.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            if (forward.sqrMagnitude < 1e-4f) forward = Vector3.forward;
            if (right.sqrMagnitude < 1e-4f) right = Vector3.right;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (right * filtered.x + forward * filtered.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            if (LocomotionMath.CanConsumeJump(_timeSinceGrounded, settings.coyoteTime, _timeSinceJumpPressed, settings.jumpBufferTime))
            {
                _verticalVelocity = LocomotionMath.ComputeJumpVelocity(settings.jumpHeight, settings.gravity);
                _timeSinceGrounded = settings.coyoteTime + 1f;
                _timeSinceJumpPressed = settings.jumpBufferTime + 1f;
                _wasGrounded = false;
                _stateMachine.TryTransition(PlayerState.Jump);
            }
            else
            {
                _verticalVelocity = LocomotionMath.StepVerticalVelocity(_verticalVelocity, settings.gravity, deltaTime, grounded);
            }

            Vector3 displacement = (moveDir * _currentPlanarSpeed + Vector3.up * _verticalVelocity) * deltaTime;
            _controller.Move(displacement);

            if (moveDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
            }

            UpdateLocomotionState(grounded, sprintHeld);
        }

        private void HandleLanding(float impactVelocity)
        {
            LandingTier tier = settings.ClassifyLanding(impactVelocity);
            Landed?.Invoke(tier, impactVelocity);

            if (tier == LandingTier.Hard)
            {
                float dmg = LocomotionMath.ComputeFallDamage(impactVelocity, settings);
                if (_health != null && dmg > 0f)
                {
                    _health.ApplyDamage(new DamageInfo(dmg, BodyPart.Limb, transform.position, Vector3.down, 0f, RagdollActivationReason.HighFall));
                }
                if (_ragdoll != null && _health != null && _health.IsAlive)
                {
                    _ragdoll.Activate(RagdollActivationReason.HighFall);
                    return;
                }
            }

            _stateMachine.TryTransition(PlayerState.Land);
        }

        private void UpdateLocomotionState(bool grounded, bool sprintHeld)
        {
            if (!grounded)
            {
                if (_verticalVelocity < -0.5f)
                    _stateMachine.TryTransition(PlayerState.Fall);
                return;
            }

            if (_isCrouching)
            {
                _stateMachine.TryTransition(PlayerState.Crouch);
            }
            else if (_currentPlanarSpeed <= 0.05f)
            {
                _stateMachine.TryTransition(PlayerState.Idle);
            }
            else if (sprintHeld && _currentPlanarSpeed > settings.runSpeed + 0.2f)
            {
                _stateMachine.TryTransition(PlayerState.Sprint);
            }
            else if (_currentPlanarSpeed > settings.walkSpeed + 0.2f)
            {
                _stateMachine.TryTransition(PlayerState.Run);
            }
            else
            {
                _stateMachine.TryTransition(PlayerState.Walk);
            }
        }

        private void ApplyColliderHeight(bool crouch)
        {
            if (_controller == null || settings == null) return;
            float h = crouch ? settings.crouchHeight : settings.standingHeight;
            _controller.height = h;
            _controller.center = new Vector3(0f, h * 0.5f, 0f);
        }
    }
}
