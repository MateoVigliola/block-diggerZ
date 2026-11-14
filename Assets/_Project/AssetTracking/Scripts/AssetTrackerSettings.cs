using UnityEditor;
using UnityEngine;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Per-machine sync settings, stored under UserSettings/ so the web app URL and secret are never committed.
    /// </summary>
    [FilePath(AssetTrackerPaths.SettingsFile, FilePathAttribute.Location.ProjectFolder)]
    public class AssetTrackerSettings : ScriptableSingleton<AssetTrackerSettings>
    {
        [Tooltip("Deployment URL of the Apps Script web app (ends with /exec).")]
        [SerializeField] private string _webAppUrl = string.Empty;

        [Tooltip("Shared secret from the sheet's Asset Tracker > Generate sync secret menu.")]
        [SerializeField] private string _secret = string.Empty;

        [Tooltip("Link to the sheet itself. Filled in automatically on the first sync.")]
        [SerializeField] private string _sheetUrl = string.Empty;

        public string WebAppUrl => _webAppUrl;
        public string Secret => _secret;
        public string SheetUrl => _sheetUrl;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_webAppUrl) && !string.IsNullOrWhiteSpace(_secret);

        public void SetConnection(string webAppUrl, string secret)
        {
            _webAppUrl = (webAppUrl ?? string.Empty).Trim();
            _secret = (secret ?? string.Empty).Trim();
            Save(true);
        }

        public void SetSheetUrl(string sheetUrl)
        {
            if (string.IsNullOrEmpty(sheetUrl) || sheetUrl == _sheetUrl)
            {
                return;
            }

            _sheetUrl = sheetUrl;
            Save(true);
        }
    }
}
