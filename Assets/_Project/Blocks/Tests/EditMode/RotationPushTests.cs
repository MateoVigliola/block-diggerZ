using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class RotationPushTests
    {
        private const int Width = 10;
        private const int Height = 22;
        private const int MaxSideways = 2;
        private const int MaxUp = 1;
        private const int FlatRotation = 2;
        private const int MidRow = 10;

        private BoardGrid _board;
        private PieceDefinition _filler;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardGrid(Width, Height);
            _filler = BlocksTestAssets.Piece("O");
        }

        [Test]
        public void FittingRotation_IsNotPushed()
        {
            var cells = BlocksTestAssets.Piece("I").GetCells(0);
            var start = new Vector2Int(3, MidRow);

            Assert.IsTrue(RotationPush.TryFind(_board, cells, start, MaxSideways, MaxUp, out Vector2Int result));
            Assert.AreEqual(start, result);
        }

        [Test]
        public void FlatI_AtLeftWall_IsPushedTwoRight()
        {
            var cells = BlocksTestAssets.Piece("I").GetCells(FlatRotation);

            Assert.IsTrue(RotationPush.TryFind(_board, cells, new Vector2Int(-2, MidRow), MaxSideways, MaxUp, out Vector2Int result));
            Assert.AreEqual(new Vector2Int(0, MidRow), result);
        }

        [Test]
        public void FlatI_AtRightWall_IsPushedTwoLeft()
        {
            var cells = BlocksTestAssets.Piece("I").GetCells(FlatRotation);

            Assert.IsTrue(RotationPush.TryFind(_board, cells, new Vector2Int(8, MidRow), MaxSideways, MaxUp, out Vector2Int result));
            Assert.AreEqual(new Vector2Int(6, MidRow), result);
        }

        [Test]
        public void BlockOnTheLeft_PushesRightFirst()
        {
            var cells = BlocksTestAssets.Piece("T").GetCells(0);
            _board.Set(4, 1, _filler);

            Assert.AreEqual(1, RotationPush.AwayDirection(_board, cells, new Vector2Int(4, 0)));
            Assert.IsTrue(RotationPush.TryFind(_board, cells, new Vector2Int(4, 0), MaxSideways, MaxUp, out Vector2Int result));
            Assert.AreEqual(new Vector2Int(5, 0), result);
        }

        [Test]
        public void BlockedByFloor_IsPushedUp()
        {
            var cells = BlocksTestAssets.Piece("T").GetCells(FlatRotation);

            Assert.IsTrue(RotationPush.TryFind(_board, cells, new Vector2Int(4, -1), MaxSideways, MaxUp, out Vector2Int result));
            Assert.AreEqual(new Vector2Int(4, 0), result);
        }

        [Test]
        public void NoRoomWithinLimits_Fails()
        {
            var cells = BlocksTestAssets.Piece("I").GetCells(FlatRotation);

            Assert.IsFalse(RotationPush.TryFind(_board, cells, new Vector2Int(-2, MidRow), 1, 0, out _),
                "one sideways cell is not enough for the I piece");
        }

        [Test]
        public void NeverPushesDown()
        {
            for (int y = 6; y <= 8; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _board.Set(x, y, _filler);
                }
            }

            var cells = BlocksTestAssets.Piece("T").GetCells(0);
            Assert.IsTrue(_board.Fits(cells, new Vector2Int(4, 3)), "one row down would fit");
            Assert.IsFalse(RotationPush.TryFind(_board, cells, new Vector2Int(4, 4), MaxSideways, MaxUp, out _));
        }
    }
}
