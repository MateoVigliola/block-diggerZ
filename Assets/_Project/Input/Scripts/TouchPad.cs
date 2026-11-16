using UnityEngine.InputSystem;

namespace BlockDiggerz.Input
{
    /// <summary>
    /// Owns the <see cref="TouchPadDevice"/> while the touch controls are shown and presses its buttons by queuing
    /// state events, exactly as hardware would. Plain class; the touch controls component creates one.
    /// </summary>
    public sealed class TouchPad
    {
        private TouchPadDevice _device;
        private uint _buttons = TouchPadMask.None;

        /// <summary>The device, or null while disconnected.</summary>
        public TouchPadDevice Device => _device;

        /// <summary>True while the device is in the Input System.</summary>
        public bool IsConnected => _device != null && _device.added;

        /// <summary>Pressed buttons as last sent (see <see cref="TouchPadMask"/>).</summary>
        public uint Buttons => _buttons;

        /// <summary>Adds the device (or reuses one left over from an earlier session) with no button pressed.</summary>
        public void Connect()
        {
            if (IsConnected)
            {
                return;
            }

            TouchPadDevice.RegisterLayout();
            _device = InputSystem.GetDevice<TouchPadDevice>() ?? InputSystem.AddDevice<TouchPadDevice>();
            _buttons = TouchPadMask.None;
            Send();
        }

        /// <summary>Releases every button and removes the device.</summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                InputSystem.RemoveDevice(_device);
            }

            _device = null;
            _buttons = TouchPadMask.None;
        }

        /// <summary>Presses or releases <paramref name="button"/>. Ignored while disconnected or unchanged.</summary>
        public void SetPressed(TouchPadButton button, bool pressed)
        {
            uint next = TouchPadMask.Set(_buttons, button, pressed);
            if (next == _buttons || !IsConnected)
            {
                return;
            }

            _buttons = next;
            Send();
        }

        /// <summary>Releases every button.</summary>
        public void ReleaseAll()
        {
            if (_buttons == TouchPadMask.None || !IsConnected)
            {
                return;
            }

            _buttons = TouchPadMask.None;
            Send();
        }

        private void Send()
        {
            InputSystem.QueueStateEvent(_device, new TouchPadState { Buttons = _buttons });
        }
    }
}
