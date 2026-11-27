using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    public enum VehicleDamageStage
    {
        Healthy = 0,
        Smoking = 1,
        Burning = 2,
        Exploded = 3
    }

    public static class VehicleMath
    {
        public const float MaxCenterOfMassOffset = 5.0f;
        public static readonly Vector3 DefaultCenterOfMass = new Vector3(0f, -0.35f, 0f);

        // Landmine #1: NaN, Infinity or |component| > 5 kills PhysX Simulate.
        public static bool IsUsableCenterOfMass(Vector3 offset)
        {
            if (float.IsNaN(offset.x) || float.IsNaN(offset.y) || float.IsNaN(offset.z))
                return false;
            if (float.IsInfinity(offset.x) || float.IsInfinity(offset.y) || float.IsInfinity(offset.z))
                return false;
            return Mathf.Abs(offset.x) <= MaxCenterOfMassOffset &&
                   Mathf.Abs(offset.y) <= MaxCenterOfMassOffset &&
                   Mathf.Abs(offset.z) <= MaxCenterOfMassOffset;
        }

        public static Vector3 SafeCenterOfMass(Vector3 candidate)
        {
            return IsUsableCenterOfMass(candidate) ? candidate : DefaultCenterOfMass;
        }

        // §1.7: clean exit < 9 m/s, eject ragdoll >= 9 m/s.
        public static bool ShouldEjectOnExit(float speedMetersPerSecond, VehicleSettings settings)
        {
            float threshold = settings != null ? settings.ejectSpeedThreshold : 9.0f;
            return speedMetersPerSecond >= threshold;
        }

        public static Vector3 ComputeEjectImpulse(Vector3 vehicleForward, VehicleSettings settings)
        {
            float fwd = settings != null ? settings.ejectForwardImpulse : 900f;
            float up = settings != null ? settings.ejectUpImpulse : 420f;
            Vector3 f = vehicleForward.sqrMagnitude > 1e-5f ? vehicleForward.normalized : Vector3.forward;
            return f * fwd + Vector3.up * up;
        }

        public static float ComputeSteeringFactor(float speed, float maxSpeed, VehicleSettings settings)
        {
            if (maxSpeed <= 0.01f) return 1f;
            float falloff = settings != null ? settings.steeringFalloff : 0.35f;
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);
            return Mathf.Lerp(1.0f, Mathf.Clamp(1.0f - falloff, 0.15f, 1.0f), normalizedSpeed);
        }

        public static VehicleDamageStage EvaluateDamageStage(float currentHealth, float maxHealth, VehicleSettings settings)
        {
            if (currentHealth <= 0f) return VehicleDamageStage.Exploded;
            if (maxHealth <= 0f) return VehicleDamageStage.Healthy;
            float ratio = currentHealth / maxHealth;
            float crit = settings != null ? settings.criticalHealthRatio : 0.25f;
            float smoke = settings != null ? settings.smokeHealthRatio : 0.50f;
            if (ratio <= crit) return VehicleDamageStage.Burning;
            if (ratio <= smoke) return VehicleDamageStage.Smoking;
            return VehicleDamageStage.Healthy;
        }
    }
}
