using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class PiecePreviewLayoutTests
    {
        private readonly List<Vector2> _positions = new();

        [Test]
        public void I_IsCenteredVertically_OnOneRow()
        {
            PiecePreviewLayout.Layout(BlocksTestAssets.Piece("I").GetCells(0), _positions);

            CollectionAssert.AreEquivalent(new[]
            {
                new Vector2(0f, 1.5f), new Vector2(1f, 1.5f), new Vector2(2f, 1.5f), new Vector2(3f, 1.5f)
            }, _positions);
        }

        [Test]
        public void O_IsCenteredBothWays()
        {
            PiecePreviewLayout.Layout(BlocksTestAssets.Piece("O").GetCells(0), _positions);

            CollectionAssert.AreEquivalent(new[]
            {
                new Vector2(1f, 1f), new Vector2(2f, 1f), new Vector2(1f, 2f), new Vector2(2f, 2f)
            }, _positions);
        }

        [Test]
        public void T_PointsUp_WithTopRowFirst()
        {
            PiecePreviewLayout.Layout(BlocksTestAssets.Piece("T").GetCells(0), _positions);

            CollectionAssert.AreEquivalent(new[]
            {
                new Vector2(0.5f, 2f), new Vector2(1.5f, 2f), new Vector2(2.5f, 2f), new Vector2(1.5f, 1f)
            }, _positions, "flat row below, nub above (UI rows count down from the top)");
        }

        [Test]
        public void EmptyCells_GiveNoPositions()
        {
            _positions.Add(Vector2.one);
            PiecePreviewLayout.Layout(System.Array.Empty<Vector2Int>(), _positions);

            Assert.IsEmpty(_positions);
        }
    }
}
