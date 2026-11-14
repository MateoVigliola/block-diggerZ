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

        [field: Header("Randomizer")]
        [field: SerializeField, Tooltip("Seed for the piece bag. 0 picks a new seed each run.")]
        public int RandomSeed { get; private set; }

        /// <summary>Visible rows plus buffer rows.</summary>
        public int TotalHeight => VisibleHeight + BufferRows;
    }
}
