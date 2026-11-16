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
        [TestCase(TouchControls.RotateOverlayName)]
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

        [Test]
        public void MoveZones_AreTheLeftAndRightHalvesOfTheBoardArea()
        {
            VisualElement root = LoadLayout();
            VisualElement boardArea = root.Q<VisualElement>(TouchControls.BoardAreaName);

            Assert.That(boardArea[0].name, Is.EqualTo(TouchControls.MoveLeftName));
            Assert.That(boardArea[1].name, Is.EqualTo(TouchControls.MoveRightName));
            Assert.That(boardArea.pickingMode, Is.EqualTo(PickingMode.Ignore));
        }

        [Test]
        public void GestureArea_IsInsideTheBoardAreaAndPickable()
        {
            VisualElement root = LoadLayout();
            VisualElement boardArea = root.Q<VisualElement>(TouchControls.BoardAreaName);
            VisualElement gestureArea = root.Q<VisualElement>(TouchControls.GestureAreaName);

            Assert.That(gestureArea, Is.Not.Null);
            Assert.That(gestureArea.parent, Is.SameAs(boardArea));
            Assert.That(gestureArea.pickingMode, Is.EqualTo(PickingMode.Position));
            Assert.That(boardArea.childCount, Is.EqualTo(3));
        }

        [Test]
        public void RotateOverlay_SitsOutsideTheControlsRootAndBlocksTaps()
        {
            VisualElement root = LoadLayout();
            VisualElement overlay = root.Q<VisualElement>(TouchControls.RotateOverlayName);
            VisualElement controlsRoot = root.Q<VisualElement>(TouchControls.RootName);

            Assert.That(controlsRoot.Contains(overlay), Is.False);
            Assert.That(overlay.pickingMode, Is.EqualTo(PickingMode.Position));
        }

        [Test]
        public void BottomRow_IsRotateDropRotate()
        {
            VisualElement root = LoadLayout();
            VisualElement row = root.Q<VisualElement>(TouchControls.HardDropName).parent;

            Assert.That(row.childCount, Is.EqualTo(3));
            Assert.That(row[0].name, Is.EqualTo(TouchControls.RotateCounterClockwiseName));
            Assert.That(row[1].name, Is.EqualTo(TouchControls.HardDropName));
            Assert.That(row[2].name, Is.EqualTo(TouchControls.RotateClockwiseName));
        }

        private static VisualElement LoadLayout()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            Assert.That(tree, Is.Not.Null);
            return tree.Instantiate();
        }
    }
}
