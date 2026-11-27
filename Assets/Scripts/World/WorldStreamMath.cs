using System.Collections.Generic;
using UnityEngine;

namespace DudeChaos.World
{
    // 64 m streaming cells; Chebyshev distance; radius 1 -> 3x3 = 9 alive cells at once (§1.7 & §1.8).
    public static class WorldStreamMath
    {
        public const float DefaultCellSize = 64f;
        public const int DefaultResidentRadius = 1; // (2*1+1)^2 = 9 cells

        public static Vector2Int WorldToCell(Vector3 worldPos, float cellSize = DefaultCellSize)
        {
            float size = Mathf.Max(1f, cellSize);
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / size),
                Mathf.FloorToInt(worldPos.z / size));
        }

        public static Vector3 CellCenterWorld(Vector2Int cell, float cellSize = DefaultCellSize)
        {
            float size = Mathf.Max(1f, cellSize);
            return new Vector3((cell.x + 0.5f) * size, 0f, (cell.y + 0.5f) * size);
        }

        public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public static void CollectResidentRing(Vector2Int centerCell, int radius, List<Vector2Int> outCells)
        {
            if (outCells == null) return;
            outCells.Clear();
            int r = Mathf.Max(0, radius);
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    outCells.Add(new Vector2Int(centerCell.x + dx, centerCell.y + dz));
                }
            }
        }
    }
}
