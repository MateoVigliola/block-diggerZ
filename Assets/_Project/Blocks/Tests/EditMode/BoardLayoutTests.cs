using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    public class BoardLayoutTests
    {
        [TestCase(10, 20, 10, 20)]
        [TestCase(2, 5, 4, 8)]
        [TestCase(99, 99, 30, 40)]
        [TestCase(4, 40, 4, 40)]
        public void ClampSize_KeepsSizesInsideLimits(int width, int height, int expectedWidth, int expectedHeight)
        {
            Assert.AreEqual(new Vector2Int(expectedWidth, expectedHeight), BoardLayout.ClampSize(width, height));
        }

        [Test]
        public void DefaultBoard_UnderTheSceneCamera_StaysAtTheOrigin()
        {
            Assert.AreEqual(Vector2.zero, BoardLayout.CenteredOrigin(new Vector2(5f, 10f), 10, 20, 1f));
        }

        [Test]
        public void WiderAndTallerBoards_StayCentered()
        {
            Vector2 center = new Vector2(5f, 10f);

            Vector2 wide = BoardLayout.CenteredOrigin(center, 16, 20, 1f);
            Vector2 tall = BoardLayout.CenteredOrigin(center, 10, 30, 1f);
            Vector2 small = BoardLayout.CenteredOrigin(center, 6, 12, 1f);

            Assert.AreEqual(new Vector2(-3f, 0f), wide);
            Assert.AreEqual(new Vector2(0f, -5f), tall);
            Assert.AreEqual(new Vector2(2f, 4f), small, "a shorter board moves up to stay centered");
        }

        [Test]
        public void CellSize_ScalesTheOffset()
        {
            Assert.AreEqual(new Vector2(-5f, -10f), BoardLayout.CenteredOrigin(Vector2.zero, 10, 20, 1f));
            Assert.AreEqual(new Vector2(-2.5f, -5f), BoardLayout.CenteredOrigin(Vector2.zero, 10, 20, 0.5f));
        }
    }
}
