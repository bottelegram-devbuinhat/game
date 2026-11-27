using UnityEngine;

namespace DudeChaos.Npc
{
    // Tick rates 60 / 20 / 6 / 0 per second by distance (§1.8 & §4.2).
    public static class NpcTickLod
    {
        public static int TickRateHz(float distanceMeters)
        {
            if (distanceMeters <= 25f) return 60;
            if (distanceMeters <= 55f) return 20;
            if (distanceMeters <= 95f) return 6;
            return 0;
        }

        public static float TickMultiplier(int tickRateHz)
        {
            if (tickRateHz <= 0) return 0f;
            return 60f / tickRateHz;
        }

        public static bool ShouldTickOnFrame(int frameCount, int agentId, int tickRateHz)
        {
            if (tickRateHz <= 0) return false;
            if (tickRateHz >= 60) return true;
            int stride = Mathf.Max(1, 60 / tickRateHz);
            return ((frameCount + Mathf.Abs(agentId)) % stride) == 0;
        }
    }
}
