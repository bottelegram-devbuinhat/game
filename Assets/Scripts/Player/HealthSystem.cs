using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Player
{
    public sealed class HealthSystem : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxArmour = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float currentArmour = 0f;

        public event Action<DamageInfo> Damaged;
        public event Action Depleted;
        public event Action<float, float> Changed;

        public float MaxHealth => maxHealth;
        public float MaxArmour => maxArmour;
        public float CurrentHealth => currentHealth;
        public float CurrentArmour => currentArmour;
        public bool IsAlive => currentHealth > 0f;
        public IPlayerInput AttachedInput => GetComponent<IPlayerInput>();

        public void Configure(float maxHp, float initialHp, float maxArm = 100f, float initialArmour = 0f)
        {
            maxHealth = Mathf.Max(1f, maxHp);
            maxArmour = Mathf.Max(0f, maxArm);
            currentHealth = Mathf.Clamp(initialHp, 0f, maxHealth);
            currentArmour = Mathf.Clamp(initialArmour, 0f, maxArmour);
            Changed?.Invoke(currentHealth, currentArmour);
        }

        // Burns armour before health and raises Depleted on zero (§1.8 & §4.6).
        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f)
                return;

            float remaining = info.amount;
            if (currentArmour > 0f)
            {
                float absorbed = Mathf.Min(currentArmour, remaining);
                currentArmour -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0f)
            {
                currentHealth = Mathf.Max(0f, currentHealth - remaining);
            }

            Damaged?.Invoke(info);
            Changed?.Invoke(currentHealth, currentArmour);

            if (currentHealth <= 0f)
            {
                Depleted?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Changed?.Invoke(currentHealth, currentArmour);
        }

        public void AddArmour(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            currentArmour = Mathf.Min(maxArmour, currentArmour + amount);
            Changed?.Invoke(currentHealth, currentArmour);
        }

        public void Revive(float healthFraction = 1f, float armourAmount = 0f)
        {
            currentHealth = Mathf.Clamp(maxHealth * Mathf.Clamp01(healthFraction), 1f, maxHealth);
            currentArmour = Mathf.Clamp(armourAmount, 0f, maxArmour);
            Changed?.Invoke(currentHealth, currentArmour);
        }
    }
}
