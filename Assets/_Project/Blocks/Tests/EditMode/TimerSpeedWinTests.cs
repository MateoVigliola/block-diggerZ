using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class TimerSpeedWinTests
    {
        private const int Step = 200;
        private const float PerStep = 0.1f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void Speed_StartsAtOne_AndAddsAFlatStepPer200Points()
        {
            Assert.AreEqual(1f, GravitySpeed.Multiplier(0, Step, PerStep, 0f), Tolerance);
            Assert.AreEqual(1f, GravitySpeed.Multiplier(199, Step, PerStep, 0f), Tolerance);
            Assert.AreEqual(1.1f, GravitySpeed.Multiplier(200, Step, PerStep, 0f), Tolerance);
            Assert.AreEqual(1.1f, GravitySpeed.Multiplier(390, Step, PerStep, 0f), Tolerance);
            Assert.AreEqual(1.2f, GravitySpeed.Multiplier(400, Step, PerStep, 0f), Tolerance);
            Assert.AreEqual(2f, GravitySpeed.Multiplier(2000, Step, PerStep, 0f), Tolerance, "flat, not compounding");
        }

        [Test]
        public void Speed_StopsAtTheCap_WhenOneIsSet()
        {
            Assert.AreEqual(1.5f, GravitySpeed.Multiplier(5000, Step, PerStep, 1.5f), Tolerance);
        }

        [Test]
        public void Speed_IsOffWithAZeroStep()
        {
            Assert.AreEqual(1f, GravitySpeed.Multiplier(5000, 0, PerStep, 0f), Tolerance);
        }

        [Test]
        public void Clock_FormatsMinutesAndSeconds()
        {
            Assert.AreEqual("0:00", RunClock.Format(0));
            Assert.AreEqual("0:07", RunClock.Format(7));
            Assert.AreEqual("1:00", RunClock.Format(60));
            Assert.AreEqual("3:45", RunClock.Format(225));
            Assert.AreEqual("72:00", RunClock.Format(4320));
        }

        [Test]
        public void Clock_RoundsDownToWholeSeconds()
        {
            Assert.AreEqual(0, RunClock.WholeSeconds(0.99f));
            Assert.AreEqual(59, RunClock.WholeSeconds(59.9f));
            Assert.AreEqual(0, RunClock.WholeSeconds(-1f));
        }

        [Test]
        public void Goal_IsClearedOnlyWhenStartingDirtIsGone()
        {
            Assert.IsTrue(LevelGoal.IsCleared(12, 0));
            Assert.IsFalse(LevelGoal.IsCleared(12, 1));
            Assert.IsFalse(LevelGoal.IsCleared(0, 0), "a level without Dirt is not won");
        }

        [Test]
        public void Board_CountsOnlyDirt()
        {
            var test = new ColorTestBoard();
            try
            {
                BoardGrid board = test.Build(
                    "D.R",
                    "XDB");

                Assert.AreEqual(2, board.CountObstacles());

                board.Remove(new[] { new UnityEngine.Vector2Int(0, 1), new UnityEngine.Vector2Int(1, 0) });

                Assert.AreEqual(0, board.CountObstacles());
            }
            finally
            {
                test.Destroy();
            }
        }
    }
}
