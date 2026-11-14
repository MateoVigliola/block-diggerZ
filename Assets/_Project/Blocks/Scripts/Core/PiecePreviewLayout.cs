using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Places a piece's spawn-orientation cells centered in a square preview grid (the HUD's next and store
    /// boxes). Positions are in grid cells from the top-left corner, as UI Toolkit lays out from the top.
    /// </summary>
    public static class PiecePreviewLayout
    {
        /// <summary>Preview grid side in cells; fits every standard piece.</summary>
        public const int GridSize = 4;

        /// <summary>
        /// Writes the top-left position (in cells, may be half cells) of each cell of <paramref name="cells"/>,
        /// centered in the grid, into <paramref name="result"/>.
        /// </summary>
        public static void Layout(IReadOnlyList<Vector2Int> cells, List<Vector2> result)
        {
            result.Clear();
            if (cells.Count == 0)
            {
                return;
            }

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            for (int i = 0; i < cells.Count; i++)
            {
                minX = Mathf.Min(minX, cells[i].x);
                maxX = Mathf.Max(maxX, cells[i].x);
                minY = Mathf.Min(minY, cells[i].y);
                maxY = Mathf.Max(maxY, cells[i].y);
            }

            float offsetX = (GridSize - (maxX - minX + 1)) * 0.5f;
            float offsetY = (GridSize - (maxY - minY + 1)) * 0.5f;
            for (int i = 0; i < cells.Count; i++)
            {
                float left = cells[i].x - minX + offsetX;
                float top = maxY - cells[i].y + offsetY;
                result.Add(new Vector2(left, top));
            }
        }
    }
}
