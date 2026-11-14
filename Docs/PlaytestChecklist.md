# Playtest Checklist

Each feature adds a section here. Write your notes under each item; Claude reads them next session and turns them into fixes or tuning changes.

## Asset Tracker (editor tool), 2026-11-14

Editor tooling, so this is a hands-on check of the workflow rather than game feel. Setup steps: `Assets/_Project/AssetTracking/Sheet/SETUP.md`.

- [ ] **Sheet setup:** follow SETUP.md steps 1 to 4. Expect Art and Audio tabs with grey Unity columns, blue people columns, a Status dropdown and status colors.
  - Notes:
- [ ] **Connect and first sync:** paste the Web App URL and secret in Tools > BlockDiggerz > Asset Tracker > Settings, Save, click Sync. Expect a status bar message (not red) like "Synced: 0 rows written..." and Open Sheet becoming clickable.
  - Notes:
- [ ] **Auto-add:** create `Assets/_Project/Test/Art/` and drop a PNG in it. Expect a Placeholder row in the window (Area "Test", Sync "New"). Sync, and it appears in the Art tab with its Asset Path.
  - Notes:
- [ ] **Planned row from the sheet:** add a row in the Audio tab with only a Name. Sync. Expect it in Unity as Missing, and an ID filled in the sheet.
  - Notes:
- [ ] **Sheet edits win:** change Description and Status of a row in the sheet, Sync. Expect Unity to show the new values.
  - Notes:
- [ ] **Unity edits win for changed fields:** change Notes in the Unity window (Sync column shows "Changed"), Sync. Expect the sheet to show the new Notes.
  - Notes:
- [ ] **Link a file to a planned row:** select the planned Audio row, drop an audio clip on Linked Asset. Expect status Placeholder and the path filled; if the clip was already auto-added, a merge prompt.
  - Notes:
- [ ] **Delete a file:** delete the test PNG in Unity. Expect its row to stay, marked Missing, in Unity and (after Sync) in the sheet.
  - Notes:
- [ ] **Clean up:** Remove Entry for the test rows, delete `Assets/_Project/Test/`, Sync. Expect the rows gone from the sheet.
  - Notes:
- [ ] **Window layout:** is the list and details split comfortable, are the filters and buttons where you expect them? Layout lives in `Assets/_Project/AssetTracking/UI/AssetTrackerWindow.uxml` and `.uss`.
  - Notes:
