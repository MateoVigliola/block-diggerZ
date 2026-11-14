using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Keyboard key slots of gameplay actions: each action has a Primary and an Alternate slot (its first two
    /// keyboard bindings). Assigning a key swaps it with any other slot that already used it, so no key does two
    /// things. A slot can be cleared. Whenever a Primary slot ends up empty while its Alternate has a key, the
    /// Alternate key moves up.
    /// </summary>
    public static class BindingSwapper
    {
        /// <summary>Binding group (control scheme) of keyboard bindings in GameControls.</summary>
        public const string KeyboardGroup = "Keyboard";

        /// <summary>Number of key slots shown per action: Primary and Alternate.</summary>
        public const int SlotCount = 2;

        /// <summary>Slot number of the Primary key.</summary>
        public const int PrimarySlot = 0;

        /// <summary>Slot number of the Alternate key.</summary>
        public const int AlternateSlot = 1;

        /// <summary>Binding index meaning the action has no binding for that slot.</summary>
        public const int NoBinding = -1;

        /// <summary>True for a plain (non-composite) binding in the keyboard group.</summary>
        public static bool IsKeyboardBinding(InputBinding binding)
        {
            return !binding.isComposite && !binding.isPartOfComposite
                   && !string.IsNullOrEmpty(binding.groups)
                   && binding.groups.Contains(KeyboardGroup);
        }

        /// <summary>True when the binding has no key (empty slot).</summary>
        public static bool IsEmpty(InputBinding binding) => string.IsNullOrEmpty(binding.effectivePath);

        /// <summary>
        /// Binding index for <paramref name="slot"/> of <paramref name="action"/> (its nth keyboard binding), or
        /// <see cref="NoBinding"/> if the action has fewer keyboard bindings.
        /// </summary>
        public static int GetBindingIndex(InputAction action, int slot)
        {
            int found = 0;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!IsKeyboardBinding(action.bindings[i]))
                {
                    continue;
                }

                if (found == slot)
                {
                    return i;
                }

                found++;
            }

            return NoBinding;
        }

        /// <summary>
        /// Gives <paramref name="action"/>'s binding <paramref name="bindingIndex"/> the control at
        /// <paramref name="newPath"/>. Returns the action whose binding was swapped onto the old key (which may be
        /// empty), or null if no other binding used <paramref name="newPath"/>.
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
            if (swapped != null)
            {
                PromoteAlternate(swapped);
            }

            PromoteAlternate(action);
            return swapped;
        }

        /// <summary>Removes the key from <paramref name="action"/>'s binding <paramref name="bindingIndex"/>.</summary>
        public static void Clear(InputAction action, int bindingIndex)
        {
            action.ApplyBindingOverride(bindingIndex, string.Empty);
            PromoteAlternate(action);
        }

        private static void PromoteAlternate(InputAction action)
        {
            int primary = GetBindingIndex(action, PrimarySlot);
            int alternate = GetBindingIndex(action, AlternateSlot);
            if (primary == NoBinding || alternate == NoBinding)
            {
                return;
            }

            if (!IsEmpty(action.bindings[primary]) || IsEmpty(action.bindings[alternate]))
            {
                return;
            }

            action.ApplyBindingOverride(primary, action.bindings[alternate].effectivePath);
            action.ApplyBindingOverride(alternate, string.Empty);
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

                    other.ApplyBindingOverride(i, oldPath ?? string.Empty);
                    return other;
                }
            }

            return null;
        }

        private static bool SamePath(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
