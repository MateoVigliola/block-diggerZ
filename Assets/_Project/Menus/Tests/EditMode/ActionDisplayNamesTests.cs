using NUnit.Framework;

namespace BlockDiggerz.Menus.Tests
{
    public class ActionDisplayNamesTests
    {
        [TestCase("MoveLeft", "Move Left")]
        [TestCase("RotateCounterClockwise", "Rotate Counter Clockwise")]
        [TestCase("Restart", "Restart")]
        [TestCase("", "")]
        public void ToLabel_SplitsWords(string actionName, string expected)
        {
            Assert.AreEqual(expected, ActionDisplayNames.ToLabel(actionName));
        }
    }
}
