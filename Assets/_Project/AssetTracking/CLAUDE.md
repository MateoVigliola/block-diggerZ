# AssetTracking

Editor-only tracker for every art and audio asset in the game, synced both ways with a Google Sheet (tabs Art and Audio). Humans use **Tools > BlockDiggerz > Asset Tracker**. Sheet setup: [Sheet/SETUP.md](Sheet/SETUP.md).

## Layout
| Path | Contents |
|---|---|
| `Scripts/` | Editor asmdef `BlockDiggerz.AssetTracking.Editor`, namespace `BlockDiggerz.AssetTracking` |
| `UI/` | `AssetTrackerWindow.uxml` / `.uss` |
| `Data/AssetTrackerDatabase.asset` | Committed source of truth (human data; never regenerate or overwrite it) |
| `Sheet/AssetTrackerSheet.gs` | Apps Script that runs in the sheet (pasted in by the sheet owner) |
| `Tests/` | EditMode tests, asmdef `BlockDiggerz.AssetTracking.Tests` |

Per-machine settings (web app URL, secret, sheet URL) live in `UserSettings/BlockDiggerz/AssetTrackerSettings.asset` and are never committed.

## Rules
- Tracked folders: `Assets/_Project/<Feature>/Art|Audio/**` (area = Feature) and `Assets/_Project/Shared/<Area>/Art|Audio/**` (area = `Shared/<Area>`). See `TrackedFolderRules`.
- Column ownership: Unity writes ID, Area, Asset Path, Asset Type, Updated In Unity, plus the tab (category). People own Name, Description, Requirements, Status and Notes. A Unity-side change to a people-owned field is marked dirty (`SheetField`) and wins on the next sync.
- Automation only adds rows or updates link fields. It never deletes rows or overwrites people's text. A deleted file sets the row to Missing.
- Merge rules live in `SheetSyncMerge` (pure, unit tested). Change the rules there and extend `SheetSyncMergeTests`.
- The JSON protocol (`SheetMessages.cs`, field keys in `SheetSyncMerge`, status names in `AssetStatusNames`) must match `AssetTrackerSheet.gs`. After changing the script, the sheet owner must paste it again and publish a new deployment version.
- Sync is manual (Sync button). The network part can't be unit tested: after protocol changes, run a real sync against a test sheet by hand.

## Tests
```bash
unity command test run --mode editor --filter BlockDiggerz.AssetTracking.Tests --filter-type Assembly --result-only
```
