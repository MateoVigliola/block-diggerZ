using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace BlockDiggerz.Input
{
    /// <summary>
    /// Memory layout of <see cref="TouchPadDevice"/>: one bit per <see cref="TouchPadButton"/>. The on-screen
    /// controls queue this state through the Input System, so the game's actions see the buttons like any device.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = StateSizeInBytes)]
    public struct TouchPadState : IInputStateTypeInfo
    {
        private const int StateSizeInBytes = 4;
        private const string ButtonLayout = "Button";

        /// <summary>Format code of the state ("BDTP").</summary>
        public static FourCC Format => new('B', 'D', 'T', 'P');

        /// <summary>Pressed buttons, one bit each (see <see cref="TouchPadMask"/>).</summary>
        [InputControl(name = TouchPadDevice.MoveLeftControl, layout = ButtonLayout, bit = (uint)TouchPadButton.MoveLeft, displayName = "Touch Left")]
        [InputControl(name = TouchPadDevice.MoveRightControl, layout = ButtonLayout, bit = (uint)TouchPadButton.MoveRight, displayName = "Touch Right")]
        [InputControl(name = TouchPadDevice.HardDropControl, layout = ButtonLayout, bit = (uint)TouchPadButton.HardDrop, displayName = "Touch Drop")]
        [InputControl(name = TouchPadDevice.RotateClockwiseControl, layout = ButtonLayout, bit = (uint)TouchPadButton.RotateClockwise, displayName = "Touch CW")]
        [InputControl(name = TouchPadDevice.RotateCounterClockwiseControl, layout = ButtonLayout, bit = (uint)TouchPadButton.RotateCounterClockwise, displayName = "Touch CCW")]
        [InputControl(name = TouchPadDevice.StorePieceControl, layout = ButtonLayout, bit = (uint)TouchPadButton.StorePiece, displayName = "Touch Store")]
        [InputControl(name = TouchPadDevice.PauseControl, layout = ButtonLayout, bit = (uint)TouchPadButton.Pause, displayName = "Touch Pause")]
        [FieldOffset(0)]
        public uint Buttons;

        /// <inheritdoc />
        public FourCC format => Format;
    }
}
