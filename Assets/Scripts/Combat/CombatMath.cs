using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public static class CombatMath
    {
        public static float BodyPartMultiplier(BodyPart part, WeaponData weapon)
        {
            float head = weapon != null ? weapon.headMultiplier : 3.2f;
            float limb = weapon != null ? weapon.limbMultiplier : 0.75f;
            switch (part)
            {
                case BodyPart.Head: return head;
                case BodyPart.Limb: return limb;
                default: return 1.0f;
            }
        }

        public static float ComputeShotDamage(float baseDamage, BodyPart part, WeaponData weapon, float ammoMultiplier = 1f)
        {
            return Mathf.Max(0f, baseDamage * BodyPartMultiplier(part, weapon) * Mathf.Max(0f, ammoMultiplier));
        }

        public static Vector3 ComputeShotImpulse(Vector3 rayDirection, float baseImpulse, float upwardBias, float ragdollMultiplier = 1f)
        {
            Vector3 dir = rayDirection.normalized;
            Vector3 biased = (dir + Vector3.up * upwardBias).normalized;
            return biased * (Mathf.Max(0f, baseImpulse) * Mathf.Max(0f, ragdollMultiplier));
        }

        public static Vector3 ApplySpread(Vector3 forward, float spreadDegrees, Vector2 sampleInUnitDisk)
        {
            Vector3 f = forward.normalized;
            if (f.sqrMagnitude < 1e-6f) f = Vector3.forward;
            if (spreadDegrees <= 0.0001f) return f;

            float yaw = sampleInUnitDisk.x * spreadDegrees;
            float pitch = sampleInUnitDisk.y * spreadDegrees;
            return (Quaternion.Euler(pitch, yaw, 0f) * f).normalized;
        }

        public static float ComputeExplosionFalloff(float distance, float radius)
        {
            if (radius <= 0.0001f || distance >= radius) return 0f;
            if (distance <= 0f) return 1f;
            return Mathf.Clamp01(1f - distance / radius);
        }

        public static bool IsBackstab(Vector3 attackerForward, Vector3 targetForward, float dotThreshold = -0.35f)
        {
            Vector3 a = attackerForward.normalized;
            Vector3 t = targetForward.normalized;
            // Attacker behind target facing same way -> vector from target to attacker is -targetForward -> dot(targetForward, toAttacker) <= dotThreshold.
            Vector3 targetToAttacker = -a;
            return Vector3.Dot(t, targetToAttacker) <= dotThreshold;
        }

        public static float ComputeMeleeDamage(
            float baseDamage,
            bool targetIsRagdolled,
            bool isBackstab,
            CombatSettings settings)
        {
            float mult = 1f;
            if (targetIsRagdolled)
                mult *= settings != null ? settings.ragdolledDamageMultiplier : 1.6f;
            if (isBackstab)
                mult *= settings != null ? settings.backstabMultiplier : 1.8f;
            return baseDamage * mult;
        }

        public static bool IsPointBlank(float distance, CombatSettings settings)
        {
            float pb = settings != null ? settings.pointBlankDistance : 2.5f;
            return distance >= 0f && distance <= pb;
        }
    }
}
