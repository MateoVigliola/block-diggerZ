namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Finds and removes color matches among resting blocks and scores them (full rows don't clear; falling blocks
    /// never match). Clearing comes before falling: after every change the caller first looks for a match, and only
    /// when there is none lets falling blocks drop a row (<see cref="BlockGravity"/>). Removals can make blocks
    /// fall and landings can make new matches (a chain).
    /// </summary>
    public sealed class ClearResolver
    {
        private readonly int _colorBlockScore;
        private readonly int _minColorMatch;

        /// <summary>Creates a resolver with the score per block and minimum color run length.</summary>
        public ClearResolver(int colorBlockScore, int minColorMatch)
        {
            _colorBlockScore = colorBlockScore;
            _minColorMatch = minColorMatch;
        }

        /// <summary>
        /// Fills <paramref name="step"/> with the color matches among blocks not marked in <paramref name="falling"/>;
        /// returns false if there are none.
        /// </summary>
        public bool TryFindStep(BoardGrid board, ClearStep step, bool[] falling = null)
        {
            step.Reset();
            int scored = ColorMatchFinder.Find(board, _minColorMatch, step.Cells, falling);
            if (scored == 0)
            {
                return false;
            }

            step.ScoredBlocks = scored;
            step.Score = scored * _colorBlockScore;
            return true;
        }

        /// <summary>Removes what <paramref name="step"/> found from the board (without dropping anything).</summary>
        public void Apply(BoardGrid board, ClearStep step)
        {
            board.Remove(step.Cells);
        }

        /// <summary>
        /// Runs the whole loop at once (update falling, clear a match if there is one, otherwise drop falling blocks a
        /// row) until nothing falls or matches, and returns the total score. <paramref name="steps"/> counts clear
        /// steps. Tests and instant resolving.
        /// </summary>
        public int ResolveAll(BoardGrid board, out int steps)
        {
            var step = new ClearStep();
            var falling = new bool[board.Width * board.Height];
            int score = 0;
            steps = 0;
            while (true)
            {
                bool anyFalling = BlockGravity.UpdateFalling(board, falling);
                if (TryFindStep(board, step, falling))
                {
                    Apply(board, step);
                    score += step.Score;
                    steps++;
                    continue;
                }

                if (!anyFalling)
                {
                    return score;
                }

                BlockGravity.Step(board, falling);
            }
        }
    }
}
