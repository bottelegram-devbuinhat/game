using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Wanted
{
    public sealed class WantedSystem : MonoBehaviour
    {
        [SerializeField] private WantedSettings settings;

        private float _heat;
        private int _level;
        private float _timeSinceLastSeen = 99f;
        private float _heatMultiplier = 1.0f;

        public event Action<int, int> LevelChanged;
        public event Action<CrimeType, float> CrimeReported;

        public WantedSettings Settings => settings;
        public float Heat => _heat;
        public int Level => _level;
        public float TimeSinceLastSeen => _timeSinceLastSeen;
        public float HeatMultiplier
        {
            get => _heatMultiplier;
            set => _heatMultiplier = Mathf.Clamp(value, 0.25f, 4.0f);
        }

        public void Awake()
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<WantedSettings>();
        }

        public void Configure(WantedSettings wantedSettings)
        {
            if (wantedSettings != null)
                settings = wantedSettings;
        }

        public void ReportCrime(CrimeType crime)
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<WantedSettings>();

            float added = settings.GetCrimeWeight(crime) * _heatMultiplier;
            _heat = Mathf.Max(0f, _heat + added);
            _timeSinceLastSeen = 0f;
            RecalculateLevel();
            CrimeReported?.Invoke(crime, added);
        }

        public void NotifySpottedByPolice()
        {
            _timeSinceLastSeen = 0f;
        }

        public void Tick(float deltaTime, bool inLineOfSight, float nearestPoliceDistance)
        {
            float dt = Mathf.Max(0f, deltaTime);
            if (inLineOfSight)
                _timeSinceLastSeen = 0f;
            else
                _timeSinceLastSeen += dt;

            if (WantedMath.CanCoolDown(_level, inLineOfSight, nearestPoliceDistance, _timeSinceLastSeen, settings))
            {
                _heat = WantedMath.CoolHeat(_heat, _level, dt, settings);
                RecalculateLevel();
            }
        }

        public void Clear()
        {
            _heat = 0f;
            _timeSinceLastSeen = 99f;
            RecalculateLevel();
        }

        public void RestoreState(float savedHeat, int savedLevel)
        {
            _heat = Mathf.Max(0f, savedHeat);
            RecalculateLevel();
            if (savedLevel > _level && settings != null && settings.thresholds != null && savedLevel <= settings.thresholds.Length)
            {
                _heat = settings.thresholds[savedLevel - 1];
                RecalculateLevel();
            }
        }

        private void RecalculateLevel()
        {
            int next = WantedMath.ComputeLevelFromHeat(_heat, settings != null ? settings.thresholds : null);
            if (next != _level)
            {
                int prev = _level;
                _level = next;
                LevelChanged?.Invoke(prev, _level);
            }
        }
    }
}
