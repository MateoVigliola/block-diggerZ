using System.Threading.Tasks;
using UnityEditor;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// One sync round: pull sheet edits, merge them, then push Unity's rows and pending deletes.
    /// </summary>
    public static class AssetTrackerSync
    {
        /// <summary>Runs a full sync and returns a one-line summary. Throws if the sheet can't be reached.</summary>
        public static async Task<string> SyncAsync(AssetTrackerDatabase database, AssetTrackerSettings settings)
        {
            SheetResponse pulled = await SheetWebClient.PostAsync(settings.WebAppUrl,
                new PullRequest { secret = settings.Secret });
            settings.SetSheetUrl(pulled.sheetUrl);

            Undo.RecordObject(database, "Asset Tracker Sync");
            PullSummary summary = SheetSyncMerge.ApplyPull(database, pulled.rows);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);

            PushRequest push = SheetSyncMerge.BuildPush(database, settings.Secret);
            SheetResponse pushed = await SheetWebClient.PostAsync(settings.WebAppUrl, push);

            SheetSyncMerge.CompletePush(database);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);

            string message = $"Synced: {pushed.written} rows written, {pushed.deleted} deleted, " +
                             $"{summary.Added} planned added, {summary.Updated} updated from sheet";
            if (summary.Forgotten > 0)
            {
                message += $", {summary.Forgotten} removed (deleted in sheet)";
            }

            if (summary.Restored > 0)
            {
                message += $", {summary.Restored} restored (deleted in sheet but file exists)";
            }

            if (summary.UnknownStatuses > 0)
            {
                message += $", {summary.UnknownStatuses} unknown status values ignored";
            }

            return message + ".";
        }
    }
}
