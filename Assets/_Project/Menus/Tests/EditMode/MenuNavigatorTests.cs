using NUnit.Framework;

namespace BlockDiggerz.Menus.Tests
{
    public class MenuNavigatorTests
    {
        [Test]
        public void StartsAtRoot_AndBackAtRootReportsFalse()
        {
            var navigator = new MenuNavigator();

            Assert.AreEqual(MenuPage.Root, navigator.Current);
            Assert.IsTrue(navigator.IsAtRoot);
            Assert.IsFalse(navigator.Back(), "at the root, Esc closes the pause menu instead");
        }

        [Test]
        public void BackFromSettings_ReturnsToRoot()
        {
            var navigator = new MenuNavigator();
            navigator.Open(MenuPage.Settings);

            Assert.AreEqual(MenuPage.Settings, navigator.Current);
            Assert.IsTrue(navigator.Back());
            Assert.AreEqual(MenuPage.Root, navigator.Current);
        }

        [Test]
        public void OpeningCurrentPageTwice_NeedsOnlyOneBack()
        {
            var navigator = new MenuNavigator();
            navigator.Open(MenuPage.Settings);
            navigator.Open(MenuPage.Settings);

            Assert.IsTrue(navigator.Back());
            Assert.IsTrue(navigator.IsAtRoot);
        }

        [Test]
        public void ResetToRoot_DropsOpenPages()
        {
            var navigator = new MenuNavigator();
            navigator.Open(MenuPage.Settings);

            navigator.ResetToRoot();

            Assert.IsTrue(navigator.IsAtRoot);
        }
    }
}
