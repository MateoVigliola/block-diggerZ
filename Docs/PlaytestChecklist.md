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

## Playtest Checklist window (editor tool), 2026-11-14

Open Tools > BlockDiggerz > Playtest Checklist. It reads and writes this file. Layout: `Assets/_Project/Playtest/UI/PlaytestChecklistWindow.uxml` and `.uss`.

- [ ] **Tabs:** expect a To check tab and a Done tab, each showing its item count, with items grouped under collapsible feature headings.
  - Notes:
- [ ] **Tick and confirm:** tick two items (click the box or the text). Expect them highlighted green and the button to read Confirm checked (2). Press it. Expect both to move to the Done tab and show `[x]` in this file.
  - Notes:
- [ ] **Clear:** tick a few items, press Clear. Expect all ticks removed and nothing moved.
  - Notes:
- [ ] **Reopen:** in the Done tab, press Reopen on an item. Expect it back in To check and `[ ]` in this file.
  - Notes:
- [ ] **Notes:** type a note (several lines too) under an item and click elsewhere. Expect "Notes saved." and the text under that item's Notes line here.
  - Notes:
- [ ] **Outside edits:** edit this file in a text editor (add an item or a note), save, then click back into the window. Expect the change to show without pressing Reload.
  - Notes:
- [ ] **Search:** type in the search box. Expect only items whose text, notes or feature name match.
  - Notes:
- [ ] **Readability:** are the rows, highlight color, note fields and footer comfortable to use?
  - Notes:
