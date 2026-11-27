using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Progression
{
    // §1.8 & §2.8: Pays on CrimeReported; diminishing returns inside 60 s window, floor 25%, 1.5 s anti-double-pay gap.
    public static class BountyRules
    {
        public const float WindowSeconds = 60.0f;
        public const float MinFloorMultiplier = 0.25f;
        public const float AntiDoublePayGapSeconds = 1.5f;

        public static int BaseBountyForCrime(CrimeType crime)
        {
            switch (crime)
            {
                case CrimeType.AssaultCivilian: return 40;
                case CrimeType.WeaponDischarge: return 25;
                case CrimeType.VehicleTheft: return 120;
                case CrimeType.RunOver: return 80;
                case CrimeType.AssaultPolice: return 110;
                case CrimeType.Explosion: return 150;
                case CrimeType.KillPolice: return 220;
                case CrimeType.DestroyPoliceVehicle: return 280;
                default: return 30;
            }
        }

        public static float DiminishingMultiplier(int payoutsInCurrentWindow)
        {
            if (payoutsInCurrentWindow <= 0) return 1.0f;
            float factor = Mathf.Pow(0.82f, payoutsInCurrentWindow);
            return Mathf.Max(MinFloorMultiplier, factor);
        }
    }
}
