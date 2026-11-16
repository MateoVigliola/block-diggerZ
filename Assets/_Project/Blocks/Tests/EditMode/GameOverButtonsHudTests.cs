using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks.Tests
{
    /// <summary>
    /// The game over panel has Restart and Main menu buttons (needed on touch screens), and the STORE panel splits
    /// into a timer group and a store group so the portrait layout can put STORE above TIME.
    /// </summary>
    public sealed class GameOverButtonsHudTests
    {
        private const string HudUxmlPath = "Assets/_Project/Blocks/UI/BlocksHud.uxml";
        private const string TimerGroupName = "timer-group";
        private const string StoreGroupName = "store-group";

        [TestCase(BlocksHud.RestartButtonName)]
        [TestCase(BlocksHud.GameOverMainMenuButtonName)]
        public void GameOverPanel_HasButton(string buttonName)
        {
            VisualElement root = LoadHud();
            VisualElement panel = root.Q<VisualElement>(BlocksHud.GameOverPanelName);

            Assert.That(panel.Q<Button>(buttonName), Is.Not.Null);
        }

        [Test]
        public void StorePanel_HasTimerThenStoreGroup()
        {
            VisualElement root = LoadHud();
            VisualElement store = root.Q<VisualElement>(BlocksHud.StorePanelName);
            VisualElement timer = store.Q<VisualElement>(TimerGroupName);
            VisualElement storeGroup = store.Q<VisualElement>(StoreGroupName);

            Assert.That(timer.parent, Is.SameAs(store));
            Assert.That(storeGroup.parent, Is.SameAs(store));
            Assert.That(store.IndexOf(timer), Is.LessThan(store.IndexOf(storeGroup)));
            Assert.That(timer.Q<Label>(BlocksHud.TimerValueName), Is.Not.Null);
            Assert.That(storeGroup.Q<VisualElement>(BlocksHud.StorePreviewName), Is.Not.Null);
        }

        private static VisualElement LoadHud()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudUxmlPath);
            Assert.That(tree, Is.Not.Null);
            return tree.Instantiate();
        }
    }
}
