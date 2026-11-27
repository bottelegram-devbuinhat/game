using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Player
{
    public static class LocomotionMath
    {
        public static Vector2 ApplyDeadZone(Vector2 rawInput, float deadZone)
        {
            float mag = rawInput.magnitude;
            if (mag <= deadZone || mag < 1e-6f)
                return Vector2.zero;
            float rescaled = Mathf.Clamp01((mag - deadZone) / Mathf.Max(0.0001f, 1f - deadZone));
            return rawInput / mag * rescaled;
        }

        public static float ComputeTargetSpeed(
            Vector2 filteredInput,
            bool sprintHeld,
            bool isCrouching,
            MovementSettings settings)
        {
            if (settings == null) return 0f;
            float mag = Mathf.Clamp01(filteredInput.magnitude);
            if (mag <= 0.0001f)
                return 0f;

            if (isCrouching)
                return settings.crouchSpeed * mag;

            if (sprintHeld && mag >= 0.65f)
                return settings.sprintSpeed;

            if (mag <= 0.55f)
            {
                float t = mag / 0.55f;
                return Mathf.Lerp(0f, settings.walkSpeed, t);
            }
            else
            {
                float t = (mag - 0.55f) / 0.45f;
                return Mathf.Lerp(settings.walkSpeed, settings.runSpeed, t);
            }
        }

        public static float ComputeJumpVelocity(float jumpHeight, float gravity)
        {
            float g = Mathf.Abs(gravity);
            if (jumpHeight <= 0f || g <= 1e-4f) return 0f;
            return Mathf.Sqrt(2f * g * jumpHeight);
        }

        public static bool CanConsumeJump(float timeSinceGrounded, float coyoteTime, float timeSinceJumpPressed, float jumpBufferTime)
        {
            return timeSinceGrounded <= coyoteTime && timeSinceJumpPressed <= jumpBufferTime;
        }

        public static float StepVerticalVelocity(float currentVerticalVelocity, float gravity, float dt, bool isGrounded)
        {
            if (isGrounded && currentVerticalVelocity < 0f)
                return -2f;
            return currentVerticalVelocity + gravity * Mathf.Max(0f, dt);
        }

        public static float ComputeFallDamage(float landingVelocity, MovementSettings settings)
        {
            if (settings == null) return 0f;
            if (settings.ClassifyLanding(landingVelocity) != LandingTier.Hard)
                return 0f;
            float excess = Mathf.Abs(landingVelocity) - Mathf.Abs(settings.landHardThreshold);
            return Mathf.Max(5f, excess * 6.5f);
        }
    }
}
