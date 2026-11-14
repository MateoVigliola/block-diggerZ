using System.Text;

namespace BlockDiggerz.Menus
{
    /// <summary>Turns action names from GameControls into labels, for example "MoveLeft" into "Move Left".</summary>
    public static class ActionDisplayNames
    {
        /// <summary>Inserts a space before each capital letter after the first character.</summary>
        public static string ToLabel(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return string.Empty;
            }

            var label = new StringBuilder(actionName.Length + actionName.Length / 2);
            for (int i = 0; i < actionName.Length; i++)
            {
                char c = actionName[i];
                if (i > 0 && char.IsUpper(c))
                {
                    label.Append(' ');
                }

                label.Append(c);
            }

            return label.ToString();
        }
    }
}
