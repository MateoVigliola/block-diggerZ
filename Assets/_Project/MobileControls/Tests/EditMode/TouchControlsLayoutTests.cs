using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace BlockDiggerz.MobileControls.Tests
{
    /// <summary>The touch layout holds every element the controls look up, and taps can reach the buttons.</summary>
    public sealed class TouchControlsLayoutTests
    {
        private const string UxmlPath = "Assets/_Project/MobileControls/UI/MobileControls.uxml";

        [TestCase(TouchControls.RootName)]
        [TestCase(TouchControls.MoveLeftName)]
        [TestCase(TouchControls.MoveRightName)]
        [TestCase(TouchControls.HardDropName)]
        [TestCase(TouchControls.RotateClockwiseName)]
        [TestCase(TouchControls.RotateCounterClockwiseName)]
        [TestCase(TouchControls.StoreSlotName)]
        [TestCase(TouchControls.PauseName)]
        [TestCase(TouchControls.StatsSlotName)]
        [TestCase(TouchControls.BoardAreaName)]
        public void Layout_HasElement(string elementName)
        {
            VisualElement root = LoadLayout();
            Assert.That(root.Q<VisualElement>(elementName), Is.Not.Null);
        }

        [TestCase(TouchControls.MoveLeftName)]
        [TestCase(TouchControls.MoveRightName)]
        [TestCase(TouchControls.HardDropName)]
        [TestCase(TouchControls.RotateClockwiseName)]
        [TestCase(TouchControls.RotateCounterClockwiseName)]
        [TestCase(TouchControls.StoreSlotName)]
        [TestCase(TouchControls.PauseName)]
        [TestCase(TouchControls.BoardAreaName)]
        public void TouchTargets_ArePickable(string elementName)
        {
            VisualElement root = LoadLayout();
            Assert.That(root.Q<VisualElement>(elementName).pickingMode, Is.EqualTo(PickingMode.Position));
        }

        [Test]
        public void HardDrop_SitsRightBelowBoardArea()
        {
            VisualElement root = LoadLayout();
            VisualElement boardArea = root.Q<VisualElement>(TouchControls.BoardAreaName);
            VisualElement hardDrop = root.Q<VisualElement>(TouchControls.HardDropName);
            VisualElement parent = boardArea.parent;

            Assert.That(hardDrop.parent.parent, Is.SameAs(parent));
            Assert.That(parent.IndexOf(hardDrop.parent), Is.EqualTo(parent.IndexOf(boardArea) + 1));
        }

        private static VisualElement LoadLayout()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            Assert.That(tree, Is.Not.Null);
            return tree.Instantiate();
        }
    }
}
