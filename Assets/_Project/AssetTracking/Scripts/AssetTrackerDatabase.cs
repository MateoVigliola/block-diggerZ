using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Every tracked art and audio asset, committed with the project. Edit it through
    /// Tools &gt; BlockDiggerz &gt; Asset Tracker rather than the Inspector, so sync bookkeeping stays correct.
    /// </summary>
    [CreateAssetMenu(fileName = "AssetTrackerDatabase", menuName = "BlockDiggerz/Asset Tracker Database")]
    public class AssetTrackerDatabase : ScriptableObject
    {
        [SerializeField] private List<TrackedAssetEntry> _entries = new List<TrackedAssetEntry>();

        [Tooltip("IDs of synced rows removed in Unity. The next sync deletes them from the sheet.")]
        [SerializeField] private List<string> _removedIds = new List<string>();

        public IReadOnlyList<TrackedAssetEntry> Entries => _entries;
        public IReadOnlyList<string> RemovedIds => _removedIds;

        /// <summary>Loads the project database, creating it on first use.</summary>
        public static AssetTrackerDatabase LoadOrCreate()
        {
            var database = AssetDatabase.LoadAssetAtPath<AssetTrackerDatabase>(AssetTrackerPaths.DatabaseAsset);
            if (database != null)
            {
                return database;
            }

            if (!AssetDatabase.IsValidFolder(AssetTrackerPaths.DataFolder))
            {
                AssetDatabase.CreateFolder(AssetTrackerPaths.FeatureRoot, "Data");
            }

            database = CreateInstance<AssetTrackerDatabase>();
            AssetDatabase.CreateAsset(database, AssetTrackerPaths.DatabaseAsset);
            AssetDatabase.SaveAssets();
            return database;
        }

        public TrackedAssetEntry FindById(string id)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Id == id)
                {
                    return _entries[i];
                }
            }

            return null;
        }

        public TrackedAssetEntry FindByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].AssetGuid == guid)
                {
                    return _entries[i];
                }
            }

            return null;
        }

        public TrackedAssetEntry FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].AssetPath == path)
                {
                    return _entries[i];
                }
            }

            return null;
        }

        public void Add(TrackedAssetEntry entry)
        {
            _entries.Add(entry);
        }

        /// <summary>Removes an entry. If the sheet already has it, the next sync deletes that row too.</summary>
        public void Remove(TrackedAssetEntry entry)
        {
            if (!_entries.Remove(entry))
            {
                return;
            }

            if (entry.SyncedOnce && !_removedIds.Contains(entry.Id))
            {
                _removedIds.Add(entry.Id);
            }
        }

        /// <summary>Removes an entry the sheet no longer has, without scheduling a sheet delete.</summary>
        public void Forget(TrackedAssetEntry entry)
        {
            _entries.Remove(entry);
        }

        public bool IsRemoved(string id)
        {
            return _removedIds.Contains(id);
        }

        public void ClearRemovedIds()
        {
            _removedIds.Clear();
        }
    }
}
