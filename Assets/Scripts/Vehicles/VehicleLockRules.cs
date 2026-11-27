using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    public static class VehicleLockRules
    {
        public static float LockChance(HandlingProfile profile, ArmourClass armour, bool parkedAtNight)
        {
            if (!ArmourRules.CanHotwire(armour))
                return 1.0f;
            float baseChance = profile == HandlingProfile.Sports ? 0.55f : 0.30f;
            if (parkedAtNight) baseChance += 0.15f;
            return Mathf.Clamp01(baseChance);
        }

        public static bool RollInitiallyLocked(float roll01, HandlingProfile profile, ArmourClass armour, bool parkedAtNight)
        {
            return roll01 < LockChance(profile, armour, parkedAtNight);
        }

        public static float HotwireDurationSeconds(HandlingProfile profile, ArmourClass armour)
        {
            if (!ArmourRules.CanHotwire(armour))
                return float.PositiveInfinity;
            float sec = profile == HandlingProfile.Sports ? 3.2f : 2.0f;
            if (armour == ArmourClass.Light) sec += 1.5f;
            return sec;
        }

        public static int BreakInCrimeHeat(ArmourClass armour)
        {
            return armour == ArmourClass.Unarmoured ? 2 : 3;
        }
    }
}
