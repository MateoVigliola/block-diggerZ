using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class ClearResolverTests
    {
        private const int BlockScore = 10;
        private const int MinMatch = 3;
        private ColorTestBoard _test;
        private ClearResolver _resolver;

        [SetUp]
        public void SetUp()
        {
            _test = new ColorTestBoard();
            _resolver = new ClearResolver(BlockScore, MinMatch);
        }

        [TearDown]
        public void TearDown()
        {
            _test.Destroy();
        }

        [Test]
        public void ColorMatch_ScoresTenPerBlock()
        {
            BoardGrid board = _test.Build("RRR..");

            Assert.AreEqual(30, _resolver.ResolveAll(board, out int steps));
            Assert.AreEqual(1, steps);
            CollectionAssert.AreEqual(new[] { "....." }, _test.Describe(board));
        }

        [Test]
        public void FullRow_DoesNotClear()
        {
            BoardGrid board = _test.Build("RGBY");

            Assert.AreEqual(0, _resolver.ResolveAll(board, out int steps));
            Assert.AreEqual(0, steps);
            CollectionAssert.AreEqual(new[] { "RGBY" }, _test.Describe(board));
        }

        [Test]
        public void FallingBlocks_MakeASecondColorMatch()
        {
            BoardGrid board = _test.Build(
                ".BB..",
                "RRRB.");

            Assert.AreEqual(60, _resolver.ResolveAll(board, out int steps));
            Assert.AreEqual(2, steps);
            CollectionAssert.AreEqual(new[] { ".....", "....." }, _test.Describe(board));
        }

        [Test]
        public void BoardSettlesBeforeMatching()
        {
            BoardGrid board = _test.Build(
                "..R",
                "...",
                "RR.");

            Assert.AreEqual(30, _resolver.ResolveAll(board, out _), "the floating red lands next to the pair");
        }

        [Test]
        public void Apply_RemovesOnly_AndLeavesFallingToGravity()
        {
            BoardGrid board = _test.Build(
                "G..",
                "RRR");
            var step = new ClearStep();

            Assert.IsTrue(_resolver.TryFindStep(board, step));
            _resolver.Apply(board, step);

            CollectionAssert.AreEqual(new[] { "G..", "..." }, _test.Describe(board));
        }

        [Test]
        public void FallingBlock_PassingAMatchSpot_DoesNotTriggerADelete()
        {
            BoardGrid board = _test.Build(
                "R..",
                "...",
                ".RR",
                ".GB");

            Assert.AreEqual(0, _resolver.ResolveAll(board, out int steps), "the red passes the red pair without matching");
            Assert.AreEqual(0, steps);
            CollectionAssert.AreEqual(new[] { "...", "...", ".RR", "RGB" }, _test.Describe(board));
        }

        [Test]
        public void Delete_HappensBeforeOtherBlocksKeepFalling()
        {
            BoardGrid board = _test.Build(
                "...B",
                "....",
                "....",
                "....",
                "..R.",
                "RR..");
            var falling = new bool[board.Width * board.Height];
            var step = new ClearStep();

            Assert.IsTrue(BlockGravity.UpdateFalling(board, falling));
            Assert.IsFalse(_resolver.TryFindStep(board, step, falling), "nothing matches while the red is in the air");
            BlockGravity.Step(board, falling);

            Assert.IsTrue(BlockGravity.UpdateFalling(board, falling), "the blue is still falling");
            Assert.IsTrue(_resolver.TryFindStep(board, step, falling), "the landed reds match right away");
            Assert.AreEqual(3, step.Cells.Count);
        }

        [Test]
        public void NothingToClear_FindsNoStep()
        {
            BoardGrid board = _test.Build("RB.R");
            var step = new ClearStep();

            Assert.IsFalse(_resolver.TryFindStep(board, step));
            Assert.AreEqual(0, step.Cells.Count);
        }
    }
}
