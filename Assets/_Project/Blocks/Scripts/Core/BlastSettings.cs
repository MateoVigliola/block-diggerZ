namespace BlockDiggerz.Blocks
{
    /// <summary>How far color matches blast past their ends (see <see cref="MatchBlast"/>).</summary>
    public readonly struct BlastSettings
    {
        /// <summary>Creates the settings.</summary>
        public BlastSettings(int horizontalBase, int horizontalPerExtra, int verticalBase, int verticalPerExtra)
        {
            HorizontalBase = horizontalBase;
            HorizontalPerExtra = horizontalPerExtra;
            VerticalBase = verticalBase;
            VerticalPerExtra = verticalPerExtra;
        }

        /// <summary>Cells a shortest row match blasts past each end.</summary>
        public int HorizontalBase { get; }

        /// <summary>Extra cells per block a row match has beyond the shortest match.</summary>
        public int HorizontalPerExtra { get; }

        /// <summary>Cells a shortest column match blasts below its bottom.</summary>
        public int VerticalBase { get; }

        /// <summary>Extra cells per block a column match has beyond the shortest match.</summary>
        public int VerticalPerExtra { get; }

        /// <summary>
        /// Cells a run of <paramref name="length"/> blasts (per side for rows, downward for columns), never below 0.
        /// </summary>
        public int Reach(int length, int minLength, bool horizontal)
        {
            int extra = System.Math.Max(0, length - minLength);
            int reach = horizontal ? HorizontalBase + extra * HorizontalPerExtra : VerticalBase + extra * VerticalPerExtra;
            return System.Math.Max(0, reach);
        }
    }
}
