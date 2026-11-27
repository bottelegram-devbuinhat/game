using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "RagdollSettings", menuName = "DudeChaos/Settings/Ragdoll")]
    public class RagdollSettings : ScriptableObject
    {
        public float fallRagdollThreshold = -16.0f;
        public float highForceImpulseThreshold = 55.0f;
        public float recoveryVelocity = 1.1f;
        public float recoveryHoldTime = 1.8f;
        public float getUpDuration = 0.9f;
        public float totalMass = 78.0f;
        public int maxFullSimRagdolls = 12;
        public float fullSimRadius = 55.0f;
        public float sleepVelocity = 0.5f;
        public float pelvisFollowLag = 0.08f;

        public bool ShouldRagdollOnLanding(float verticalVelocity)
        {
            return verticalVelocity <= fallRagdollThreshold;
        }

        public bool ShouldRagdollOnImpulse(float impulseMagnitude)
        {
            return impulseMagnitude >= highForceImpulseThreshold;
        }
    }
}
