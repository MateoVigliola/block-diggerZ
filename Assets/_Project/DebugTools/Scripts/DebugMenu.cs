using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BlockDiggerz.DebugTools
{
    /// <summary>
    /// Registry for the in-game debug menu. Features call <see cref="Register"/> (usually in OnEnable)
    /// to add a section of controls, and <see cref="Unregister"/> (in OnDisable) to remove it.
    /// Both calls are compiled out of release builds, including the lambdas passed to them,
    /// so callers need no #if blocks. The same registry is driven from the CLI by DebugMenuCommands.
    /// </summary>
    public static class DebugMenu
    {
        private static readonly List<DebugSection> _sections = new();

        /// <summary>Raised when sections are added or removed, so the menu can rebuild.</summary>
        public static event Action SectionsChanged;

        /// <summary>All registered sections, in registration order.</summary>
        public static IReadOnlyList<DebugSection> Sections => _sections;

        /// <summary>
        /// Adds a section of controls under <paramref name="title"/>, owned by <paramref name="owner"/>.
        /// Several owners can share a title; their controls are shown together.
        /// </summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
        public static void Register(object owner, string title, Action<DebugSection> build)
        {
            if (owner == null || build == null)
            {
                return;
            }

            var section = new DebugSection(owner, title);
            build(section);
            _sections.Add(section);
            SectionsChanged?.Invoke();
        }

        /// <summary>Removes every section registered by <paramref name="owner"/>.</summary>
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
        public static void Unregister(object owner)
        {
            if (_sections.RemoveAll(s => s.Owner == owner) > 0)
            {
                SectionsChanged?.Invoke();
            }
        }

        /// <summary>Clears stale sections when entering Play mode with domain reload disabled.</summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _sections.Clear();
            SectionsChanged = null;
        }
    }
}
