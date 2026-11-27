using UnityEngine;

namespace DudeChaos.Settings
{
    public struct RuntimeQualityPreset
    {
        public int tier;
        public bool shadowsEnabled;
        public float shadowDistance;
        public float lodBias;
        public int antiAliasing;
        public int targetFps;
        public float fogDensity;
        public int npcBudget;
    }

    // Exact runtime tier rules from §1.9.2 & §1.9.5 & R6.
    public static class QualityRules
    {
        public static int PickTier(int memoryMegabytes, int cpuCores)
        {
            if (memoryMegabytes >= 6000 && cpuCores >= 8)
                return 2;
            if (memoryMegabytes < 3000 || cpuCores < 4)
                return 0;
            return 1;
        }

        public static RuntimeQualityPreset GetPreset(int tier)
        {
            switch (Mathf.Clamp(tier, 0, 2))
            {
                case 0:
                    return new RuntimeQualityPreset
                    {
                        tier = 0,
                        shadowsEnabled = false,
                        shadowDistance = 22f,
                        lodBias = 0.6f,
                        antiAliasing = 0,
                        targetFps = 30,
                        fogDensity = 0.012f,
                        npcBudget = 12
                    };
                case 2:
                    return new RuntimeQualityPreset
                    {
                        tier = 2,
                        shadowsEnabled = true,
                        shadowDistance = 70f,
                        lodBias = 2.0f,
                        antiAliasing = 4,
                        targetFps = 60,
                        fogDensity = 0.006f,
                        npcBudget = 40
                    };
                default:
                    return new RuntimeQualityPreset
                    {
                        tier = 1,
                        shadowsEnabled = true,
                        shadowDistance = 45f,
                        lodBias = 1.2f,
                        antiAliasing = 2,
                        targetFps = 30,
                        fogDensity = 0.009f,
                        npcBudget = 24
                    };
            }
        }

        // R6: Player setting can only lower shadow cost, NEVER raise it above the tier. 0 = tier's own distance.
        public static float ShadowDistanceFor(int tier, bool playerShadowsOn, float playerShadowDistance)
        {
            RuntimeQualityPreset preset = GetPreset(tier);
            if (!preset.shadowsEnabled || !playerShadowsOn)
                return 0f;
            if (playerShadowDistance <= 0f)
                return preset.shadowDistance;
            return Mathf.Min(preset.shadowDistance, Mathf.Clamp(playerShadowDistance, 0f, 100f));
        }

        // R6: Player setting can only lower targetFps, NEVER raise it above the tier.
        public static int TargetFrameRateFor(int tier, int playerTargetFps)
        {
            RuntimeQualityPreset preset = GetPreset(tier);
            if (playerTargetFps <= 0)
                return preset.targetFps;
            return Mathf.Min(preset.targetFps, Mathf.Clamp(playerTargetFps, 30, 120));
        }

        // §1.9.5: Clamped 60..500 m so slider never pushes past the 500 m scene far plane.
        public static float FarClipFor(float drawDistanceSetting)
        {
            return Mathf.Clamp(drawDistanceSetting, 60f, 500f);
        }
    }
}
