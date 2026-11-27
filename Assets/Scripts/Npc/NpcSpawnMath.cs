using UnityEngine;

namespace DudeChaos.Npc
{
    public static class NpcSpawnMath
    {
        public const float MinSpawnDistance = 18f;
        public const float MaxSpawnDistance = 62f;
        public const float DespawnDistance = 85f;

        public static int DesiredCount(int qualityNpcBudget, float districtCrowdMultiplier)
        {
            int scaled = Mathf.RoundToInt(Mathf.Max(0, qualityNpcBudget) * Mathf.Max(0f, districtCrowdMultiplier));
            return Mathf.Clamp(scaled, 0, Mathf.Max(0, qualityNpcBudget));
        }

        public static bool ShouldDespawn(Vector3 viewerPos, Vector3 npcPos)
        {
            Vector3 flat = npcPos - viewerPos;
            flat.y = 0f;
            return flat.sqrMagnitude > DespawnDistance * DespawnDistance;
        }

        public static Vector3 DeterministicRingPoint(Vector3 center, int slotIndex, int totalSlots)
        {
            int count = Mathf.Max(1, totalSlots);
            float angle = (slotIndex % count) / (float)count * Mathf.PI * 2f;
            float ringT = ((slotIndex * 7) % 11) / 10f;
            float radius = Mathf.Lerp(MinSpawnDistance, MaxSpawnDistance, ringT);
            return center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
