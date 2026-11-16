using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

namespace BlockDiggerz.Input
{
    /// <summary>
    /// Virtual device behind the on-screen touch controls. GameControls binds its buttons (group "Touch") next to
    /// the keyboard and gamepad, so touch presses reach the game through the same actions and
    /// <see cref="GameInput"/>. Only exists while the touch controls are shown (see <see cref="TouchPad"/>).
    /// </summary>
    [InputControlLayout(stateType = typeof(TouchPadState), displayName = "BlockDiggerz Touch Pad")]
    public sealed class TouchPadDevice : InputDevice
    {
        /// <summary>Layout name used in binding paths, for example "&lt;BlockDiggerzTouchPad&gt;/moveLeft".</summary>
        public const string LayoutName = "BlockDiggerzTouchPad";

        /// <summary>Control name of the move left button.</summary>
        public const string MoveLeftControl = "moveLeft";

        /// <summary>Control name of the move right button.</summary>
        public const string MoveRightControl = "moveRight";

        /// <summary>Control name of the hard drop button.</summary>
        public const string HardDropControl = "hardDrop";

        /// <summary>Control name of the rotate clockwise button.</summary>
        public const string RotateClockwiseControl = "rotateClockwise";

        /// <summary>Control name of the rotate counter-clockwise button.</summary>
        public const string RotateCounterClockwiseControl = "rotateCounterClockwise";

        /// <summary>Control name of the store piece button.</summary>
        public const string StorePieceControl = "storePiece";

        /// <summary>Control name of the pause button.</summary>
        public const string PauseControl = "pause";

        private static readonly string[] ControlNames =
        {
            MoveLeftControl,
            MoveRightControl,
            HardDropControl,
            RotateClockwiseControl,
            RotateCounterClockwiseControl,
            StorePieceControl,
            PauseControl
        };

        private ButtonControl[] _buttons;

        /// <summary>The control of <paramref name="button"/>.</summary>
        public ButtonControl GetButton(TouchPadButton button)
        {
            return _buttons[(int)button];
        }

        /// <summary>Control name of <paramref name="button"/> in binding paths.</summary>
        public static string ControlName(TouchPadButton button)
        {
            return ControlNames[(int)button];
        }

        /// <inheritdoc />
        protected override void FinishSetup()
        {
            _buttons = new ButtonControl[ControlNames.Length];
            for (int i = 0; i < ControlNames.Length; i++)
            {
                _buttons[i] = GetChildControl<ButtonControl>(ControlNames[i]);
            }

            base.FinishSetup();
        }

        /// <summary>
        /// Registers the layout so bindings resolve and the device can be added. Safe to call more than once.
        /// </summary>
        public static void RegisterLayout()
        {
            InputSystem.RegisterLayout<TouchPadDevice>(LayoutName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterAtStartup()
        {
            RegisterLayout();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void RegisterInEditor()
        {
            RegisterLayout();
        }
#endif
    }
}
