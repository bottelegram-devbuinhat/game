using UnityEngine;

namespace DudeChaos.Visuals
{
    public sealed class VehicleLights : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Light[] headlights = System.Array.Empty<Light>();
        [SerializeField] private Renderer brakeLightRenderer;

        private MaterialPropertyBlock _block;

        public void Apply(float hourOfDay, bool isBraking, bool hazardActive, float timeSeconds)
        {
            float headIntensity = AnimationMath.HeadlightIntensityForHour(hourOfDay);
            if (headlights != null)
            {
                for (int i = 0; i < headlights.Length; i++)
                {
                    if (headlights[i] == null) continue;
                    headlights[i].intensity = headIntensity;
                    headlights[i].enabled = headIntensity > 0.01f;
                }
            }

            if (brakeLightRenderer != null)
            {
                if (_block == null) _block = new MaterialPropertyBlock();
                bool hazardBlink = hazardActive && Mathf.Repeat(timeSeconds * 2f, 1f) < 0.5f;
                Color c = isBraking
                    ? new Color(1f, 0.1f, 0.1f) * 2.2f
                    : (hazardBlink ? new Color(1f, 0.55f, 0.1f) * 1.8f : Color.black);
                _block.SetColor(EmissionColorId, c);
                brakeLightRenderer.SetPropertyBlock(_block);
            }
        }
    }
}
