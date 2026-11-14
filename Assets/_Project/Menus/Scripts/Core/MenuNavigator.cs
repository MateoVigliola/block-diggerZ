using System.Collections.Generic;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Page history for one menu screen. Opening a page pushes it; Back (Esc or the Back button) pops it and
    /// reports false when already at the root, so the screen can decide what that means (close the pause menu,
    /// or nothing on the main menu).
    /// </summary>
    public sealed class MenuNavigator
    {
        private readonly List<MenuPage> _history = new();

        /// <summary>Starts at <see cref="MenuPage.Root"/>.</summary>
        public MenuNavigator()
        {
            _history.Add(MenuPage.Root);
        }

        /// <summary>The page on top.</summary>
        public MenuPage Current => _history[_history.Count - 1];

        /// <summary>True when only the root page is open.</summary>
        public bool IsAtRoot => _history.Count == 1;

        /// <summary>Opens <paramref name="page"/> on top of the current one; opening the current page does nothing.</summary>
        public void Open(MenuPage page)
        {
            if (page != Current)
            {
                _history.Add(page);
            }
        }

        /// <summary>Goes back one page. Returns false (and stays) when already at the root.</summary>
        public bool Back()
        {
            if (IsAtRoot)
            {
                return false;
            }

            _history.RemoveAt(_history.Count - 1);
            return true;
        }

        /// <summary>Drops every page above the root.</summary>
        public void ResetToRoot()
        {
            _history.RemoveRange(1, _history.Count - 1);
        }
    }
}
