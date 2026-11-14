using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// Reads and edits the Markdown playtest checklist in place. Edits touch only the checkbox character
    /// and the notes lines of one item, so every other line of the file is written back unchanged.
    /// </summary>
    /// <remarks>
    /// Format: <c>## Section</c> headings, items as <c>- [ ] text</c> or <c>- [x] text</c>, and under each
    /// item an indented <c>- Notes:</c> bullet. Further notes lines are indented by four spaces.
    /// </remarks>
    public sealed class PlaytestChecklistDocument
    {
        private const string NotesPrefix = "  - Notes:";
        private const string ContinuationIndent = "    ";
        private const int CheckboxCharIndex = 3;
        private const char CheckedChar = 'x';
        private const char UncheckedChar = ' ';
        private const string WindowsNewline = "\r\n";
        private const string UnixNewline = "\n";
        private const string DescriptionSeparator = " ";

        private static readonly Regex SectionPattern = new Regex(@"^##\s+(.+?)\s*$");
        private static readonly Regex ItemPattern = new Regex(@"^- \[([ xX])\]\s+(.*?)\s*$");
        private static readonly Regex NotesPattern = new Regex(@"^\s+- Notes:\s?(.*?)\s*$");
        private static readonly Regex ContinuationPattern = new Regex("^" + ContinuationIndent);

        private readonly List<string> _lines;
        private readonly string _newline;
        private readonly bool _endsWithNewline;
        private List<PlaytestSection> _sections = new List<PlaytestSection>();

        private PlaytestChecklistDocument(List<string> lines, string newline, bool endsWithNewline)
        {
            _lines = lines;
            _newline = newline;
            _endsWithNewline = endsWithNewline;
            Reparse();
        }

        public IReadOnlyList<PlaytestSection> Sections => _sections;

        /// <summary>Parses checklist Markdown. Never throws; text without items gives an empty checklist.</summary>
        public static PlaytestChecklistDocument Parse(string text)
        {
            text ??= string.Empty;
            string newline = text.Contains(WindowsNewline) ? WindowsNewline : UnixNewline;
            string normalized = text.Replace(WindowsNewline, UnixNewline);
            bool endsWithNewline = normalized.EndsWith(UnixNewline, StringComparison.Ordinal);
            if (endsWithNewline)
            {
                normalized = normalized.Substring(0, normalized.Length - UnixNewline.Length);
            }

            var lines = normalized.Length == 0 ? new List<string>() : new List<string>(normalized.Split('\n'));
            return new PlaytestChecklistDocument(lines, newline, endsWithNewline);
        }

        /// <summary>The whole file, with the original line endings.</summary>
        public string ToText()
        {
            string body = string.Join(_newline, _lines);
            return _endsWithNewline ? body + _newline : body;
        }

        /// <summary>Every item across all sections, in file order.</summary>
        public IEnumerable<PlaytestItem> AllItems()
        {
            foreach (PlaytestSection section in _sections)
            {
                foreach (PlaytestItem item in section.Items)
                {
                    yield return item;
                }
            }
        }

        /// <summary>Finds an item by <see cref="PlaytestItem.Key"/>, or null when it is no longer in the file.</summary>
        public PlaytestItem Find(string key)
        {
            foreach (PlaytestItem item in AllItems())
            {
                if (item.Key == key)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>Marks an item <c>[x]</c> or <c>[ ]</c>. Returns false when the item is not found.</summary>
        public bool SetChecked(string key, bool isChecked)
        {
            PlaytestItem item = Find(key);
            if (item == null)
            {
                return false;
            }

            var line = new StringBuilder(_lines[item.LineIndex]);
            line[CheckboxCharIndex] = isChecked ? CheckedChar : UncheckedChar;
            _lines[item.LineIndex] = line.ToString();
            Reparse();
            return true;
        }

        /// <summary>Replaces an item's notes, adding a <c>- Notes:</c> bullet when it has none. Returns false when the item is not found.</summary>
        public bool SetNotes(string key, string notes)
        {
            PlaytestItem item = Find(key);
            if (item == null)
            {
                return false;
            }

            List<string> notesLines = FormatNotes(notes);
            if (item.NotesLineIndex < 0)
            {
                _lines.InsertRange(item.LineIndex + 1, notesLines);
            }
            else
            {
                _lines.RemoveRange(item.NotesLineIndex, item.NotesLineCount);
                _lines.InsertRange(item.NotesLineIndex, notesLines);
            }

            Reparse();
            return true;
        }

        private static List<string> FormatNotes(string notes)
        {
            var noteLines = new List<string>((notes ?? string.Empty).Replace(WindowsNewline, UnixNewline).Split('\n'));
            for (int i = 0; i < noteLines.Count; i++)
            {
                noteLines[i] = noteLines[i].TrimEnd();
            }

            while (noteLines.Count > 0 && noteLines[noteLines.Count - 1].Length == 0)
            {
                noteLines.RemoveAt(noteLines.Count - 1);
            }

            var result = new List<string>();
            string first = noteLines.Count > 0 ? noteLines[0] : string.Empty;
            result.Add(first.Length > 0 ? NotesPrefix + " " + first : NotesPrefix);
            for (int i = 1; i < noteLines.Count; i++)
            {
                result.Add(ContinuationIndent + noteLines[i]);
            }

            return result;
        }

        private void Reparse()
        {
            var sections = new List<PlaytestSection>();
            string sectionTitle = string.Empty;
            var description = new List<string>();
            var items = new List<PlaytestItem>();
            bool sectionOpen = false;

            void CloseSection()
            {
                if (sectionOpen || items.Count > 0)
                {
                    sections.Add(new PlaytestSection(sectionTitle, string.Join(DescriptionSeparator, description), items));
                }

                description = new List<string>();
                items = new List<PlaytestItem>();
            }

            int index = 0;
            while (index < _lines.Count)
            {
                string line = _lines[index];
                Match sectionMatch = SectionPattern.Match(line);
                if (sectionMatch.Success)
                {
                    CloseSection();
                    sectionTitle = sectionMatch.Groups[1].Value;
                    sectionOpen = true;
                    index++;
                    continue;
                }

                Match itemMatch = ItemPattern.Match(line);
                if (itemMatch.Success)
                {
                    index = ReadItem(index, sectionTitle, itemMatch, items);
                    continue;
                }

                if (sectionOpen && items.Count == 0 && line.Trim().Length > 0)
                {
                    description.Add(line.Trim());
                }

                index++;
            }

            CloseSection();
            _sections = sections;
        }

        private int ReadItem(int itemLine, string sectionTitle, Match itemMatch, List<PlaytestItem> items)
        {
            bool isChecked = itemMatch.Groups[1].Value != " ";
            string text = itemMatch.Groups[2].Value;
            int notesLine = -1;
            int notesCount = 0;
            var notes = new List<string>();
            int next = itemLine + 1;

            if (next < _lines.Count)
            {
                Match notesMatch = NotesPattern.Match(_lines[next]);
                if (notesMatch.Success)
                {
                    notesLine = next;
                    notes.Add(notesMatch.Groups[1].Value);
                    next++;
                    while (next < _lines.Count && ContinuationPattern.IsMatch(_lines[next]))
                    {
                        notes.Add(_lines[next].Substring(ContinuationIndent.Length).TrimEnd());
                        next++;
                    }

                    notesCount = next - notesLine;
                }
            }

            while (notes.Count > 0 && notes[notes.Count - 1].Length == 0)
            {
                notes.RemoveAt(notes.Count - 1);
            }

            if (notes.Count > 0 && notes[0].Length == 0)
            {
                notes.RemoveAt(0);
            }

            items.Add(new PlaytestItem(sectionTitle, text, isChecked, string.Join(UnixNewline, notes), itemLine, notesLine, notesCount));
            return next;
        }
    }
}
