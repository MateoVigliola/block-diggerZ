using UnityEditor;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Full rescan of the tracked folders. Catches files added before the tracker existed, files merged in
    /// from other branches, and linked files that no longer exist.
    /// </summary>
    public static class AssetTrackerScanner
    {
        /// <summary>Result counts of a rescan.</summary>
        public struct ScanResult
        {
            public int Added;
            public int Updated;
            public int MarkedMissing;
        }

        public static ScanResult Rescan(AssetTrackerDatabase database)
        {
            var result = new ScanResult();

            for (int i = 0; i < database.Entries.Count; i++)
            {
                TrackedAssetEntry entry = database.Entries[i];
                if (entry.IsLinked && !AssetDatabase.AssetPathExists(AssetDatabase.GUIDToAssetPath(entry.AssetGuid)))
                {
                    TrackedAssetUpdater.MarkMissing(entry);
                    result.MarkedMissing++;
                }
            }

            if (AssetDatabase.IsValidFolder(TrackedFolderRules.ProjectRoot))
            {
                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { TrackedFolderRules.ProjectRoot });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (AssetDatabase.IsValidFolder(path) || !TrackedFolderRules.TryClassify(path, out _, out _))
                    {
                        continue;
                    }

                    bool known = database.FindByGuid(guids[i]) != null;
                    if (TrackedAssetUpdater.ApplyMoved(database, path, guids[i],
                            AssetTrackerImportWatcher.AssetTypeName(path)))
                    {
                        if (known)
                        {
                            result.Updated++;
                        }
                        else
                        {
                            result.Added++;
                        }
                    }
                }
            }

            if (result.Added + result.Updated + result.MarkedMissing > 0)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssetIfDirty(database);
            }

            return result;
        }
    }
}
