using System.Collections.Generic;
using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class StartColumnsGeneratorTests
    {
        private const int MinMatch = 3;
        private const int Runs = 200;
        private static readonly int[] SixColors = { 0, 1, 2, 3, 4, 5 };

        private static int LongestRun(List<List<int>> columns)
        {
            int longest = 0;
            for (int x = 0; x < columns.Count; x++)
            {
                for (int y = 0; y < columns[x].Count; y++)
                {
                    int color = columns[x][y];
                    int up = 1;
                    while (y + up < columns[x].Count && columns[x][y + up] == color)
                    {
                        up++;
                    }

                    int right = 1;
                    while (x + right < columns.Count && y < columns[x + right].Count && columns[x + right][y] == color)
                    {
                        right++;
                    }

                    longest = System.Math.Max(longest, System.Math.Max(up, right));
                }
            }

            return longest;
        }

        [Test]
        public void EveryColumn_Gets1To4Blocks()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(1);
            for (int run = 0; run < Runs; run++)
            {
                StartColumnsGenerator.Generate(15, 1, 4, 15, MinMatch, SixColors, random, columns);

                Assert.AreEqual(15, columns.Count);
                foreach (List<int> column in columns)
                {
                    Assert.That(column.Count, Is.InRange(1, 4));
                }
            }
        }

        [Test]
        public void WithSixColors_NeverMakesALineOfThree()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(2);
            for (int run = 0; run < Runs; run++)
            {
                int forced = StartColumnsGenerator.Generate(15, 1, 4, 15, MinMatch, SixColors, random, columns);

                Assert.AreEqual(0, forced);
                Assert.Less(LongestRun(columns), MinMatch);
            }
        }

        [Test]
        public void WithTwoColors_StillAvoidsLines_WhenItCan()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(3);
            for (int run = 0; run < Runs; run++)
            {
                int forced = StartColumnsGenerator.Generate(10, 4, 4, 20, MinMatch, new[] { 0, 1 }, random, columns);
                if (forced == 0)
                {
                    Assert.Less(LongestRun(columns), MinMatch);
                }
            }
        }

        [Test]
        public void WithOneColor_PlacesBlocksAnyway_AndCountsThem()
        {
            var columns = new List<List<int>>();

            int forced = StartColumnsGenerator.Generate(3, 3, 3, 10, MinMatch, new[] { 0 }, new System.Random(4), columns);

            Assert.AreEqual(9, columns[0].Count + columns[1].Count + columns[2].Count);
            Assert.Greater(forced, 0, "a single color can't avoid lines of three");
        }

        [Test]
        public void Heights_AreCappedAtTheBoard()
        {
            var columns = new List<List<int>>();

            StartColumnsGenerator.Generate(5, 4, 4, 2, MinMatch, SixColors, new System.Random(5), columns);

            foreach (List<int> column in columns)
            {
                Assert.AreEqual(2, column.Count);
            }
        }

        [Test]
        public void SameSeed_GivesTheSameStart()
        {
            var a = new List<List<int>>();
            var b = new List<List<int>>();

            StartColumnsGenerator.Generate(15, 1, 4, 15, MinMatch, SixColors, new System.Random(9), a);
            StartColumnsGenerator.Generate(15, 1, 4, 15, MinMatch, SixColors, new System.Random(9), b);

            for (int x = 0; x < a.Count; x++)
            {
                CollectionAssert.AreEqual(a[x], b[x]);
            }
        }
    }
}
