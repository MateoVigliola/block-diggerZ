using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class PiecePainterTests
    {
        private const int Paintings = 300;
        private static readonly string[] PieceIds = { "I", "O", "T", "S", "Z", "J", "L" };
        private static readonly int[] Palette = { 0, 1, 2, 3 };

        private static IReadOnlyList<Vector2Int> Cells(string id) => BlocksTestAssets.Piece(id).GetCells(0);

        private static string Split(List<int> paint)
        {
            return string.Join("+", paint.GroupBy(c => c).Select(g => g.Count()).OrderByDescending(n => n));
        }

        [TestCaseSource(nameof(PieceIds))]
        public void EveryPainting_FollowsTheRules(string id)
        {
            var random = new System.Random(1);
            var paint = new List<int>();
            for (int i = 0; i < Paintings; i++)
            {
                Assert.IsTrue(PiecePainter.Paint(Cells(id), Palette, random, paint));
                Assert.IsTrue(PiecePainter.IsValid(Cells(id), paint), $"{id}: {string.Join(",", paint)}");
                Assert.That(Split(paint), Is.EqualTo("2+2").Or.EqualTo("2+1+1"));
            }
        }

        [Test]
        public void T_IsAlways211_BecauseNo22PaintingTouches()
        {
            var random = new System.Random(2);
            var paint = new List<int>();
            for (int i = 0; i < Paintings; i++)
            {
                PiecePainter.Paint(Cells("T"), Palette, random, paint);
                Assert.AreEqual("2+1+1", Split(paint));
            }
        }

        [Test]
        public void O_GetsBothSplits()
        {
            var random = new System.Random(3);
            var paint = new List<int>();
            var seen = new HashSet<string>();
            for (int i = 0; i < Paintings; i++)
            {
                PiecePainter.Paint(Cells("O"), Palette, random, paint);
                seen.Add(Split(paint));
            }

            CollectionAssert.AreEquivalent(new[] { "2+2", "2+1+1" }, seen);
        }

        [Test]
        public void SameSeed_GivesSamePaintings()
        {
            var a = new List<int>();
            var b = new List<int>();
            var randomA = new System.Random(7);
            var randomB = new System.Random(7);
            for (int i = 0; i < 20; i++)
            {
                PiecePainter.Paint(Cells("L"), Palette, randomA, a);
                PiecePainter.Paint(Cells("L"), Palette, randomB, b);
                CollectionAssert.AreEqual(a, b);
            }
        }

        [Test]
        public void IsValid_RejectsBrokenPaintings()
        {
            IReadOnlyList<Vector2Int> iCells = Cells("I");
            List<Vector2Int> ordered = iCells.OrderBy(c => c.x).ToList();

            Assert.IsFalse(PiecePainter.IsValid(ordered, new[] { 0, 0, 0, 1 }), "3+1 split");
            Assert.IsFalse(PiecePainter.IsValid(ordered, new[] { 0, 1, 0, 2 }), "pair not touching");
            Assert.IsFalse(PiecePainter.IsValid(ordered, new[] { 0, 1, 2, 3 }), "four colors");
            Assert.IsFalse(PiecePainter.IsValid(ordered, new[] { 0, 0, 0, 0 }), "one color");
            Assert.IsTrue(PiecePainter.IsValid(ordered, new[] { 0, 0, 1, 1 }), "2+2 touching");
            Assert.IsTrue(PiecePainter.IsValid(ordered, new[] { 1, 0, 0, 2 }), "2+1+1 touching");
        }

        [Test]
        public void TooFewColors_ReturnsFalse_ButStillPaintsEveryCell()
        {
            var paint = new List<int>();

            bool painted = PiecePainter.Paint(Cells("T"), new[] { 0, 1 }, new System.Random(1), paint);

            Assert.IsFalse(painted, "a T has no 2+2 painting, so two colors can't paint it");
            Assert.AreEqual(4, paint.Count);
        }

        [Test]
        public void Rules_UseAllSixPaletteColors()
        {
            Assert.AreEqual(6, BlocksTestAssets.Rules.ColorCount);
            var palette = UnityEditor.AssetDatabase.LoadAssetAtPath<BlockPalette>("Assets/_Project/Blocks/Data/BlockPalette.asset");
            Assert.AreEqual(6, palette.Colors.Count);
            var active = new List<BlockColor>();
            palette.GetActive(BlocksTestAssets.Rules.ColorCount, active);
            CollectionAssert.AreEqual(palette.Colors.ToList(), active);
        }
    }
}
