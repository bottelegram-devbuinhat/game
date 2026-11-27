using DudeChaos.City;
using UnityEngine;

namespace DudeChaos.Visuals
{
    public struct WalkPoseAngles
    {
        public float leftThighPitch;
        public float rightThighPitch;
        public float leftCalfPitch;
        public float rightCalfPitch;
        public float leftArmPitch;
        public float rightArmPitch;
        public float hipBobOffset;
    }

    public static class AnimationMath
    {
        public static WalkPoseAngles EvaluateWalkPose(float planarSpeed, float phaseRadians)
        {
            float amp = Mathf.Clamp01(planarSpeed / 6.0f);
            float s = Mathf.Sin(phaseRadians);
            float c = Mathf.Cos(phaseRadians);
            float swing = 34f * amp;

            return new WalkPoseAngles
            {
                leftThighPitch = s * swing,
                rightThighPitch = -s * swing,
                leftCalfPitch = Mathf.Max(0f, -c) * 28f * amp,
                rightCalfPitch = Mathf.Max(0f, c) * 28f * amp,
                leftArmPitch = -s * (swing * 0.75f),
                rightArmPitch = s * (swing * 0.75f),
                hipBobOffset = Mathf.Abs(s) * 0.045f * amp
            };
        }

        // §3.9: Wheel spin delta in degrees = (distance / radius) * Rad2Deg.
        public static float WheelSpinDeltaDegrees(float signedDistanceMeters, float wheelRadiusMeters)
        {
            float r = Mathf.Max(0.05f, wheelRadiusMeters);
            return (signedDistanceMeters / r) * Mathf.Rad2Deg;
        }

        public static (bool redOn, bool blueOn) EvaluateSiren(float timeSeconds, float cycleHz = 3.2f)
        {
            float phase = Mathf.Repeat(timeSeconds * Mathf.Max(0.2f, cycleHz), 1f);
            return (phase < 0.5f, phase >= 0.5f);
        }

        public static float EvaluateNeonBrightness(int signSeed, float timeSeconds)
        {
            float slow = Mathf.Sin(timeSeconds * 1.7f + signSeed * 0.73f);
            float buzz = Mathf.Sin(timeSeconds * 23.0f + signSeed * 3.11f);
            if (slow < -0.92f && buzz < -0.4f)
                return 0.15f; // Brief tube dip
            return 0.88f + buzz * 0.12f;
        }

        public static float HeadlightIntensityForHour(float hourOfDay)
        {
            return DistrictRules.IsNightHour(hourOfDay) ? 2.2f : 0f;
        }
    }
}
