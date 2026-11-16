using BlockDiggerz.Input;
using UnityEngine.UIElements;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Makes an element a held button of the <see cref="TouchPad"/>: a finger (or mouse) down on it presses
    /// <see cref="Button"/> until that same finger lifts, is cancelled or loses capture. Each element follows one
    /// finger, so several elements can be held at once. Adds <see cref="PressedClass"/> while held.
    /// </summary>
    public sealed class TouchPressManipulator : Manipulator
    {
        /// <summary>USS class on the element while it is held.</summary>
        public const string PressedClass = "mobile-pressed";

        private const int NoPointer = -1;

        private readonly TouchPad _touchPad;
        private int _pointerId = NoPointer;

        /// <summary>Creates a manipulator that presses <paramref name="button"/> on <paramref name="touchPad"/>.</summary>
        public TouchPressManipulator(TouchPad touchPad, TouchPadButton button)
        {
            _touchPad = touchPad;
            Button = button;
        }

        /// <summary>The touch pad button this element presses.</summary>
        public TouchPadButton Button { get; }

        /// <summary>True while a finger holds the element.</summary>
        public bool IsHeld => _pointerId != NoPointer;

        /// <summary>Releases the button if held (for example when the controls hide).</summary>
        public void Cancel()
        {
            if (!IsHeld)
            {
                return;
            }

            int pointerId = _pointerId;
            _pointerId = NoPointer;
            if (target != null && target.HasPointerCapture(pointerId))
            {
                target.ReleasePointer(pointerId);
            }

            target?.RemoveFromClassList(PressedClass);
            _touchPad.SetPressed(Button, false);
        }

        /// <inheritdoc />
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
        }

        /// <inheritdoc />
        protected override void UnregisterCallbacksFromTarget()
        {
            Cancel();
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (IsHeld)
            {
                return;
            }

            _pointerId = evt.pointerId;
            target.CapturePointer(evt.pointerId);
            target.AddToClassList(PressedClass);
            _touchPad.SetPressed(Button, true);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId == _pointerId)
            {
                Cancel();
                evt.StopPropagation();
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
