using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Board size limits and placement. Sizes are clamped so every piece can spawn and rotate and the board stays
    /// readable; the board is placed so its visible area is centered on a point (the camera's center).
    /// </summary>
    public static class BoardLayout
    {
        /// <summary>Fewest columns a board can have (the I piece needs 4).</summary>
        public const int MinWidth = 4;

        /// <summary>Most columns a board can have.</summary>
        public const int MaxWidth = 30;

        /// <summary>Fewest visible rows a board can have.</summary>
        public const int MinHeight = 8;

        /// <summary>Most visible rows a board can have.</summary>
        public const int MaxHeight = 40;

        /// <summary>Clamps a board size to the limits above.</summary>
        public static Vector2Int ClampSize(int width, int height)
        {
            return new Vector2Int(Mathf.Clamp(width, MinWidth, MaxWidth), Mathf.Clamp(height, MinHeight, MaxHeight));
        }

        /// <summary>
        /// World position of the board's bottom-left corner so that its <paramref name="width"/> x
        /// <paramref name="visibleHeight"/> visible cells are centered on <paramref name="center"/>.
        /// </summary>
        public static Vector2 CenteredOrigin(Vector2 center, int width, int visibleHeight, float cellSize)
        {
            return center - new Vector2(width, visibleHeight) * (cellSize * 0.5f);
        }
    }
}
