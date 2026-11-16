using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Picks how many visible rows the board gets on the touch layout so that, fitted to the full width of the
    /// board area, the whole board still fits its height.
    /// </summary>
    public static class MobileBoardRows
    {
        private const float RoundingTolerance = 0.0001f;

        /// <summary>
        /// Rows that fit in an <paramref name="areaWidth"/> x <paramref name="areaHeight"/> area when
        /// <paramref name="columns"/> columns plus <paramref name="paddingCells"/> on each side span its width, the
        /// same padding kept above and below. Clamped to <paramref name="minRows"/>..<paramref name="maxRows"/>.
        /// Returns <paramref name="maxRows"/> when any size is not positive.
        /// </summary>
        public static int Fit(float areaWidth, float areaHeight, int columns, float paddingCells, int minRows, int maxRows)
        {
            int max = Mathf.Max(minRows, maxRows);
            if (areaWidth <= 0f || areaHeight <= 0f || columns <= 0)
            {
                return max;
            }

            float padding = Mathf.Max(0f, paddingCells);
            float cellSize = areaWidth / (columns + 2f * padding);
            int rows = Mathf.FloorToInt(areaHeight / cellSize - 2f * padding + RoundingTolerance);
            return Mathf.Clamp(rows, minRows, max);
        }
    }
}
