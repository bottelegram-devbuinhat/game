using System;
using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.World
{
    public sealed class WorldStreamer : MonoBehaviour
    {
        [SerializeField] private Transform trackedTarget;
        [SerializeField] private float cellSize = WorldStreamMath.DefaultCellSize;
        [SerializeField] private int residentRadius = WorldStreamMath.DefaultResidentRadius;
        [SerializeField] private float unloadIntervalSeconds = 1.0f;

        private readonly HashSet<Vector2Int> _loadedCells = new HashSet<Vector2Int>();
        private readonly List<Vector2Int> _desiredScratch = new List<Vector2Int>(16);
        private readonly List<Vector2Int> _unloadScratch = new List<Vector2Int>(16);
        private float _timer;

        public event Action<Vector2Int> CellLoaded;
        public event Action<Vector2Int> CellUnloaded;

        public int LoadedCellCount => _loadedCells.Count;
        public IReadOnlyCollection<Vector2Int> LoadedCells => _loadedCells;

        public void Configure(Transform target, float cellSizeMeters = 64f, int radius = 1)
        {
            trackedTarget = target;
            cellSize = Mathf.Max(8f, cellSizeMeters);
            residentRadius = Mathf.Max(0, radius);
        }

        public void UpdateForPosition(Vector3 worldPosition)
        {
            Vector2Int center = WorldStreamMath.WorldToCell(worldPosition, cellSize);
            WorldStreamMath.CollectResidentRing(center, residentRadius, _desiredScratch);

            _unloadScratch.Clear();
            foreach (Vector2Int existing in _loadedCells)
            {
                if (WorldStreamMath.ChebyshevDistance(existing, center) > residentRadius)
                    _unloadScratch.Add(existing);
            }

            for (int i = 0; i < _unloadScratch.Count; i++)
            {
                Vector2Int cell = _unloadScratch[i];
                _loadedCells.Remove(cell);
                CellUnloaded?.Invoke(cell);
            }

            for (int i = 0; i < _desiredScratch.Count; i++)
            {
                Vector2Int cell = _desiredScratch[i];
                if (_loadedCells.Add(cell))
                    CellLoaded?.Invoke(cell);
            }
        }

        public void Tick(float deltaTime)
        {
            _timer += Mathf.Max(0f, deltaTime);
            if (_timer < unloadIntervalSeconds) return;
            _timer = 0f;
            if (trackedTarget != null)
                UpdateForPosition(trackedTarget.position);
        }

        public void Update() => Tick(Time.deltaTime);
    }
}
