using UnityEngine;

namespace DudeChaos.Wanted
{
    public static class PoliceChaseMath
    {
        public static Vector3 ComputeChaseDirection(Vector3 pursuerPos, Vector3 targetPos)
        {
            Vector3 flat = targetPos - pursuerPos;
            flat.y = 0f;
            return flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.zero;
        }

        public static float ComputeSteerInput(Vector3 carForward, Vector3 toTarget, float maxSteerAngle = 36f)
        {
            Vector3 f = new Vector3(carForward.x, 0f, carForward.z).normalized;
            Vector3 t = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
            if (f.sqrMagnitude < 1e-4f || t.sqrMagnitude < 1e-4f)
                return 0f;
            float signed = Vector3.SignedAngle(f, t, Vector3.up);
            return Mathf.Clamp(signed / Mathf.Max(5f, maxSteerAngle), -1f, 1f);
        }

        public static bool ShouldBrake(float distanceToTarget, float currentSpeed, bool allowRam)
        {
            if (allowRam) return false;
            return distanceToTarget < 6.5f && currentSpeed > 4.0f;
        }

        public static bool IsWedged(float commandedThrottle, float actualSpeed, float stuckSeconds, float thresholdSeconds = 1.6f)
        {
            return Mathf.Abs(commandedThrottle) > 0.4f && actualSpeed < 0.6f && stuckSeconds >= thresholdSeconds;
        }
    }
}
