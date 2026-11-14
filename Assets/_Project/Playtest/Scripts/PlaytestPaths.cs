namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// Project paths used by the playtest checklist window. Move these files together with this class.
    /// </summary>
    public static class PlaytestPaths
    {
        public const string FeatureRoot = "Assets/_Project/Playtest";
        public const string WindowUxml = FeatureRoot + "/UI/PlaytestChecklistWindow.uxml";
        public const string WindowUss = FeatureRoot + "/UI/PlaytestChecklistWindow.uss";

        /// <summary>Checklist file relative to the project root (outside Assets, so Unity does not import it).</summary>
        public const string ChecklistFile = "Docs/PlaytestChecklist.md";
    }
}
