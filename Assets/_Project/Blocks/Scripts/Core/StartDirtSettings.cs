namespace BlockDiggerz.Blocks
{
    /// <summary>How much starting Dirt each column gets and where it goes (see <see cref="DirtRoll"/>).</summary>
    public readonly struct StartDirtSettings
    {
        /// <summary>Creates the settings.</summary>
        public StartDirtSettings(int minPerColumn, int maxPerColumn, float spread, float bottomFalloff)
        {
            MinPerColumn = minPerColumn;
            MaxPerColumn = maxPerColumn;
            Spread = spread;
            BottomFalloff = bottomFalloff;
        }

        /// <summary>Fewest Dirt blocks per column.</summary>
        public int MinPerColumn { get; }

        /// <summary>Most Dirt blocks per column.</summary>
        public int MaxPerColumn { get; }

        /// <summary>Standard deviation of the bell curve the count is rolled from.</summary>
        public float Spread { get; }

        /// <summary>How likely each slot is compared to the slot below it (0 = always the bottom, 1 = anywhere).</summary>
        public float BottomFalloff { get; }
    }
}
