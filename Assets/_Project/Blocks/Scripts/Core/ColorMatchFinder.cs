using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Finds color matches: runs of at least a minimum length of same-colored blocks in a straight row or column
    /// (bends such as L shapes don't count). Every block of a long run is part of the match. A block in both a row
    /// run and a column run (a cross) is removed once but scores for each run.
    /// </summary>
    public static class ColorMatchFinder
    {
        /// <summary>
        /// Writes each matched cell once into <paramref name="cells"/> and returns the number of scored blocks (the
        /// summed length of every run, so crossing blocks count twice). Unpainted blocks never match, nor do cells
        /// marked in <paramref name="excluded"/> (indexed y * width + x; falling blocks).
        /// </summary>
        public static int Find(BoardGrid board, int minLength, List<Vector2Int> cells, bool[] excluded = null)
        {
            cells.Clear();
            var marked = new bool[board.Width, board.Height];
            int scored = 0;

            for (int y = 0; y < board.Height; y++)
            {
                int runStart = 0;
                for (int x = 1; x <= board.Width; x++)
                {
                    if (x < board.Width && SameColor(board, excluded, runStart, y, x, y))
                    {
                        continue;
                    }

                    scored += MarkRun(board, excluded, minLength, cells, marked, runStart, y, x - runStart, Vector2Int.right);
                    runStart = x;
                }
            }

            for (int x = 0; x < board.Width; x++)
            {
                int runStart = 0;
                for (int y = 1; y <= board.Height; y++)
                {
                    if (y < board.Height && SameColor(board, excluded, x, runStart, x, y))
                    {
                        continue;
                    }

                    scored += MarkRun(board, excluded, minLength, cells, marked, x, runStart, y - runStart, Vector2Int.up);
                    runStart = y;
                }
            }

            return scored;
        }

        private static BlockColor ColorAt(BoardGrid board, bool[] excluded, int x, int y)
        {
            bool skip = excluded != null && excluded[y * board.Width + x];
            return skip ? null : board.GetColor(x, y);
        }

        private static bool SameColor(BoardGrid board, bool[] excluded, int ax, int ay, int bx, int by)
        {
            BlockColor a = ColorAt(board, excluded, ax, ay);
            return a != null && a == ColorAt(board, excluded, bx, by);
        }

        private static int MarkRun(BoardGrid board, bool[] excluded, int minLength, List<Vector2Int> cells, bool[,] marked,
            int startX, int startY, int length, Vector2Int step)
        {
            if (length < minLength || ColorAt(board, excluded, startX, startY) == null)
            {
                return 0;
            }

            for (int i = 0; i < length; i++)
            {
                int x = startX + step.x * i;
                int y = startY + step.y * i;
                if (!marked[x, y])
                {
                    marked[x, y] = true;
                    cells.Add(new Vector2Int(x, y));
                }
            }

            return length;
        }
    }
}
