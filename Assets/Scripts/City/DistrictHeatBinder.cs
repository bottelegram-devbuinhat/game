using DudeChaos.Data;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.City
{
    public sealed class DistrictHeatBinder : MonoBehaviour
    {
        [SerializeField] private DistrictRegistry registry;
        [SerializeField] private WantedSystem wantedSystem;
        [SerializeField] private DayNightCycle dayNight;
        [SerializeField] private bool playerHasRestrictedPass;

        public void Bind(DistrictRegistry districtRegistry, WantedSystem wanted, DayNightCycle clock = null)
        {
            if (registry != null)
                registry.DistrictChanged -= OnDistrictChanged;
            registry = districtRegistry;
            wantedSystem = wanted;
            dayNight = clock;
            if (registry != null)
                registry.DistrictChanged += OnDistrictChanged;
        }

        public void SetPlayerPass(bool hasPass) => playerHasRestrictedPass = hasPass;

        public void OnDestroy()
        {
            if (registry != null)
                registry.DistrictChanged -= OnDistrictChanged;
        }

        private void OnDistrictChanged(DistrictDefinition district)
        {
            if (wantedSystem == null) return;
            float hour = dayNight != null ? dayNight.CurrentHour : 12f;
            wantedSystem.HeatMultiplier = DistrictHeatRules.HeatMultiplierForDistrict(district, hour);
            if (DistrictHeatRules.TriggersTrespassCrime(district, playerHasRestrictedPass))
            {
                wantedSystem.ReportCrime(CrimeType.AssaultPolice);
            }
        }
    }
}
