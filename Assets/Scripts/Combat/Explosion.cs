using DudeChaos.Data;
using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Combat
{
    // Single OverlapSphereNonAlloc with static buffer and linear falloff (§1.7 & §1.8).
    public static class Explosion
    {
        private static Collider[] _buffer = new Collider[64];

        public static int Detonate(
            Vector3 center,
            CombatSettings settings,
            GameObject instigator = null,
            float radiusOverride = -1f,
            float damageOverride = -1f,
            float impulseOverride = -1f)
        {
            float radius = radiusOverride > 0f ? radiusOverride : (settings != null ? settings.explosionRadius : 6f);
            float maxDamage = damageOverride >= 0f ? damageOverride : (settings != null ? settings.explosionDamage : 140f);
            float maxImpulse = impulseOverride >= 0f ? impulseOverride : (settings != null ? settings.explosionImpulse : 260f);
            int bufCap = settings != null && settings.explosionBufferSize > 0 ? settings.explosionBufferSize : 64;

            if (_buffer.Length < bufCap)
                _buffer = new Collider[bufCap];

            int count = Physics.OverlapSphereNonAlloc(center, radius, _buffer, -1, QueryTriggerInteraction.Ignore);
            int affected = 0;

            for (int i = 0; i < count; i++)
            {
                Collider col = _buffer[i];
                if (col == null) continue;

                Vector3 targetPos = col.transform.position;
                float dist = Vector3.Distance(center, targetPos);
                float falloff = CombatMath.ComputeExplosionFalloff(dist, radius);
                if (falloff <= 0f) continue;

                Vector3 dir = (targetPos - center);
                dir = dir.sqrMagnitude > 1e-5f ? dir.normalized : Vector3.up;
                Vector3 impulseVec = (dir + Vector3.up * 0.35f).normalized * (maxImpulse * falloff);
                float dmg = maxDamage * falloff;

                IDamageable target = null;
                if (col.TryGetComponent(out BodyPartTag tag))
                    target = tag.Damageable;
                if (target == null)
                    col.TryGetComponent(out target);

                if (target != null)
                {
                    target.ApplyDamage(new DamageInfo(dmg, BodyPart.Torso, targetPos, dir, impulseVec.magnitude, RagdollActivationReason.Explosion, instigator));
                    affected++;
                }

                if (col.TryGetComponent(out RagdollController ragdoll))
                {
                    ragdoll.Activate(RagdollActivationReason.Explosion, impulseVec, BodyPart.Torso);
                }
                else if (col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic)
                {
                    col.attachedRigidbody.AddForce(impulseVec, ForceMode.Impulse);
                }
            }

            return affected;
        }
    }
}
