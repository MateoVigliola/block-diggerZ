using NUnit.Framework;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>Turning the phone sideways shows the rotate message and pauses the game once.</summary>
    public sealed class PortraitGuardTests
    {
        [TestCase(1080, 2340, false)]
        [TestCase(2340, 1080, true)]
        [TestCase(1000, 1000, false)]
        public void IsLandscape_WhenWiderThanTall(int width, int height, bool expected)
        {
            Assert.That(PortraitGuard.IsLandscape(width, height), Is.EqualTo(expected));
        }

        [Test]
        public void TurningSideways_WhilePlaying_Pauses()
        {
            Assert.That(PortraitGuard.ShouldPause(true, false, true, true), Is.True);
        }

        [Test]
        public void StartingSideways_WhilePlaying_Pauses()
        {
            Assert.That(PortraitGuard.ShouldPause(false, false, true, true), Is.True);
        }

        [Test]
        public void StayingSideways_DoesNotPauseAgain()
        {
            Assert.That(PortraitGuard.ShouldPause(true, true, true, true), Is.False);
        }

        [Test]
        public void TurningSideways_WhileControlsHidden_DoesNotPause()
        {
            Assert.That(PortraitGuard.ShouldPause(true, false, true, false), Is.False);
        }

        [Test]
        public void TurningUpright_DoesNotPause()
        {
            Assert.That(PortraitGuard.ShouldPause(true, true, false, true), Is.False);
        }
    }
}
