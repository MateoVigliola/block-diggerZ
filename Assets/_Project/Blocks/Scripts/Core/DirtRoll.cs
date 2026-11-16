namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Random rolls for starting Dirt: how many Dirt blocks a column gets (a bell curve between a minimum and a
    /// maximum) and where each one goes in the column (more likely near the bottom).
    /// </summary>
    public static class DirtRoll
    {
        private const int MaxCountTries = 64;

        /// <summary>
        /// A whole number between <paramref name="min"/> and <paramref name="max"/> (inclusive) from a normal
        /// distribution centered between them with standard deviation <paramref name="spread"/>, rounded to the
        /// nearest whole number. Rolls outside the range are rolled again, so the curve keeps its shape. A spread
        /// of 0 or less always gives the center.
        /// </summary>
        public static int Count(int min, int max, float spread, System.Random random)
        {
            int low = System.Math.Min(min, max);
            int high = System.Math.Max(min, max);
            double center = (low + high) * 0.5;
            if (spread <= 0f || low == high)
            {
                return RoundToRange(center, low, high);
            }

            for (int i = 0; i < MaxCountTries; i++)
            {
                int value = (int)System.Math.Floor(center + spread * StandardNormal(random) + 0.5);
                if (value >= low && value <= high)
                {
                    return value;
                }
            }

            return RoundToRange(center, low, high);
        }

        /// <summary>
        /// Picks one of <paramref name="slotCount"/> slots, slot 0 being the bottom. Each slot is
        /// <paramref name="falloff"/> times as likely as the slot below it: 0 always picks the bottom, 1 picks any
        /// slot evenly.
        /// </summary>
        public static int Slot(int slotCount, float falloff, System.Random random)
        {
            if (slotCount <= 1 || falloff <= 0f)
            {
                return 0;
            }

            double ratio = System.Math.Min(falloff, 1f);
            double total = 0;
            double weight = 1;
            for (int i = 0; i < slotCount; i++)
            {
                total += weight;
                weight *= ratio;
            }

            double pick = random.NextDouble() * total;
            weight = 1;
            for (int i = 0; i < slotCount; i++)
            {
                if (pick < weight)
                {
                    return i;
                }

                pick -= weight;
                weight *= ratio;
            }

            return slotCount - 1;
        }

        private static double StandardNormal(System.Random random)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = random.NextDouble();
            return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
        }

        private static int RoundToRange(double value, int low, int high)
        {
            int rounded = (int)System.Math.Floor(value + 0.5);
            return System.Math.Clamp(rounded, low, high);
        }
    }
}
