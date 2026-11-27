using UnityEngine;

namespace DudeChaos.Net
{
    public enum ValidationVerdict
    {
        Accepted = 0,
        StaleTick = 1,
        TeleportDetected = 2,
        SpeedHackDetected = 3,
        FireRateAbuse = 4,
        InvalidDamageClaim = 5
    }

    // Server-side authority validator (§1.8 & §4.2 & P10).
    public static class AuthorityValidator
    {
        public const ushort FireButtonBit = 1 << 0;
        public const float TeleportDistanceThreshold = 15.0f;

        public static ValidationVerdict Validate(
            NetworkCommand incoming,
            uint lastAcceptedTick,
            Vector3 lastAcceptedPosition,
            float fixedTickSeconds,
            float maxAllowedSpeed,
            uint lastFireTick,
            uint minTicksBetweenShots,
            byte maxLegalDamagePerTick)
        {
            if (incoming.tick <= lastAcceptedTick)
                return ValidationVerdict.StaleTick;

            uint tickDelta = incoming.tick - lastAcceptedTick;
            float dt = Mathf.Max(fixedTickSeconds, tickDelta * fixedTickSeconds);
            float dist = Vector3.Distance(incoming.position, lastAcceptedPosition);

            if (dist > TeleportDistanceThreshold && tickDelta <= 2)
                return ValidationVerdict.TeleportDetected;

            float impliedSpeed = dist / dt;
            if (impliedSpeed > maxAllowedSpeed * 1.18f)
                return ValidationVerdict.SpeedHackDetected;

            bool firing = (incoming.buttonsMask & FireButtonBit) != 0;
            if (firing && lastFireTick > 0 && (incoming.tick - lastFireTick) < minTicksBetweenShots)
                return ValidationVerdict.FireRateAbuse;

            if (incoming.claimedDamage > maxLegalDamagePerTick)
                return ValidationVerdict.InvalidDamageClaim;

            return ValidationVerdict.Accepted;
        }
    }
}
