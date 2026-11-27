using System;
using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.Player
{
    // Pure selection logic for ragdoll simulation budget (§1.7 & §1.8):
    // <= 12 full-sim bodies inside 55 m sorted by distance; distant or overflow bodies simplified.
    public static class RagdollBudgetModel
    {
        public struct Candidate
        {
            public int id;
            public Vector3 position;
            public bool active;
        }

        public static void SelectFullSimulationIds(
            IReadOnlyList<Candidate> candidates,
            Vector3 viewerPosition,
            int maxFullSimCount,
            float fullSimRadius,
            HashSet<int> outFullSimIds)
        {
            if (outFullSimIds == null) return;
            outFullSimIds.Clear();
            if (candidates == null || candidates.Count == 0 || maxFullSimCount <= 0 || fullSimRadius <= 0f)
                return;

            float maxDistSqr = fullSimRadius * fullSimRadius;
            List<(int id, float distSqr)> eligible = new List<(int, float)>(candidates.Count);
            for (int i = 0; i < candidates.Count; i++)
            {
                Candidate c = candidates[i];
                if (!c.active) continue;
                float dSqr = (c.position - viewerPosition).sqrMagnitude;
                if (dSqr <= maxDistSqr)
                {
                    eligible.Add((c.id, dSqr));
                }
            }

            eligible.Sort((a, b) =>
            {
                int cmp = a.distSqr.CompareTo(b.distSqr);
                return cmp != 0 ? cmp : a.id.CompareTo(b.id);
            });

            int count = Math.Min(maxFullSimCount, eligible.Count);
            for (int i = 0; i < count; i++)
            {
                outFullSimIds.Add(eligible[i].id);
            }
        }
    }
}
