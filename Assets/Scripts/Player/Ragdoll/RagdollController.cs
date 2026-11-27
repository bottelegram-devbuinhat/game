using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Player
{
    public sealed class RagdollController : MonoBehaviour, IRagdollTargetSource
    {
        [SerializeField] private RagdollSettings settings;
        [SerializeField] private Transform pelvis;
        [SerializeField] private Rigidbody[] boneBodies = Array.Empty<Rigidbody>();
        [SerializeField] private Collider[] boneColliders = Array.Empty<Collider>();
        [SerializeField] private Animator characterAnimator;

        private PlayerStateMachine _stateMachine;
        private CharacterController _characterController;
        private HealthSystem _health;
        private bool _isActive;
        private bool _isFullSim = true;
        private bool _isDead;
        private float _lowVelocityTimer;
        private float _getUpTimer;
        private RagdollActivationReason _lastReason;

        public event Action<RagdollActivationReason> Activated;
        public event Action Recovered;

        public Vector3 Position => pelvis != null ? pelvis.position : transform.position;
        public Transform PelvisTransform => pelvis != null ? pelvis : transform;
        public bool IsActiveRagdoll => _isActive;
        public bool IsFullSimulation => _isFullSim;
        public bool IsDead => _isDead;
        public RagdollActivationReason LastReason => _lastReason;
        public RagdollSettings Settings => settings;

        public void Bind(PlayerStateMachine stateMachine, RagdollSettings ragdollSettings = null)
        {
            _stateMachine = stateMachine;
            if (ragdollSettings != null)
                settings = ragdollSettings;
        }

        public void ConfigureBones(Transform pelvisTransform, Rigidbody[] bodies, Collider[] colliders)
        {
            pelvis = pelvisTransform;
            boneBodies = bodies ?? Array.Empty<Rigidbody>();
            boneColliders = colliders ?? Array.Empty<Collider>();
            DistributeMass();
            SetBonesSimulated(false);
        }

        public void Awake()
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<RagdollSettings>();
            _characterController = GetComponent<CharacterController>();
            _health = GetComponent<HealthSystem>();
            if (_health != null)
            {
                _health.Depleted += HandleDeath;
            }
            DistributeMass();
            SetBonesSimulated(false);
        }

        public void OnEnable()
        {
            RagdollBudget.Register(this);
        }

        public void OnDisable()
        {
            RagdollBudget.Unregister(this);
        }

        public void OnDestroy()
        {
            if (_health != null)
                _health.Depleted -= HandleDeath;
            RagdollBudget.Unregister(this);
        }

        public bool Activate(RagdollActivationReason reason, Vector3 impulse = default, BodyPart targetPart = BodyPart.Torso)
        {
            if (_stateMachine != null && !_stateMachine.TryEnterRagdoll(reason))
                return false;

            _isActive = true;
            _isFullSim = true;
            _lastReason = reason;
            _lowVelocityTimer = 0f;
            _getUpTimer = 0f;

            if (_characterController != null)
                _characterController.enabled = false;
            if (characterAnimator != null)
                characterAnimator.enabled = false;

            SetBonesSimulated(true);
            ApplyImpulseToBones(impulse, targetPart);
            Activated?.Invoke(reason);
            return true;
        }

        public void HandleDeath()
        {
            _isDead = true;
            if (_stateMachine != null)
                _stateMachine.ForceDie();
            _isActive = true;
            if (_characterController != null)
                _characterController.enabled = false;
            if (characterAnimator != null)
                characterAnimator.enabled = false;
            SetBonesSimulated(true);
        }

        public void SetFullSimulation(bool fullSim)
        {
            _isFullSim = fullSim;
            if (!_isActive || boneBodies == null) return;
            for (int i = 0; i < boneBodies.Length; i++)
            {
                Rigidbody rb = boneBodies[i];
                if (rb == null) continue;
                if (!fullSim)
                {
                    rb.isKinematic = true;
                    rb.Sleep();
                }
                else
                {
                    rb.isKinematic = false;
                    rb.WakeUp();
                }
            }
        }

        public float ComputeAverageSpeed()
        {
            if (boneBodies == null || boneBodies.Length == 0)
                return 0f;
            float sum = 0f;
            int count = 0;
            for (int i = 0; i < boneBodies.Length; i++)
            {
                if (boneBodies[i] == null) continue;
                sum += boneBodies[i].linearVelocity.magnitude;
                count++;
            }
            return count > 0 ? sum / count : 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!_isActive) return;

            if (_stateMachine != null && _stateMachine.Current == PlayerState.GettingUp)
            {
                _getUpTimer += deltaTime;
                if (_getUpTimer >= settings.getUpDuration)
                {
                    FinishRecovery();
                }
                return;
            }

            if (_isDead)
            {
                float deadSpeed = ComputeAverageSpeed();
                if (deadSpeed < settings.sleepVelocity && boneBodies != null)
                {
                    for (int i = 0; i < boneBodies.Length; i++)
                        boneBodies[i]?.Sleep();
                }
                return;
            }

            float avgSpeed = ComputeAverageSpeed();
            if (avgSpeed < settings.recoveryVelocity)
            {
                _lowVelocityTimer += deltaTime;
                if (_lowVelocityTimer >= settings.recoveryHoldTime)
                {
                    BeginGetUp();
                }
            }
            else
            {
                _lowVelocityTimer = 0f;
            }
        }

        public Vector3[] ReadBonePositions()
        {
            if (boneBodies == null || boneBodies.Length == 0)
                return Array.Empty<Vector3>();
            Vector3[] positions = new Vector3[boneBodies.Length];
            for (int i = 0; i < boneBodies.Length; i++)
            {
                positions[i] = boneBodies[i] != null ? boneBodies[i].position : transform.position;
            }
            return positions;
        }

        private void BeginGetUp()
        {
            if (pelvis != null)
                transform.position = pelvis.position;
            SetBonesSimulated(false);
            _getUpTimer = 0f;
            if (_stateMachine != null)
            {
                _stateMachine.TryTransition(PlayerState.GettingUp);
            }
            else
            {
                FinishRecovery();
            }
        }

        private void FinishRecovery()
        {
            _isActive = false;
            _lowVelocityTimer = 0f;
            _getUpTimer = 0f;
            if (_characterController != null)
                _characterController.enabled = true;
            if (characterAnimator != null)
                characterAnimator.enabled = true;
            _stateMachine?.TryTransition(PlayerState.Idle);
            Recovered?.Invoke();
        }

        private void DistributeMass()
        {
            if (boneBodies == null || boneBodies.Length == 0 || settings == null)
                return;
            float perBone = Mathf.Max(0.5f, settings.totalMass / boneBodies.Length);
            for (int i = 0; i < boneBodies.Length; i++)
            {
                if (boneBodies[i] != null)
                    boneBodies[i].mass = perBone;
            }
        }

        private void SetBonesSimulated(bool simulated)
        {
            if (boneBodies != null)
            {
                for (int i = 0; i < boneBodies.Length; i++)
                {
                    if (boneBodies[i] == null) continue;
                    boneBodies[i].isKinematic = !simulated;
                    if (!simulated)
                    {
                        boneBodies[i].linearVelocity = Vector3.zero;
                        boneBodies[i].angularVelocity = Vector3.zero;
                    }
                }
            }

            if (boneColliders != null)
            {
                for (int i = 0; i < boneColliders.Length; i++)
                {
                    if (boneColliders[i] != null)
                        boneColliders[i].enabled = simulated;
                }
            }
        }

        private void ApplyImpulseToBones(Vector3 impulse, BodyPart part)
        {
            if (impulse.sqrMagnitude < 1e-6f || boneBodies == null || boneBodies.Length == 0)
                return;

            int targetIdx = 0;
            if (part == BodyPart.Head && boneBodies.Length > 1)
                targetIdx = boneBodies.Length - 1;
            else if (part == BodyPart.Limb && boneBodies.Length > 2)
                targetIdx = boneBodies.Length / 2;

            if (boneBodies[targetIdx] != null)
                boneBodies[targetIdx].AddForce(impulse, ForceMode.Impulse);
        }
    }
}
