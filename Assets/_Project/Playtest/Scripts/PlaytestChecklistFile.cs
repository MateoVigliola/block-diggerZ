using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// Loads and saves <see cref="PlaytestPaths.ChecklistFile"/>. Every edit reloads the file first, so changes
    /// made outside Unity (by a person or by Claude adding a section) are kept.
    /// </summary>
    public static class PlaytestChecklistFile
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        /// <summary>Absolute path of the checklist file.</summary>
        public static string FullPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", PlaytestPaths.ChecklistFile));

        public static bool Exists => File.Exists(FullPath);

        /// <summary>Last write time of the file, or <see cref="DateTime.MinValue"/> when it does not exist.</summary>
        public static DateTime LastWriteUtc => Exists ? File.GetLastWriteTimeUtc(FullPath) : DateTime.MinValue;

        /// <summary>Reads the checklist; a missing file gives an empty checklist.</summary>
        public static PlaytestChecklistDocument Load()
        {
            return PlaytestChecklistDocument.Parse(Exists ? File.ReadAllText(FullPath) : string.Empty);
        }

        /// <summary>Reloads the file, applies <paramref name="edit"/> and writes it back when the edit succeeded.</summary>
        /// <returns>False when the edit found nothing to change, for example because the item was removed from the file.</returns>
        public static bool Edit(Func<PlaytestChecklistDocument, bool> edit)
        {
            PlaytestChecklistDocument document = Load();
            if (!edit(document))
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            File.WriteAllText(FullPath, document.ToText(), Utf8NoBom);
            return true;
        }
    }
}
