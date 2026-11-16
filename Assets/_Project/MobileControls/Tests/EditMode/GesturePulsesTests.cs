using BlockDiggerz.Input;
using NUnit.Framework;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>Gestures become one-frame presses, each followed by a release frame.</summary>
    public sealed class GesturePulsesTests
    {
        private static bool Has(uint mask, TouchPadButton button) => TouchPadMask.IsPressed(mask, button);

        [Test]
        public void TapRight_PressesClockwiseForOneFrame()
        {
            var pulses = new GesturePulses();
            pulses.QueueTap(TouchTap.Right);

            Assert.That(Has(pulses.Next(0), TouchPadButton.RotateClockwise), Is.True);
            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
        }

        [Test]
        public void TapLeft_PressesCounterClockwise()
        {
            var pulses = new GesturePulses();
            pulses.QueueTap(TouchTap.Left);

            uint mask = pulses.Next(0);
            Assert.That(Has(mask, TouchPadButton.RotateCounterClockwise), Is.True);
            Assert.That(Has(mask, TouchPadButton.RotateClockwise), Is.False);
        }

        [Test]
        public void TwoQuickTaps_PressTwice_WithAReleaseBetween()
        {
            var pulses = new GesturePulses();
            pulses.QueueTap(TouchTap.Right);
            pulses.QueueTap(TouchTap.Right);

            Assert.That(Has(pulses.Next(0), TouchPadButton.RotateClockwise), Is.True);
            Assert.That(Has(pulses.Next(0), TouchPadButton.RotateClockwise), Is.False);
            Assert.That(Has(pulses.Next(0), TouchPadButton.RotateClockwise), Is.True);
            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
        }

        [Test]
        public void TapsBeyondQueueLimit_AreDropped()
        {
            var pulses = new GesturePulses { MaxQueuedTaps = 1 };
            pulses.QueueTap(TouchTap.Right);
            pulses.QueueTap(TouchTap.Right);

            pulses.Next(0);
            pulses.Next(0);
            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
        }

        [Test]
        public void Drag_PressesMoveEveryOtherFrame()
        {
            var pulses = new GesturePulses();

            Assert.That(Has(pulses.Next(1), TouchPadButton.MoveRight), Is.True);
            Assert.That(pulses.Next(1), Is.EqualTo(TouchPadMask.None));
            Assert.That(Has(pulses.Next(-1), TouchPadButton.MoveLeft), Is.True);
            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
        }

        [Test]
        public void TapDuringDrag_RotatesAndMovesTogether()
        {
            var pulses = new GesturePulses();
            pulses.QueueTap(TouchTap.Left);

            uint mask = pulses.Next(1);
            Assert.That(Has(mask, TouchPadButton.RotateCounterClockwise), Is.True);
            Assert.That(Has(mask, TouchPadButton.MoveRight), Is.True);
        }

        [Test]
        public void Clear_DropsQueuedTaps()
        {
            var pulses = new GesturePulses();
            pulses.QueueTap(TouchTap.Right);
            pulses.Clear();

            Assert.That(pulses.Next(0), Is.EqualTo(TouchPadMask.None));
        }
    }
}
