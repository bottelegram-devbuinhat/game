using System.Collections.Generic;
using DudeChaos.City;
using DudeChaos.Core;
using DudeChaos.Data;
using DudeChaos.World;
using UnityEngine;

namespace DudeChaos.Npc
{
    public sealed class NpcSpawner : MonoBehaviour
    {
        [SerializeField] private WorldStreamer streamer;
        [SerializeField] private PrefabPool pool;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private DistrictRegistry districtRegistry;
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private int hardBudget = 24;

        private readonly List<NpcAgent> _active = new List<NpcAgent>();
        private int _seedCounter = 1;

        public int ActiveCount => _active.Count;

        public void OnEnable()
        {
            if (streamer != null)
            {
                streamer.CellLoaded += OnCellChanged;
                streamer.CellUnloaded += OnCellChanged;
            }
        }

        public void OnDisable()
        {
            if (streamer != null)
            {
                streamer.CellLoaded -= OnCellChanged;
                streamer.CellUnloaded -= OnCellChanged;
            }
        }

        public void Configure(WorldStreamer worldStreamer, Transform target, PrefabPool npcPool = null)
        {
            if (streamer != null)
            {
                streamer.CellLoaded -= OnCellChanged;
                streamer.CellUnloaded -= OnCellChanged;
            }
            streamer = worldStreamer;
            playerTransform = target;
            if (npcPool != null) pool = npcPool;
            if (streamer != null && isActiveAndEnabled)
            {
                streamer.CellLoaded += OnCellChanged;
                streamer.CellUnloaded += OnCellChanged;
            }
        }

        public void SetHardBudget(int budget) => hardBudget = Mathf.Clamp(budget, 0, 64);

        private void OnCellChanged(Vector2Int _)
        {
            Vector3 viewer = playerTransform != null ? playerTransform.position : transform.position;
            DistrictKind kind = districtRegistry != null && districtRegistry.CurrentDistrict != null
                ? districtRegistry.CurrentDistrict.kind
                : DistrictKind.Downtown;
            float hour = dayNightCycle != null ? dayNightCycle.CurrentHour : 12f;
            Reconcile(viewer, kind, hour);
        }

        public void Reconcile(Vector3 viewerPos, DistrictKind district, float hourOfDay)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                NpcAgent a = _active[i];
                if (a == null || !a.IsAlive || NpcSpawnMath.ShouldDespawn(viewerPos, a.transform.position))
                {
                    if (a != null)
                    {
                        if (pool != null) pool.Return(a.gameObject);
                        else a.gameObject.SetActive(false);
                    }
                    _active.RemoveAt(i);
                }
            }

            float mult = DistrictRules.CrowdDensityMultiplier(district, hourOfDay);
            int desired = NpcSpawnMath.DesiredCount(hardBudget, mult);

            while (_active.Count < desired)
            {
                Vector3 pos = NpcSpawnMath.DeterministicRingPoint(viewerPos, _seedCounter, Mathf.Max(1, desired));
                GameObject go = pool != null ? pool.Rent(pos, Quaternion.identity) : new GameObject($"NPC_{_seedCounter}");
                if (go == null) break;
                NpcAgent agent = go.GetComponent<NpcAgent>() ?? go.AddComponent<NpcAgent>();
                agent.ResetAgent(_seedCounter++, pos);
                _active.Add(agent);
            }
        }
    }
}
