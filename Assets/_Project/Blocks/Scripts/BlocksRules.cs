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

        [field: Header("Level start")]
        [field: SerializeField, Min(0), Tooltip("Fewest single blocks a column starts with. 0 allows empty columns.")]
        public int StartColumnMinHeight { get; private set; } = 1;

        [field: SerializeField, Min(0), Tooltip("Most single blocks a column starts with (capped at the board height).")]
        public int StartColumnMaxHeight { get; private set; } = 4;

        [field: SerializeField, Min(0f), Tooltip("Seconds over which the starting columns drop in, one column at a time from left to right.")]
        public float StartFillDuration { get; private set; } = 3f;

        [field: SerializeField, Min(0.005f), Tooltip("Seconds per row the starting blocks fall while dropping in.")]
        public float StartFillFallInterval { get; private set; } = 0.02f;

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
