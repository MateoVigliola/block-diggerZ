using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Relative drag on the board: a finger that goes down on the board (not just anywhere in the element) and moves
    /// sideways steps the falling piece one column per <c>stepSize</c> of travel (<see cref="DragStepper"/>). A
    /// blocked step drops the leftover travel, so dragging back responds at once.
    /// </summary>
    public sealed class BoardDragManipulator : Manipulator
    {
        private const int NoPointer = -1;

        private readonly DragStepper _stepper = new();
        private readonly Func<Vector2, bool> _canStartAt;
        private readonly Func<float> _stepSize;
        private readonly Func<int, bool> _tryStep;
        private int _pointerId = NoPointer;
        private float _lastX;

        /// <summary>
        /// Creates the drag. <paramref name="canStartAt"/> says whether a panel position is on the board,
        /// <paramref name="stepSize"/> gives the travel per column in panel units, and <paramref name="tryStep"/>
        /// moves the piece one column (+1 right, -1 left) and returns false when blocked.
        /// </summary>
        public BoardDragManipulator(Func<Vector2, bool> canStartAt, Func<float> stepSize, Func<int, bool> tryStep)
        {
            _canStartAt = canStartAt;
            _stepSize = stepSize;
            _tryStep = tryStep;
        }

        /// <summary>True while a finger drags.</summary>
        public bool IsDragging => _pointerId != NoPointer;

        /// <summary>Ends the drag if one is running (for example when the controls hide).</summary>
        public void Cancel()
        {
            if (!IsDragging)
            {
                return;
            }

            int pointerId = _pointerId;
            _pointerId = NoPointer;
            _stepper.Reset();
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
            if (IsDragging || !_canStartAt((Vector2)evt.position))
            {
                return;
            }

            _pointerId = evt.pointerId;
            _lastX = evt.position.x;
            _stepper.Reset();
            target.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId)
            {
                return;
            }

            float x = evt.position.x;
            int steps = _stepper.Feed(x - _lastX, _stepSize());
            _lastX = x;
            int direction = Math.Sign(steps);
            for (int i = 0; i < Math.Abs(steps); i++)
            {
                if (!_tryStep(direction))
                {
                    _stepper.Reset();
                    break;
                }
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                Cancel();
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
                _pointerId = NoPointer;
                _stepper.Reset();
            }
        }
    }
}
