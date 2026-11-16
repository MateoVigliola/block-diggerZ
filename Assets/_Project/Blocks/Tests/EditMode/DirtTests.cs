using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class DirtTests
    {
        private const int MinMatch = 3;
        private const int Rolls = 20000;
        private const int DirtValue = -1;
        private static readonly int[] SixColors = { 0, 1, 2, 3, 4, 5 };
        private static readonly int[] OneDirt = { DirtValue };
        private static readonly StartDirtSettings Default = new(1, 3, 0.75f, 0.5f);

        private ColorTestBoard _test;
        private readonly List<Vector2Int> _cells = new();
        private readonly List<BlockColor> _sixColors = new();

        [SetUp]
        public void SetUp()
        {
            _test = new ColorTestBoard();
            for (int i = 0; i < SixColors.Length; i++)
            {
                _sixColors.Add(ScriptableObject.CreateInstance<BlockColor>());
            }
        }

        [TearDown]
        public void TearDown()
        {
            _test.Destroy();
            foreach (BlockColor color in _sixColors)
            {
                Object.DestroyImmediate(color);
            }

            _sixColors.Clear();
        }

        [Test]
        public void Count_StaysInRange_AndPeaksInTheMiddle()
        {
            var random = new System.Random(1);
            var counts = new int[4];
            for (int i = 0; i < Rolls; i++)
            {
                int count = DirtRoll.Count(1, 3, 0.75f, random);
                Assert.That(count, Is.InRange(1, 3));
                counts[count]++;
            }

            Assert.Greater(counts[2], counts[1]);
            Assert.Greater(counts[2], counts[3]);
            Assert.Greater(counts[1], 0);
            Assert.Greater(counts[3], 0);
            Assert.AreEqual(counts[1], counts[3], Rolls * 0.03, "the curve is symmetric");
        }

        [Test]
        public void Count_WithNoSpread_IsAlwaysTheMiddle()
        {
            var random = new System.Random(2);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(2, DirtRoll.Count(1, 3, 0f, random));
            }
        }

        [Test]
        public void Slot_FavorsTheBottom()
        {
            var random = new System.Random(3);
            var picks = new int[4];
            for (int i = 0; i < Rolls; i++)
            {
                picks[DirtRoll.Slot(4, 0.5f, random)]++;
            }

            Assert.Greater(picks[0], picks[1]);
            Assert.Greater(picks[1], picks[2]);
            Assert.Greater(picks[2], picks[3]);
            Assert.Greater(picks[3], 0, "higher slots still happen");
        }

        [Test]
        public void Slot_WithZeroFalloff_IsAlwaysTheBottom()
        {
            var random = new System.Random(4);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(0, DirtRoll.Slot(5, 0f, random));
            }
        }

        [Test]
        public void EveryColumn_GetsColoredBlocksPlusOneToThreeDirt()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(5);
            for (int run = 0; run < 200; run++)
            {
                StartColumnsGenerator.Generate(10, 1, 4, 20, MinMatch, SixColors, OneDirt, Default, random, columns);

                foreach (List<int> column in columns)
                {
                    int dirt = column.FindAll(v => v == DirtValue).Count;
                    Assert.That(dirt, Is.InRange(1, 3));
                    Assert.That(column.Count - dirt, Is.InRange(1, 4));
                }
            }
        }

        [Test]
        public void Dirt_IsMoreOftenAtTheBottom_ButAlsoMixedIn()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(6);
            int bottom = 0;
            int above = 0;
            for (int run = 0; run < 200; run++)
            {
                StartColumnsGenerator.Generate(10, 1, 4, 20, MinMatch, SixColors, OneDirt, Default, random, columns);
                foreach (List<int> column in columns)
                {
                    bottom += column[0] == DirtValue ? 1 : 0;
                    above += column[column.Count - 1] == DirtValue ? 1 : 0;
                }
            }

            Assert.Greater(bottom, above);
            Assert.Greater(above, 0);
        }

        [Test]
        public void ColoredBlocks_StillNeverMakeALine_WithDirtMixedIn()
        {
            var columns = new List<List<int>>();
            var random = new System.Random(7);
            for (int run = 0; run < 200; run++)
            {
                int forced = StartColumnsGenerator.Generate(10, 1, 4, 20, MinMatch, SixColors, OneDirt, Default,
                    random, columns);

                Assert.AreEqual(0, forced);
                BoardGrid board = ToBoard(columns, 20);
                Assert.AreEqual(0, ColorMatchFinder.Find(board, MinMatch, _cells));
            }
        }

        [Test]
        public void Columns_WithDirt_AreCappedAtTheBoard()
        {
            var columns = new List<List<int>>();

            StartColumnsGenerator.Generate(5, 4, 4, 5, MinMatch, SixColors, OneDirt, new StartDirtSettings(3, 3, 0f, 0.5f),
                new System.Random(8), columns);

            foreach (List<int> column in columns)
            {
                Assert.AreEqual(5, column.Count);
            }
        }

        [Test]
        public void ThreeDirtInARowOrColumn_NeverMatch()
        {
            BoardGrid board = _test.Build(
                "D...",
                "D...",
                "DDDD");

            Assert.AreEqual(0, ColorMatchFinder.Find(board, MinMatch, _cells));
        }

        [Test]
        public void Dirt_BreaksAColorLine()
        {
            BoardGrid board = _test.Build("RRDR");

            Assert.AreEqual(0, ColorMatchFinder.Find(board, MinMatch, _cells));
        }

        [Test]
        public void TouchingDirt_FallsAsSingleBlocks()
        {
            BoardGrid board = _test.Build(
                "DD",
                "D.",
                "R.");
            var groups = new int[board.Width * board.Height];

            Assert.AreEqual(4, BlockGravity.FindGroups(board, groups));

            board.Remove(new[] { new Vector2Int(0, 0) });
            BlockGravity.Settle(board);

            CollectionAssert.AreEqual(new[] { "..", "D.", "DD" }, _test.Describe(board),
                "the right Dirt was held only by its neighbour, so it falls on its own");
        }

        private BoardGrid ToBoard(List<List<int>> columns, int height)
        {
            List<BlockColor> colors = _sixColors;
            var board = new BoardGrid(columns.Count, height);
            for (int x = 0; x < columns.Count; x++)
            {
                for (int y = 0; y < columns[x].Count; y++)
                {
                    int value = columns[x][y];
                    board.Set(x, y, _test.Piece, value == DirtValue ? _test[ColorTestBoard.Dirt] : colors[value]);
                }
            }

            return board;
        }
    }
}
