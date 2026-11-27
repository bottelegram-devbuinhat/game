using UnityEngine;

namespace DudeChaos.UI
{
    // Anchors RectTransform to Screen.safeArea; recomputes on resize (§1.8).
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private int _lastWidth;
        private int _lastHeight;

        public void Awake()
        {
            _rect = GetComponent<RectTransform>();
            RefreshIfChanged();
        }

        public void RefreshIfChanged()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_rect == null) return;

            Rect safe = Screen.safeArea;
            if (safe.x == _lastSafeArea.x && safe.y == _lastSafeArea.y &&
                safe.width == _lastSafeArea.width && safe.height == _lastSafeArea.height &&
                Screen.width == _lastWidth && Screen.height == _lastHeight)
            {
                return;
            }

            _lastSafeArea = safe;
            _lastWidth = Mathf.Max(1, Screen.width);
            _lastHeight = Mathf.Max(1, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= _lastWidth;
            min.y /= _lastHeight;
            max.x /= _lastWidth;
            max.y /= _lastHeight;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
        }

        public void Update() => RefreshIfChanged();
    }
}
