using UnityEngine;

namespace DudeChaos.City
{
    // §1.7 & §1.9.4:
    // 1 game day = 24 real min (hoursPerSecond = 1/60).
    // Sun peak 68 deg at 12:00.
    // Ambient 1.1 day -> 0.28 at 03:00, never 0.
    // Street lamps fade in 18:36 (18.6h), out 06:12 (6.2h) over 0.5 hour.
    public static class DayNightRules
    {
        public const float HoursPerSecond = 1f / 60f;
        public const float NoonSunPitch = 68f;
        public const float LampFadeInStart = 18.6f;  // 18:36
        public const float LampFadeOutStart = 6.2f;  // 06:12
        public const float LampFadeDuration = 0.5f;

        public static float AdvanceHour(float currentHour, float deltaTimeSeconds)
        {
            return Mathf.Repeat(currentHour + Mathf.Max(0f, deltaTimeSeconds) * HoursPerSecond, 24f);
        }

        public static float SunPitch(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            float rad = ((h - 6f) / 24f) * Mathf.PI * 2f;
            return Mathf.Sin(rad) * NoonSunPitch;
        }

        public static float AmbientFor(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            // Peak 1.1 around 15:00/noon, minimum 0.28 at 03:00, never 0.
            float phase = ((h - 3f) / 24f) * Mathf.PI * 2f;
            float wave01 = (1f - Mathf.Cos(phase)) * 0.5f; // 0 at 03:00, 1 at 15:00
            return Mathf.Lerp(0.28f, 1.1f, wave01);
        }

        public static float LightIntensity(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            if (h >= LampFadeInStart)
            {
                return Mathf.Clamp01((h - LampFadeInStart) / LampFadeDuration);
            }
            if (h < LampFadeOutStart)
            {
                return 1.0f;
            }
            if (h < LampFadeOutStart + LampFadeDuration)
            {
                return 1.0f - Mathf.Clamp01((h - LampFadeOutStart) / LampFadeDuration);
            }
            return 0f;
        }
    }
}
