using NUnit.Framework;

namespace BlockDiggerz.Blocks.Tests
{
    public class LockDelayTimerTests
    {
        private const float LockDelay = 0.5f;
        private const float Bonus = 0.25f;
        private const int MaxExtensions = 3;
        private const int StartRow = 10;

        private LockDelayTimer _timer;

        [SetUp]
        public void SetUp()
        {
            _timer = new LockDelayTimer();
            _timer.Reset(StartRow);
        }

        [Test]
        public void Tick_LocksOnceDelayIsReached()
        {
            Assert.IsFalse(_timer.Tick(0.3f, LockDelay));
            Assert.IsTrue(_timer.Tick(0.3f, LockDelay));
        }

        [Test]
        public void Extend_GivesBackBonus_NeverBelowZero()
        {
            _timer.Tick(0.4f, LockDelay);
            Assert.IsTrue(_timer.TryExtend(Bonus, MaxExtensions));
            Assert.AreEqual(0.15f, _timer.Elapsed, 0.0001f);

            Assert.IsTrue(_timer.TryExtend(Bonus, MaxExtensions));
            Assert.AreEqual(0f, _timer.Elapsed, "time left never exceeds the full lock delay");
        }

        [Test]
        public void Extend_StopsAfterMaxPerPiece()
        {
            for (int i = 0; i < MaxExtensions; i++)
            {
                Assert.IsTrue(_timer.TryExtend(Bonus, MaxExtensions));
            }

            _timer.Tick(0.4f, LockDelay);
            Assert.IsFalse(_timer.TryExtend(Bonus, MaxExtensions));
            Assert.AreEqual(0.4f, _timer.Elapsed, 0.0001f, "a refused extension gives nothing back");
            Assert.AreEqual(MaxExtensions, _timer.ExtensionsUsed);
        }

        [Test]
        public void ReachingLowerRow_RestartsTimeAndCount()
        {
            _timer.Tick(0.4f, LockDelay);
            _timer.TryExtend(Bonus, MaxExtensions);

            _timer.ReachRow(StartRow - 1);

            Assert.AreEqual(0f, _timer.Elapsed);
            Assert.AreEqual(0, _timer.ExtensionsUsed);
            Assert.AreEqual(StartRow - 1, _timer.LowestRow);
        }

        [Test]
        public void SameOrHigherRow_KeepsTimeAndCount()
        {
            _timer.Tick(0.4f, LockDelay);
            _timer.TryExtend(Bonus, MaxExtensions);

            _timer.ReachRow(StartRow);
            _timer.ReachRow(StartRow + 1);

            Assert.AreEqual(0.15f, _timer.Elapsed, 0.0001f, "pushing up and falling back can't refill the timer");
            Assert.AreEqual(1, _timer.ExtensionsUsed);
        }
    }
}
