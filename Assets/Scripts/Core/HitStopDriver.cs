using UnityEngine;

namespace DudeChaos.Core
{
    // Unscaled-time hit-stop driver; yields cleanly when GameManager is paused (§1.8).
    public sealed class HitStopDriver : MonoBehaviour
    {
        private float _remainingUnscaled;
        private bool _active;

        public bool IsActive => _active;
        public float RemainingSeconds => _remainingUnscaled;

        public void Trigger(float durationSeconds)
        {
            if (durationSeconds <= 0f) return;
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            if (durationSeconds > _remainingUnscaled)
                _remainingUnscaled = durationSeconds;

            if (!_active)
            {
                _active = true;
                Time.timeScale = 0.02f;
            }
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (!_active) return;

            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            {
                _active = false;
                _remainingUnscaled = 0f;
                Time.timeScale = 0f;
                return;
            }

            _remainingUnscaled -= Mathf.Max(0f, unscaledDeltaTime);
            if (_remainingUnscaled <= 0f)
            {
                _remainingUnscaled = 0f;
                _active = false;
                Time.timeScale = 1f;
            }
        }

        public void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }
    }
}
