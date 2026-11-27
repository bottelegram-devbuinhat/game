using UnityEngine;

namespace DudeChaos.Progression
{
    public static class StuntRules
    {
        public const float MinDriftSlipAngle = 18f;
        public const float MinDriftSpeed = 8f;
        public const float SettleDelaySeconds = 1.25f;
        public const float MoneyPerPoint = 0.25f;

        public static float ComputeSlipAngleDegrees(Vector3 forward, Vector3 planarVelocity)
        {
            Vector3 f = new Vector3(forward.x, 0f, forward.z).normalized;
            Vector3 v = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
            if (f.sqrMagnitude < 1e-4f || v.sqrMagnitude < 1f)
                return 0f;
            return Vector3.Angle(f, v.normalized);
        }

        public static float DriftPointsPerSecond(float slipAngleDeg, float speed)
        {
            if (speed < MinDriftSpeed || slipAngleDeg < MinDriftSlipAngle || slipAngleDeg > 135f)
                return 0f;
            return (slipAngleDeg - MinDriftSlipAngle) * (speed * 0.18f);
        }

        public static float AirtimePointsPerSecond(float speed)
        {
            return speed > 6f ? 85f + speed * 2.5f : 0f;
        }

        public static int NearMissPoints(float relativeSpeed)
        {
            return relativeSpeed >= 12f ? Mathf.RoundToInt(40f + relativeSpeed * 2f) : 0;
        }

        public static int ComboCap(int combo) => Mathf.Clamp(combo, 1, 5);

        public static int PointsToMoney(float bankedPoints)
        {
            return Mathf.Max(0, Mathf.RoundToInt(bankedPoints * MoneyPerPoint));
        }
    }
}
