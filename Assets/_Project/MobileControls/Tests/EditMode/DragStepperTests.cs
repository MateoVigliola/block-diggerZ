using NUnit.Framework;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>Relative drag: travel to column steps, remainder carry-over and reset.</summary>
    public sealed class DragStepperTests
    {
        private const float Cell = 50f;

        [Test]
        public void LessThanOneCell_NoStep()
        {
            var stepper = new DragStepper();
            Assert.That(stepper.Feed(49f, Cell), Is.EqualTo(0));
            Assert.That(stepper.Remainder, Is.EqualTo(49f).Within(0.001f));
        }

        [Test]
        public void SmallMovesAddUp_ToOneStep()
        {
            var stepper = new DragStepper();
            Assert.That(stepper.Feed(30f, Cell), Is.EqualTo(0));
            Assert.That(stepper.Feed(30f, Cell), Is.EqualTo(1));
            Assert.That(stepper.Remainder, Is.EqualTo(10f).Within(0.001f));
        }

        [Test]
        public void FastMove_GivesSeveralSteps()
        {
            var stepper = new DragStepper();
            Assert.That(stepper.Feed(175f, Cell), Is.EqualTo(3));
            Assert.That(stepper.Remainder, Is.EqualTo(25f).Within(0.001f));
        }

        [Test]
        public void LeftTravel_GivesNegativeSteps()
        {
            var stepper = new DragStepper();
            Assert.That(stepper.Feed(-120f, Cell), Is.EqualTo(-2));
            Assert.That(stepper.Remainder, Is.EqualTo(-20f).Within(0.001f));
        }

        [Test]
        public void TurningBack_CancelsRemainderFirst()
        {
            var stepper = new DragStepper();
            stepper.Feed(40f, Cell);
            Assert.That(stepper.Feed(-40f, Cell), Is.EqualTo(0));
            Assert.That(stepper.Feed(-50f, Cell), Is.EqualTo(-1));
        }

        [Test]
        public void Reset_ForgetsRemainder()
        {
            var stepper = new DragStepper();
            stepper.Feed(40f, Cell);
            stepper.Reset();
            Assert.That(stepper.Feed(20f, Cell), Is.EqualTo(0));
            Assert.That(stepper.Remainder, Is.EqualTo(20f).Within(0.001f));
        }

        [Test]
        public void NoStepSize_NoSteps()
        {
            var stepper = new DragStepper();
            Assert.That(stepper.Feed(500f, 0f), Is.EqualTo(0));
        }
    }
}
