using DudeChaos.Data;
using DudeChaos.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace DudeChaos.CameraRig
{
    public sealed class ThirdPersonCameraRig : MonoBehaviour, ICameraPunch
    {
        public const float DegreesPerPunchUnit = 1.6f;
        public const float PunchDecaySeconds = 0.09f;
        public const float MaxPunchStrength = 3.0f;

        [SerializeField] private Transform yawPivot;
        [SerializeField] private Transform pitchPivot;
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private float minPitch = -55f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float sprintFovBlendSpeed = 10f;

        private float _yaw;
        private float _pitch = 12f;
        private float _baseFov = 60f;
        private float _currentSprintKick;
        private float _punchStrength;

        public float Yaw => _yaw;
        public float Pitch => _pitch;
        public float BaseFov => _baseFov;
        public float CurrentFov => virtualCamera != null ? virtualCamera.Lens.FieldOfView : _baseFov;

        public void Awake()
        {
            if (virtualCamera != null)
            {
                _baseFov = virtualCamera.Lens.FieldOfView > 1f ? virtualCamera.Lens.FieldOfView : 60f;
            }
        }

        public void Configure(
            Transform yaw,
            Transform pitch,
            CinemachineCamera vcam,
            PlayerMotor motor = null,
            MovementSettings settings = null)
        {
            yawPivot = yaw;
            pitchPivot = pitch;
            virtualCamera = vcam;
            playerMotor = motor;
            movementSettings = settings;
            if (virtualCamera != null && virtualCamera.Lens.FieldOfView > 1f)
                _baseFov = virtualCamera.Lens.FieldOfView;
        }

        public void AddLookDelta(float yawDelta, float pitchDelta)
        {
            _yaw = Mathf.Repeat(_yaw + yawDelta, 360f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);
            ApplyPivotRotations();
        }

        public void Punch(float strength)
        {
            if (strength <= 0f) return;
            _punchStrength = Mathf.Clamp(_punchStrength + strength, 0f, MaxPunchStrength);
        }

        public void Step(float deltaTime)
        {
            ApplyPivotRotations();

            float targetSprintKick = 0f;
            if (playerMotor != null && playerMotor.IsSprinting)
            {
                targetSprintKick = movementSettings != null ? movementSettings.sprintFovKick : 5f;
            }
            _currentSprintKick = Mathf.MoveTowards(_currentSprintKick, targetSprintKick, sprintFovBlendSpeed * Mathf.Max(0f, deltaTime));

            if (_punchStrength > 0f)
            {
                float decayRate = MaxPunchStrength / PunchDecaySeconds;
                _punchStrength = Mathf.MoveTowards(_punchStrength, 0f, decayRate * Mathf.Max(0f, deltaTime));
            }

            // §1.9.5: Cinemachine 3 Lens is a struct — copy, edit, write back.
            if (virtualCamera != null)
            {
                LensSettings lens = virtualCamera.Lens;
                lens.FieldOfView = _baseFov + _currentSprintKick + (_punchStrength * DegreesPerPunchUnit);
                virtualCamera.Lens = lens;
            }
        }

        public void LateUpdate()
        {
            Step(Time.deltaTime);
        }

        private void ApplyPivotRotations()
        {
            if (yawPivot != null)
                yawPivot.localRotation = Quaternion.Euler(0f, _yaw, 0f);
            if (pitchPivot != null)
                pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
