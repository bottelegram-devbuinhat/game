using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.Core
{
    public sealed class PrefabPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialCapacity = 16;
        [SerializeField] private int maxCapacity = 64;

        private readonly Stack<GameObject> _available = new Stack<GameObject>();
        private int _totalCreated;

        public int AvailableCount => _available.Count;
        public int TotalCreated => _totalCreated;

        public void Configure(GameObject targetPrefab, int prewarmCount, int hardCap)
        {
            prefab = targetPrefab;
            initialCapacity = Mathf.Max(0, prewarmCount);
            maxCapacity = Mathf.Max(1, hardCap);
        }

        public void Awake()
        {
            Prewarm();
        }

        public void Prewarm()
        {
            if (prefab == null) return;
            while (_totalCreated < initialCapacity && _totalCreated < maxCapacity)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                _available.Push(instance);
                _totalCreated++;
            }
        }

        public GameObject Rent(Vector3 position, Quaternion rotation)
        {
            GameObject instance = null;
            while (_available.Count > 0 && instance == null)
            {
                instance = _available.Pop();
            }

            if (instance == null)
            {
                if (prefab == null || _totalCreated >= maxCapacity)
                    return null;
                instance = Instantiate(prefab, position, rotation);
                _totalCreated++;
            }
            else
            {
                instance.transform.position = position;
                instance.transform.rotation = rotation;
            }

            instance.SetActive(true);
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null) return;
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            if (_available.Count < maxCapacity)
            {
                _available.Push(instance);
            }
        }
    }
}
