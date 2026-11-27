using System;
using DudeChaos.City;
using UnityEngine;

namespace DudeChaos.Visuals
{
    // Single batch component driving all neon signs; NO per-sign Update (§1.3 Rule 10 & §1.9.6).
    public sealed class NeonFlicker : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Serializable]
        public struct NeonEntry
        {
            public Renderer renderer;
            public Color baseColor;
            public int seed;
        }

        [SerializeField] private NeonEntry[] signs = Array.Empty<NeonEntry>();
        [SerializeField] private DayNightCycle dayNight;

        private MaterialPropertyBlock _block;

        public void Step(float timeSeconds, float hourOfDay)
        {
            if (signs == null || signs.Length == 0) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            bool night = DistrictRules.IsNightHour(hourOfDay);

            for (int i = 0; i < signs.Length; i++)
            {
                Renderer r = signs[i].renderer;
                if (r == null) continue;
                float b = night ? AnimationMath.EvaluateNeonBrightness(signs[i].seed, timeSeconds) : 0f;
                _block.SetColor(EmissionColorId, signs[i].baseColor * (b * 2.2f));
                r.SetPropertyBlock(_block);
            }
        }

        public void Update()
        {
            float hour = dayNight != null ? dayNight.CurrentHour : 22f;
            Step(Time.time, hour);
        }
    }
}
