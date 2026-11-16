using System.Collections.Generic;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Builds the starting rubble: every column gets a random number of single colored blocks plus, optionally, a
    /// bell-curve number of Dirt blocks mixed in (more likely near the bottom, see <see cref="DirtRoll"/>), stacked
    /// from the floor. Colors are picked left to right, bottom to top, at random from the colors that don't complete
    /// a straight line of <c>minMatch</c> with the blocks already to the left or below; Dirt breaks lines. If no
    /// color is safe, a random one is used anyway.
    /// </summary>
    public static class StartColumnsGenerator
    {
        /// <summary>
        /// Fills <paramref name="columns"/> (one list per column, bottom to top) with colored blocks only and returns
        /// how many blocks had no safe color. Heights are rolled between <paramref name="minHeight"/> and
        /// <paramref name="maxHeight"/> (inclusive) and capped at <paramref name="maxAllowedHeight"/>.
        /// </summary>
        public static int Generate<T>(int width, int minHeight, int maxHeight, int maxAllowedHeight, int minMatch,
            IReadOnlyList<T> colors, System.Random random, List<List<T>> columns)
        {
            return Generate(width, minHeight, maxHeight, maxAllowedHeight, minMatch, colors, System.Array.Empty<T>(),
                default, random, columns);
        }

        /// <summary>
        /// Like the overload without Dirt, but each column also gets a number of Dirt blocks rolled from
        /// <paramref name="dirtSettings"/>, each one of <paramref name="dirt"/> (picked evenly) and placed among the
        /// colored blocks. Dirt values must differ from every color. Columns are capped at
        /// <paramref name="maxAllowedHeight"/> after Dirt is added.
        /// </summary>
        public static int Generate<T>(int width, int minHeight, int maxHeight, int maxAllowedHeight, int minMatch,
            IReadOnlyList<T> colors, IReadOnlyList<T> dirt, StartDirtSettings dirtSettings, System.Random random,
            List<List<T>> columns)
        {
            columns.Clear();
            int low = System.Math.Min(minHeight, maxHeight);
            int high = System.Math.Max(minHeight, maxHeight);
            var safe = new List<T>(colors.Count);
            var isDirt = new List<bool>();
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            int forced = 0;
            for (int x = 0; x < width; x++)
            {
                int colored = colors.Count > 0 ? random.Next(low, high + 1) : 0;
                int dirtCount = dirt.Count > 0
                    ? DirtRoll.Count(dirtSettings.MinPerColumn, dirtSettings.MaxPerColumn, dirtSettings.Spread, random)
                    : 0;
                isDirt.Clear();
                for (int i = 0; i < colored; i++)
                {
                    isDirt.Add(false);
                }

                for (int i = 0; i < dirtCount; i++)
                {
                    isDirt.Insert(DirtRoll.Slot(isDirt.Count + 1, dirtSettings.BottomFalloff, random), true);
                }

                int height = System.Math.Min(isDirt.Count, System.Math.Max(0, maxAllowedHeight));
                var column = new List<T>(height);
                columns.Add(column);
                for (int y = 0; y < height; y++)
                {
                    if (isDirt[y])
                    {
                        column.Add(dirt[random.Next(dirt.Count)]);
                        continue;
                    }

                    safe.Clear();
                    for (int c = 0; c < colors.Count; c++)
                    {
                        if (!CompletesLine(columns, x, y, colors[c], minMatch, comparer))
                        {
                            safe.Add(colors[c]);
                        }
                    }

                    if (safe.Count == 0)
                    {
                        forced++;
                        column.Add(colors[random.Next(colors.Count)]);
                    }
                    else
                    {
                        column.Add(safe[random.Next(safe.Count)]);
                    }
                }
            }

            return forced;
        }

        private static bool CompletesLine<T>(List<List<T>> columns, int x, int y, T color, int minMatch,
            EqualityComparer<T> comparer)
        {
            int below = 0;
            List<T> own = columns[x];
            for (int i = y - 1; i >= 0 && comparer.Equals(own[i], color); i--)
            {
                below++;
            }

            if (below + 1 >= minMatch)
            {
                return true;
            }

            int left = 0;
            for (int i = x - 1; i >= 0 && y < columns[i].Count && comparer.Equals(columns[i][y], color); i--)
            {
                left++;
            }

            return left + 1 >= minMatch;
        }
    }
}
