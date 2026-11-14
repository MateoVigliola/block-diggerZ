namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// The store (hold) slot. Storing puts the falling piece in the slot and hands back what was there (null the
    /// first time, meaning "take the next piece from the bag"). After a store, storing stays locked until a piece
    /// locks into the board, so the player can't swap back and forth.
    /// </summary>
    public sealed class PieceStore
    {
        /// <summary>The stored piece, or null if the slot is empty.</summary>
        public PieceDefinition Stored { get; private set; }

        /// <summary>False from a store until the next piece locks.</summary>
        public bool CanStore { get; private set; } = true;

        /// <summary>
        /// Stores <paramref name="current"/> and returns the piece that comes out (null if the slot was empty).
        /// Returns false and changes nothing while storing is locked.
        /// </summary>
        public bool TryStore(PieceDefinition current, out PieceDefinition released)
        {
            released = null;
            if (!CanStore || current == null)
            {
                return false;
            }

            released = Stored;
            Stored = current;
            CanStore = false;
            return true;
        }

        /// <summary>Unlocks storing; call when a piece locks into the board.</summary>
        public void OnPieceLocked()
        {
            CanStore = true;
        }

        /// <summary>Empties the slot and unlocks storing, for a new run.</summary>
        public void Reset()
        {
            Stored = null;
            CanStore = true;
        }
    }
}
