using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Board size and timing for the block dropper. Defaults follow common guideline values (10 x 20 board,
    /// 1 second gravity at level 1, soft drop 20 times faster, 0.5 second lock delay).
    /// </summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Blocks Rules", fileName = "BlocksRules")]
    public sealed class BlocksRules : ScriptableObject
    {
        [field: Header("Board")]
        [field: SerializeField, Min(4), Tooltip("Columns on the board.")]
        public int BoardWidth { get; private set; } = 10;

        [field: SerializeField, Min(4), Tooltip("Visible rows on the board.")]
        public int VisibleHeight { get; private set; } = 20;

        [field: SerializeField, Min(0), Tooltip("Hidden rows above the visible board, so pieces can rotate at the top.")]
        public int BufferRows { get; private set; } = 2;

        [field: Header("Falling")]
        [field: SerializeField, Min(0.01f), Tooltip("Seconds per row the piece falls on its own.")]
        public float GravityInterval { get; private set; } = 1f;

        [field: SerializeField, Min(0.01f), Tooltip("How many times faster than the piece placed blocks with space below fall. 2 means twice as fast.")]
        public float PlacedFallSpeedMultiplier { get; private set; } = 2f;

        [field: SerializeField, Min(0.005f), Tooltip("Seconds per row while soft drop is held.")]
        public float SoftDropInterval { get; private set; } = 0.05f;

        [field: SerializeField, Min(0f), Tooltip("Seconds a piece can rest on the stack before it locks. Restarts when it reaches a lower row than before.")]
        public float LockDelay { get; private set; } = 0.5f;

        [field: SerializeField, Min(0f), Tooltip("Seconds a sideways move or rotation gives back while the piece rests. The time left never exceeds Lock Delay.")]
        public float LockExtensionBonus { get; private set; } = 0.25f;

        [field: SerializeField, Min(0), Tooltip("How many times per piece a move or rotation can extend the lock delay. Restarts when the piece reaches a lower row.")]
        public int MaxLockExtensions { get; private set; } = 15;

        [field: Header("Rotation push")]
        [field: SerializeField, Min(0), Tooltip("How many cells sideways a blocked rotation may push the piece.")]
        public int MaxSidewaysPush { get; private set; } = 2;

        [field: SerializeField, Min(0), Tooltip("How many cells up a blocked rotation may push the piece. Pieces are never pushed down.")]
        public int MaxUpPush { get; private set; } = 1;

        [field: Header("Sideways movement")]
        [field: SerializeField, Min(0f), Tooltip("Seconds a move key must be held before the piece starts sliding on its own.")]
        public float AutoShiftDelay { get; private set; } = 0.167f;

        [field: SerializeField, Min(0.005f), Tooltip("Seconds between moves while sliding.")]
        public float AutoRepeatInterval { get; private set; } = 0.033f;

        [field: Header("Colors")]
        [field: SerializeField, Min(PiecePainter.MaxColorsPerPiece), Tooltip("How many palette colors a run uses (the first N of the Block Palette). Fewer colors make matches more frequent. A difficulty mode will set this later.")]
        public int ColorCount { get; private set; } = 4;

        [field: SerializeField, Min(2), Tooltip("Shortest straight row or column of same-colored blocks that gets deleted.")]
        public int MinColorMatch { get; private set; } = 3;

        [field: SerializeField, Min(0f), Tooltip("Seconds matched blocks flash before they are removed, per clear step (each chain step flashes again). 0 clears instantly.")]
        public float ClearStepDuration { get; private set; } = 0.25f;

        [field: Header("Speed-up")]
        [field: SerializeField, Min(0), Tooltip("Points per speed-up step. Every full step adds Speed Up Per Step to the falling piece's speed multiplier (the fall interval is divided by it). 0 turns the speed-up off.")]
        public int SpeedUpScoreStep { get; private set; } = 200;

        [field: SerializeField, Min(0f), Tooltip("Flat amount added to the falling piece's speed multiplier per Speed Up Score Step points (0.1: 1.0, 1.1, 1.2 ...). Settling blocks are not affected.")]
        public float SpeedUpPerStep { get; private set; } = 0.1f;

        [field: SerializeField, Min(0f), Tooltip("Highest speed multiplier the speed-up can reach. 0 means no cap.")]
        public float MaxSpeedMultiplier { get; private set; }

        [field: Header("Match blasts")]
        [field: SerializeField, Min(0), Tooltip("Cells a shortest row match (Min Color Match blocks) blasts past each end, left and right. Blasted blocks are removed whatever they are, Dirt included, and don't score. Empty cells use up reach.")]
        public int HorizontalBlastBase { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Extra cells a row match blasts on each side per block beyond the shortest match (with 1: 3 blocks reach 1, 4 reach 2, 5 reach 3).")]
        public int HorizontalBlastPerExtraBlock { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Cells a shortest column match (Min Color Match blocks) blasts below its bottom block. Empty cells use up reach.")]
        public int VerticalBlastBase { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Extra cells a column match blasts downward per block beyond the shortest match.")]
        public int VerticalBlastPerExtraBlock { get; private set; } = 1;

        /// <summary>Blast settings for <see cref="ClearResolver"/>.</summary>
        public BlastSettings Blast => new(HorizontalBlastBase, HorizontalBlastPerExtraBlock, VerticalBlastBase,
            VerticalBlastPerExtraBlock);

        [field: Header("Level start")]
        [field: SerializeField, Min(0), Tooltip("Fewest single blocks a column starts with. 0 allows empty columns.")]
        public int StartColumnMinHeight { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Most single blocks a column starts with (capped at the board height).")]
        public int StartColumnMaxHeight { get; private set; } = 4;

        [field: SerializeField, Min(0f), Tooltip("Seconds over which the starting columns drop in, one column at a time from left to right.")]
        public float StartFillDuration { get; private set; } = 3f;

        [field: SerializeField, Min(0.005f), Tooltip("Seconds per row the starting blocks fall while dropping in.")]
        public float StartFillFallInterval { get; private set; } = 0.02f;

        [field: Header("Level start Dirt")]
        [field: SerializeField, Min(0), Tooltip("Fewest Dirt blocks a column starts with, on top of its colored blocks. Dirt never matches.")]
        public int StartDirtMinPerColumn { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Most Dirt blocks a column starts with. The count is rolled from a bell curve centered between min and max.")]
        public int StartDirtMaxPerColumn { get; private set; } = 3;

        [field: SerializeField, Min(0f), Tooltip("Width (standard deviation) of the Dirt count bell curve. Smaller makes the middle count more common; 0 always gives the middle.")]
        public float StartDirtSpread { get; private set; } = 0.75f;

        [field: SerializeField, Range(0f, 1f), Tooltip("How likely each Dirt block is to sit one row higher in its column, compared to the row below. 0 puts all Dirt at the bottom, 1 mixes it in evenly.")]
        public float StartDirtBottomFalloff { get; private set; } = 0.5f;

        /// <summary>Starting Dirt settings for <see cref="StartColumnsGenerator"/>.</summary>
        public StartDirtSettings StartDirt => new(StartDirtMinPerColumn, StartDirtMaxPerColumn, StartDirtSpread,
            StartDirtBottomFalloff);

        [field: Header("Scoring")]
        [field: SerializeField, Min(0), Tooltip("Points per block deleted by a color match. A block in a cross counts for both lines.")]
        public int ColorBlockScore { get; private set; } = 10;

        [field: Header("Randomizer")]
        [field: SerializeField, Tooltip("Seed for the piece bag and block painting. 0 picks a new seed each run.")]
        public int RandomSeed { get; private set; }

        /// <summary>Seconds per row placed blocks fall: <see cref="GravityInterval"/> divided by <see cref="PlacedFallSpeedMultiplier"/>.</summary>
        public float PlacedFallInterval => GravityInterval / PlacedFallSpeedMultiplier;

        /// <summary>Visible rows plus buffer rows.</summary>
        public int TotalHeight => VisibleHeight + BufferRows;
    }
}
