using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "CombatSettings", menuName = "DudeChaos/Settings/Combat")]
    public class CombatSettings : ScriptableObject
    {
        public float lightMeleeForce = 1200f;
        public float lightHitStop = 0.04f;
        public float heavyMeleeForce = 3800f;
        public float heavyHitStop = 0.11f;
        public float ragdolledDamageMultiplier = 1.6f;
        public float backstabMultiplier = 1.8f;
        public float backstabDotThreshold = -0.35f;
        public float muzzleLightDuration = 0.055f;
        public float recoilRecoveryTime = 0.18f;
        public float pointBlankDistance = 2.5f;
        public float baseShotImpulse = 45f;
        public float shotUpwardBias = 0.25f;
        public float explosionRadius = 6.0f;
        public float explosionImpulse = 260f;
        public float explosionDamage = 140f;
        public int explosionBufferSize = 64;
    }
}
