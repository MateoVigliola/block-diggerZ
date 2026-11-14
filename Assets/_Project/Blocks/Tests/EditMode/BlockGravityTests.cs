using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class BlockGravityTests
    {
        private ColorTestBoard _test;

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

        private string[] Settle(params string[] rows)
        {
            BoardGrid board = _test.Build(rows);
            BlockGravity.Settle(board);
            return _test.Describe(board);
        }

        [Test]
        public void TouchingSameColor_FormsOneGroup_DifferentColorsDont()
        {
            BoardGrid board = _test.Build(
                "RR.",
                "BRG");
            var groups = new int[board.Width * board.Height];

            int count = BlockGravity.FindGroups(board, groups);

            Assert.AreEqual(3, count, "the three reds, blue, green");
        }

        [Test]
        public void FloatingBlock_FallsToTheFloor()
        {
            CollectionAssert.AreEqual(new[]
            {
                "...",
                "...",
                ".R."
            }, Settle(
                ".R.",
                "...",
                "..."));
        }

        [Test]
        public void GroupWithOneBlockSupported_StaysAsAWhole()
        {
            string[] rows =
            {
                "RRR",
                "..G"
            };

            CollectionAssert.AreEqual(rows, Settle(rows), "the red overhang rests on green through one block");
        }

        [Test]
        public void Overhang_OfAnotherColor_Falls()
        {
            CollectionAssert.AreEqual(new[]
            {
                ".RR",
                "B.G"
            }, Settle(
                "BRR",
                "..G"), "blue has nothing below and falls; red rests on green");
        }

        [Test]
        public void SplitPiece_PartsFallSeparately()
        {
            CollectionAssert.AreEqual(new[]
            {
                "....",
                "....",
                ".G..",
                "RRY."
            }, Settle(
                ".G..",
                "RR..",
                "..Y.",
                "...."));
        }

        [Test]
        public void GroupsStackedOnFallingGroups_FallWithThem()
        {
            BoardGrid board = _test.Build(
                "B",
                "R",
                ".");
            var falling = new bool[board.Width * board.Height];

            Assert.IsTrue(BlockGravity.UpdateFalling(board, falling));
            BlockGravity.Step(board, falling);
            CollectionAssert.AreEqual(new[] { ".", "B", "R" }, _test.Describe(board), "both fell one row in the same step");
            Assert.IsFalse(BlockGravity.UpdateFalling(board, falling));
        }

        [Test]
        public void FallingGroup_PassingSameColor_DoesNotStick()
        {
            CollectionAssert.AreEqual(new[]
            {
                "..",
                "..",
                ".R",
                "RG"
            }, Settle(
                "R.",
                "..",
                ".R",
                ".G"), "the falling red slides past the resting red and lands on the floor");
        }

        [Test]
        public void HolesBelowGetFilled()
        {
            CollectionAssert.AreEqual(new[]
            {
                "..",
                "..",
                "G.",
                "YB"
            }, Settle(
                "G.",
                "..",
                "Y.",
                ".B"));
        }

        [Test]
        public void UnpaintedBlocks_FallAlone()
        {
            CollectionAssert.AreEqual(new[]
            {
                ".X",
                "XX"
            }, Settle(
                "XX",
                ".X"));
        }
    }
}
