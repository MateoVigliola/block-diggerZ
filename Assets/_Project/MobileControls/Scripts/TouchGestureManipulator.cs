using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Makes an element the gesture surface over the board: follows one finger (or the mouse) through a
    /// <see cref="GestureTracker"/>, reports taps on its left or right half through <see cref="Tapped"/>, and
    /// exposes a drag in progress through <see cref="IsDragging"/> and <see cref="Position"/>. Other fingers are
    /// left to other elements (the DROP button keeps working during a drag).
    /// </summary>
    public sealed class TouchGestureManipulator : Manipulator
    {
        private const int NoPointer = -1;

        private readonly GestureTracker _tracker = new();
        private int _pointerId = NoPointer;

        /// <summary>Raised when a finger taps the element.</summary>
        public event Action<TouchTap> Tapped;

        /// <summary>Distance in panel pixels a finger must travel to start a drag.</summary>
        public float DragThreshold
        {
            get => _tracker.DragThreshold;
            set => _tracker.DragThreshold = value;
        }

        /// <summary>Longest press, in seconds, that still counts as a tap.</summary>
        public float TapMaxDuration
        {
            get => _tracker.TapMaxDuration;
            set => _tracker.TapMaxDuration = value;
        }

        /// <summary>True while the tracked finger is dragging.</summary>
        public bool IsDragging => _tracker.IsTracking && _tracker.IsDragging;

        /// <summary>The tracked finger's position in panel coordinates.</summary>
        public Vector2 Position => _tracker.Position;

        /// <summary>Forgets the tracked finger without a tap (for example when the controls hide).</summary>
        public void Cancel()
        {
            _tracker.Cancel();
            if (_pointerId == NoPointer)
            {
                return;
            }

            int pointerId = _pointerId;
            _pointerId = NoPointer;
            if (target != null && target.HasPointerCapture(pointerId))
            {
                target.ReleasePointer(pointerId);
            }
        }

        /// <inheritdoc />
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
        }

        /// <inheritdoc />
        protected override void UnregisterCallbacksFromTarget()
        {
            Cancel();
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_pointerId != NoPointer)
            {
                return;
            }

            _pointerId = evt.pointerId;
            target.CapturePointer(evt.pointerId);
            _tracker.Begin(evt.position, Time.unscaledTime);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                _tracker.Move(evt.position);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            TouchTap tap = _tracker.End(evt.position, Time.unscaledTime, target.worldBound.center.x);
            Cancel();
            evt.StopPropagation();
            if (tap != TouchTap.None)
            {
                Tapped?.Invoke(tap);
            }
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                Cancel();
            }
        }

        private void OnCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                Cancel();
            }
        }
    }
}
