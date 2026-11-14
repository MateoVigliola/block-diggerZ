namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Project paths used by the asset tracker. Move these files together with this class.
    /// </summary>
    public static class AssetTrackerPaths
    {
        public const string FeatureRoot = "Assets/_Project/AssetTracking";
        public const string DataFolder = FeatureRoot + "/Data";
        public const string DatabaseAsset = DataFolder + "/AssetTrackerDatabase.asset";
        public const string WindowUxml = FeatureRoot + "/UI/AssetTrackerWindow.uxml";
        public const string WindowUss = FeatureRoot + "/UI/AssetTrackerWindow.uss";
        public const string AppsScript = FeatureRoot + "/Sheet/AssetTrackerSheet.gs";
        public const string SetupGuide = FeatureRoot + "/Sheet/SETUP.md";
        public const string SettingsFile = "UserSettings/BlockDiggerz/AssetTrackerSettings.asset";
    }
}
