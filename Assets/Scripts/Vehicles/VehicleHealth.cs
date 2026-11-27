using System;
using DudeChaos.Combat;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    public sealed class VehicleHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private VehicleData vehicleData;
        [SerializeField] private VehicleSettings settings;
        [SerializeField] private CombatSettings combatSettings;

        private float _maxHealth = 900f;
        private float _currentHealth = 900f;
        private VehicleDamageStage _stage = VehicleDamageStage.Healthy;
        private float _burnTimer;
        private bool _exploded;

        public event Action<VehicleDamageStage> StageChanged;
        public event Action Exploded;

        public bool IsAlive => !_exploded && _currentHealth > 0f;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public VehicleDamageStage Stage => _stage;

        public void Awake()
        {
            if (settings == null) settings = ScriptableObject.CreateInstance<VehicleSettings>();
            if (combatSettings == null) combatSettings = ScriptableObject.CreateInstance<CombatSettings>();
            InitialiseFromData(vehicleData);
        }

        public void InitialiseFromData(VehicleData data, int armourUpgradeLevel = 0)
        {
            vehicleData = data;
            float baseHp = data != null ? data.maxHealth : 900f;
            ArmourClass ac = data != null ? data.armourClass : ArmourClass.Unarmoured;
            _maxHealth = ArmourRules.EffectiveMaxHealth(baseHp, ac, armourUpgradeLevel);
            _currentHealth = _maxHealth;
            _stage = VehicleDamageStage.Healthy;
            _burnTimer = 0f;
            _exploded = false;
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (_exploded || info.amount <= 0f) return;

            ArmourClass ac = vehicleData != null ? vehicleData.armourClass : ArmourClass.Unarmoured;
            float mult = 1f;
            if (info.reason == RagdollActivationReason.Bullet)
                mult = ArmourRules.BulletDamageMultiplier(ac);
            else if (info.reason == RagdollActivationReason.Explosion)
                mult = ArmourRules.ExplosionDamageMultiplier(ac);
            else if (info.reason == RagdollActivationReason.VehicleImpact)
                mult = ArmourRules.RamDamageMultiplier(ac);

            _currentHealth = Mathf.Max(0f, _currentHealth - info.amount * mult);
            UpdateStage();
        }

        public void Tick(float deltaTime)
        {
            if (_exploded) return;
            if (_stage == VehicleDamageStage.Burning || _currentHealth <= 0f)
            {
                _burnTimer += Mathf.Max(0f, deltaTime);
                float delay = settings != null ? settings.destructionDelay : 2.8f;
                if (_burnTimer >= delay)
                {
                    TriggerExplosion();
                }
            }
        }

        public void Update() => Tick(Time.deltaTime);

        private void UpdateStage()
        {
            VehicleDamageStage next = VehicleMath.EvaluateDamageStage(_currentHealth, _maxHealth, settings);
            if (next == VehicleDamageStage.Exploded && !_exploded)
            {
                // Burning countdown handles final detonation after destructionDelay (2.8 s, §1.7).
                next = VehicleDamageStage.Burning;
            }
            if (next != _stage)
            {
                _stage = next;
                StageChanged?.Invoke(_stage);
            }
        }

        private void TriggerExplosion()
        {
            if (_exploded) return;
            _exploded = true;
            _currentHealth = 0f;
            _stage = VehicleDamageStage.Exploded;
            StageChanged?.Invoke(_stage);
            Explosion.Detonate(transform.position, combatSettings, gameObject);
            Exploded?.Invoke();
        }
    }
}
