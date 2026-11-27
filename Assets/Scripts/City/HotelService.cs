using DudeChaos.Player;
using DudeChaos.Progression;
using UnityEngine;

namespace DudeChaos.City
{
    public sealed class HotelService : MonoBehaviour
    {
        [SerializeField] private Transform bedSpawnPoint;

        private HotelTier _rentedTier = HotelTier.Basic;
        private int _nightsRemaining;

        public int NightsRemaining => _nightsRemaining;
        public HotelTier CurrentTier => _rentedTier;
        public bool HasActiveRental => _nightsRemaining > 0;
        public Vector3 BedRespawnPosition => bedSpawnPoint != null ? bedSpawnPoint.position : transform.position;

        public void ConfigureBedSpawn(Transform bed) => bedSpawnPoint = bed;

        public bool TryBookRoom(HotelTier tier, int nights, Wallet wallet, bool hasVipPass)
        {
            if (wallet == null) return false;
            if (!HotelRules.CanBook(tier, nights, hasVipPass, HasActiveRental))
                return false;
            if (_nightsRemaining + nights > HotelRules.MaxNights)
                return false;

            int cost = HotelRules.ComputeBookingCost(tier, nights);
            if (!wallet.TrySpend(cost))
                return false;

            _rentedTier = tier;
            _nightsRemaining += nights;
            return true;
        }

        public bool TrySleep(DayNightCycle clock, HealthSystem playerHealth)
        {
            if (_nightsRemaining <= 0) return false;
            _nightsRemaining--;
            clock?.SetHour(HotelRules.WakeUpHour);
            if (playerHealth != null)
            {
                float healAmount = playerHealth.MaxHealth * HotelRules.HealFraction(_rentedTier);
                playerHealth.Heal(healAmount);
            }
            return true;
        }

        public int CheckoutEarly(Wallet wallet)
        {
            if (_nightsRemaining <= 0) return 0;
            int refund = HotelRules.ComputeCheckoutRefund(_rentedTier, _nightsRemaining);
            _nightsRemaining = 0;
            wallet?.Add(refund);
            return refund;
        }

        public void RestoreNights(int nightsLeft, HotelTier tier = HotelTier.Standard)
        {
            _nightsRemaining = Mathf.Clamp(nightsLeft, 0, HotelRules.MaxNights);
            _rentedTier = tier;
        }
    }
}
