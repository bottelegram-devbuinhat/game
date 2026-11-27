using System;
using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.City
{
    public sealed class DistrictRegistry : MonoBehaviour
    {
        [SerializeField] private DistrictDefinition[] districts = Array.Empty<DistrictDefinition>();
        [SerializeField] private Transform trackedTarget;
        [SerializeField] private float checkIntervalSeconds = 0.5f;

        private DistrictDefinition _current;
        private float _timer;

        public event Action<DistrictDefinition> DistrictChanged;

        public DistrictDefinition CurrentDistrict => _current;
        public IReadOnlyList<DistrictDefinition> AllDistricts => districts;

        public void Configure(DistrictDefinition[] definitions, Transform target = null)
        {
            districts = definitions ?? Array.Empty<DistrictDefinition>();
            if (target != null) trackedTarget = target;
            ResolveImmediate(trackedTarget != null ? trackedTarget.position : Vector3.zero);
        }

        public DistrictDefinition ResolveImmediate(Vector3 position)
        {
            DistrictDefinition next = DistrictBounds.ResolveContaining(districts, position);
            if (!ReferenceEquals(next, _current))
            {
                _current = next;
                DistrictChanged?.Invoke(_current);
            }
            return _current;
        }

        public void Tick(float deltaTime)
        {
            _timer += Mathf.Max(0f, deltaTime);
            if (_timer < checkIntervalSeconds) return;
            _timer = 0f;
            if (trackedTarget != null)
                ResolveImmediate(trackedTarget.position);
        }

        public void Update() => Tick(Time.deltaTime);
    }
}
