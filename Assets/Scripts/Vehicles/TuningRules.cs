using UnityEngine;

namespace DudeChaos.Vehicles
{
    public enum TuningCategory
    {
        Engine = 0,
        Brakes = 1,
        Tires = 2,
        Weight = 3,
        Armour = 4,
        Nitro = 5
    }

    public static class TuningRules
    {
        public const int MaxUpgradeLevel = 5;

        public static int GetUpgradeCost(TuningCategory category, int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= MaxUpgradeLevel)
                return 0;
            int baseCost = category == TuningCategory.Armour || category == TuningCategory.Nitro ? 1600 : 1200;
            return baseCost * (currentLevel + 1);
        }

        public static float TopSpeedMultiplier(int engineLevel, int weightLevel)
        {
            float e = 1f + Mathf.Clamp(engineLevel, 0, MaxUpgradeLevel) * 0.05f;
            float w = 1f + Mathf.Clamp(weightLevel, 0, MaxUpgradeLevel) * 0.02f;
            return e * w;
        }

        public static float AccelerationMultiplier(int engineLevel, int weightLevel)
        {
            float e = 1f + Mathf.Clamp(engineLevel, 0, MaxUpgradeLevel) * 0.06f;
            float w = 1f + Mathf.Clamp(weightLevel, 0, MaxUpgradeLevel) * 0.04f;
            return e * w;
        }

        public static float BrakeMultiplier(int brakeLevel)
        {
            return 1f + Mathf.Clamp(brakeLevel, 0, MaxUpgradeLevel) * 0.07f;
        }

        public static float GripMultiplier(int tireLevel)
        {
            return 1f + Mathf.Clamp(tireLevel, 0, MaxUpgradeLevel) * 0.06f;
        }
    }
}
