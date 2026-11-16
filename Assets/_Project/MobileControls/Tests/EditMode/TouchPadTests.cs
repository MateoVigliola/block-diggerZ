using BlockDiggerz.Input;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>
    /// The virtual touch pad: mask bits, each button's control on a real device, and that GameControls binds every
    /// touch button (group Touch) to its action.
    /// </summary>
    public sealed class TouchPadTests
    {
        private const string ControlsPath = "Assets/_Project/Input/Data/GameControls.inputactions";

        [Test]
        public void Mask_SetAndClear_KeepsOtherButtons()
        {
            uint mask = TouchPadMask.Set(TouchPadMask.None, TouchPadButton.MoveLeft, true);
            mask = TouchPadMask.Set(mask, TouchPadButton.HardDrop, true);
            mask = TouchPadMask.Set(mask, TouchPadButton.MoveLeft, false);

            Assert.That(TouchPadMask.IsPressed(mask, TouchPadButton.MoveLeft), Is.False);
            Assert.That(TouchPadMask.IsPressed(mask, TouchPadButton.HardDrop), Is.True);
            Assert.That(TouchPadMask.IsPressed(mask, TouchPadButton.Pause), Is.False);
        }

        [Test]
        public void Device_EachBit_PressesItsOwnControl()
        {
            TouchPadDevice.RegisterLayout();
            var device = InputSystem.AddDevice<TouchPadDevice>();
            try
            {
                foreach (TouchPadButton button in System.Enum.GetValues(typeof(TouchPadButton)))
                {
                    InputState.Change(device, new TouchPadState { Buttons = TouchPadMask.Bit(button) });
                    foreach (TouchPadButton other in System.Enum.GetValues(typeof(TouchPadButton)))
                    {
                        Assert.That(device.GetButton(other).isPressed, Is.EqualTo(other == button), $"{button} pressed, checking {other}");
                    }

                    Assert.That(device.GetButton(button).name, Is.EqualTo(TouchPadDevice.ControlName(button)));
                }
            }
            finally
            {
                InputSystem.RemoveDevice(device);
            }
        }

        [TestCase("MoveLeft", TouchPadButton.MoveLeft)]
        [TestCase("MoveRight", TouchPadButton.MoveRight)]
        [TestCase("HardDrop", TouchPadButton.HardDrop)]
        [TestCase("RotateClockwise", TouchPadButton.RotateClockwise)]
        [TestCase("RotateCounterClockwise", TouchPadButton.RotateCounterClockwise)]
        [TestCase("StorePiece", TouchPadButton.StorePiece)]
        [TestCase("Pause", TouchPadButton.Pause)]
        public void GameControls_BindsTouchButton(string actionName, TouchPadButton button)
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            Assert.That(asset, Is.Not.Null);
            InputAction action = asset.FindAction(actionName, true);
            string path = $"<{TouchPadDevice.LayoutName}>/{TouchPadDevice.ControlName(button)}";

            bool found = false;
            foreach (InputBinding binding in action.bindings)
            {
                if (binding.path == path)
                {
                    found = true;
                    Assert.That(binding.groups, Does.Contain(GameInput.TouchGroup));
                }
            }

            Assert.That(found, Is.True, $"{actionName} has no binding {path}");
        }
    }
}
