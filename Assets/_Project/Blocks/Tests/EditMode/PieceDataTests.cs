using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class PieceDataTests
    {
        private const int StandardPieceCount = 7;
        private const int CellsPerPiece = 4;
        private static readonly string[] StandardIds = { "I", "O", "T", "S", "Z", "J", "L" };

        [Test]
        public void StandardSet_HasTheSevenPiecesWithFourCellsEach()
        {
            PieceSet set = BlocksTestAssets.PieceSet;
            Assert.AreEqual(StandardPieceCount, set.Pieces.Count, "the classic seven pieces");
            CollectionAssert.AreEquivalent(StandardIds, set.Pieces.Select(p => p.Id));
            foreach (PieceDefinition piece in set.Pieces)
            {
                Assert.AreEqual(CellsPerPiece, piece.GetCells(0).Count, piece.Id);
                Assert.AreEqual(CellsPerPiece, piece.GetCells(0).Distinct().Count(), $"{piece.Id} has duplicate cells");
            }
        }

        [Test]
        public void EveryStandardPiece_IsTaggedBasic()
        {
            PieceTag basic = BlocksTestAssets.BasicTag;
            Assert.IsNotNull(basic);
            foreach (PieceDefinition piece in BlocksTestAssets.PieceSet.Pieces)
            {
                Assert.IsTrue(piece.HasTag(basic), $"{piece.Id} should carry the Basic tag");
            }
        }

        [Test]
        public void Rules_DefaultBoardSize_IsInsideTheLimits()
        {
            BlocksRules rules = BlocksTestAssets.Rules;
            Vector2Int size = new Vector2Int(rules.BoardWidth, rules.VisibleHeight);
            Assert.AreEqual(size, BoardLayout.ClampSize(size.x, size.y));
        }

        [Test]
        public void T_RotatesClockwiseThroughFourStates()
        {
            PieceDefinition t = BlocksTestAssets.Piece("T");
            AssertCells(t.GetCells(0), (0, 1), (1, 1), (2, 1), (1, 2));
            AssertCells(t.GetCells(1), (1, 0), (1, 1), (1, 2), (2, 1));
            AssertCells(t.GetCells(2), (0, 1), (1, 1), (2, 1), (1, 0));
            AssertCells(t.GetCells(3), (1, 0), (1, 1), (1, 2), (0, 1));
            AssertCells(t.GetCells(4), (0, 1), (1, 1), (2, 1), (1, 2));
        }

        [Test]
        public void I_TurnsVerticalInThirdColumn()
        {
            PieceDefinition i = BlocksTestAssets.Piece("I");
            AssertCells(i.GetCells(1), (2, 0), (2, 1), (2, 2), (2, 3));
        }

        [Test]
        public void O_LooksTheSameInEveryRotation()
        {
            PieceDefinition o = BlocksTestAssets.Piece("O");
            for (int r = 1; r < PieceDefinition.RotationCount; r++)
            {
                CollectionAssert.AreEquivalent(o.GetCells(0), o.GetCells(r));
            }
        }

        [Test]
        public void Bag_DealsEachPieceOncePerBag()
        {
            PieceSet set = BlocksTestAssets.PieceSet;
            var bag = new PieceBag(set.Pieces, 1234);
            for (int round = 0; round < 3; round++)
            {
                var dealt = new HashSet<PieceDefinition>();
                for (int i = 0; i < set.Pieces.Count; i++)
                {
                    Assert.IsTrue(dealt.Add(bag.Next()), $"piece repeated within bag {round}");
                }
            }
        }

        [Test]
        public void Bag_PeekMatchesNext()
        {
            var bag = new PieceBag(BlocksTestAssets.PieceSet.Pieces, 99);
            for (int i = 0; i < 10; i++)
            {
                PieceDefinition peeked = bag.Peek();
                Assert.AreEqual(peeked, bag.Next());
            }
        }

        private static void AssertCells(IReadOnlyList<Vector2Int> actual, params (int x, int y)[] expected)
        {
            CollectionAssert.AreEquivalent(expected.Select(e => new Vector2Int(e.x, e.y)), actual);
        }
    }
}
