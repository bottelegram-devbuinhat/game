using System.Collections.Generic;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Npc
{
    public sealed class AnimalSpawner : MonoBehaviour
    {
        private readonly List<AnimalAgent> _active = new List<AnimalAgent>();
        private int _seed = 100;

        public int ActiveCount => _active.Count;

        public void Reconcile(Vector3 viewerPos, DistrictKind district, float hour)
        {
            int targetCount = AnimalRules.BudgetForDistrict(district, hour);
            AnimalSpecies species = AnimalRules.PrimarySpeciesForDistrict(district, hour);

            while (_active.Count > targetCount)
            {
                int last = _active.Count - 1;
                if (_active[last] != null) _active[last].gameObject.SetActive(false);
                _active.RemoveAt(last);
            }

            while (_active.Count < targetCount)
            {
                Vector3 pos = NpcSpawnMath.DeterministicRingPoint(viewerPos, _seed, Mathf.Max(1, targetCount));
                AnimalAgent agent = new GameObject($"Animal_{species}_{_seed}").AddComponent<AnimalAgent>();
                agent.Initialise(species, _seed++, pos);
                _active.Add(agent);
            }
        }
    }
}
