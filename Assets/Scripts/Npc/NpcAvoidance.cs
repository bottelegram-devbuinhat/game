using UnityEngine;

namespace DudeChaos.Npc
{
    // Two feeler probes; turn toward clearer side; deterministic tie-break when both blocked so zero jitter (§1.8 & §4.2).
    public static class NpcAvoidance
    {
        public static Vector3 SteerClear(
            Vector3 forward,
            bool leftBlocked,
            float leftClearance,
            bool rightBlocked,
            float rightClearance,
            int agentTieBreakId = 0)
        {
            Vector3 f = new Vector3(forward.x, 0f, forward.z).normalized;
            if (f.sqrMagnitude < 1e-5f) f = Vector3.forward;

            if (!leftBlocked && !rightBlocked)
                return f;

            float turnSign;
            if (leftBlocked && !rightBlocked)
            {
                turnSign = 1f; // Turn right
            }
            else if (!leftBlocked && rightBlocked)
            {
                turnSign = -1f; // Turn left
            }
            else
            {
                // Both blocked: pick clearer side, or deterministic sign by agent ID (no frame-to-frame oscillation!).
                float diff = rightClearance - leftClearance;
                if (Mathf.Abs(diff) > 0.05f)
                    turnSign = diff > 0f ? 1f : -1f;
                else
                    turnSign = (agentTieBreakId & 1) == 0 ? 1f : -1f;
            }

            return (Quaternion.Euler(0f, turnSign * 55f, 0f) * f).normalized;
        }
    }
}
