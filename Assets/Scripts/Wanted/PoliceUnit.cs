using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Wanted
{
    public sealed class PoliceUnit : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float moveSpeed = 5.6f;
        [SerializeField] private float sightRange = 45f;
        [SerializeField] private WantedSettings settings;

        private readonly RaycastHit[] _losHits = new RaycastHit[8];
        private float _health = 100f;
        private int _currentWantedLevel;

        public bool IsAlive => _health > 0f;
        public bool MayOpenFire => _currentWantedLevel >= (settings != null ? settings.openFireFromLevel : 2);

        public void ResetUnit(int wantedLevel, WantedSettings wantedSettings = null)
        {
            _health = maxHealth;
            _currentWantedLevel = wantedLevel;
            if (wantedSettings != null) settings = wantedSettings;
            gameObject.SetActive(true);
        }

        public void SetWantedLevel(int level) => _currentWantedLevel = level;

        public bool HasLineOfSight(Transform target)
        {
            if (!IsAlive || target == null) return false;
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 dest = target.position + Vector3.up * 1.0f;
            Vector3 diff = dest - origin;
            float dist = diff.magnitude;
            if (dist > sightRange || dist < 0.01f) return false;

            int count = Physics.RaycastNonAlloc(origin, diff / dist, _losHits, dist, -1, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider col = _losHits[i].collider;
                if (col == null || col.gameObject == gameObject || col.transform == target)
                    continue;
                return false;
            }
            return true;
        }

        public void StepChase(Vector3 targetPosition, float deltaTime)
        {
            if (!IsAlive || _currentWantedLevel <= 0) return;
            Vector3 dir = PoliceChaseMath.ComputeChaseDirection(transform.position, targetPosition);
            float dist = Vector3.Distance(transform.position, targetPosition);
            float stopDist = MayOpenFire ? 12f : 1.8f;
            if (dist > stopDist && dir.sqrMagnitude > 1e-4f)
            {
                transform.position += dir * (moveSpeed * Mathf.Max(0f, deltaTime));
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            }
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            _health = Mathf.Max(0f, _health - info.amount);
        }
    }
}
