using System.Collections.Generic;
using UnityEditor;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Watches imports, moves and deletes, and applies them to the tracker database after the import finishes.
    /// Changes are applied on the next Editor tick because creating or saving assets during an import is unsafe.
    /// </summary>
    public class AssetTrackerImportWatcher : AssetPostprocessor
    {
        private static readonly List<string> PendingImports = new List<string>();
        private static readonly List<string> PendingDeletes = new List<string>();
        private static bool _flushScheduled;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool queued = Queue(importedAssets, PendingImports);
            queued |= Queue(movedAssets, PendingImports);
            queued |= Queue(deletedAssets, PendingDeletes);

            if (queued && !_flushScheduled)
            {
                _flushScheduled = true;
                EditorApplication.delayCall += Flush;
            }
        }

        private static bool Queue(string[] paths, List<string> target)
        {
            bool queued = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].StartsWith(TrackedFolderRules.ProjectRoot) && !paths[i].StartsWith(AssetTrackerPaths.FeatureRoot))
                {
                    target.Add(paths[i]);
                    queued = true;
                }
            }

            return queued;
        }

        private static void Flush()
        {
            _flushScheduled = false;
            if (PendingImports.Count == 0 && PendingDeletes.Count == 0)
            {
                return;
            }

            AssetTrackerDatabase database = AssetTrackerDatabase.LoadOrCreate();
            bool changed = false;

            for (int i = 0; i < PendingDeletes.Count; i++)
            {
                changed |= TrackedAssetUpdater.ApplyDeleted(database, PendingDeletes[i]);
            }

            for (int i = 0; i < PendingImports.Count; i++)
            {
                string path = PendingImports[i];
                if (AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                {
                    continue;
                }

                changed |= TrackedAssetUpdater.ApplyMoved(database, path, guid, AssetTypeName(path));
            }

            PendingImports.Clear();
            PendingDeletes.Clear();

            if (changed)
            {
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssetIfDirty(database);
                AssetTrackerWindow.RefreshOpenWindows();
            }
        }

        /// <summary>Short type name of the main asset at a path, for the sheet's Asset Type column.</summary>
        public static string AssetTypeName(string path)
        {
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return type == null ? string.Empty : type.Name;
        }
    }
}
