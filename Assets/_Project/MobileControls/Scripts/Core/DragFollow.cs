using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Rules for moving the falling piece under a dragging finger. Columns are measured in cells from the board's
    /// left edge, as floats, so a piece's center can sit between two columns (even-width pieces).
    /// </summary>
    public static class DragFollow
    {
        /// <summary>Distance in cells the piece center may be from the finger before it steps toward it.</summary>
        public const float StepThreshold = 0.5f;

        /// <summary>
        /// The finger's position across the board in cells (0 at the left edge, <paramref name="boardWidth"/> at
        /// the right edge), clamped to the board.
        /// </summary>
        public static float FingerCell(float fingerX, Rect boardRect, int boardWidth)
        {
            if (boardRect.width <= 0f || boardWidth <= 0)
            {
                return 0f;
            }

            float cell = (fingerX - boardRect.xMin) / boardRect.width * boardWidth;
            return Mathf.Clamp(cell, 0f, boardWidth);
        }

        /// <summary>
        /// Horizontal center of the piece in cells: halfway between the left edge of its leftmost block and the
        /// right edge of its rightmost block. <paramref name="cells"/> are relative to <paramref name="positionX"/>.
        /// </summary>
        public static float PieceCenter(IReadOnlyList<Vector2Int> cells, int positionX)
        {
            if (cells == null || cells.Count == 0)
            {
                return positionX;
            }

            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (Vector2Int cell in cells)
            {
                min = Mathf.Min(min, cell.x);
                max = Mathf.Max(max, cell.x);
            }

            return positionX + (min + max + 1) * 0.5f;
        }

        /// <summary>
        /// One column step toward the finger: +1 right, -1 left, 0 when the piece center is within
        /// <see cref="StepThreshold"/> of it. After a step the gap is at most the threshold, so the piece never
        /// jitters between two columns.
        /// </summary>
        public static int StepDirection(float fingerCell, float pieceCenter)
        {
            float gap = fingerCell - pieceCenter;
            if (gap > StepThreshold)
            {
                return 1;
            }

            return gap < -StepThreshold ? -1 : 0;
        }
    }
}
