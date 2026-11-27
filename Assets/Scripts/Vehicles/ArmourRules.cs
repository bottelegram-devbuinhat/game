using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    public static class ArmourRules
    {
        public static float BulletDamageMultiplier(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Light: return 0.60f;
                case ArmourClass.Heavy: return 0.15f;
                case ArmourClass.Air: return 0.75f;
                default: return 1.0f;
            }
        }

        public static float ExplosionDamageMultiplier(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Light: return 0.75f;
                case ArmourClass.Heavy: return 0.45f;
                case ArmourClass.Air: return 1.15f;
                default: return 1.0f;
            }
        }

        public static float RamDamageMultiplier(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Light: return 0.65f;
                case ArmourClass.Heavy: return 0.25f;
                case ArmourClass.Air: return 1.30f;
                default: return 1.0f;
            }
        }

        public static bool CanHotwire(ArmourClass armour)
        {
            return armour == ArmourClass.Unarmoured || armour == ArmourClass.Light;
        }

        public static float SpeedMultiplier(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Light: return 0.95f;
                case ArmourClass.Heavy: return 0.85f;
                default: return 1.0f;
            }
        }

        public static int PoliceDeterrenceLevel(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Light: return 1;
                case ArmourClass.Heavy: return 3;
                case ArmourClass.Air: return 2;
                default: return 0;
            }
        }

        public static int PassiveDrivingHeatPerMinute(ArmourClass armour)
        {
            switch (armour)
            {
                case ArmourClass.Heavy: return 4;
                case ArmourClass.Air: return 2;
                default: return 0;
            }
        }

        public static float EffectiveMaxHealth(float baseHealth, ArmourClass armour, int armourUpgradeLevel = 0)
        {
            float mult = 1.0f;
            if (armour == ArmourClass.Light) mult = 1.25f;
            else if (armour == ArmourClass.Heavy) mult = 1.60f;
            float upg = 1.0f + Mathf.Clamp(armourUpgradeLevel, 0, 5) * 0.10f;
            return Mathf.Max(1f, baseHealth * mult * upg);
        }
    }
}
