using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class ColorMatchTests
    {
        private const int MinMatch = 3;
        private ColorTestBoard _test;
        private readonly List<Vector2Int> _cells = new();

        [SetUp]
        public void SetUp()
        {
            _test = new ColorTestBoard();
        }

        [TearDown]
        public void TearDown()
        {
            _test.Destroy();
        }

        private int Find(params string[] rows) => ColorMatchFinder.Find(_test.Build(rows), MinMatch, _cells);

        [Test]
        public void ThreeInARow_Matches()
        {
            Assert.AreEqual(3, Find(
                ".....",
                "RRRB."));
            Assert.AreEqual(3, _cells.Count);
        }

        [Test]
        public void ThreeInAColumn_Matches()
        {
            Assert.AreEqual(3, Find(
                "G..",
                "G..",
                "G.."));
        }

        [Test]
        public void TwoInARow_DoesNotMatch()
        {
            Assert.AreEqual(0, Find("RR.R"));
            Assert.AreEqual(0, _cells.Count);
        }

        [Test]
        public void LShape_DoesNotMatch()
        {
            Assert.AreEqual(0, Find(
                "R..",
                "RR."));
        }

        [Test]
        public void LongRun_MatchesEveryBlock()
        {
            Assert.AreEqual(5, Find("BRRRRRB"));
            Assert.AreEqual(5, _cells.Count);
        }

        [Test]
        public void Cross_RemovesSharedBlockOnce_ButScoresItTwice()
        {
            int scored = Find(
                ".Y.",
                "YYY",
                ".Y.");

            Assert.AreEqual(6, scored, "two runs of 3");
            Assert.AreEqual(5, _cells.Count, "the middle block is removed once");
        }

        [Test]
        public void UnpaintedBlocks_NeverMatch()
        {
            Assert.AreEqual(0, Find("XXX"));
        }
    }
}
