namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Decides which project files the tracker picks up automatically:
    /// Assets/_Project/&lt;Feature&gt;/Art|Audio/... (area = Feature) and
    /// Assets/_Project/Shared/&lt;Area&gt;/Art|Audio/... (area = Shared/Area). Subfolders are allowed.
    /// </summary>
    public static class TrackedFolderRules
    {
        public const string AssetsFolder = "Assets";
        public const string ProjectFolder = "_Project";
        public const string SharedFolder = "Shared";
        public const string ArtFolder = "Art";
        public const string AudioFolder = "Audio";
        public const string ProjectRoot = AssetsFolder + "/" + ProjectFolder;

        private const char Separator = '/';
        private const int FeatureIndex = 2;
        private const int FeatureKindIndex = 3;
        private const int SharedAreaIndex = 3;
        private const int SharedKindIndex = 4;

        /// <summary>
        /// Returns true if the file path sits in a tracked Art or Audio folder, with its category and area.
        /// </summary>
        public static bool TryClassify(string path, out AssetCategory category, out string area)
        {
            category = AssetCategory.Art;
            area = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string[] parts = path.Replace('\\', Separator).Split(Separator);
            if (parts.Length <= FeatureKindIndex + 1 || parts[0] != AssetsFolder || parts[1] != ProjectFolder)
            {
                return false;
            }

            if (parts[FeatureIndex] == SharedFolder)
            {
                if (parts.Length <= SharedKindIndex + 1 || !TryKind(parts[SharedKindIndex], out category))
                {
                    return false;
                }

                area = SharedFolder + Separator + parts[SharedAreaIndex];
                return true;
            }

            if (!TryKind(parts[FeatureKindIndex], out category))
            {
                return false;
            }

            area = parts[FeatureIndex];
            return true;
        }

        private static bool TryKind(string folder, out AssetCategory category)
        {
            category = AssetCategory.Art;
            if (folder == ArtFolder)
            {
                return true;
            }

            if (folder == AudioFolder)
            {
                category = AssetCategory.Audio;
                return true;
            }

            return false;
        }
    }
}
