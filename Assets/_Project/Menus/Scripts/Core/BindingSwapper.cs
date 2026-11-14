using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Assigns a new key to one keyboard binding. If another keyboard binding of the given actions already uses
    /// that key, it takes over the key being replaced, so no key does two things and nothing is left unbound.
    /// </summary>
    public static class BindingSwapper
    {
        /// <summary>Binding group (control scheme) of keyboard bindings in GameControls.</summary>
        public const string KeyboardGroup = "Keyboard";

        /// <summary>True for a plain (non-composite) binding in the keyboard group.</summary>
        public static bool IsKeyboardBinding(InputBinding binding)
        {
            return !binding.isComposite && !binding.isPartOfComposite
                   && !string.IsNullOrEmpty(binding.groups)
                   && binding.groups.Contains(KeyboardGroup);
        }

        /// <summary>
        /// Gives <paramref name="action"/>'s binding <paramref name="bindingIndex"/> the control at
        /// <paramref name="newPath"/>. Returns the action whose binding was swapped onto the old key, or null if no
        /// other binding used <paramref name="newPath"/>.
        /// </summary>
        public static InputAction Assign(IReadOnlyList<InputAction> actions, InputAction action, int bindingIndex, string newPath)
        {
            string oldPath = action.bindings[bindingIndex].effectivePath;
            if (SamePath(oldPath, newPath))
            {
                return null;
            }

            InputAction swapped = FindAndSwap(actions, action, bindingIndex, newPath, oldPath);
            action.ApplyBindingOverride(bindingIndex, newPath);
            return swapped;
        }

        private static InputAction FindAndSwap(IReadOnlyList<InputAction> actions, InputAction action, int bindingIndex,
            string newPath, string oldPath)
        {
            for (int a = 0; a < actions.Count; a++)
            {
                InputAction other = actions[a];
                for (int i = 0; i < other.bindings.Count; i++)
                {
                    bool isTarget = other == action && i == bindingIndex;
                    InputBinding binding = other.bindings[i];
                    if (isTarget || !IsKeyboardBinding(binding) || !SamePath(binding.effectivePath, newPath))
                    {
                        continue;
                    }

                    other.ApplyBindingOverride(i, oldPath);
                    return other;
                }
            }

            return null;
        }

        private static bool SamePath(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
