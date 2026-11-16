using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class MatchBlastTests
    {
        private const int BlockScore = 10;
        private const int MinMatch = 3;
        private static readonly BlastSettings Default = new(1, 1, 1, 1);

        private ColorTestBoard _test;
        private ClearResolver _resolver;
        private readonly ClearStep _step = new();

        [SetUp]
        public void SetUp()
        {
            _test = new ColorTestBoard();
            _resolver = new ClearResolver(BlockScore, MinMatch, Default);
        }

        [TearDown]
        public void TearDown()
        {
            _test.Destroy();
        }

        private string[] ClearOnce(params string[] rows)
        {
            BoardGrid board = _test.Build(rows);
            Assert.IsTrue(_resolver.TryFindStep(board, _step));
            _resolver.Apply(board, _step);
            return _test.Describe(board);
        }

        [Test]
        public void Reach_IsOneForThree_AndGrowsByOnePerExtraBlock()
        {
            Assert.AreEqual(1, Default.Reach(3, MinMatch, true));
            Assert.AreEqual(2, Default.Reach(4, MinMatch, true));
            Assert.AreEqual(3, Default.Reach(5, MinMatch, true));
            Assert.AreEqual(1, Default.Reach(3, MinMatch, false));
            Assert.AreEqual(3, Default.Reach(5, MinMatch, false));
        }

        [Test]
        public void RowOfThree_BlastsOneCellEachSide()
        {
            CollectionAssert.AreEqual(new[] { "G.....YB" }, ClearOnce("GBRRRYYB"));
            Assert.AreEqual(2, _step.BlastedBlocks);
            Assert.AreEqual(3, _step.MatchedBlocks);
        }

        [Test]
        public void RowOfFour_BlastsTwoCellsEachSide()
        {
            CollectionAssert.AreEqual(new[] { "Y........Y" }, ClearOnce("YGBRRRRGBY"));
            Assert.AreEqual(4, _step.BlastedBlocks);
        }

        [Test]
        public void RowOfFive_BlastsThreeCellsEachSide()
        {
            CollectionAssert.AreEqual(new[] { "Y...........Y" }, ClearOnce("YGBGRRRRRGBGY"));
            Assert.AreEqual(6, _step.BlastedBlocks);
        }

        [Test]
        public void ColumnOfThree_BlastsOneCellDownward_NotUp()
        {
            CollectionAssert.AreEqual(new[]
            {
                "B",
                ".",
                ".",
                ".",
                ".",
                "Y",
            }, ClearOnce(
                "B",
                "R",
                "R",
                "R",
                "G",
                "Y"));
        }

        [Test]
        public void ColumnOfFour_BlastsTwoCellsDownward()
        {
            CollectionAssert.AreEqual(new[] { ".", ".", ".", ".", ".", ".", "Y" }, ClearOnce(
                "R",
                "R",
                "R",
                "R",
                "G",
                "B",
                "Y"));
        }

        [Test]
        public void Blast_DestroysDirt()
        {
            CollectionAssert.AreEqual(new[] { "....." }, ClearOnce("DRRRD"));
            CollectionAssert.AreEqual(new[] { ".", ".", ".", "." }, ClearOnce("R", "R", "R", "D"));
        }

        [Test]
        public void EmptyCells_UseUpReach()
        {
            CollectionAssert.AreEqual(new[] { "G.....G" }, ClearOnce("G.RRR.G"));
            Assert.AreEqual(0, _step.BlastedBlocks);
        }

        [Test]
        public void Blast_StopsAtTheBoardEdge()
        {
            CollectionAssert.AreEqual(new[] { "...." }, ClearOnce("RRRB"));
            Assert.AreEqual(1, _step.BlastedBlocks);
        }

        [Test]
        public void BlastedBlocks_DontScore()
        {
            BoardGrid board = _test.Build("GBRRRYG");

            Assert.AreEqual(3 * BlockScore, _resolver.ResolveAll(board, out _));
        }

        [Test]
        public void ResolverWithoutBlast_LeavesNeighbours()
        {
            var plain = new ClearResolver(BlockScore, MinMatch);
            BoardGrid board = _test.Build("GRRRG");

            plain.ResolveAll(board, out _);

            CollectionAssert.AreEqual(new[] { "G...G" }, _test.Describe(board));
        }

        [Test]
        public void DebugChainLayout_StillChainsWithBlasts()
        {
            BoardGrid board = _test.Build(
                "...B",
                ".BB.",
                "RRR.");

            _resolver.ResolveAll(board, out int steps);

            Assert.AreEqual(2, steps, "the reds clear first, then the blues land in a line");
            CollectionAssert.AreEqual(new[] { "....", "....", "...." }, _test.Describe(board));
        }
    }
}
