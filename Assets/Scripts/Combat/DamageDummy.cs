using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public sealed class DamageDummy : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 200f;
        private float _currentHealth = 200f;
        private int _hitCount;
        private DamageInfo _lastHit;

        public bool IsAlive => _currentHealth > 0f;
        public float CurrentHealth => _currentHealth;
        public int HitCount => _hitCount;
        public DamageInfo LastHit => _lastHit;

        public void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            _currentHealth = Mathf.Max(0f, _currentHealth - info.amount);
            _hitCount++;
            _lastHit = info;
        }

        public void ResetDummy(float hp = 200f)
        {
            maxHealth = Mathf.Max(1f, hp);
            _currentHealth = maxHealth;
            _hitCount = 0;
            _lastHit = default;
        }
    }
}
