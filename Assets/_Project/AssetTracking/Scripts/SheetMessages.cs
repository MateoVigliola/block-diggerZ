using System;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// JSON messages exchanged with the Apps Script web app (AssetTrackerSheet.gs). Field names are part of
    /// the protocol: rename them in both places or not at all.
    /// </summary>
    public static class SheetActions
    {
        public const string Pull = "pull";
        public const string Push = "push";
    }

    /// <summary>A row as read from the sheet.</summary>
    [Serializable]
    public class SheetRow
    {
        public string id;
        public string category;
        public string name;
        public string area;
        public string description;
        public string requirements;
        public string status;
        public string notes;
    }

    /// <summary>A row Unity writes. Sheet-owned fields are only written when listed in writeFields.</summary>
    [Serializable]
    public class PushRow
    {
        public string id;
        public string category;
        public string area;
        public string assetPath;
        public string assetType;
        public string unityUpdated;
        public string[] writeFields;
        public string name;
        public string description;
        public string requirements;
        public string status;
        public string notes;
    }

    [Serializable]
    public class PullRequest
    {
        public string secret;
        public string action = SheetActions.Pull;
    }

    [Serializable]
    public class PushRequest
    {
        public string secret;
        public string action = SheetActions.Push;
        public PushRow[] rows;
        public string[] deleteIds;
    }

    [Serializable]
    public class SheetResponse
    {
        public bool ok;
        public string error;
        public string sheetUrl;
        public SheetRow[] rows;
        public int written;
        public int deleted;
    }
}
