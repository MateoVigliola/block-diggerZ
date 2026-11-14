using System.Collections.Generic;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Bag randomizer: deals every piece of the set once in a shuffled order, then refills and shuffles again,
    /// so droughts of one piece are short.
    /// </summary>
    public sealed class PieceBag
    {
        private readonly IReadOnlyList<PieceDefinition> _pieces;
        private readonly List<PieceDefinition> _queue = new();
        private System.Random _random;

        /// <summary>Creates a bag over <paramref name="pieces"/> with a fixed seed.</summary>
        public PieceBag(IReadOnlyList<PieceDefinition> pieces, int seed)
        {
            _pieces = pieces;
            Reset(seed);
        }

        /// <summary>Empties the bag and restarts the random sequence from <paramref name="seed"/>.</summary>
        public void Reset(int seed)
        {
            _random = new System.Random(seed);
            _queue.Clear();
        }

        /// <summary>The piece <see cref="Next"/> will return, without taking it.</summary>
        public PieceDefinition Peek()
        {
            EnsureFilled();
            return _queue[0];
        }

        /// <summary>Takes the next piece.</summary>
        public PieceDefinition Next()
        {
            EnsureFilled();
            PieceDefinition piece = _queue[0];
            _queue.RemoveAt(0);
            return piece;
        }

        private void EnsureFilled()
        {
            if (_queue.Count > 0)
            {
                return;
            }

            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] != null)
                {
                    _queue.Add(_pieces[i]);
                }
            }

            for (int i = _queue.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (_queue[i], _queue[j]) = (_queue[j], _queue[i]);
            }
        }
    }
}
