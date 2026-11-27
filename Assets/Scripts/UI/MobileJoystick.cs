using DudeChaos.Player;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DudeChaos.UI
{
    public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float handleRangePixels = 64f;
        [SerializeField] private MonoBehaviour playerInputBehaviour;

        private Vector2 _currentValue;

        public Vector2 CurrentValue => _currentValue;

        public void BindInput(IPlayerInput input)
        {
            _explicitInput = input;
            playerInputBehaviour = input as MonoBehaviour;
        }

        private IPlayerInput _explicitInput;
        private IPlayerInput ResolveInput() => _explicitInput ?? (playerInputBehaviour as IPlayerInput);

        public void ApplyVisualPrefs(float sizeScale, float opacity, bool leftHanded)
        {
            if (background != null)
            {
                background.localScale = Vector3.one * Mathf.Clamp(sizeScale, 0.6f, 1.6f);
                background.anchorMin = leftHanded ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
                background.anchorMax = background.anchorMin;
            }
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp(opacity, 0.2f, 1.0f);
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null) return;
            Vector2 delta = eventData.position - eventData.pressPosition;
            float range = Mathf.Max(8f, handleRangePixels);
            _currentValue = Vector2.ClampMagnitude(delta / range, 1f);
            if (handle != null)
                handle.anchoredPosition = _currentValue * range;
            ResolveInput()?.SetVirtualMove(_currentValue);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _currentValue = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
            ResolveInput()?.SetVirtualMove(Vector2.zero);
        }
    }
}
