using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.City
{
    // XZ rectangle containment; smallest containing area wins so Secret inside Downtown resolves to Secret (§1.8 & §2.5).
    public static class DistrictBounds
    {
        public static bool ContainsXZ(DistrictDefinition district, Vector3 worldPos)
        {
            if (district == null) return false;
            float halfX = Mathf.Abs(district.size.x) * 0.5f;
            float halfZ = Mathf.Abs(district.size.y) * 0.5f;
            return worldPos.x >= (district.centre.x - halfX) &&
                   worldPos.x <= (district.centre.x + halfX) &&
                   worldPos.z >= (district.centre.y - halfZ) &&
                   worldPos.z <= (district.centre.y + halfZ);
        }

        public static float AreaSquareMeters(DistrictDefinition district)
        {
            if (district == null) return 0f;
            return Mathf.Abs(district.size.x) * Mathf.Abs(district.size.y);
        }

        public static float TotalAreaSquareKilometers(IReadOnlyList<DistrictDefinition> districts)
        {
            if (districts == null) return 0f;
            double sum = 0.0;
            for (int i = 0; i < districts.Count; i++)
                sum += AreaSquareMeters(districts[i]);
            return (float)(sum / 1_000_000.0);
        }

        public static DistrictDefinition ResolveContaining(IReadOnlyList<DistrictDefinition> districts, Vector3 worldPos)
        {
            if (districts == null || districts.Count == 0)
                return null;
            DistrictDefinition best = null;
            float bestArea = float.MaxValue;
            for (int i = 0; i < districts.Count; i++)
            {
                DistrictDefinition d = districts[i];
                if (!ContainsXZ(d, worldPos)) continue;
                float area = AreaSquareMeters(d);
                if (area < bestArea)
                {
                    bestArea = area;
                    best = d;
                }
            }
            return best;
        }
    }
}
