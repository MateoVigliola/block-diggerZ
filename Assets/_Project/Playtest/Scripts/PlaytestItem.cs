namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// One checkbox line of the playtest checklist, with the notes written under it.
    /// </summary>
    public sealed class PlaytestItem
    {
        private const string KeySeparator = "\n";

        public PlaytestItem(string section, string text, bool isChecked, string notes, int lineIndex, int notesLineIndex, int notesLineCount)
        {
            Section = section;
            Text = text;
            IsChecked = isChecked;
            Notes = notes;
            LineIndex = lineIndex;
            NotesLineIndex = notesLineIndex;
            NotesLineCount = notesLineCount;
        }

        /// <summary>Title of the section the item belongs to.</summary>
        public string Section { get; }

        /// <summary>Item text after the checkbox, as written in the Markdown file.</summary>
        public string Text { get; }

        /// <summary>True when the file marks the item <c>[x]</c>.</summary>
        public bool IsChecked { get; }

        /// <summary>Notes text, possibly several lines; empty when nothing was written.</summary>
        public string Notes { get; }

        /// <summary>Stable identity of the item (section plus text), used to find it again after the file is reloaded.</summary>
        public string Key => Section + KeySeparator + Text;

        internal int LineIndex { get; }

        /// <summary>Line of the <c>- Notes:</c> bullet, or -1 when the item has none.</summary>
        internal int NotesLineIndex { get; }

        /// <summary>Number of lines the notes take, including the <c>- Notes:</c> line itself.</summary>
        internal int NotesLineCount { get; }
    }
}
