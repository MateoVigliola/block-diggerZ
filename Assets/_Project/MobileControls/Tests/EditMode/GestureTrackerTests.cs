using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>Taps and drags are told apart by distance travelled and press length.</summary>
    public sealed class GestureTrackerTests
    {
        private const float Threshold = 30f;
        private const float MaxTap = 0.3f;
        private const float SplitX = 500f;

        private static GestureTracker Create()
        {
            return new GestureTracker { DragThreshold = Threshold, TapMaxDuration = MaxTap };
        }

        [Test]
        public void QuickTap_OnLeftHalf_IsLeft()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(100f, 400f), 0f);

            Assert.That(tracker.End(new Vector2(105f, 402f), 0.1f, SplitX), Is.EqualTo(TouchTap.Left));
        }

        [Test]
        public void QuickTap_OnRightHalf_IsRight()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(900f, 400f), 0f);

            Assert.That(tracker.End(new Vector2(900f, 400f), 0.1f, SplitX), Is.EqualTo(TouchTap.Right));
        }

        [Test]
        public void LongPress_IsNotATap()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(100f, 400f), 0f);

            Assert.That(tracker.End(new Vector2(100f, 400f), MaxTap + 0.1f, SplitX), Is.EqualTo(TouchTap.None));
        }

        [Test]
        public void MovingPastThreshold_StartsDrag_AndIsNotATap()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(100f, 400f), 0f);

            Assert.That(tracker.Move(new Vector2(100f + Threshold - 1f, 400f)), Is.False);
            Assert.That(tracker.Move(new Vector2(100f + Threshold, 400f)), Is.True);
            Assert.That(tracker.Position.x, Is.EqualTo(100f + Threshold));
            Assert.That(tracker.End(new Vector2(100f + Threshold, 400f), 0.1f, SplitX), Is.EqualTo(TouchTap.None));
        }

        [Test]
        public void Drag_StaysADrag_WhenFingerComesBack()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(100f, 400f), 0f);
            tracker.Move(new Vector2(200f, 400f));

            Assert.That(tracker.Move(new Vector2(100f, 400f)), Is.True);
            Assert.That(tracker.End(new Vector2(100f, 400f), 0.1f, SplitX), Is.EqualTo(TouchTap.None));
        }

        [Test]
        public void Cancel_StopsTracking_WithoutTap()
        {
            GestureTracker tracker = Create();
            tracker.Begin(new Vector2(100f, 400f), 0f);
            tracker.Cancel();

            Assert.That(tracker.IsTracking, Is.False);
            Assert.That(tracker.End(new Vector2(100f, 400f), 0.1f, SplitX), Is.EqualTo(TouchTap.None));
        }
    }
}
