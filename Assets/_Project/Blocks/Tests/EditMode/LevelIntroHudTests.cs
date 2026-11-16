using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks.Tests
{
    /// <summary>Checks the HUD layout holds the "Clear the dirt!" level start message.</summary>
    public sealed class LevelIntroHudTests
    {
        private const string HudUxmlPath = "Assets/_Project/Blocks/UI/BlocksHud.uxml";
        private const string ExpectedText = "Clear the dirt!";

        [Test]
        public void HudLayout_HasLevelIntroMessage()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HudUxmlPath);
            Assert.That(tree, Is.Not.Null);

            VisualElement root = tree.Instantiate();
            VisualElement intro = root.Q<VisualElement>(BlocksHud.LevelIntroName);
            Assert.That(intro, Is.Not.Null);

            Label label = intro.Q<Label>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Is.EqualTo(ExpectedText));
        }
    }
}
