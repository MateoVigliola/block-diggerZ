using System;
using UnityEngine;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// One row of the asset tracker. Unity owns the link to the file (GUID, path, type, category, area);
    /// the Google Sheet owns the fields listed in <see cref="SheetField"/>.
    /// </summary>
    [Serializable]
    public class TrackedAssetEntry
    {
        private const string TimestampFormat = "yyyy-MM-dd HH:mm 'UTC'";

        [Tooltip("Stable row ID shared with the sheet. Never edit by hand.")]
        [SerializeField] private string _id;
        [SerializeField] private AssetCategory _category;
        [Tooltip("Feature name, or Shared/<Area> for shared assets.")]
        [SerializeField] private string _area;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [Tooltip("What the final asset needs, for example size, length or style.")]
        [SerializeField] private string _requirements;
        [SerializeField] private AssetStatus _status;
        [SerializeField] private string _notes;
        [SerializeField] private string _assetGuid;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _assetType;
        [Tooltip("When Unity last changed the file link (UTC).")]
        [SerializeField] private string _unityUpdated;
        [Tooltip("True once this row has been written to the sheet.")]
        [SerializeField] private bool _syncedOnce;
        [Tooltip("Sheet columns changed in Unity since the last sync.")]
        [SerializeField] private SheetField _dirtyFields;

        public TrackedAssetEntry(string id, AssetCategory category, string name)
        {
            _id = id;
            _category = category;
            _name = name ?? string.Empty;
            _area = string.Empty;
            _description = string.Empty;
            _requirements = string.Empty;
            _notes = string.Empty;
            _assetGuid = string.Empty;
            _assetPath = string.Empty;
            _assetType = string.Empty;
            _unityUpdated = string.Empty;
            _status = AssetStatus.Missing;
            _dirtyFields = SheetField.All;
        }

        public string Id => _id;
        public AssetCategory Category => _category;
        public string Area => _area;
        public string Name => _name;
        public string Description => _description;
        public string Requirements => _requirements;
        public AssetStatus Status => _status;
        public string Notes => _notes;
        public string AssetGuid => _assetGuid;
        public string AssetPath => _assetPath;
        public string AssetType => _assetType;
        public string UnityUpdated => _unityUpdated;
        public bool SyncedOnce => _syncedOnce;
        public SheetField DirtyFields => _dirtyFields;

        /// <summary>True when the entry points at a file in the project.</summary>
        public bool IsLinked => !string.IsNullOrEmpty(_assetGuid);

        /// <summary>Creates a new unique row ID.</summary>
        public static string NewId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public bool IsDirty(SheetField field)
        {
            return (_dirtyFields & field) != 0;
        }

        /// <summary>Sets a sheet-owned field. A local edit marks it dirty so the next sync writes it to the sheet.</summary>
        public void SetSheetField(SheetField field, string value, bool fromSheet)
        {
            string safe = value ?? string.Empty;
            switch (field)
            {
                case SheetField.Name:
                    _name = safe;
                    break;
                case SheetField.Description:
                    _description = safe;
                    break;
                case SheetField.Requirements:
                    _requirements = safe;
                    break;
                case SheetField.Notes:
                    _notes = safe;
                    break;
                default:
                    throw new ArgumentException("Use SetStatus for the Status field.", nameof(field));
            }

            MarkField(field, fromSheet);
        }

        public string GetSheetField(SheetField field)
        {
            switch (field)
            {
                case SheetField.Name:
                    return _name;
                case SheetField.Description:
                    return _description;
                case SheetField.Requirements:
                    return _requirements;
                case SheetField.Status:
                    return AssetStatusNames.ToSheet(_status);
                case SheetField.Notes:
                    return _notes;
                default:
                    throw new ArgumentException("Not a single sheet field.", nameof(field));
            }
        }

        public void SetStatus(AssetStatus status, bool fromSheet)
        {
            _status = status;
            MarkField(SheetField.Status, fromSheet);
        }

        /// <summary>Sets the tab and area. Only used for unlinked entries, or by Unity from the file's folder.</summary>
        public void SetPlacement(AssetCategory category, string area)
        {
            _category = category;
            _area = area ?? string.Empty;
        }

        /// <summary>Links the entry to a project file and records the change time.</summary>
        public void Link(string guid, string path, string assetType)
        {
            _assetGuid = guid ?? string.Empty;
            _assetPath = path ?? string.Empty;
            _assetType = assetType ?? string.Empty;
            TouchUnity();
        }

        /// <summary>Updates the path and type of an already linked file. Returns true if anything changed.</summary>
        public bool UpdateFileInfo(string path, string assetType)
        {
            string safePath = path ?? string.Empty;
            string safeType = assetType ?? string.Empty;
            if (_assetPath == safePath && _assetType == safeType)
            {
                return false;
            }

            _assetPath = safePath;
            _assetType = safeType;
            TouchUnity();
            return true;
        }

        /// <summary>Removes the file link and keeps the row.</summary>
        public void Unlink()
        {
            _assetGuid = string.Empty;
            _assetPath = string.Empty;
            _assetType = string.Empty;
            TouchUnity();
        }

        /// <summary>Called after a successful push: the sheet now matches this entry.</summary>
        public void MarkSynced()
        {
            _syncedOnce = true;
            _dirtyFields = SheetField.None;
        }

        /// <summary>Called when a linked row was deleted from the sheet: it is written again in full next push.</summary>
        public void MarkUnsynced()
        {
            _syncedOnce = false;
            _dirtyFields = SheetField.All;
        }

        private void MarkField(SheetField field, bool fromSheet)
        {
            if (fromSheet)
            {
                _dirtyFields &= ~field;
            }
            else
            {
                _dirtyFields |= field;
            }
        }

        private void TouchUnity()
        {
            _unityUpdated = DateTime.UtcNow.ToString(TimestampFormat);
        }
    }
}
