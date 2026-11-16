using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>Fitting the camera so the board fills a screen area: zoom, placement and alignment.</summary>
    public sealed class BoardFitTests
    {
        private const float Tolerance = 0.001f;
        private static readonly Vector2 Centered = new(0.5f, 0.5f);
        private static readonly Vector2 BottomCenter = new(0.5f, 0f);

        [Test]
        public void WholeScreen_HeightLimited_FillsHeight()
        {
            var viewport = new Vector2(1000f, 2000f);
            var board = new Rect(-5f, -10f, 10f, 20f);

            CameraFit fit = BoardFit.Fit(viewport, new Rect(Vector2.zero, viewport), board, 0f, Centered);

            Assert.That(fit.IsValid, Is.True);
            Assert.That(fit.OrthographicSize, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(fit.Position.x, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(fit.Position.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void NarrowArea_WidthLimited_ZoomsOut()
        {
            var viewport = new Vector2(1000f, 2000f);
            var target = new Rect(0f, 0f, 500f, 2000f);
            var board = new Rect(0f, 0f, 10f, 20f);

            CameraFit fit = BoardFit.Fit(viewport, target, board, 0f, Centered);

            Assert.That(fit.OrthographicSize, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void OffsetArea_BoardCenterLandsOnAreaCenter()
        {
            var viewport = new Vector2(1000f, 2000f);
            var target = new Rect(100f, 200f, 800f, 1600f);
            var board = new Rect(0f, 0f, 10f, 20f);

            CameraFit fit = BoardFit.Fit(viewport, target, board, 0f, Centered);

            float pixelsPerUnit = 80f;
            Vector2 boardCenterOnScreen = ScreenPoint(fit, viewport, pixelsPerUnit, board.center);
            Assert.That(boardCenterOnScreen.x, Is.EqualTo(target.center.x).Within(Tolerance));
            Assert.That(boardCenterOnScreen.y, Is.EqualTo(target.center.y).Within(Tolerance));
        }

        [Test]
        public void BottomAlignment_BoardBottomOnAreaBottom()
        {
            var viewport = new Vector2(1000f, 2000f);
            var target = new Rect(0f, 300f, 1000f, 1400f);
            var board = new Rect(0f, 0f, 10f, 10f);

            CameraFit fit = BoardFit.Fit(viewport, target, board, 0f, BottomCenter);

            float pixelsPerUnit = 100f;
            Assert.That(fit.OrthographicSize, Is.EqualTo(10f).Within(Tolerance));
            Vector2 bottomLeft = ScreenPoint(fit, viewport, pixelsPerUnit, board.min);
            Assert.That(bottomLeft.y, Is.EqualTo(target.yMin).Within(Tolerance));
            Assert.That(bottomLeft.x, Is.EqualTo(target.xMin).Within(Tolerance));
        }

        [Test]
        public void Padding_LeavesSpaceAroundBoard()
        {
            var viewport = new Vector2(1000f, 2000f);
            var board = new Rect(0f, 0f, 9f, 19f);

            CameraFit fit = BoardFit.Fit(viewport, new Rect(Vector2.zero, viewport), board, 0.5f, Centered);

            Assert.That(fit.OrthographicSize, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void EmptyArea_IsInvalid()
        {
            CameraFit fit = BoardFit.Fit(new Vector2(1000f, 2000f), new Rect(0f, 0f, 0f, 100f), new Rect(0f, 0f, 10f, 20f), 0f, Centered);

            Assert.That(fit.IsValid, Is.False);
        }

        private static Vector2 ScreenPoint(CameraFit fit, Vector2 viewport, float pixelsPerUnit, Vector2 world)
        {
            Assert.That(viewport.y * 0.5f / fit.OrthographicSize, Is.EqualTo(pixelsPerUnit).Within(Tolerance));
            return viewport * 0.5f + (world - fit.Position) * pixelsPerUnit;
        }
    }
}
