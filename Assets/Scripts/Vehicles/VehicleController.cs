using DudeChaos.Data;
using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    // Arcade yaw + forward force model; NO WheelCollider (§1.2 & §1.7 & §1.8).
    // Landmines #1, #13, #20 all guarded here.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class VehicleController : MonoBehaviour
    {
        [SerializeField] private VehicleData vehicleData;
        [SerializeField] private VehicleSettings settings;
        [SerializeField] private Transform seatAnchor;
        [SerializeField] private MonoBehaviour driverInputBehaviour;

        private Rigidbody _rb;
        private VehicleLock _lock;
        private NitroSystem _nitro;
        private IPlayerInput _resolvedInput;
        private PlayerMotor _driverMotor;
        private RagdollController _driverRagdoll;
        private CharacterController _driverCC;

        private float _transitionTimer;
        private bool _entering;
        private bool _exiting;
        private bool _pendingEject;
        private bool _warnedMissingInput;

        public VehicleData Data => vehicleData;
        public VehicleSettings Settings => settings;
        public bool HasDriver => _driverMotor != null;
        public bool IsTransitioning => _entering || _exiting;
        public float SpeedMetersPerSecond => _rb != null ? _rb.linearVelocity.magnitude : 0f;
        public float SignedForwardSpeed => _rb != null ? Vector3.Dot(_rb.linearVelocity, transform.forward) : 0f;

        public void Awake()
        {
            if (settings == null) settings = ScriptableObject.CreateInstance<VehicleSettings>();
            _rb = GetComponent<Rigidbody>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();
            _lock = GetComponent<VehicleLock>();
            _nitro = GetComponent<NitroSystem>();

            // Landmine #1: Validate centerOfMassOffset in Awake before PhysX Simulate ever runs.
            Vector3 rawCom = vehicleData != null ? vehicleData.centerOfMassOffset : VehicleMath.DefaultCenterOfMass;
            if (!VehicleMath.IsUsableCenterOfMass(rawCom))
            {
                string assetName = vehicleData != null ? vehicleData.name : name;
                Debug.LogError($"VehicleController '{assetName}' had invalid centerOfMassOffset {rawCom}; using {VehicleMath.DefaultCenterOfMass}.");
                _rb.centerOfMass = VehicleMath.DefaultCenterOfMass;
            }
            else
            {
                _rb.centerOfMass = rawCom;
            }
        }

        public void Configure(VehicleData data, VehicleSettings vehicleSettings = null, IPlayerInput explicitInput = null)
        {
            vehicleData = data;
            if (vehicleSettings != null) settings = vehicleSettings;
            if (explicitInput != null) _explicitDriverInput = explicitInput;
            if (_rb == null) _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            Vector3 com = data != null ? data.centerOfMassOffset : VehicleMath.DefaultCenterOfMass;
            if (!VehicleMath.IsUsableCenterOfMass(com))
            {
                Debug.LogError($"VehicleController '{(data != null ? data.name : name)}' had invalid centerOfMassOffset; using fallback.");
                _rb.centerOfMass = VehicleMath.DefaultCenterOfMass;
            }
            else
            {
                _rb.centerOfMass = com;
            }
        }

        private IPlayerInput _explicitDriverInput;

        // Landmine #20: Fallback chain driverInput -> motor.Input -> GetComponent<IPlayerInput>() -> warning.
        public IPlayerInput ResolveInput()
        {
            if (_explicitDriverInput != null) return _explicitDriverInput;
            if (driverInputBehaviour is IPlayerInput serialized) return serialized;
            if (_driverMotor != null && _driverMotor.Input != null) return _driverMotor.Input;
            if (_driverMotor != null && _driverMotor.TryGetComponent(out IPlayerInput onDriver)) return onDriver;
            if (TryGetComponent(out IPlayerInput onSelf)) return onSelf;
            if (!_warnedMissingInput)
            {
                _warnedMissingInput = true;
                Debug.LogWarning($"VehicleController '{name}' has no driver IPlayerInput bound.");
            }
            return null;
        }

        public bool TryEnter(PlayerMotor driver)
        {
            if (driver == null || HasDriver || IsTransitioning)
                return false;
            if (_lock != null && _lock.IsLocked)
                return false;
            if (!driver.StateMachine.TryTransition(PlayerState.EnteringVehicle))
                return false;

            _driverMotor = driver;
            _driverRagdoll = driver.GetComponent<RagdollController>();
            _driverCC = driver.GetComponent<CharacterController>();
            if (_driverCC != null) _driverCC.enabled = false;

            _entering = true;
            _exiting = false;
            _pendingEject = false;
            _transitionTimer = settings != null ? settings.enterDuration : 0.45f;
            _warnedMissingInput = false;
            _resolvedInput = ResolveInput();

            Transform seat = seatAnchor != null ? seatAnchor : transform;
            driver.transform.SetParent(seat, false);
            driver.transform.localPosition = Vector3.zero;
            return true;
        }

        // Landmine #13: Route through ExitingVehicle before Ragdoll so InVehicle -> Ragdoll never softlocks.
        public bool TryExit()
        {
            if (!HasDriver || _entering || _exiting)
                return false;

            if (!_driverMotor.StateMachine.TryTransition(PlayerState.ExitingVehicle))
                return false;

            _driverMotor.transform.SetParent(null, true);
            float exitOffset = settings != null ? settings.exitOffset : 1.4f;
            _driverMotor.transform.position = transform.position - transform.right * exitOffset + Vector3.up * 0.5f;

            float speed = SpeedMetersPerSecond;
            if (VehicleMath.ShouldEjectOnExit(speed, settings))
            {
                _pendingEject = true;
                Vector3 ejectImpulse = VehicleMath.ComputeEjectImpulse(transform.forward, settings);
                bool ragdolled = false;
                if (_driverRagdoll != null)
                {
                    ragdolled = _driverRagdoll.Activate(RagdollActivationReason.EjectedFromVehicle, ejectImpulse, BodyPart.Torso);
                }
                else
                {
                    ragdolled = _driverMotor.StateMachine.TryEnterRagdoll(RagdollActivationReason.EjectedFromVehicle);
                }

                if (ragdolled)
                {
                    ClearDriverReferences(reEnableCollider: _driverRagdoll == null);
                    return true;
                }
            }

            _exiting = true;
            _transitionTimer = settings != null ? settings.exitDuration : 0.45f;
            return true;
        }

        public void Step(float fixedDeltaTime)
        {
            float dt = Mathf.Max(0f, fixedDeltaTime);
            if (_entering)
            {
                _transitionTimer -= dt;
                if (_transitionTimer <= 0f)
                {
                    _entering = false;
                    _driverMotor?.StateMachine.TryTransition(PlayerState.InVehicle);
                }
                return;
            }

            if (_exiting)
            {
                _transitionTimer -= dt;
                if (_transitionTimer <= 0f)
                {
                    _exiting = false;
                    _pendingEject = false;
                    _driverMotor?.StateMachine.TryTransition(PlayerState.Idle);
                    ClearDriverReferences(reEnableCollider: true);
                }
                return;
            }

            if (!HasDriver || _rb == null)
                return;

            _resolvedInput = ResolveInput();
            Vector2 move = _resolvedInput != null ? _resolvedInput.Move : Vector2.zero;
            float throttle = Mathf.Clamp(move.y, -1f, 1f);
            float steer = Mathf.Clamp(move.x, -1f, 1f);

            float maxSpeed = vehicleData != null ? vehicleData.maxSpeed : 33f;
            float accel = vehicleData != null ? vehicleData.acceleration : 14f;
            float brake = vehicleData != null ? vehicleData.brakeForce : 26f;
            float maxSteer = vehicleData != null ? vehicleData.steerAngle : 36f;
            ArmourClass ac = vehicleData != null ? vehicleData.armourClass : ArmourClass.Unarmoured;
            maxSpeed *= ArmourRules.SpeedMultiplier(ac);

            float nitroMult = _nitro != null && _resolvedInput != null
                ? _nitro.Step(_resolvedInput.NitroHeld, SpeedMetersPerSecond, dt)
                : 1.0f;

            float fwdSpeed = SignedForwardSpeed;
            bool braking = (throttle < -0.05f && fwdSpeed > 0.5f) || (throttle > 0.05f && fwdSpeed < -0.5f);

            if (braking)
            {
                Vector3 v = _rb.linearVelocity;
                Vector3 planar = new Vector3(v.x, 0f, v.z);
                planar = Vector3.MoveTowards(planar, Vector3.zero, brake * dt);
                _rb.linearVelocity = new Vector3(planar.x, v.y, planar.z);
            }
            else if (Mathf.Abs(throttle) > 0.01f)
            {
                float cap = throttle > 0f ? maxSpeed * nitroMult : maxSpeed * 0.35f;
                if (Mathf.Abs(fwdSpeed) < cap)
                {
                    _rb.AddForce(transform.forward * (throttle * accel * nitroMult), ForceMode.Acceleration);
                }
            }

            if (Mathf.Abs(fwdSpeed) > 0.4f && Mathf.Abs(steer) > 0.01f)
            {
                float steerFactor = VehicleMath.ComputeSteeringFactor(fwdSpeed, maxSpeed, settings);
                float dirSign = fwdSpeed >= 0f ? 1f : -1f;
                float yawDelta = steer * maxSteer * steerFactor * dirSign * dt * 2.4f;
                transform.rotation = transform.rotation * Quaternion.Euler(0f, yawDelta, 0f);
            }

            float downforce = settings != null ? settings.downforce : 12f;
            _rb.AddForce(Vector3.down * downforce, ForceMode.Acceleration);
        }

        public void FixedUpdate()
        {
            Step(Time.fixedDeltaTime);
        }

        private void ClearDriverReferences(bool reEnableCollider)
        {
            if (reEnableCollider && _driverCC != null)
                _driverCC.enabled = true;
            _driverMotor = null;
            _driverRagdoll = null;
            _driverCC = null;
            _resolvedInput = null;
            _entering = false;
            _exiting = false;
        }
    }
}
