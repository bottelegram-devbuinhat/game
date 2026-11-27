using System.Collections.Generic;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Progression
{
    public static class MissionSelectRules
    {
        public static bool IsUnlocked(int missionIndex, IReadOnlyList<MissionData> chain, ISet<string> completedIds)
        {
            if (chain == null || missionIndex < 0 || missionIndex >= chain.Count)
                return false;
            if (missionIndex == 0)
                return true;
            MissionData prev = chain[missionIndex - 1];
            if (prev == null || string.IsNullOrEmpty(prev.missionId))
                return false;
            return completedIds != null && completedIds.Contains(prev.missionId);
        }

        public static float ComputeProgressPercent(int totalMissions, ISet<string> completedIds)
        {
            if (totalMissions <= 0 || completedIds == null)
                return 0f;
            return Mathf.Clamp01(completedIds.Count / (float)totalMissions) * 100f;
        }
    }
}
