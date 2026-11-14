using System;
using System.Collections.Generic;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Merge rules between the local database and the sheet. No network or Editor calls, so it is unit tested.
    /// <list type="bullet">
    /// <item>Pull: sheet-owned fields come from the sheet, unless Unity changed that field since the last sync.</item>
    /// <item>Pull: rows only in the sheet become planned (unlinked) entries.</item>
    /// <item>Pull: a synced row deleted in the sheet is forgotten if unlinked, or written back if it links a file.</item>
    /// <item>Push: Unity-owned columns are always written; sheet-owned ones only for new rows or dirty fields.</item>
    /// </list>
    /// </summary>
    public static class SheetSyncMerge
    {
        public const string NameKey = "name";
        public const string DescriptionKey = "description";
        public const string RequirementsKey = "requirements";
        public const string StatusKey = "status";
        public const string NotesKey = "notes";

        private static readonly SheetField[] TextFields =
        {
            SheetField.Name, SheetField.Description, SheetField.Requirements, SheetField.Notes
        };

        private static readonly SheetField[] AllFields =
        {
            SheetField.Name, SheetField.Description, SheetField.Requirements, SheetField.Status, SheetField.Notes
        };

        public static PullSummary ApplyPull(AssetTrackerDatabase database, IReadOnlyList<SheetRow> rows)
        {
            var summary = new PullSummary();
            var seenIds = new HashSet<string>();

            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    ApplyRow(database, rows[i], seenIds, ref summary);
                }
            }

            var entries = new List<TrackedAssetEntry>(database.Entries);
            for (int i = 0; i < entries.Count; i++)
            {
                TrackedAssetEntry entry = entries[i];
                if (!entry.SyncedOnce || seenIds.Contains(entry.Id))
                {
                    continue;
                }

                if (entry.IsLinked)
                {
                    entry.MarkUnsynced();
                    summary.Restored++;
                }
                else
                {
                    database.Forget(entry);
                    summary.Forgotten++;
                }
            }

            return summary;
        }

        public static PushRequest BuildPush(AssetTrackerDatabase database, string secret)
        {
            var rows = new PushRow[database.Entries.Count];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = ToPushRow(database.Entries[i]);
            }

            return new PushRequest
            {
                secret = secret,
                rows = rows,
                deleteIds = new List<string>(database.RemovedIds).ToArray()
            };
        }

        /// <summary>Call after the sheet confirmed a push: everything local is now in the sheet.</summary>
        public static void CompletePush(AssetTrackerDatabase database)
        {
            for (int i = 0; i < database.Entries.Count; i++)
            {
                database.Entries[i].MarkSynced();
            }

            database.ClearRemovedIds();
        }

        public static string KeyOf(SheetField field)
        {
            switch (field)
            {
                case SheetField.Name:
                    return NameKey;
                case SheetField.Description:
                    return DescriptionKey;
                case SheetField.Requirements:
                    return RequirementsKey;
                case SheetField.Status:
                    return StatusKey;
                case SheetField.Notes:
                    return NotesKey;
                default:
                    throw new ArgumentException("Not a single sheet field.", nameof(field));
            }
        }

        private static void ApplyRow(AssetTrackerDatabase database, SheetRow row, HashSet<string> seenIds,
            ref PullSummary summary)
        {
            if (row == null || string.IsNullOrEmpty(row.id))
            {
                return;
            }

            seenIds.Add(row.id);
            if (database.IsRemoved(row.id) || !Enum.TryParse(row.category, out AssetCategory category))
            {
                return;
            }

            TrackedAssetEntry entry = database.FindById(row.id);
            if (entry == null)
            {
                entry = new TrackedAssetEntry(row.id, category, row.name);
                entry.SetPlacement(category, row.area);
                ApplySheetFields(entry, row, ref summary);
                entry.MarkSynced();
                database.Add(entry);
                summary.Added++;
                return;
            }

            bool changed = ApplySheetFields(entry, row, ref summary);
            if (!entry.IsLinked && (entry.Category != category || entry.Area != (row.area ?? string.Empty)))
            {
                entry.SetPlacement(category, row.area);
                changed = true;
            }

            if (changed)
            {
                summary.Updated++;
            }
        }

        private static bool ApplySheetFields(TrackedAssetEntry entry, SheetRow row, ref PullSummary summary)
        {
            bool changed = false;
            for (int i = 0; i < TextFields.Length; i++)
            {
                SheetField field = TextFields[i];
                string incoming = ReadRowField(row, field) ?? string.Empty;
                if (entry.IsDirty(field) && entry.SyncedOnce)
                {
                    continue;
                }

                changed |= entry.GetSheetField(field) != incoming;
                entry.SetSheetField(field, incoming, true);
            }

            if (entry.IsDirty(SheetField.Status) && entry.SyncedOnce)
            {
                return changed;
            }

            if (!AssetStatusNames.TryParse(row.status, out AssetStatus status))
            {
                summary.UnknownStatuses++;
                return changed;
            }

            changed |= entry.Status != status;
            entry.SetStatus(status, true);
            return changed;
        }

        private static string ReadRowField(SheetRow row, SheetField field)
        {
            switch (field)
            {
                case SheetField.Name:
                    return row.name;
                case SheetField.Description:
                    return row.description;
                case SheetField.Requirements:
                    return row.requirements;
                case SheetField.Status:
                    return row.status;
                case SheetField.Notes:
                    return row.notes;
                default:
                    return null;
            }
        }

        private static PushRow ToPushRow(TrackedAssetEntry entry)
        {
            var writeFields = new List<string>();
            for (int i = 0; i < AllFields.Length; i++)
            {
                if (!entry.SyncedOnce || entry.IsDirty(AllFields[i]))
                {
                    writeFields.Add(KeyOf(AllFields[i]));
                }
            }

            return new PushRow
            {
                id = entry.Id,
                category = entry.Category.ToString(),
                area = entry.Area,
                assetPath = entry.AssetPath,
                assetType = entry.AssetType,
                unityUpdated = entry.UnityUpdated,
                writeFields = writeFields.ToArray(),
                name = entry.Name,
                description = entry.Description,
                requirements = entry.Requirements,
                status = AssetStatusNames.ToSheet(entry.Status),
                notes = entry.Notes
            };
        }
    }
}
