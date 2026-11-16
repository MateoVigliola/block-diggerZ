using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// One clear step once the board has settled: the color matches to remove, the blocks their blasts remove
    /// (<see cref="MatchBlast"/>) and what the matches score.
    /// </summary>
    public sealed class ClearStep
    {
        /// <summary>Every cell this step removes, matched first, then blasted (the views flash them).</summary>
        public List<Vector2Int> Cells { get; } = new();

        /// <summary>The straight runs that matched.</summary>
        public List<ColorRun> Runs { get; } = new();

        /// <summary>Scored color blocks; blocks in a cross count once per run.</summary>
        public int ScoredBlocks { get; internal set; }

        /// <summary>Matched blocks removed (each cell once).</summary>
        public int MatchedBlocks { get; internal set; }

        /// <summary>Blocks removed by blasts, not part of a match. They don't score.</summary>
        public int BlastedBlocks { get; internal set; }

        /// <summary>Points this step gives.</summary>
        public int Score { get; internal set; }

        internal void Reset()
        {
            Cells.Clear();
            Runs.Clear();
            ScoredBlocks = 0;
            MatchedBlocks = 0;
            BlastedBlocks = 0;
            Score = 0;
        }
    }
}
