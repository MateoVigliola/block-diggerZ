using BlockDiggerz.Input;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Turns gestures into one-frame touch pad presses. Each frame <see cref="Next"/> returns the buttons to hold
    /// this frame: a button pressed last frame is always released this frame, so the game sees every press as a
    /// fresh one (one column per move press, never long enough to auto repeat). Taps queue rotations, a drag asks
    /// for one column step per press.
    /// </summary>
    public sealed class GesturePulses
    {
        private int _pendingClockwise;
        private int _pendingCounterClockwise;

        /// <summary>Most rotations of one direction that can wait in the queue; extra taps are dropped.</summary>
        public int MaxQueuedTaps { get; set; } = 2;

        /// <summary>Buttons held by the last call to <see cref="Next"/>.</summary>
        public uint Held { get; private set; } = TouchPadMask.None;

        /// <summary>Gesture buttons this class presses, for releasing them on the touch pad.</summary>
        public static uint Buttons =>
            TouchPadMask.Bit(TouchPadButton.MoveLeft) | TouchPadMask.Bit(TouchPadButton.MoveRight)
            | TouchPadMask.Bit(TouchPadButton.RotateClockwise) | TouchPadMask.Bit(TouchPadButton.RotateCounterClockwise);

        /// <summary>Queues a rotation for a tap: left half counter-clockwise, right half clockwise.</summary>
        public void QueueTap(TouchTap tap)
        {
            if (tap == TouchTap.Right && _pendingClockwise < MaxQueuedTaps)
            {
                _pendingClockwise++;
            }
            else if (tap == TouchTap.Left && _pendingCounterClockwise < MaxQueuedTaps)
            {
                _pendingCounterClockwise++;
            }
        }

        /// <summary>
        /// Buttons to hold this frame. <paramref name="moveDirection"/> is the column step the drag wants (+1
        /// right, -1 left, 0 none); it is ignored on frames that release a move press, so the caller should read
        /// the piece position again before asking for the next step.
        /// </summary>
        public uint Next(int moveDirection)
        {
            uint last = Held;
            uint next = TouchPadMask.None;

            if (_pendingClockwise > 0 && !TouchPadMask.IsPressed(last, TouchPadButton.RotateClockwise))
            {
                _pendingClockwise--;
                next = TouchPadMask.Set(next, TouchPadButton.RotateClockwise, true);
            }

            if (_pendingCounterClockwise > 0 && !TouchPadMask.IsPressed(last, TouchPadButton.RotateCounterClockwise))
            {
                _pendingCounterClockwise--;
                next = TouchPadMask.Set(next, TouchPadButton.RotateCounterClockwise, true);
            }

            bool moveReleasing = TouchPadMask.IsPressed(last, TouchPadButton.MoveLeft) || TouchPadMask.IsPressed(last, TouchPadButton.MoveRight);
            if (!moveReleasing && moveDirection != 0)
            {
                next = TouchPadMask.Set(next, moveDirection > 0 ? TouchPadButton.MoveRight : TouchPadButton.MoveLeft, true);
            }

            Held = next;
            return next;
        }

        /// <summary>Drops queued rotations and forgets held buttons.</summary>
        public void Clear()
        {
            _pendingClockwise = 0;
            _pendingCounterClockwise = 0;
            Held = TouchPadMask.None;
        }
    }
}
