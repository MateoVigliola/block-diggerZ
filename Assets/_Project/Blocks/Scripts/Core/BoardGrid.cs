using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// The board's locked cells. Column x runs left to right from 0, row y bottom to top from 0. Each filled cell
    /// keeps the <see cref="PieceDefinition"/> it came from (so its tags survive locking) and its
    /// <see cref="BlockColor"/>.
    /// </summary>
    public sealed class BoardGrid
    {
        private readonly PieceDefinition[] _cells;
        private readonly BlockColor[] _colors;

        /// <summary>Creates an empty board.</summary>
        public BoardGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new PieceDefinition[width * height];
            _colors = new BlockColor[width * height];
        }

        /// <summary>Columns.</summary>
        public int Width { get; }

        /// <summary>Rows, including any hidden rows above the visible board.</summary>
        public int Height { get; }

        /// <summary>True if the cell is on the board.</summary>
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>The piece that filled a cell, or null if it is empty or off the board.</summary>
        public PieceDefinition Get(int x, int y)
        {
            return IsInside(x, y) ? _cells[Index(x, y)] : null;
        }

        /// <summary>The color of a cell's block, or null if it is empty, unpainted or off the board.</summary>
        public BlockColor GetColor(int x, int y)
        {
            return IsInside(x, y) ? _colors[Index(x, y)] : null;
        }

        /// <summary>True if the cell is filled.</summary>
        public bool IsOccupied(int x, int y)
        {
            return Get(x, y) != null;
        }

        /// <summary>Fills or empties one cell (an unpainted block when no color is given).</summary>
        public void Set(int x, int y, PieceDefinition piece, BlockColor color = null)
        {
            if (IsInside(x, y))
            {
                int index = Index(x, y);
                _cells[index] = piece;
                _colors[index] = piece != null ? color : null;
            }
        }

        /// <summary>True if every cell, shifted by <paramref name="offset"/>, is on the board and empty.</summary>
        public bool Fits(IReadOnlyList<Vector2Int> cells, Vector2Int offset)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                int x = cells[i].x + offset.x;
                int y = cells[i].y + offset.y;
                if (!IsInside(x, y) || _cells[Index(x, y)] != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Where cells at <paramref name="offset"/> come to rest when dropped straight down: the lowest offset in the
        /// same column that still fits. Returns <paramref name="offset"/> itself if it can't move down.
        /// </summary>
        public Vector2Int LandingOffset(IReadOnlyList<Vector2Int> cells, Vector2Int offset)
        {
            Vector2Int landing = offset;
            while (Fits(cells, landing + Vector2Int.down))
            {
                landing += Vector2Int.down;
            }

            return landing;
        }

        /// <summary>
        /// Fills the cells, shifted by <paramref name="offset"/>, with <paramref name="piece"/>. Cell i gets color i
        /// of <paramref name="colors"/> when given.
        /// </summary>
        public void Place(IReadOnlyList<Vector2Int> cells, Vector2Int offset, PieceDefinition piece,
            IReadOnlyList<BlockColor> colors = null)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                BlockColor color = colors != null && i < colors.Count ? colors[i] : null;
                Set(cells[i].x + offset.x, cells[i].y + offset.y, piece, color);
            }
        }

        /// <summary>Empties the given cells. Blocks above are left in place; <see cref="BlockGravity"/> drops them.</summary>
        public void Remove(IReadOnlyList<Vector2Int> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Set(cells[i].x, cells[i].y, null);
            }
        }

        /// <summary>Empties the whole board.</summary>
        public void Clear()
        {
            System.Array.Clear(_cells, 0, _cells.Length);
            System.Array.Clear(_colors, 0, _colors.Length);
        }

        private int Index(int x, int y)
        {
            return y * Width + x;
        }
    }
}
