using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "WantedSettings", menuName = "DudeChaos/Settings/Wanted")]
    public class WantedSettings : ScriptableObject
    {
        public int[] thresholds = new int[] { 1, 4, 9, 16, 25, 36 };
        public float[] cooldowns = new float[] { 6f, 9f, 12f, 16f, 20f, 25f };
        public float searchRadius = 48.0f;
        public float memorySeconds = 9.0f;

        public int assaultCivilianWeight = 1;
        public int assaultPoliceWeight = 2;
        public int killPoliceWeight = 3;
        public int weaponDischargeWeight = 1;
        public int vehicleTheftWeight = 2;
        public int runOverWeight = 2;
        public int explosionWeight = 2;
        public int destroyPoliceVehicleWeight = 3;

        public int[] unitsPerLevel = new int[] { 2, 3, 4, 5, 6, 6 };
        public int helicopterFromLevel = 3;
        public int roadblockFromLevel = 3;
        public int swatFromLevel = 4;
        public int openFireFromLevel = 2;

        public int GetCrimeWeight(CrimeType crime)
        {
            switch (crime)
            {
                case CrimeType.AssaultCivilian: return assaultCivilianWeight;
                case CrimeType.AssaultPolice: return assaultPoliceWeight;
                case CrimeType.KillPolice: return killPoliceWeight;
                case CrimeType.WeaponDischarge: return weaponDischargeWeight;
                case CrimeType.VehicleTheft: return vehicleTheftWeight;
                case CrimeType.RunOver: return runOverWeight;
                case CrimeType.Explosion: return explosionWeight;
                case CrimeType.DestroyPoliceVehicle: return destroyPoliceVehicleWeight;
                default: return 1;
            }
        }
    }
}
