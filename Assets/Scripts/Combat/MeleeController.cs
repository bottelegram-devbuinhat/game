using System;
using DudeChaos.Core;
using DudeChaos.Data;
using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Combat
{
    public sealed class MeleeController : MonoBehaviour
    {
        [SerializeField] private CombatSettings settings;
        [SerializeField] private HitStopDriver hitStopDriver;
        [SerializeField] private float lightDamage = 25f;
        [SerializeField] private float heavyDamage = 55f;
        [SerializeField] private float reach = 2.0f;
        [SerializeField] private float swingCooldown = 0.38f;

        private readonly Collider[] _overlapBuffer = new Collider[16];
        private float _cooldown;

        public event Action<bool, int> SwingConnected;

        public void Awake()
        {
            if (settings == null)
                settings = ScriptableObject.CreateInstance<CombatSettings>();
        }

        public void Configure(CombatSettings combatSettings, HitStopDriver hitStop = null)
        {
            if (combatSettings != null) settings = combatSettings;
            if (hitStop != null) hitStopDriver = hitStop;
        }

        public void Tick(float deltaTime)
        {
            if (_cooldown > 0f)
                _cooldown = Mathf.Max(0f, _cooldown - Mathf.Max(0f, deltaTime));
        }

        public void Update() => Tick(Time.deltaTime);

        public int TrySwing(bool heavy)
        {
            if (_cooldown > 0f) return 0;
            _cooldown = heavy ? swingCooldown * 1.55f : swingCooldown;

            Vector3 center = transform.position + transform.forward * (reach * 0.6f);
            int count = Physics.OverlapSphereNonAlloc(center, reach * 0.65f, _overlapBuffer, -1, QueryTriggerInteraction.Ignore);
            int hits = 0;

            float force = heavy ? settings.heavyMeleeForce : settings.lightMeleeForce;
            float baseDmg = heavy ? heavyDamage : lightDamage;

            for (int i = 0; i < count; i++)
            {
                Collider col = _overlapBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                IDamageable target = null;
                if (col.TryGetComponent(out BodyPartTag tag))
                    target = tag.Damageable;
                if (target == null)
                    col.TryGetComponent(out target);

                col.TryGetComponent(out RagdollController ragdoll);
                bool isRagdolled = ragdoll != null && ragdoll.IsActiveRagdoll;
                bool backstab = CombatMath.IsBackstab(transform.forward, col.transform.forward, settings.backstabDotThreshold);
                float dmg = CombatMath.ComputeMeleeDamage(baseDmg, isRagdolled, backstab, settings);

                Vector3 pushDir = (col.transform.position - transform.position).normalized;
                if (pushDir.sqrMagnitude < 1e-4f) pushDir = transform.forward;
                Vector3 impulse = (pushDir + Vector3.up * 0.25f).normalized * (force * 0.05f);

                if (target != null)
                {
                    target.ApplyDamage(new DamageInfo(dmg, BodyPart.Torso, col.transform.position, pushDir, impulse.magnitude, RagdollActivationReason.Melee, gameObject));
                    hits++;
                }

                if (ragdoll != null && (heavy || force >= settings.lightMeleeForce))
                {
                    ragdoll.Activate(RagdollActivationReason.Melee, impulse, BodyPart.Torso);
                }
            }

            if (hits > 0 && hitStopDriver != null)
            {
                hitStopDriver.Trigger(heavy ? settings.heavyHitStop : settings.lightHitStop);
            }

            SwingConnected?.Invoke(heavy, hits);
            return hits;
        }
    }
}
