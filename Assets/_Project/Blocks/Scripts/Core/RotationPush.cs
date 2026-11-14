using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Finds where to push a rotated piece that overlaps a wall, the floor or locked cells. Offsets are tried in a
    /// fixed order and the first that fits wins: no push, then sideways (1 away from what blocks, 1 the other way,
    /// then 2 away, 2 the other way, up to the sideways limit), then each row up (straight up, then 1 away, then
    /// 1 the other way). Never pushes down.
    /// </summary>
    public static class RotationPush
    {
        /// <summary>How far sideways a push that also moves up may go.</summary>
        public const int SidewaysWhenPushingUp = 1;

        private const int Right = 1;
        private const int Left = -1;

        /// <summary>
        /// Returns true and the position to use if <paramref name="cells"/> fit at <paramref name="position"/> or at a
        /// pushed position within the limits.
        /// </summary>
        public static bool TryFind(BoardGrid board, IReadOnlyList<Vector2Int> cells, Vector2Int position,
            int maxSideways, int maxUp, out Vector2Int result)
        {
            result = position;
            if (board.Fits(cells, position))
            {
                return true;
            }

            int away = AwayDirection(board, cells, position);
            for (int distance = 1; distance <= maxSideways; distance++)
            {
                if (TryOffset(board, cells, position, new Vector2Int(away * distance, 0), ref result)
                    || TryOffset(board, cells, position, new Vector2Int(-away * distance, 0), ref result))
                {
                    return true;
                }
            }

            int upSideways = Mathf.Min(SidewaysWhenPushingUp, maxSideways);
            for (int up = 1; up <= maxUp; up++)
            {
                if (TryOffset(board, cells, position, new Vector2Int(0, up), ref result))
                {
                    return true;
                }

                for (int distance = 1; distance <= upSideways; distance++)
                {
                    if (TryOffset(board, cells, position, new Vector2Int(away * distance, up), ref result)
                        || TryOffset(board, cells, position, new Vector2Int(-away * distance, up), ref result))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Direction to push first: away from the side where the blocked cells are. Compares the blocked cells'
        /// columns with the piece's center column; right when it can't tell.
        /// </summary>
        public static int AwayDirection(BoardGrid board, IReadOnlyList<Vector2Int> cells, Vector2Int position)
        {
            float center = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                center += cells[i].x;
            }

            center = center / Mathf.Max(1, cells.Count) + position.x;

            float blockedSide = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                int x = cells[i].x + position.x;
                int y = cells[i].y + position.y;
                if (!board.IsInside(x, y) || board.IsOccupied(x, y))
                {
                    blockedSide += x - center;
                }
            }

            return blockedSide > 0f ? Left : Right;
        }

        private static bool TryOffset(BoardGrid board, IReadOnlyList<Vector2Int> cells, Vector2Int position,
            Vector2Int offset, ref Vector2Int result)
        {
            Vector2Int candidate = position + offset;
            if (!board.Fits(cells, candidate))
            {
                return false;
            }

            result = candidate;
            return true;
        }
    }
}
