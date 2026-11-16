using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Tells taps from drags for one finger. A finger that travels at least <see cref="DragThreshold"/> from where
    /// it went down becomes a drag (and stays one until it lifts). A finger that lifts before that, within
    /// <see cref="TapMaxDuration"/>, is a tap on the left or right half. Positions are panel coordinates.
    /// </summary>
    public sealed class GestureTracker
    {
        private Vector2 _start;
        private float _startTime;

        /// <summary>Distance in panel pixels a finger must travel to start a drag.</summary>
        public float DragThreshold { get; set; }

        /// <summary>Longest press, in seconds, that still counts as a tap.</summary>
        public float TapMaxDuration { get; set; }

        /// <summary>True while a finger is down.</summary>
        public bool IsTracking { get; private set; }

        /// <summary>True once the tracked finger has travelled far enough to be a drag.</summary>
        public bool IsDragging { get; private set; }

        /// <summary>Where the tracked finger is now.</summary>
        public Vector2 Position { get; private set; }

        /// <summary>Starts tracking a finger that went down at <paramref name="position"/>.</summary>
        public void Begin(Vector2 position, float time)
        {
            IsTracking = true;
            IsDragging = false;
            _start = position;
            _startTime = time;
            Position = position;
        }

        /// <summary>Follows the finger; returns true while it is a drag.</summary>
        public bool Move(Vector2 position)
        {
            if (!IsTracking)
            {
                return false;
            }

            Position = position;
            if (!IsDragging && Vector2.Distance(_start, position) >= DragThreshold)
            {
                IsDragging = true;
            }

            return IsDragging;
        }

        /// <summary>
        /// Stops tracking the finger lifting at <paramref name="position"/>. Returns which half was tapped
        /// (split at <paramref name="splitX"/>), or <see cref="TouchTap.None"/> for a drag or a long press.
        /// </summary>
        public TouchTap End(Vector2 position, float time, float splitX)
        {
            if (!IsTracking)
            {
                return TouchTap.None;
            }

            Move(position);
            bool tap = !IsDragging && time - _startTime <= TapMaxDuration;
            Cancel();
            if (!tap)
            {
                return TouchTap.None;
            }

            return _start.x < splitX ? TouchTap.Left : TouchTap.Right;
        }

        /// <summary>Forgets the finger without a tap.</summary>
        public void Cancel()
        {
            IsTracking = false;
            IsDragging = false;
        }
    }
}
