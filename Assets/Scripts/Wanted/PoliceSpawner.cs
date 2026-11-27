using System.Collections.Generic;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Wanted
{
    // Matches active officer and car counts to WantedSystem.Level, despawns on drop; NO NavMesh (§1.8).
    public sealed class PoliceSpawner : MonoBehaviour
    {
        [SerializeField] private WantedSystem wantedSystem;
        [SerializeField] private PoliceUnit unitPrefab;
        [SerializeField] private PoliceCar carPrefab;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float spawnRingRadius = 42f;

        private readonly List<PoliceUnit> _activeUnits = new List<PoliceUnit>();
        private readonly List<PoliceCar> _activeCars = new List<PoliceCar>();

        public int ActiveUnitCount => _activeUnits.Count;
        public int ActiveCarCount => _activeCars.Count;

        public void Configure(WantedSystem system, Transform target, PoliceUnit unitTemplate = null, PoliceCar carTemplate = null)
        {
            if (wantedSystem != null)
                wantedSystem.LevelChanged -= OnLevelChanged;
            wantedSystem = system;
            playerTransform = target;
            if (unitTemplate != null) unitPrefab = unitTemplate;
            if (carTemplate != null) carPrefab = carTemplate;
            if (wantedSystem != null)
                wantedSystem.LevelChanged += OnLevelChanged;
        }

        public void OnDestroy()
        {
            if (wantedSystem != null)
                wantedSystem.LevelChanged -= OnLevelChanged;
        }

        private void OnLevelChanged(int previous, int current)
        {
            ReconcileCounts(current);
        }

        public void ReconcileCounts(int level)
        {
            WantedSettings settings = wantedSystem != null ? wantedSystem.Settings : null;
            PoliceResponse resp = WantedMath.GetResponse(level, settings);

            while (_activeUnits.Count > resp.maxUnits)
            {
                int last = _activeUnits.Count - 1;
                if (_activeUnits[last] != null)
                    _activeUnits[last].gameObject.SetActive(false);
                _activeUnits.RemoveAt(last);
            }

            while (_activeCars.Count > resp.maxCars)
            {
                int last = _activeCars.Count - 1;
                if (_activeCars[last] != null)
                    _activeCars[last].gameObject.SetActive(false);
                _activeCars.RemoveAt(last);
            }

            Vector3 center = playerTransform != null ? playerTransform.position : transform.position;
            while (_activeUnits.Count < resp.maxUnits)
            {
                float angle = _activeUnits.Count * 1.15f;
                Vector3 pos = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRingRadius;
                PoliceUnit unit = unitPrefab != null
                    ? Instantiate(unitPrefab, pos, Quaternion.identity)
                    : new GameObject($"PoliceUnit_{_activeUnits.Count}").AddComponent<PoliceUnit>();
                unit.transform.position = pos;
                unit.ResetUnit(level, settings);
                _activeUnits.Add(unit);
            }

            while (_activeCars.Count < resp.maxCars)
            {
                float angle = _activeCars.Count * 2.1f + 0.5f;
                Vector3 pos = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (spawnRingRadius + 12f);
                PoliceCar car = carPrefab != null
                    ? Instantiate(carPrefab, pos, Quaternion.identity)
                    : new GameObject($"PoliceCar_{_activeCars.Count}").AddComponent<PoliceCar>();
                car.transform.position = pos;
                car.SetWantedLevel(level);
                _activeCars.Add(car);
            }

            for (int i = 0; i < _activeUnits.Count; i++)
                _activeUnits[i]?.SetWantedLevel(level);
            for (int i = 0; i < _activeCars.Count; i++)
                _activeCars[i]?.SetWantedLevel(level);
        }
    }
}
