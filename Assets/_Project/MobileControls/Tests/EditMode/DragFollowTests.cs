using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>The piece steps one column at a time toward the column under the finger.</summary>
    public sealed class DragFollowTests
    {
        private static readonly Rect Board = new(0f, 100f, 1000f, 1500f);
        private const int Width = 10;

        [Test]
        public void FingerCell_MapsPanelXToColumns_Clamped()
        {
            Assert.That(DragFollow.FingerCell(0f, Board, Width), Is.EqualTo(0f));
            Assert.That(DragFollow.FingerCell(450f, Board, Width), Is.EqualTo(4.5f).Within(1e-4f));
            Assert.That(DragFollow.FingerCell(-50f, Board, Width), Is.EqualTo(0f));
            Assert.That(DragFollow.FingerCell(2000f, Board, Width), Is.EqualTo((float)Width));
        }

        [Test]
        public void PieceCenter_IsMiddleOfOccupiedColumns()
        {
            var horizontalI = new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(3, 1) };
            var verticalI = new[] { new Vector2Int(2, 0), new Vector2Int(2, 1), new Vector2Int(2, 2), new Vector2Int(2, 3) };

            Assert.That(DragFollow.PieceCenter(horizontalI, 3), Is.EqualTo(5f));
            Assert.That(DragFollow.PieceCenter(verticalI, 3), Is.EqualTo(5.5f));
        }

        [TestCase(7.6f, 5.5f, 1)]
        [TestCase(3.4f, 5.5f, -1)]
        [TestCase(5.9f, 5.5f, 0)]
        [TestCase(5.1f, 5.5f, 0)]
        [TestCase(6.0f, 5.5f, 0)]
        public void StepDirection_StepsOnlyWhenMoreThanHalfACellAway(float finger, float piece, int expected)
        {
            Assert.That(DragFollow.StepDirection(finger, piece), Is.EqualTo(expected));
        }

        [Test]
        public void Stepping_SettlesUnderFinger_WithoutJitter()
        {
            float piece = 1.5f;
            const float finger = 7.2f;
            for (int i = 0; i < 20; i++)
            {
                piece += DragFollow.StepDirection(finger, piece);
            }

            Assert.That(piece, Is.EqualTo(7.5f));
            Assert.That(DragFollow.StepDirection(finger, piece), Is.EqualTo(0));
        }
    }
}
