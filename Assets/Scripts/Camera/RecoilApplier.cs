using DudeChaos.Combat;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.CameraRig
{
    public sealed class RecoilApplier : MonoBehaviour
    {
        [SerializeField] private Transform aimPivot;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private CombatSettings combatSettings;

        private Vector2 _currentOffset;
        private Vector2 _recoveryVelocity;

        public Vector2 CurrentOffset => _currentOffset;

        public void OnEnable()
        {
            if (weaponController != null)
                weaponController.RecoilPublished += OnRecoil;
        }

        public void OnDisable()
        {
            if (weaponController != null)
                weaponController.RecoilPublished -= OnRecoil;
        }

        public void Bind(WeaponController weapon, Transform pivot, CombatSettings settings = null)
        {
            if (weaponController != null)
                weaponController.RecoilPublished -= OnRecoil;
            weaponController = weapon;
            aimPivot = pivot;
            if (settings != null) combatSettings = settings;
            if (weaponController != null)
                weaponController.RecoilPublished += OnRecoil;
        }

        public void OnRecoil(Vector2 step)
        {
            _currentOffset += step;
            _currentOffset.x = Mathf.Clamp(_currentOffset.x, -12f, 12f);
            _currentOffset.y = Mathf.Clamp(_currentOffset.y, 0f, 22f);
        }

        public void Step(float deltaTime)
        {
            float recTime = combatSettings != null ? combatSettings.recoilRecoveryTime : 0.18f;
            _currentOffset.x = Mathf.SmoothDamp(_currentOffset.x, 0f, ref _recoveryVelocity.x, recTime, float.PositiveInfinity, Mathf.Max(0.0001f, deltaTime));
            _currentOffset.y = Mathf.SmoothDamp(_currentOffset.y, 0f, ref _recoveryVelocity.y, recTime, float.PositiveInfinity, Mathf.Max(0.0001f, deltaTime));
            if (aimPivot != null)
            {
                aimPivot.localRotation = Quaternion.Euler(-_currentOffset.y, _currentOffset.x, 0f);
            }
        }

        public void LateUpdate() => Step(Time.deltaTime);
    }
}
