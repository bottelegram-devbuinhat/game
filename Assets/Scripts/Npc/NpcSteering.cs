using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.Npc
{
    public static class NpcSteering
    {
        public static Vector3 ArriveVelocity(Vector3 currentPos, Vector3 targetPos, float maxSpeed, float slowingRadius = 3.0f)
        {
            Vector3 toTarget = targetPos - currentPos;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;
            if (dist < 0.05f || maxSpeed <= 0f)
                return Vector3.zero;

            float ramped = dist < slowingRadius
                ? maxSpeed * (dist / Mathf.Max(0.01f, slowingRadius))
                : maxSpeed;
            return (toTarget / dist) * ramped;
        }

        public static int AdvanceWaypointIndex(Vector3 currentPos, IReadOnlyList<Vector3> waypoints, int currentIndex, float reachRadius = 1.2f)
        {
            if (waypoints == null || waypoints.Count == 0) return 0;
            int idx = Mathf.Clamp(currentIndex, 0, waypoints.Count - 1);
            Vector3 diff = waypoints[idx] - currentPos;
            diff.y = 0f;
            if (diff.sqrMagnitude <= reachRadius * reachRadius)
                return (idx + 1) % waypoints.Count;
            return idx;
        }

        public static Vector3 SeparationOffset(Vector3 selfPos, IReadOnlyList<Vector3> neighbors, float personalRadius = 1.4f)
        {
            if (neighbors == null || neighbors.Count == 0 || personalRadius <= 0.01f)
                return Vector3.zero;

            Vector3 push = Vector3.zero;
            float rSqr = personalRadius * personalRadius;
            for (int i = 0; i < neighbors.Count; i++)
            {
                Vector3 away = selfPos - neighbors[i];
                away.y = 0f;
                float dSqr = away.sqrMagnitude;
                if (dSqr > 1e-5f && dSqr < rSqr)
                {
                    float d = Mathf.Sqrt(dSqr);
                    push += (away / d) * (1f - d / personalRadius);
                }
            }
            return push;
        }

        public static bool IsInViewCone(Vector3 origin, Vector3 forward, Vector3 point, float maxRange, float halfAngleDegrees)
        {
            Vector3 toPt = point - origin;
            toPt.y = 0f;
            float dSqr = toPt.sqrMagnitude;
            if (dSqr > maxRange * maxRange || dSqr < 1e-6f)
                return false;
            Vector3 f = new Vector3(forward.x, 0f, forward.z).normalized;
            float cos = Vector3.Dot(f, toPt.normalized);
            return cos >= Mathf.Cos(halfAngleDegrees * Mathf.Deg2Rad);
        }

        public static Vector3 DeterministicWanderDirection(int agentSeed, float timeSeconds)
        {
            float phase = (agentSeed * 1.618f) + (timeSeconds * 0.45f);
            float angle = Mathf.Sin(phase) * Mathf.PI;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
        }
    }
}
