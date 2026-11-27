using UnityEngine;

namespace DudeChaos.Visuals
{
    public sealed class SirenController : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color SirenRed = new Color(0.92f, 0.22f, 0.22f, 1f);
        private static readonly Color SirenBlue = new Color(0.17f, 0.29f, 0.84f, 1f);

        [SerializeField] private Renderer redBarRenderer;
        [SerializeField] private Renderer blueBarRenderer;
        [SerializeField] private bool sirenActive;

        private MaterialPropertyBlock _block;

        public void SetSirenActive(bool active) => sirenActive = active;

        public void Step(float timeSeconds)
        {
            if (_block == null) _block = new MaterialPropertyBlock();
            var (redOn, blueOn) = sirenActive ? AnimationMath.EvaluateSiren(timeSeconds) : (false, false);

            if (redBarRenderer != null)
            {
                _block.SetColor(EmissionColorId, redOn ? SirenRed * 2.5f : Color.black);
                redBarRenderer.SetPropertyBlock(_block);
            }

            if (blueBarRenderer != null)
            {
                _block.SetColor(EmissionColorId, blueOn ? SirenBlue * 2.5f : Color.black);
                blueBarRenderer.SetPropertyBlock(_block);
            }
        }
    }
}
