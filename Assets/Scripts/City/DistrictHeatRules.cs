using DudeChaos.Data;

namespace DudeChaos.City
{
    // Lives inside City (referencing Wanted, never Wanted -> City) to prevent assembly cycle (§1.5 & Landmine #4).
    public static class DistrictHeatRules
    {
        public static float HeatMultiplierForDistrict(DistrictDefinition district, float hourOfDay)
        {
            if (district == null) return 1.0f;
            float baseWeight = DistrictRules.PoliceResponseWeight(district.kind);
            if (DistrictRules.IsNightHour(hourOfDay) && district.kind == DistrictKind.Slums)
                baseWeight *= 0.85f;
            return baseWeight;
        }

        public static bool TriggersTrespassCrime(DistrictDefinition district, bool playerHasPass)
        {
            return district != null && district.requiresPass && !playerHasPass;
        }
    }
}
