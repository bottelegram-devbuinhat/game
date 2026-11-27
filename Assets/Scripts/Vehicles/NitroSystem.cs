using UnityEngine;

namespace DudeChaos.Vehicles
{
    public sealed class NitroSystem : MonoBehaviour
    {
        [SerializeField] private int upgradeLevel = 1;
        private float _charge = 100f;
        private float _cooldownSinceBurn = 99f;
        private bool _active;

        public float Charge => _charge;
        public float MaxCharge => NitroRules.MaxChargeForLevel(upgradeLevel);
        public float NormalizedCharge => MaxCharge > 0f ? Mathf.Clamp01(_charge / MaxCharge) : 0f;
        public bool IsBoosting => _active;

        public void SetUpgradeLevel(int level)
        {
            upgradeLevel = Mathf.Clamp(level, 0, 5);
            _charge = Mathf.Min(_charge, MaxCharge);
        }

        public float Step(bool requested, float speed, float deltaTime)
        {
            if (upgradeLevel <= 0)
            {
                _active = false;
                return 1.0f;
            }

            float dt = Mathf.Max(0f, deltaTime);
            bool canStart = _active ? _charge > 0.1f : _charge >= NitroRules.MinActivationCharge;
            if (requested && speed > 2f && canStart)
            {
                _active = true;
                _cooldownSinceBurn = 0f;
                _charge = Mathf.Max(0f, _charge - NitroRules.BurnPerSecond * dt);
                if (_charge <= 0f) _active = false;
                return NitroRules.ThrustMultiplierForLevel(upgradeLevel);
            }

            _active = false;
            _cooldownSinceBurn += dt;
            if (_cooldownSinceBurn >= NitroRules.RechargeDelaySeconds)
            {
                _charge = Mathf.Min(MaxCharge, _charge + NitroRules.RechargePerSecond * dt);
            }
            return 1.0f;
        }
    }
}
