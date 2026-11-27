using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.City
{
    public enum SideJobKind
    {
        CourierDrop = 0,
        TaxiFare = 1,
        RepoTow = 2,
        StreetBrawl = 3,
        TimeTrial = 4
    }

    public struct SideJobOffer
    {
        public SideJobKind kind;
        public DistrictKind district;
        public int rewardMoney;
        public float timeLimitSeconds;
    }

    public static class SideMissionRules
    {
        public const float NightPayMultiplier = 1.25f;

        public static SideJobOffer GenerateOffer(DistrictKind district, float hourOfDay, int deterministicSeed)
        {
            unchecked
            {
                uint h = (uint)deterministicSeed ^ ((uint)district * 2654435761u) ^ (uint)(Mathf.FloorToInt(hourOfDay) * 40503);
                h ^= h >> 15;
                h *= 0x85EBCA6Bu;
                SideJobKind kind = PickWeightedKind(district, h);
                bool night = DistrictRules.IsNightHour(hourOfDay);
                int basePay = 300 + ((int)kind * 110) + ((int)(h % 5u) * 35);
                if (night) basePay = Mathf.RoundToInt(basePay * NightPayMultiplier);

                return new SideJobOffer
                {
                    kind = kind,
                    district = district,
                    rewardMoney = basePay,
                    timeLimitSeconds = 90f + ((int)(h % 4u) * 30f)
                };
            }
        }

        private static SideJobKind PickWeightedKind(DistrictKind district, uint hash)
        {
            switch (district)
            {
                case DistrictKind.Market:
                case DistrictKind.Port:
                    return (hash % 2u == 0u) ? SideJobKind.CourierDrop : SideJobKind.RepoTow;
                case DistrictKind.Slums:
                    return (hash % 2u == 0u) ? SideJobKind.StreetBrawl : SideJobKind.CourierDrop;
                case DistrictKind.Showroom:
                case DistrictKind.Airport:
                    return (hash % 2u == 0u) ? SideJobKind.TimeTrial : SideJobKind.RepoTow;
                default:
                    return (SideJobKind)(hash % 5u);
            }
        }
    }
}
