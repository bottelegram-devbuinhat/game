using UnityEngine;

namespace DudeChaos.Progression
{
    // §1.8 & §2.6: 100 rep per level -> 0 rep = level 1, 1815 rep = level 19.
    public static class ProgressionCurve
    {
        public const int RepPerLevel = 100;

        public static int LevelForReputation(int reputation)
        {
            int clamped = Mathf.Max(0, reputation);
            return 1 + (clamped / RepPerLevel);
        }

        public static int ReputationForLevel(int level)
        {
            if (level <= 1) return 0;
            return (level - 1) * RepPerLevel;
        }

        public static float ProgressWithinLevel01(int reputation)
        {
            int clamped = Mathf.Max(0, reputation);
            return (clamped % RepPerLevel) / (float)RepPerLevel;
        }
    }
}
