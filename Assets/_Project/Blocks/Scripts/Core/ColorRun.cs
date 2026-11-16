using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>One straight color match found by <see cref="ColorMatchFinder"/>.</summary>
    public readonly struct ColorRun
    {
        /// <summary>Creates a run.</summary>
        public ColorRun(Vector2Int start, int length, bool horizontal)
        {
            Start = start;
            Length = length;
            Horizontal = horizontal;
        }

        /// <summary>Leftmost cell of a row run, or bottom cell of a column run.</summary>
        public Vector2Int Start { get; }

        /// <summary>Blocks in the run.</summary>
        public int Length { get; }

        /// <summary>True for a row run, false for a column run.</summary>
        public bool Horizontal { get; }

        /// <summary>Rightmost cell of a row run, or top cell of a column run.</summary>
        public Vector2Int End => Start + (Horizontal ? Vector2Int.right : Vector2Int.up) * (Length - 1);
    }
}
