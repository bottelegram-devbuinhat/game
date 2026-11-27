using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.Player
{
    // One static per-frame budget driver; no per-ragdoll Update (§1.3 Rule 10 & §1.8).
    // Landmine #18: ALWAYS reads target.Position (pelvis/root), NEVER ReadBonePositions()[0].
    public static class RagdollBudget
    {
        private static readonly List<IRagdollTargetSource> Registered = new List<IRagdollTargetSource>(32);
        private static readonly List<RagdollBudgetModel.Candidate> ScratchCandidates = new List<RagdollBudgetModel.Candidate>(32);
        private static readonly HashSet<int> ScratchSelected = new HashSet<int>();

        public static int RegisteredCount => Registered.Count;

        public static void Register(IRagdollTargetSource source)
        {
            if (source != null && !Registered.Contains(source))
                Registered.Add(source);
        }

        public static void Unregister(IRagdollTargetSource source)
        {
            if (source != null)
                Registered.Remove(source);
        }

        public static void ClearAll()
        {
            Registered.Clear();
            ScratchCandidates.Clear();
            ScratchSelected.Clear();
        }

        public static void Tick(
            Vector3 viewerPosition,
            int maxFullSim = 12,
            float fullSimRadius = 55f,
            float deltaTime = 0f,
            IRagdollTargetSource skipStepTarget = null)
        {
            ScratchCandidates.Clear();
            for (int i = Registered.Count - 1; i >= 0; i--)
            {
                IRagdollTargetSource src = Registered[i];
                if (src == null)
                {
                    Registered.RemoveAt(i);
                    continue;
                }
                ScratchCandidates.Add(new RagdollBudgetModel.Candidate
                {
                    id = i,
                    position = src.Position,
                    active = src.IsActiveRagdoll
                });
            }

            RagdollBudgetModel.SelectFullSimulationIds(
                ScratchCandidates,
                viewerPosition,
                maxFullSim,
                fullSimRadius,
                ScratchSelected);

            for (int i = 0; i < Registered.Count; i++)
            {
                IRagdollTargetSource src = Registered[i];
                if (src == null || !src.IsActiveRagdoll) continue;
                src.SetFullSimulation(ScratchSelected.Contains(i));
                if (deltaTime > 0f && !ReferenceEquals(src, skipStepTarget) && src is RagdollController rc)
                {
                    rc.Tick(deltaTime);
                }
            }
        }
    }
}
