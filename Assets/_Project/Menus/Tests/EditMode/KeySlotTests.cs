using NUnit.Framework;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus.Tests
{
    /// <summary>Primary and Alternate key slots: lookup, empty slots, clearing and moving the Alternate up.</summary>
    public class KeySlotTests
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

        private static string Key(InputAction action, int slot)
        {
            return action.bindings[BindingSwapper.GetBindingIndex(action, slot)].effectivePath ?? string.Empty;
        }

        [Test]
        public void GetBindingIndex_CountsOnlyKeyboardBindings()
        {
            Assert.AreEqual(0, BindingSwapper.GetBindingIndex(_actions.MoveLeft, BindingSwapper.PrimarySlot));
            Assert.AreEqual(1, BindingSwapper.GetBindingIndex(_actions.MoveLeft, BindingSwapper.AlternateSlot));
            Assert.AreEqual(1, BindingSwapper.GetBindingIndex(_actions.HardDrop, BindingSwapper.AlternateSlot));
            Assert.AreEqual(BindingSwapper.NoBinding, BindingSwapper.GetBindingIndex(_actions.Rotate, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void EmptyAlternate_TakesFreeKey()
        {
            BindingSwapper.Assign(_actions.All, _actions.HardDrop, 1, "<Keyboard>/f");

            Assert.AreEqual("<Keyboard>/w", Key(_actions.HardDrop, BindingSwapper.PrimarySlot));
            Assert.AreEqual("<Keyboard>/f", Key(_actions.HardDrop, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void KeyTakenFromAnotherPrimary_IntoEmptySlot_MovesThatActionsAlternateUp()
        {
            InputAction swapped = BindingSwapper.Assign(_actions.All, _actions.HardDrop, 1, "<Keyboard>/a");

            Assert.AreEqual(_actions.MoveLeft, swapped);
            Assert.AreEqual("<Keyboard>/a", Key(_actions.HardDrop, BindingSwapper.AlternateSlot));
            Assert.AreEqual("<Keyboard>/leftArrow", Key(_actions.MoveLeft, BindingSwapper.PrimarySlot));
            Assert.AreEqual(string.Empty, Key(_actions.MoveLeft, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void KeyTakenFromAnotherAlternate_IntoEmptySlot_LeavesThatAlternateEmpty()
        {
            BindingSwapper.Assign(_actions.All, _actions.HardDrop, 1, "<Keyboard>/leftArrow");

            Assert.AreEqual("<Keyboard>/a", Key(_actions.MoveLeft, BindingSwapper.PrimarySlot));
            Assert.AreEqual(string.Empty, Key(_actions.MoveLeft, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void ClearAlternate_LeavesPrimary()
        {
            BindingSwapper.Clear(_actions.MoveLeft, 1);

            Assert.AreEqual("<Keyboard>/a", Key(_actions.MoveLeft, BindingSwapper.PrimarySlot));
            Assert.AreEqual(string.Empty, Key(_actions.MoveLeft, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void ClearPrimary_MovesAlternateUp()
        {
            BindingSwapper.Clear(_actions.MoveLeft, 0);

            Assert.AreEqual("<Keyboard>/leftArrow", Key(_actions.MoveLeft, BindingSwapper.PrimarySlot));
            Assert.AreEqual(string.Empty, Key(_actions.MoveLeft, BindingSwapper.AlternateSlot));
        }

        [Test]
        public void ClearOnlyKey_LeavesActionUnbound()
        {
            BindingSwapper.Clear(_actions.HardDrop, 0);

            Assert.IsTrue(BindingSwapper.IsEmpty(_actions.HardDrop.bindings[0]));
            Assert.IsTrue(BindingSwapper.IsEmpty(_actions.HardDrop.bindings[1]));
        }

        [Test]
        public void ClearedKey_SurvivesSaveAndLoad()
        {
            var store = new MemoryKeyValueStore();
            var settings = new GameSettings(store);
            BindingSwapper.Clear(_actions.HardDrop, 0);
            settings.SaveBindings(_actions.Asset);
            _actions.Asset.RemoveAllBindingOverrides();

            new GameSettings(store).LoadBindings(_actions.Asset);

            Assert.IsTrue(BindingSwapper.IsEmpty(_actions.HardDrop.bindings[0]));
        }
    }
}
