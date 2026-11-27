using UnityEngine;

namespace DudeChaos.City
{
    public enum HotelTier
    {
        Basic = 0,
        Standard = 1,
        Deluxe = 2,
        Penthouse = 3
    }

    // §1.8 & §2.8: Basic 150/60%, Standard 400/80%, Deluxe 900/100%, Penthouse 2500/100% + pass/rental;
    // max 7 nights, sleep until 07:00, early checkout refunds 50% of unused nights.
    public static class HotelRules
    {
        public const int MaxNights = 7;
        public const float WakeUpHour = 7.0f;
        public const float RefundRatio = 0.50f;

        public static int NightlyRate(HotelTier tier)
        {
            switch (tier)
            {
                case HotelTier.Basic: return 150;
                case HotelTier.Standard: return 400;
                case HotelTier.Deluxe: return 900;
                case HotelTier.Penthouse: return 2500;
                default: return 150;
            }
        }

        public static float HealFraction(HotelTier tier)
        {
            switch (tier)
            {
                case HotelTier.Basic: return 0.60f;
                case HotelTier.Standard: return 0.80f;
                default: return 1.00f;
            }
        }

        public static bool CanBook(HotelTier tier, int nights, bool hasVipPass, bool hasExistingRental)
        {
            if (nights < 1 || nights > MaxNights)
                return false;
            if (tier == HotelTier.Penthouse && !hasVipPass && !hasExistingRental)
                return false;
            return true;
        }

        public static int ComputeBookingCost(HotelTier tier, int nights)
        {
            return NightlyRate(tier) * Mathf.Clamp(nights, 0, MaxNights);
        }

        public static int ComputeCheckoutRefund(HotelTier tier, int nightsRemaining)
        {
            if (nightsRemaining <= 0) return 0;
            return Mathf.RoundToInt(NightlyRate(tier) * nightsRemaining * RefundRatio);
        }
    }
}
