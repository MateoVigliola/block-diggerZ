using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Paints a piece's blocks. A valid painting uses between <see cref="MinColorsPerPiece"/> and
    /// <see cref="MaxColorsPerPiece"/> different colors, no color more than <see cref="MaxSameColor"/> times, and
    /// blocks sharing a color touch each other (side by side, not diagonally). For four blocks that means a 2+2 or
    /// 2+1+1 split. <see cref="Paint{T}"/> picks uniformly among every valid painting.
    /// </summary>
    public static class PiecePainter
    {
        /// <summary>Fewest different colors on one piece.</summary>
        public const int MinColorsPerPiece = 2;

        /// <summary>Most different colors on one piece.</summary>
        public const int MaxColorsPerPiece = 3;

        /// <summary>Most blocks of one color on one piece.</summary>
        public const int MaxSameColor = 2;

        /// <summary>
        /// Writes one color per cell into <paramref name="result"/>, chosen uniformly among all valid paintings.
        /// Returns false if none exists (for example too few colors); <paramref name="result"/> then cycles through
        /// the colors so the piece is still drawable.
        /// </summary>
        public static bool Paint<T>(IReadOnlyList<Vector2Int> cells, IReadOnlyList<T> colors, System.Random random, List<T> result)
        {
            result.Clear();
            if (cells.Count == 0 || colors.Count == 0)
            {
                return false;
            }

            var current = new int[cells.Count];
            var chosen = new int[cells.Count];
            var counts = new int[colors.Count];
            int found = 0;
            Search(0);

            for (int i = 0; i < cells.Count; i++)
            {
                result.Add(colors[found > 0 ? chosen[i] : i % colors.Count]);
            }

            return found > 0;

            void Search(int cell)
            {
                if (cell == cells.Count)
                {
                    if (!IsValidIndices(cells, current, colors.Count))
                    {
                        return;
                    }

                    found++;
                    if (random.Next(found) == 0)
                    {
                        Array.Copy(current, chosen, current.Length);
                    }

                    return;
                }

                for (int c = 0; c < colors.Count; c++)
                {
                    if (counts[c] >= MaxSameColor)
                    {
                        continue;
                    }

                    counts[c]++;
                    current[cell] = c;
                    Search(cell + 1);
                    counts[c]--;
                }
            }
        }

        /// <summary>True if <paramref name="paint"/> (one color per cell) follows the painting rules.</summary>
        public static bool IsValid<T>(IReadOnlyList<Vector2Int> cells, IReadOnlyList<T> paint)
        {
            var distinct = new List<T>();
            var indices = new int[paint.Count];
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < paint.Count; i++)
            {
                int index = distinct.FindIndex(c => comparer.Equals(c, paint[i]));
                if (index < 0)
                {
                    index = distinct.Count;
                    distinct.Add(paint[i]);
                }

                indices[i] = index;
            }

            return IsValidIndices(cells, indices, distinct.Count);
        }

        private static bool IsValidIndices(IReadOnlyList<Vector2Int> cells, int[] paint, int colorCount)
        {
            int used = 0;
            for (int c = 0; c < colorCount; c++)
            {
                int count = 0;
                for (int i = 0; i < paint.Length; i++)
                {
                    if (paint[i] == c)
                    {
                        count++;
                    }
                }

                if (count == 0)
                {
                    continue;
                }

                used++;
                if (count > MaxSameColor || !IsConnected(cells, paint, c))
                {
                    return false;
                }
            }

            return used >= MinColorsPerPiece && used <= MaxColorsPerPiece;
        }

        private static bool IsConnected(IReadOnlyList<Vector2Int> cells, int[] paint, int color)
        {
            var reached = new bool[cells.Count];
            var open = new Stack<int>();
            int total = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (paint[i] != color)
                {
                    continue;
                }

                total++;
                if (total == 1)
                {
                    reached[i] = true;
                    open.Push(i);
                }
            }

            int visited = 0;
            while (open.Count > 0)
            {
                int i = open.Pop();
                visited++;
                for (int j = 0; j < cells.Count; j++)
                {
                    if (!reached[j] && paint[j] == color && IsNeighbor(cells[i], cells[j]))
                    {
                        reached[j] = true;
                        open.Push(j);
                    }
                }
            }

            return visited == total;
        }

        private static bool IsNeighbor(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }
    }
}
