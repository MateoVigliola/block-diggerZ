using NUnit.Framework;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus.Tests
{
    public class GameSettingsTests
    {
        private MemoryKeyValueStore _store;
        private TestActions _actions;

        [SetUp]
        public void SetUp()
        {
            _store = new MemoryKeyValueStore();
            _actions = new TestActions();
        }

        [TearDown]
        public void TearDown()
        {
            _actions.Destroy();
        }

        [Test]
        public void Volumes_DefaultTo80_WhenNothingSaved()
        {
            var settings = new GameSettings(_store);

            Assert.AreEqual(GameSettings.DefaultVolume, settings.MusicVolume);
            Assert.AreEqual(GameSettings.DefaultVolume, settings.SfxVolume);
        }

        [Test]
        public void Volumes_AreSavedClamped_AndReloaded()
        {
            var settings = new GameSettings(_store);
            settings.SetMusicVolume(150f);
            settings.SetSfxVolume(25f);

            var reloaded = new GameSettings(_store);

            Assert.AreEqual(GameSettings.MaxVolume, reloaded.MusicVolume);
            Assert.AreEqual(25f, reloaded.SfxVolume);
            Assert.Greater(_store.SaveCount, 0);
        }

        [Test]
        public void ResetVolumes_RestoresDefault()
        {
            var settings = new GameSettings(_store);
            settings.SetMusicVolume(10f);

            settings.ResetVolumes();

            Assert.AreEqual(GameSettings.DefaultVolume, new GameSettings(_store).MusicVolume);
        }

        [Test]
        public void Bindings_SaveAndLoad_RestoresOverrides()
        {
            var settings = new GameSettings(_store);
            BindingSwapper.Assign(_actions.All, _actions.Rotate, 0, "<Keyboard>/w");
            settings.SaveBindings(_actions.Asset);
            _actions.Asset.RemoveAllBindingOverrides();
            Assert.AreEqual("<Keyboard>/e", _actions.Rotate.bindings[0].effectivePath, "overrides cleared, like a new session");

            new GameSettings(_store).LoadBindings(_actions.Asset);

            Assert.AreEqual("<Keyboard>/w", _actions.Rotate.bindings[0].effectivePath);
            Assert.AreEqual("<Keyboard>/e", _actions.HardDrop.bindings[0].effectivePath);
        }

        [Test]
        public void LoadBindings_WithNothingSaved_ClearsOverrides()
        {
            BindingSwapper.Assign(_actions.All, _actions.Rotate, 0, "<Keyboard>/f");

            new GameSettings(_store).LoadBindings(_actions.Asset);

            Assert.AreEqual("<Keyboard>/e", _actions.Rotate.bindings[0].effectivePath);
        }

        [Test]
        public void ResetBindings_RemovesOverridesAndSavedJson()
        {
            var settings = new GameSettings(_store);
            BindingSwapper.Assign(_actions.All, _actions.Rotate, 0, "<Keyboard>/f");
            settings.SaveBindings(_actions.Asset);

            settings.ResetBindings(_actions.Asset);

            Assert.AreEqual("<Keyboard>/e", _actions.Rotate.bindings[0].effectivePath);
            Assert.IsFalse(_store.Values.ContainsKey(GameSettings.BindingsKey));
        }
    }
}
