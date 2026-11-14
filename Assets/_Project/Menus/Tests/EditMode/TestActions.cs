using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus.Tests
{
    /// <summary>
    /// A small actions asset like GameControls, built in code for tests. MoveLeft has two keys, HardDrop has one key
    /// and an empty Alternate slot, RotateClockwise has only one keyboard binding.
    /// </summary>
    internal sealed class TestActions
    {
        public const string Keyboard = BindingSwapper.KeyboardGroup;
        public const string Gamepad = "Gamepad";
        public const string Unbound = "";

        public readonly InputActionAsset Asset;
        public readonly InputAction MoveLeft;
        public readonly InputAction HardDrop;
        public readonly InputAction Rotate;

        public TestActions()
        {
            Asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap map = Asset.AddActionMap("Gameplay");
            MoveLeft = map.AddAction("MoveLeft");
            MoveLeft.AddBinding("<Keyboard>/a", groups: Keyboard);
            MoveLeft.AddBinding("<Keyboard>/leftArrow", groups: Keyboard);
            MoveLeft.AddBinding("<Gamepad>/dpad/left", groups: Gamepad);
            HardDrop = map.AddAction("HardDrop");
            HardDrop.AddBinding("<Keyboard>/w", groups: Keyboard);
            HardDrop.AddBinding(Unbound, groups: Keyboard);
            HardDrop.AddBinding("<Gamepad>/dpad/up", groups: Gamepad);
            Rotate = map.AddAction("RotateClockwise");
            Rotate.AddBinding("<Keyboard>/e", groups: Keyboard);
        }

        public InputAction[] All => new[] { MoveLeft, HardDrop, Rotate };

        public void Destroy()
        {
            Object.DestroyImmediate(Asset);
        }
    }
}
