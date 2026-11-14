using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Unity.Pipeline.Commands;

namespace BlockDiggerz.DebugTools.Editor
{
    /// <summary>
    /// Drives the in-game debug menu from the CLI: lists every section's controls with their current values, and
    /// presses a button or sets a toggle or slider by section and label, for example
    /// `unity command debugmenu do --section Blocks --label "Pause gravity" --value true`.
    /// Sections exist only while their owners are enabled, so both commands are for Play mode.
    /// </summary>
    public static class DebugMenuCommands
    {
        private const int NumberDecimals = 3;

        [CliCommand("debugmenu", "list", "Every debug menu section with its controls and their current values.")]
        public static string List([CliArg("section", "Only this section (optional)")] string section = "")
        {
            IEnumerable<string> sections = DebugMenu.Sections
                .Where(s => string.IsNullOrEmpty(section) || s.Title == section)
                .GroupBy(s => s.Title)
                .Select(g => "{" + Field("section", Quote(g.Key)) + "," +
                             Field("controls", Array(g.SelectMany(s => s.Controls).Select(Describe))) + "}");
            return Array(sections);
        }

        [CliCommand("debugmenu", "do", "Press a debug button, or set a toggle (true/false) or slider (number), by section and label. Returns the section's values afterwards.")]
        public static string Do(
            [CliArg("section", "Section title, e.g. Blocks")] string section,
            [CliArg("label", "Control label, e.g. \"Pause gravity\"")] string label,
            [CliArg("value", "For toggles: true/false. For sliders: a number.")] string value = "")
        {
            DebugControl control = DebugMenu.Sections.Where(s => s.Title == section).SelectMany(s => s.Controls)
                .FirstOrDefault(c => c.Label == label);
            if (control == null)
            {
                return Error($"no control '{label}' in section '{section}' (is the game in Play mode?)");
            }

            if (control.OnClick != null)
            {
                control.OnClick();
            }
            else if (control.SetBool != null)
            {
                if (!TryParseBool(value, out bool flag))
                {
                    return Error($"'{label}' is a toggle: pass --value true or false");
                }

                control.SetBool(flag);
            }
            else if (control.SetFloat != null)
            {
                if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                {
                    return Error($"'{label}' is a slider: pass --value <number>");
                }

                control.SetFloat(number);
            }
            else
            {
                return Error($"'{label}' is a readout and can't be set");
            }

            return List(section);
        }

        private static bool TryParseBool(string value, out bool result)
        {
            switch ((value ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "true":
                case "1":
                case "on":
                    result = true;
                    return true;
                case "false":
                case "0":
                case "off":
                    result = false;
                    return true;
                default:
                    result = false;
                    return false;
            }
        }

        private static string Describe(DebugControl c)
        {
            var fields = new List<string> { Field("label", Quote(c.Label)), Field("kind", Quote(c.Kind.ToString())) };
            if (c.GetText != null)
            {
                fields.Add(Field("value", Quote(c.GetText())));
            }
            else if (c.GetBool != null)
            {
                fields.Add(Field("value", c.GetBool() ? "true" : "false"));
            }
            else if (c.GetFloat != null)
            {
                fields.Add(Field("value", Number(c.GetFloat())));
            }

            return "{" + string.Join(",", fields) + "}";
        }

        private static string Error(string message) => "{" + Field("error", Quote(message)) + "}";

        private static string Field(string key, string json) => $"{Quote(key)}:{json}";

        private static string Array(IEnumerable<string> items) => "[" + string.Join(",", items) + "]";

        private static string Number(float f) =>
            float.IsFinite(f) ? System.Math.Round(f, NumberDecimals).ToString(CultureInfo.InvariantCulture) : "null";

        private static string Quote(string s)
        {
            if (s == null)
            {
                return "null";
            }

            var sb = new StringBuilder("\"");
            foreach (char c in s)
            {
                if (c == '"' || c == '\\')
                {
                    sb.Append('\\').Append(c);
                }
                else if (c < ' ')
                {
                    sb.Append("\\u").Append(((int)c).ToString("x4"));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.Append('"').ToString();
        }
    }
}
