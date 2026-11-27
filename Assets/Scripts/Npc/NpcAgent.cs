using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Npc
{
    public sealed class NpcAgent : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 60f;
        [SerializeField] private float walkSpeed = 1.9f;

        private float _health = 60f;
        private int _agentSeed;

        public bool IsAlive => _health > 0f;
        public float CurrentHealth => _health;

        public void ResetAgent(int seed, Vector3 spawnPosition)
        {
            _agentSeed = seed;
            _health = maxHealth;
            transform.position = spawnPosition;
            gameObject.SetActive(true);
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive || info.amount <= 0f) return;
            _health = Mathf.Max(0f, _health - info.amount);
        }

        public void StepWander(float deltaTime, float worldTime)
        {
            if (!IsAlive) return;
            Vector3 dir = NpcSteering.DeterministicWanderDirection(_agentSeed, worldTime);
            transform.position += dir * (walkSpeed * Mathf.Max(0f, deltaTime));
            if (dir.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
