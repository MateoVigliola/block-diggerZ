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
        public void ClearFullRows_RemovesFullRowAndShiftsRowsAboveDown()
        {
            FillRow(0, _filler);
            _board.Set(3, 1, _other);

            int cleared = _board.ClearFullRows();

            Assert.AreEqual(1, cleared);
            Assert.AreEqual(_other, _board.Get(3, 0), "row above moved down");
            Assert.IsNull(_board.Get(3, 1));
            Assert.IsFalse(_board.IsRowFull(0));
        }

        [Test]
        public void ClearFullRows_ClearsSeveralNonAdjacentRows()
        {
            FillRow(0, _filler);
            _board.Set(5, 1, _other);
            FillRow(2, _filler);
            FillRow(3, _filler);
            _board.Set(7, 4, _other);

            int cleared = _board.ClearFullRows();

            Assert.AreEqual(3, cleared);
            Assert.AreEqual(_other, _board.Get(5, 0));
            Assert.AreEqual(_other, _board.Get(7, 1));
            for (int y = 2; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Assert.IsNull(_board.Get(x, y), $"cell {x},{y} should be empty");
                }
            }
        }

        [Test]
        public void ClearFullRows_IgnoresRowWithOneGap()
        {
            FillRow(0, _filler);
            _board.Set(4, 0, null);

            Assert.AreEqual(0, _board.ClearFullRows());
            Assert.AreEqual(_filler, _board.Get(0, 0));
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

        private void FillRow(int y, PieceDefinition piece)
        {
            for (int x = 0; x < Width; x++)
            {
                _board.Set(x, y, piece);
            }
        }
    }
}
