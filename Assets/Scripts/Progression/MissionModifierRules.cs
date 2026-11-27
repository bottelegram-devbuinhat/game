using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Progression
{
    public enum MissionVariant
    {
        None = 0,
        TimeAttack = 1,
        HotZone = 2,
        Fragile = 3
    }

    // §1.8 & §2.8: Variants only allowed on a mission already completed at least once.
    // Time Attack: -40% clock (x0.60), +15% payout.
    // Hot Zone: fail one wanted level earlier, +30% payout.
    // Fragile: double damage weight for grading, +20% payout.
    public static class MissionModifierRules
    {
        public static bool CanRunVariant(MissionVariant variant, bool missionCompletedAtLeastOnce)
        {
            if (variant == MissionVariant.None) return true;
            return missionCompletedAtLeastOnce;
        }

        public static float EffectiveTimeLimit(MissionData mission, MissionVariant variant)
        {
            if (mission == null || mission.timeLimitSeconds <= 0f)
                return 0f;
            return variant == MissionVariant.TimeAttack
                ? mission.timeLimitSeconds * 0.60f
                : mission.timeLimitSeconds;
        }

        public static int EffectiveFailWantedLevel(MissionData mission, MissionVariant variant)
        {
            int baseFail = mission != null ? mission.failAtWantedLevel : 3;
            if (baseFail <= 0) return 0;
            if (variant == MissionVariant.HotZone)
                return Mathf.Max(1, baseFail - 1);
            return baseFail;
        }

        public static float DamageGradingMultiplier(MissionVariant variant)
        {
            return variant == MissionVariant.Fragile ? 2.0f : 1.0f;
        }

        public static float PayoutMultiplier(MissionVariant variant)
        {
            switch (variant)
            {
                case MissionVariant.TimeAttack: return 1.15f;
                case MissionVariant.HotZone: return 1.30f;
                case MissionVariant.Fragile: return 1.20f;
                default: return 1.0f;
            }
        }
    }
}
