using System;
using UnityEngine.UIElements;

namespace BlockDiggerz.Menus
{
    /// <summary>Small UI Toolkit helpers shared by the menu screens.</summary>
    public static class MenuUi
    {
        /// <summary>Shows or hides an element (display flex or none); ignores null.</summary>
        public static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        /// <summary>Adds a click handler to the named button under <paramref name="root"/>; returns the button or null.</summary>
        public static Button BindButton(VisualElement root, string name, Action onClick)
        {
            Button button = root.Q<Button>(name);
            if (button != null)
            {
                button.clicked += onClick;
            }

            return button;
        }
    }
}
