using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public enum WeaponMechanicKind
    {
        Auto = 0,
        Semi = 1,
        Burst = 2,
        Pump = 3,
        Bolt = 4,
        Charge = 5,
        Beam = 6,
        Projectile = 7,
        Throwable = 8
    }

    // Implements all nine mechanics (§1.8 & §2.1).
    public static class WeaponMechanics
    {
        public static WeaponMechanicKind ResolveKind(WeaponData weapon)
        {
            if (weapon == null) return WeaponMechanicKind.Semi;

            if (!string.IsNullOrWhiteSpace(weapon.mechanicName) &&
                !string.Equals(weapon.mechanicName, "class", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse(weapon.mechanicName.Trim(), true, out WeaponMechanicKind parsed))
                    return parsed;
            }

            // Class defaults (§2.1): Pistol Semi, SMG Auto, Rifle Auto, Shotgun Pump, Sniper Bolt, Heavy Auto, Melee Semi.
            switch (weapon.weaponClass)
            {
                case WeaponClass.Pistol: return weapon.isAutomatic ? WeaponMechanicKind.Auto : WeaponMechanicKind.Semi;
                case WeaponClass.SMG: return WeaponMechanicKind.Auto;
                case WeaponClass.Rifle: return weapon.isAutomatic ? WeaponMechanicKind.Auto : WeaponMechanicKind.Semi;
                case WeaponClass.Shotgun: return weapon.isAutomatic ? WeaponMechanicKind.Auto : WeaponMechanicKind.Pump;
                case WeaponClass.Sniper: return weapon.fireRate >= 1.0f ? WeaponMechanicKind.Bolt : WeaponMechanicKind.Semi;
                case WeaponClass.Heavy: return weapon.isAutomatic ? WeaponMechanicKind.Auto : WeaponMechanicKind.Projectile;
                case WeaponClass.Melee: return WeaponMechanicKind.Semi;
                default: return WeaponMechanicKind.Semi;
            }
        }

        public static int ShotsPerPull(WeaponData weapon)
        {
            WeaponMechanicKind kind = ResolveKind(weapon);
            if (kind == WeaponMechanicKind.Beam)
                return 0;
            if (kind == WeaponMechanicKind.Burst)
            {
                int b = weapon != null ? weapon.burstCount : 3;
                return Mathf.Clamp(b, 2, 6);
            }
            return 1;
        }

        public static float EffectiveCycleSeconds(WeaponData weapon)
        {
            if (weapon == null) return 0.2f;
            WeaponMechanicKind kind = ResolveKind(weapon);
            float baseRate = Mathf.Max(0.02f, weapon.fireRate);
            switch (kind)
            {
                case WeaponMechanicKind.Pump:
                    return baseRate + 0.12f;
                case WeaponMechanicKind.Bolt:
                    return baseRate + 0.20f;
                case WeaponMechanicKind.Charge:
                    return baseRate + Mathf.Max(0f, weapon.chargeSeconds);
                case WeaponMechanicKind.Throwable:
                    return Mathf.Max(0.8f, baseRate + Mathf.Max(0f, weapon.chargeSeconds));
                default:
                    return baseRate;
            }
        }

        public static float ComputeHeatDelta(WeaponData weapon, float deltaTime)
        {
            if (weapon == null) return 0f;
            WeaponMechanicKind kind = ResolveKind(weapon);
            if (kind == WeaponMechanicKind.Beam)
            {
                float rate = weapon.heatPerShot > 0f ? weapon.heatPerShot : 0.35f;
                return rate * Mathf.Max(0f, deltaTime);
            }
            return Mathf.Max(0f, weapon.heatPerShot);
        }

        public static float SelfDamageAtDistance(WeaponData weapon, float hitDistance, CombatSettings settings)
        {
            if (weapon == null) return 0f;
            WeaponMechanicKind kind = ResolveKind(weapon);
            if (kind != WeaponMechanicKind.Projectile && kind != WeaponMechanicKind.Throwable)
                return 0f;
            if (!CombatMath.IsPointBlank(hitDistance, settings))
                return 0f;
            return weapon.selfDamage > 0f ? weapon.selfDamage : weapon.damage * 0.35f;
        }

        public static float MoveSpeedMultiplier(WeaponMechanicKind kind)
        {
            switch (kind)
            {
                case WeaponMechanicKind.Charge: return 0.82f;
                case WeaponMechanicKind.Beam: return 0.85f;
                case WeaponMechanicKind.Bolt: return 0.88f;
                case WeaponMechanicKind.Projectile: return 0.85f;
                default: return 1.0f;
            }
        }

        public static float NoiseMultiplier(WeaponMechanicKind kind)
        {
            switch (kind)
            {
                case WeaponMechanicKind.Beam: return 0.5f;
                case WeaponMechanicKind.Throwable: return 1.4f;
                case WeaponMechanicKind.Projectile: return 1.5f;
                case WeaponMechanicKind.Bolt: return 1.35f;
                default: return 1.0f;
            }
        }
    }
}
