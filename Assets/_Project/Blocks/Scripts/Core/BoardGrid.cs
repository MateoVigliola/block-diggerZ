using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// The board's locked cells. Column x runs left to right from 0, row y bottom to top from 0. Each filled cell
    /// keeps the <see cref="PieceDefinition"/> it came from, so its color and tags survive locking.
    /// </summary>
    public sealed class BoardGrid
    {
        private readonly PieceDefinition[] _cells;

        /// <summary>Creates an empty board.</summary>
        public BoardGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new PieceDefinition[width * height];
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

        /// <summary>True if the cell is filled.</summary>
        public bool IsOccupied(int x, int y)
        {
            return Get(x, y) != null;
        }

        /// <summary>Fills or empties one cell.</summary>
        public void Set(int x, int y, PieceDefinition piece)
        {
            if (IsInside(x, y))
            {
                _cells[Index(x, y)] = piece;
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

        /// <summary>Fills the cells, shifted by <paramref name="offset"/>, with <paramref name="piece"/>.</summary>
        public void Place(IReadOnlyList<Vector2Int> cells, Vector2Int offset, PieceDefinition piece)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Set(cells[i].x + offset.x, cells[i].y + offset.y, piece);
            }
        }

        /// <summary>True if every cell in the row is filled.</summary>
        public bool IsRowFull(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                if (_cells[Index(x, y)] == null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Removes every full row, moves the rows above down, and returns how many were removed.</summary>
        public int ClearFullRows()
        {
            int cleared = 0;
            int write = 0;
            for (int read = 0; read < Height; read++)
            {
                if (IsRowFull(read))
                {
                    cleared++;
                    continue;
                }

                if (write != read)
                {
                    CopyRow(read, write);
                }

                write++;
            }

            for (int y = write; y < Height; y++)
            {
                ClearRow(y);
            }

            return cleared;
        }

        /// <summary>Empties the whole board.</summary>
        public void Clear()
        {
            System.Array.Clear(_cells, 0, _cells.Length);
        }

        private void CopyRow(int from, int to)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[Index(x, to)] = _cells[Index(x, from)];
            }
        }

        private void ClearRow(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                _cells[Index(x, y)] = null;
            }
        }

        private int Index(int x, int y)
        {
            return y * Width + x;
        }
    }
}
