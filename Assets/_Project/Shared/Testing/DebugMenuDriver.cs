using System.Linq;
using BlockDiggerz.DebugTools;
using NUnit.Framework;

namespace BlockDiggerz.Testing
{
    /// <summary>
    /// Lets tests set up state through the same debug menu controls the CLI and the in-game menu use, so test
    /// setup and manual setup never drift apart.
    /// </summary>
    public static class DebugMenuDriver
    {
        /// <summary>Presses a debug button.</summary>
        public static void Press(string section, string label)
        {
            DebugControl control = Find(section, label);
            Assert.IsNotNull(control.OnClick, $"Debug control '{section}/{label}' is not a button");
            control.OnClick();
        }

        /// <summary>Sets a debug toggle.</summary>
        public static void Set(string section, string label, bool value)
        {
            DebugControl control = Find(section, label);
            Assert.IsNotNull(control.SetBool, $"Debug control '{section}/{label}' is not a toggle");
            control.SetBool(value);
        }

        /// <summary>Reads a debug readout.</summary>
        public static string Read(string section, string label)
        {
            DebugControl control = Find(section, label);
            Assert.IsNotNull(control.GetText, $"Debug control '{section}/{label}' is not a readout");
            return control.GetText();
        }

        private static DebugControl Find(string section, string label)
        {
            DebugControl control = DebugMenu.Sections.Where(s => s.Title == section).SelectMany(s => s.Controls)
                .FirstOrDefault(c => c.Label == label);
            Assert.IsNotNull(control, $"No debug control '{label}' in section '{section}'");
            return control;
        }
    }
}
