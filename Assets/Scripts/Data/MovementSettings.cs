using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "DudeChaos/Settings/Movement")]
    public class MovementSettings : ScriptableObject
    {
        public float deadZone = 0.18f;
        public float walkSpeed = 3.2f;
        public float runSpeed = 6.0f;
        public float sprintSpeed = 8.8f;
        public float crouchSpeed = 2.1f;
        public float sprintFovKick = 5.0f;
        public float coyoteTime = 0.12f;
        public float jumpBufferTime = 0.10f;
        public float gravity = -22.0f;
        public float jumpHeight = 1.35f;
        public float landSoftThreshold = -8.0f;
        public float landHardThreshold = -14.0f;
        public float standingHeight = 1.8f;
        public float crouchHeight = 1.1f;

        // Locked §1.7: soft > -8, medium -8..-14 (exact -8 is Medium), hard < -14.
        public LandingTier ClassifyLanding(float verticalVelocity)
        {
            if (verticalVelocity > landSoftThreshold)
                return LandingTier.Soft;
            if (verticalVelocity >= landHardThreshold)
                return LandingTier.Medium;
            return LandingTier.Hard;
        }
    }
}
