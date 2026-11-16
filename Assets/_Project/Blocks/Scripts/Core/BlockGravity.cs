using System.Collections.Generic;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Gravity for locked blocks. Each block is either resting or falling; the state is kept between steps in a
    /// bool array indexed y * width + x (true = falling). Touching blocks of the same color and the same state form a
    /// group (an unpainted or obstacle block is a group of its own), so a falling group never joins a resting group of its color
    /// on the way down. A group rests if any of its blocks is on the floor or directly on a resting block of another
    /// group; every other group falls, including groups on top of falling groups. A falling group that lands this
    /// way becomes resting.
    /// </summary>
    public static class BlockGravity
    {
        /// <summary>Group id of an empty cell.</summary>
        public const int NoGroup = -1;

        /// <summary>
        /// Labels every block with its group id (0, 1, 2 ...) in <paramref name="groups"/> (indexed y * width + x,
        /// <see cref="NoGroup"/> for empty cells) and returns the number of groups. With <paramref name="falling"/>,
        /// blocks only group with blocks of the same state.
        /// </summary>
        public static int FindGroups(BoardGrid board, int[] groups, bool[] falling = null)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = NoGroup;
            }

            int count = 0;
            var open = new Stack<int>();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int index = Index(board, x, y);
                    if (!board.IsOccupied(x, y) || groups[index] != NoGroup)
                    {
                        continue;
                    }

                    int id = count++;
                    groups[index] = id;
                    BlockColor color = board.GetColor(x, y);
                    if (!BlockColor.CanMatch(color))
                    {
                        continue;
                    }

                    bool state = falling != null && falling[index];
                    open.Push(index);
                    while (open.Count > 0)
                    {
                        int cell = open.Pop();
                        int cx = cell % board.Width;
                        int cy = cell / board.Width;
                        Join(board, groups, falling, open, id, color, state, cx - 1, cy);
                        Join(board, groups, falling, open, id, color, state, cx + 1, cy);
                        Join(board, groups, falling, open, id, color, state, cx, cy - 1);
                        Join(board, groups, falling, open, id, color, state, cx, cy + 1);
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Updates <paramref name="falling"/> (indexed y * width + x, kept between steps): resting groups that lost
        /// their support start falling, falling groups that reached support land. Returns true if any block falls.
        /// Start with an all-false array; a freshly locked piece is resting until this says otherwise.
        /// </summary>
        public static bool UpdateFalling(BoardGrid board, bool[] falling)
        {
            for (int i = 0; i < falling.Length; i++)
            {
                if (falling[i] && !board.IsOccupied(i % board.Width, i / board.Width))
                {
                    falling[i] = false;
                }
            }

            var groups = new int[board.Width * board.Height];
            int count = FindGroups(board, groups, falling);
            var supported = new bool[count];
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        int group = groups[Index(board, x, y)];
                        if (group == NoGroup || supported[group] || !RestsOnSomething(board, groups, supported, x, y, group))
                        {
                            continue;
                        }

                        supported[group] = true;
                        changed = true;
                    }
                }
            }

            bool any = false;
            for (int i = 0; i < falling.Length; i++)
            {
                falling[i] = groups[i] != NoGroup && !supported[groups[i]];
                any |= falling[i];
            }

            return any;
        }

        /// <summary>
        /// Moves every block marked in <paramref name="falling"/> down one row, carrying its state with it. Call
        /// <see cref="UpdateFalling"/> first.
        /// </summary>
        public static void Step(BoardGrid board, bool[] falling)
        {
            for (int y = 1; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int from = Index(board, x, y);
                    if (!falling[from])
                    {
                        continue;
                    }

                    int to = Index(board, x, y - 1);
                    board.Set(x, y - 1, board.Get(x, y), board.GetColor(x, y));
                    board.Set(x, y, null);
                    falling[to] = true;
                    falling[from] = false;
                }
            }
        }

        /// <summary>Lets everything fall until it rests; returns the number of steps. Tests and instant settling.</summary>
        public static int Settle(BoardGrid board)
        {
            var falling = new bool[board.Width * board.Height];
            int steps = 0;
            while (UpdateFalling(board, falling))
            {
                Step(board, falling);
                steps++;
            }

            return steps;
        }

        private static bool RestsOnSomething(BoardGrid board, int[] groups, bool[] supported, int x, int y, int group)
        {
            if (y == 0)
            {
                return true;
            }

            int below = groups[Index(board, x, y - 1)];
            return below != NoGroup && below != group && supported[below];
        }

        private static void Join(BoardGrid board, int[] groups, bool[] falling, Stack<int> open, int id, BlockColor color,
            bool state, int x, int y)
        {
            if (!board.IsInside(x, y))
            {
                return;
            }

            int index = Index(board, x, y);
            bool sameState = (falling != null && falling[index]) == state;
            if (groups[index] != NoGroup || board.GetColor(x, y) != color || !sameState)
            {
                return;
            }

            groups[index] = id;
            open.Push(index);
        }

        private static int Index(BoardGrid board, int x, int y) => y * board.Width + x;
    }
}
