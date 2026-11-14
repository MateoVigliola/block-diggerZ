using System.IO;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Applies file imports, moves and deletes to the database. It only adds entries or updates the
    /// Unity-owned link fields; it never deletes a row or overwrites text that people wrote.
    /// </summary>
    public static class TrackedAssetUpdater
    {
        /// <summary>A file was imported or reimported. Returns true if the database changed.</summary>
        public static bool ApplyImported(AssetTrackerDatabase database, string path, string guid, string assetType)
        {
            TrackedAssetEntry existing = database.FindByGuid(guid);
            if (existing != null)
            {
                return ApplyMoved(database, existing, path, assetType);
            }

            if (!TrackedFolderRules.TryClassify(path, out AssetCategory category, out string area))
            {
                return false;
            }

            var entry = new TrackedAssetEntry(TrackedAssetEntry.NewId(), category,
                Path.GetFileNameWithoutExtension(path));
            entry.SetPlacement(category, area);
            entry.SetStatus(AssetStatus.Placeholder, false);
            entry.Link(guid, path, assetType);
            database.Add(entry);
            return true;
        }

        /// <summary>A file was moved or renamed. Returns true if the database changed.</summary>
        public static bool ApplyMoved(AssetTrackerDatabase database, string path, string guid, string assetType)
        {
            TrackedAssetEntry existing = database.FindByGuid(guid);
            if (existing == null)
            {
                return ApplyImported(database, path, guid, assetType);
            }

            return ApplyMoved(database, existing, path, assetType);
        }

        /// <summary>A file was deleted. Its row stays and goes back to Missing. Returns true if it was tracked.</summary>
        public static bool ApplyDeleted(AssetTrackerDatabase database, string path)
        {
            TrackedAssetEntry entry = database.FindByPath(path);
            if (entry == null)
            {
                return false;
            }

            MarkMissing(entry);
            return true;
        }

        /// <summary>Unlinks the entry and sets its status to Missing.</summary>
        public static void MarkMissing(TrackedAssetEntry entry)
        {
            entry.Unlink();
            if (entry.Status != AssetStatus.Missing)
            {
                entry.SetStatus(AssetStatus.Missing, false);
            }
        }

        private static bool ApplyMoved(AssetTrackerDatabase database, TrackedAssetEntry entry, string path,
            string assetType)
        {
            bool changed = entry.UpdateFileInfo(path, assetType);
            if (TrackedFolderRules.TryClassify(path, out AssetCategory category, out string area)
                && (entry.Category != category || entry.Area != area))
            {
                entry.SetPlacement(category, area);
                changed = true;
            }

            return changed;
        }
    }
}
