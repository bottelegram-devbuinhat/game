using UnityEngine;

namespace DudeChaos.Data
{
    public struct DamageInfo
    {
        public float amount;
        public BodyPart bodyPart;
        public Vector3 hitPoint;
        public Vector3 hitDirection;
        public float impulse;
        public RagdollActivationReason reason;
        public GameObject instigator;

        public DamageInfo(
            float amount,
            BodyPart bodyPart = BodyPart.Torso,
            Vector3 hitPoint = default,
            Vector3 hitDirection = default,
            float impulse = 0f,
            RagdollActivationReason reason = RagdollActivationReason.Bullet,
            GameObject instigator = null)
        {
            this.amount = amount;
            this.bodyPart = bodyPart;
            this.hitPoint = hitPoint;
            this.hitDirection = hitDirection;
            this.impulse = impulse;
            this.reason = reason;
            this.instigator = instigator;
        }
    }
}
