using UnityEngine;

namespace DudeChaos.Vehicles
{
    public static class NitroRules
    {
        public const float BaseCapacity = 100f;
        public const float BurnPerSecond = 34f;
        public const float RechargePerSecond = 14f;
        public const float RechargeDelaySeconds = 1.6f;
        public const float MinActivationCharge = 15f;

        public static float MaxChargeForLevel(int nitroUpgradeLevel)
        {
            return BaseCapacity + Mathf.Clamp(nitroUpgradeLevel, 0, 5) * 15f;
        }

        public static float ThrustMultiplierForLevel(int nitroUpgradeLevel)
        {
            if (nitroUpgradeLevel <= 0) return 1.0f;
            return 1.35f + Mathf.Clamp(nitroUpgradeLevel, 1, 5) * 0.08f;
        }
    }
}
