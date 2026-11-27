using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Wanted
{
    public struct PoliceResponse
    {
        public int level;
        public int maxUnits;
        public int maxCars;
        public bool openFire;
        public bool spawnRoadblock;
        public bool spawnHelicopter;
        public bool spawnSwat;
    }

    public static class WantedMath
    {
        // Computes wanted level 0..6 from heat, skipping zero or negative threshold entries (§1.8 & §4.2).
        public static int ComputeLevelFromHeat(float heat, int[] thresholds)
        {
            if (heat <= 0f || thresholds == null || thresholds.Length == 0)
                return 0;

            int level = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                int t = thresholds[i];
                if (t <= 0) continue;
                if (heat >= t)
                    level++;
            }
            return Mathf.Clamp(level, 0, 6);
        }

        // §1.7: Cooldown only ticks when out of sight AND beyond 48 m AND memory (9 s) has elapsed.
        public static bool CanCoolDown(
            int currentLevel,
            bool inLineOfSight,
            float nearestPoliceDistance,
            float timeSinceLastSeen,
            WantedSettings settings)
        {
            if (currentLevel <= 0) return false;
            if (inLineOfSight) return false;
            float radius = settings != null ? settings.searchRadius : 48f;
            float memory = settings != null ? settings.memorySeconds : 9f;
            return nearestPoliceDistance > radius && timeSinceLastSeen >= memory;
        }

        public static float CoolHeat(float currentHeat, int currentLevel, float deltaTime, WantedSettings settings)
        {
            if (currentHeat <= 0f || currentLevel <= 0 || deltaTime <= 0f)
                return Mathf.Max(0f, currentHeat);

            float cooldownDuration = 10f;
            if (settings != null && settings.cooldowns != null && settings.cooldowns.Length > 0)
            {
                int idx = Mathf.Clamp(currentLevel - 1, 0, settings.cooldowns.Length - 1);
                cooldownDuration = Mathf.Max(1f, settings.cooldowns[idx]);
            }

            float thresholdSpan = 5f;
            if (settings != null && settings.thresholds != null && currentLevel <= settings.thresholds.Length)
            {
                int upper = settings.thresholds[currentLevel - 1];
                int lower = currentLevel >= 2 ? settings.thresholds[currentLevel - 2] : 0;
                thresholdSpan = Mathf.Max(1f, upper - lower);
            }

            float rate = thresholdSpan / cooldownDuration;
            return Mathf.Max(0f, currentHeat - rate * deltaTime);
        }

        public static PoliceResponse GetResponse(int level, WantedSettings settings)
        {
            level = Mathf.Clamp(level, 0, 6);
            if (level == 0)
                return default;

            int units = 2;
            if (settings != null && settings.unitsPerLevel != null && settings.unitsPerLevel.Length > 0)
            {
                int idx = Mathf.Clamp(level - 1, 0, settings.unitsPerLevel.Length - 1);
                units = settings.unitsPerLevel[idx];
            }

            int fireLvl = settings != null ? settings.openFireFromLevel : 2;
            int heliLvl = settings != null ? settings.helicopterFromLevel : 3;
            int blockLvl = settings != null ? settings.roadblockFromLevel : 3;
            int swatLvl = settings != null ? settings.swatFromLevel : 4;

            return new PoliceResponse
            {
                level = level,
                maxUnits = units,
                maxCars = Mathf.Max(1, units / 2),
                openFire = level >= fireLvl,
                spawnHelicopter = level >= heliLvl,
                spawnRoadblock = level >= blockLvl,
                spawnSwat = level >= swatLvl
            };
        }
    }
}
