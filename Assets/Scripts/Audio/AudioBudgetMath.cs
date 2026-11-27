using UnityEngine;

namespace DudeChaos.Audio
{
    public enum AudioBand
    {
        Near = 0,
        Mid = 1,
        Far = 2,
        Culled = 3
    }

    // §1.8 & §4.2: 12 near (<=20 m) / 8 mid (20..50 m) / 4 far (50..90 m) / 0 past 90 m;
    // steal rule only steals barely-audible voices; distance falloff & pitch jitter.
    public static class AudioBudgetMath
    {
        public const int MaxNearVoices = 12;
        public const int MaxMidVoices = 8;
        public const int MaxFarVoices = 4;
        public const int TotalPoolVoices = MaxNearVoices + MaxMidVoices + MaxFarVoices; // 24
        public const float MaxAudibleDistance = 90f;
        public const float StealAudibilityCeiling = 0.18f;

        public static AudioBand ClassifyBand(float distanceMeters)
        {
            if (distanceMeters <= 20f) return AudioBand.Near;
            if (distanceMeters <= 50f) return AudioBand.Mid;
            if (distanceMeters <= MaxAudibleDistance) return AudioBand.Far;
            return AudioBand.Culled;
        }

        public static int CapForBand(AudioBand band)
        {
            switch (band)
            {
                case AudioBand.Near: return MaxNearVoices;
                case AudioBand.Mid: return MaxMidVoices;
                case AudioBand.Far: return MaxFarVoices;
                default: return 0;
            }
        }

        public static float ComputeFalloffVolume(float baseVolume, float distanceMeters)
        {
            if (baseVolume <= 0f || distanceMeters > MaxAudibleDistance)
                return 0f;
            if (distanceMeters <= 2f)
                return Mathf.Clamp01(baseVolume);
            float t = 1f - Mathf.Clamp01((distanceMeters - 2f) / (MaxAudibleDistance - 2f));
            return Mathf.Clamp01(baseVolume * t * t);
        }

        public static bool CanStealVoice(float candidateEffectiveVolume, float existingEffectiveVolume)
        {
            if (candidateEffectiveVolume <= 0.01f) return false;
            return existingEffectiveVolume <= StealAudibilityCeiling &&
                   candidateEffectiveVolume > existingEffectiveVolume * 1.25f;
        }

        public static float JitterPitch(float basePitch, float sampleMinusOneToOne, float maxJitter = 0.06f)
        {
            return Mathf.Clamp(basePitch + Mathf.Clamp(sampleMinusOneToOne, -1f, 1f) * maxJitter, 0.5f, 2.0f);
        }
    }
}
