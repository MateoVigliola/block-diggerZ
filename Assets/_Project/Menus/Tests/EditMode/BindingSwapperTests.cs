using NUnit.Framework;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus.Tests
{
    public class BindingSwapperTests
    {
        private TestActions _actions;

        [SetUp]
        public void SetUp()
        {
            _actions = new TestActions();
        }

        [TearDown]
        public void TearDown()
        {
            _actions.Destroy();
        }

        [Test]
        public void FreeKey_IsAssigned_WithNoSwap()
        {
            InputAction swapped = BindingSwapper.Assign(_actions.All, _actions.Rotate, 0, "<Keyboard>/f");

            Assert.IsNull(swapped);
            Assert.AreEqual("<Keyboard>/f", _actions.Rotate.bindings[0].effectivePath);
        }

        [Test]
        public void KeyUsedByAnotherAction_SwapsKeys()
        {
            InputAction swapped = BindingSwapper.Assign(_actions.All, _actions.Rotate, 0, "<Keyboard>/w");

            Assert.AreEqual(_actions.HardDrop, swapped);
            Assert.AreEqual("<Keyboard>/w", _actions.Rotate.bindings[0].effectivePath);
            Assert.AreEqual("<Keyboard>/e", _actions.HardDrop.bindings[0].effectivePath, "hard drop takes the replaced key");
        }

        [Test]
        public void KeyUsedBySameActionsOtherBinding_SwapsWithinAction()
        {
            BindingSwapper.Assign(_actions.All, _actions.MoveLeft, 1, "<Keyboard>/a");

            Assert.AreEqual("<Keyboard>/a", _actions.MoveLeft.bindings[1].effectivePath);
            Assert.AreEqual("<Keyboard>/leftArrow", _actions.MoveLeft.bindings[0].effectivePath);
        }

        [Test]
        public void SameKey_ChangesNothing()
        {
            InputAction swapped = BindingSwapper.Assign(_actions.All, _actions.HardDrop, 0, "<Keyboard>/w");

            Assert.IsNull(swapped);
            Assert.IsTrue(string.IsNullOrEmpty(_actions.HardDrop.bindings[0].overridePath));
        }

        [Test]
        public void GamepadBindings_AreNotKeyboardBindings()
        {
            Assert.IsTrue(BindingSwapper.IsKeyboardBinding(_actions.MoveLeft.bindings[0]));
            Assert.IsFalse(BindingSwapper.IsKeyboardBinding(_actions.MoveLeft.bindings[2]));
        }
    }
}
