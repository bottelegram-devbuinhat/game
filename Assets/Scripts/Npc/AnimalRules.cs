using DudeChaos.City;
using DudeChaos.Data;

namespace DudeChaos.Npc
{
    public enum AnimalSpecies
    {
        StreetDog = 0,
        AlleyCat = 1,
        Chicken = 2,
        WaterBuffalo = 3,
        Pigeon = 4,
        Seagull = 5,
        Rat = 6
    }

    public struct AnimalSpec
    {
        public AnimalSpecies species;
        public float wanderSpeed;
        public float fleeSpeed;
        public float fearRadius;
        public bool flies;
        public bool nocturnal;
    }

    public static class AnimalRules
    {
        public static AnimalSpecies PrimarySpeciesForDistrict(DistrictKind district, float hour)
        {
            bool night = DistrictRules.IsNightHour(hour);
            switch (district)
            {
                case DistrictKind.Farm: return night ? AnimalSpecies.StreetDog : AnimalSpecies.Chicken;
                case DistrictKind.Port: return AnimalSpecies.Seagull;
                case DistrictKind.Slums: return night ? AnimalSpecies.Rat : AnimalSpecies.StreetDog;
                case DistrictKind.Market: return night ? AnimalSpecies.AlleyCat : AnimalSpecies.Pigeon;
                case DistrictKind.Residential: return AnimalSpecies.StreetDog;
                default: return night ? AnimalSpecies.AlleyCat : AnimalSpecies.Pigeon;
            }
        }

        public static AnimalSpec GetSpec(AnimalSpecies species)
        {
            switch (species)
            {
                case AnimalSpecies.StreetDog:
                    return new AnimalSpec { species = species, wanderSpeed = 1.8f, fleeSpeed = 6.2f, fearRadius = 8f, flies = false, nocturnal = false };
                case AnimalSpecies.AlleyCat:
                    return new AnimalSpec { species = species, wanderSpeed = 1.6f, fleeSpeed = 7.0f, fearRadius = 9f, flies = false, nocturnal = true };
                case AnimalSpecies.Chicken:
                    return new AnimalSpec { species = species, wanderSpeed = 1.1f, fleeSpeed = 4.2f, fearRadius = 7f, flies = false, nocturnal = false };
                case AnimalSpecies.WaterBuffalo:
                    return new AnimalSpec { species = species, wanderSpeed = 0.9f, fleeSpeed = 3.5f, fearRadius = 4f, flies = false, nocturnal = false };
                case AnimalSpecies.Pigeon:
                    return new AnimalSpec { species = species, wanderSpeed = 2.5f, fleeSpeed = 9.5f, fearRadius = 6f, flies = true, nocturnal = false };
                case AnimalSpecies.Seagull:
                    return new AnimalSpec { species = species, wanderSpeed = 3.5f, fleeSpeed = 10.5f, fearRadius = 10f, flies = true, nocturnal = false };
                default:
                    return new AnimalSpec { species = AnimalSpecies.Rat, wanderSpeed = 1.4f, fleeSpeed = 4.8f, fearRadius = 5f, flies = false, nocturnal = true };
            }
        }

        public static int BudgetForDistrict(DistrictKind district, float hour)
        {
            bool night = DistrictRules.IsNightHour(hour);
            if (district == DistrictKind.Farm) return night ? 2 : 8;
            if (district == DistrictKind.Military || district == DistrictKind.Secret) return 0;
            return night ? 3 : 5;
        }
    }
}
