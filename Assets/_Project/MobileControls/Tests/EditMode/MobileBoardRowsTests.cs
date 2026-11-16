using NUnit.Framework;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>The touch layout cuts the board to the rows that fit the board area at full width.</summary>
    public sealed class MobileBoardRowsTests
    {
        private const int Columns = 10;
        private const int MinRows = 12;
        private const int MaxRows = 20;

        [Test]
        public void TallArea_GetsEveryRuleRow()
        {
            Assert.That(MobileBoardRows.Fit(1000f, 2500f, Columns, 0f, MinRows, MaxRows), Is.EqualTo(MaxRows));
        }

        [Test]
        public void ShorterArea_GetsOnlyWholeRowsThatFit()
        {
            Assert.That(MobileBoardRows.Fit(1000f, 1550f, Columns, 0f, MinRows, MaxRows), Is.EqualTo(15));
        }

        [Test]
        public void ExactFit_CountsTheLastRow()
        {
            Assert.That(MobileBoardRows.Fit(1000f, 1600f, Columns, 0f, MinRows, MaxRows), Is.EqualTo(16));
        }

        [Test]
        public void VeryShortArea_KeepsTheMinimum()
        {
            Assert.That(MobileBoardRows.Fit(1000f, 500f, Columns, 0f, MinRows, MaxRows), Is.EqualTo(MinRows));
        }

        [Test]
        public void Padding_ShrinksCellsAndTakesRowsTopAndBottom()
        {
            Assert.That(MobileBoardRows.Fit(1100f, 1600f, Columns, 0.5f, MinRows, MaxRows), Is.EqualTo(15));
        }

        [Test]
        public void InvalidArea_KeepsTheRuleRows()
        {
            Assert.That(MobileBoardRows.Fit(0f, 1600f, Columns, 0f, MinRows, MaxRows), Is.EqualTo(MaxRows));
        }
    }
}
