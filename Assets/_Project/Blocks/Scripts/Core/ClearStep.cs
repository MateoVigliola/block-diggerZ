using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>One clear step once the board has settled: the color matches to remove and what they score.</summary>
    public sealed class ClearStep
    {
        /// <summary>Every cell this step removes (the views flash them).</summary>
        public List<Vector2Int> Cells { get; } = new();

        /// <summary>Scored color blocks; blocks in a cross count once per run.</summary>
        public int ScoredBlocks { get; internal set; }

        /// <summary>Points this step gives.</summary>
        public int Score { get; internal set; }

        internal void Reset()
        {
            Cells.Clear();
            ScoredBlocks = 0;
            Score = 0;
        }
    }
}
