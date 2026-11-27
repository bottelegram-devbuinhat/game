using System;
using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.Core
{
    // Rule 3 & R6: Single global manager; systems register with it.
    // Never writes QualitySettings.shadowDistance or Application.targetFrameRate (owned by QualityApplier).
    [DisallowMultipleComponent]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private bool _isPaused;

        public event Action<bool> PauseChanged;

        public bool IsPaused => _isPaused;

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                _services.Clear();
                Instance = null;
            }
        }

        public void Register<T>(T service) where T : class
        {
            if (service == null) return;
            _services[typeof(T)] = service;
        }

        public void Unregister<T>(T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object current) && ReferenceEquals(current, service))
            {
                _services.Remove(typeof(T));
            }
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object raw) && raw is T typed)
            {
                service = typed;
                return true;
            }
            service = null;
            return false;
        }

        public T Get<T>() where T : class
        {
            TryGet(out T service);
            return service;
        }

        public void SetPaused(bool paused)
        {
            if (_isPaused == paused) return;
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            PauseChanged?.Invoke(_isPaused);
        }

        public static void ResetStaticForTests()
        {
            Instance = null;
        }
    }
}
