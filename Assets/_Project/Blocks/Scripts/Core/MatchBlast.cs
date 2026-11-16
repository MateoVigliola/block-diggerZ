using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Color matches blast past their ends: a row match removes the cells beyond both its left and right end, a
    /// column match the cells below its bottom. Reach grows with the match length
    /// (<see cref="BlastSettings.Reach"/>; by default 3 blocks reach 1 cell, 4 reach 2, and so on). Reach counts
    /// cells, so empty cells use it up, and it stops at the board edge. Every block in reach is removed, whatever
    /// its color, Dirt included.
    /// </summary>
    public static class MatchBlast
    {
        /// <summary>
        /// Adds every occupied cell in reach of <paramref name="runs"/> that isn't already in
        /// <paramref name="cells"/> to it, and returns how many were added.
        /// </summary>
        public static int Add(BoardGrid board, IReadOnlyList<ColorRun> runs, int minLength, BlastSettings settings,
            List<Vector2Int> cells)
        {
            var taken = new HashSet<Vector2Int>(cells);
            int added = 0;
            for (int i = 0; i < runs.Count; i++)
            {
                ColorRun run = runs[i];
                int reach = settings.Reach(run.Length, minLength, run.Horizontal);
                if (run.Horizontal)
                {
                    added += AddLine(board, run.Start, Vector2Int.left, reach, taken, cells);
                    added += AddLine(board, run.End, Vector2Int.right, reach, taken, cells);
                }
                else
                {
                    added += AddLine(board, run.Start, Vector2Int.down, reach, taken, cells);
                }
            }

            return added;
        }

        private static int AddLine(BoardGrid board, Vector2Int from, Vector2Int step, int reach,
            HashSet<Vector2Int> taken, List<Vector2Int> cells)
        {
            int added = 0;
            for (int i = 1; i <= reach; i++)
            {
                Vector2Int cell = from + step * i;
                if (!board.IsInside(cell.x, cell.y))
                {
                    break;
                }

                if (board.IsOccupied(cell.x, cell.y) && taken.Add(cell))
                {
                    cells.Add(cell);
                    added++;
                }
            }

            return added;
        }
    }
}
