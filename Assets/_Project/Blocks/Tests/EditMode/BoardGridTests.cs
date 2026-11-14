using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class BoardGridTests
    {
        private const int Width = 10;
        private const int Height = 22;

        private PieceDefinition _filler;
        private PieceDefinition _other;
        private BoardGrid _board;

        [SetUp]
        public void SetUp()
        {
            _filler = BlocksTestAssets.Piece("O");
            _other = BlocksTestAssets.Piece("T");
            _board = new BoardGrid(Width, Height);
        }

        [Test]
        public void Fits_RejectsCellsOutsideOrOccupied()
        {
            var cells = new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            Assert.IsTrue(_board.Fits(cells, new Vector2Int(0, 0)));
            Assert.IsFalse(_board.Fits(cells, new Vector2Int(-1, 0)), "left wall");
            Assert.IsFalse(_board.Fits(cells, new Vector2Int(Width - 1, 0)), "right wall");
            Assert.IsFalse(_board.Fits(cells, new Vector2Int(0, -1)), "floor");
            Assert.IsFalse(_board.Fits(cells, new Vector2Int(0, Height)), "ceiling");

            _board.Set(1, 0, _filler);
            Assert.IsFalse(_board.Fits(cells, new Vector2Int(0, 0)), "occupied cell");
        }

        [Test]
        public void LandingOffset_RestsOnFloorOrStack_InSameColumn()
        {
            var o = _filler.GetCells(0);
            Assert.AreEqual(new Vector2Int(4, 0), _board.LandingOffset(o, new Vector2Int(4, 18)), "empty board: floor");

            _board.Set(5, 3, _other);
            Assert.AreEqual(new Vector2Int(4, 4), _board.LandingOffset(o, new Vector2Int(4, 18)), "rests on top of the stack");

            Assert.AreEqual(new Vector2Int(4, 4), _board.LandingOffset(o, new Vector2Int(4, 4)), "already resting stays put");
        }

        [Test]
        public void LockedCells_KeepTheirPieceAndTags()
        {
            _board.Place(_other.GetCells(0), new Vector2Int(3, 0), _other);

            PieceDefinition cell = _board.Get(4, 1);
            Assert.AreEqual(_other, cell);
            Assert.IsTrue(cell.HasTag(BlocksTestAssets.BasicTag));
        }
    }
}
