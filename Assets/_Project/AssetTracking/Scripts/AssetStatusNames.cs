using System;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Converts <see cref="AssetStatus"/> values to and from the text used in the Google Sheet Status column.
    /// The Apps Script uses the same strings for its dropdown, so keep both in step.
    /// </summary>
    public static class AssetStatusNames
    {
        public const string Missing = "Missing";
        public const string Placeholder = "Placeholder";
        public const string NeedsReplacement = "Needs Replacement";
        public const string Final = "Final";

        /// <summary>All sheet status names in enum order.</summary>
        public static readonly string[] All = { Missing, Placeholder, NeedsReplacement, Final };

        public static string ToSheet(AssetStatus status)
        {
            return All[(int)status];
        }

        /// <summary>
        /// Parses sheet text, ignoring case and surrounding spaces. A blank cell counts as Missing.
        /// Returns false for text that matches no status.
        /// </summary>
        public static bool TryParse(string text, out AssetStatus status)
        {
            string trimmed = text == null ? string.Empty : text.Trim();
            if (trimmed.Length == 0)
            {
                status = AssetStatus.Missing;
                return true;
            }

            for (int i = 0; i < All.Length; i++)
            {
                if (string.Equals(All[i], trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    status = (AssetStatus)i;
                    return true;
                }
            }

            status = AssetStatus.Missing;
            return false;
        }
    }
}
