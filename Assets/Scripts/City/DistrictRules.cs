using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.City
{
    // §2.5 day/night district roles:
    // Slums busiest at night; Market empties late; Restaurant busier at night; Airport & Farm empty at night;
    // Downtown dense; Hotel 24h; Military guarded; Secret hidden.
    public static class DistrictRules
    {
        public static bool IsNightHour(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            return h >= 19.5f || h < 5.5f;
        }

        public static float CrowdDensityMultiplier(DistrictKind kind, float hour)
        {
            bool night = IsNightHour(hour);
            switch (kind)
            {
                case DistrictKind.Slums: return night ? 1.45f : 0.85f;
                case DistrictKind.Market: return night ? 0.15f : 1.40f;
                case DistrictKind.Restaurant: return night ? 1.35f : 0.75f;
                case DistrictKind.Airport: return night ? 0.10f : 0.90f;
                case DistrictKind.Farm: return night ? 0.05f : 0.45f;
                case DistrictKind.Downtown: return night ? 1.15f : 1.30f;
                case DistrictKind.Hotel: return 1.00f;
                case DistrictKind.Military: return 0.35f;
                case DistrictKind.Secret: return 0.10f;
                default: return night ? 0.65f : 1.00f;
            }
        }

        public static float PoliceResponseWeight(DistrictKind kind)
        {
            switch (kind)
            {
                case DistrictKind.Military: return 2.0f;
                case DistrictKind.Airport: return 1.6f;
                case DistrictKind.Downtown: return 1.3f;
                case DistrictKind.Showroom: return 1.25f;
                case DistrictKind.Slums: return 0.65f;
                case DistrictKind.Farm: return 0.5f;
                default: return 1.0f;
            }
        }

        public static bool HasWalkableInterior(DistrictKind kind)
        {
            return kind == DistrictKind.Hotel ||
                   kind == DistrictKind.Showroom ||
                   kind == DistrictKind.Restaurant ||
                   kind == DistrictKind.Airport ||
                   kind == DistrictKind.Downtown;
        }
    }
}
