# Asset Tracker: Google Sheet setup

Do this once per project. The person who owns the sheet runs steps 1 to 4. Every contributor runs step 5 on their own machine.

## 1. Create the sheet

1. Create an empty Google Sheet, for example "BlockDiggerz Assets".
2. Open **Extensions > Apps Script**.
3. In Unity, open **Tools > BlockDiggerz > Asset Tracker**, open **Settings** and click **Copy Apps Script**. You can also copy the contents of `Assets/_Project/AssetTracking/Sheet/AssetTrackerSheet.gs` yourself.
4. Replace everything in the script editor's `Code.gs` with the copied script and save it.

## 2. Build the tabs

1. Reload the sheet. An **Asset Tracker** menu appears; this can take a few seconds.
2. Run **Asset Tracker > Set up tabs**. The first time, Google asks you to authorize the script. Accept, then run the menu item again if it stopped.
3. You now have **Art** and **Audio** tabs. Each one has the headers, a Status dropdown, status colors and grey Unity-owned columns. You can delete the empty default tab.

## 3. Create the sync secret

1. Run **Asset Tracker > Generate sync secret**.
2. Copy the secret it shows, because it is shown only once. Share it privately with contributors, not in the repo.

## 4. Deploy the web app

1. In the Apps Script editor: **Deploy > New deployment**, type **Web app**.
2. Set **Execute as** to **Me** and **Who has access** to **Anyone**. Unity sends no Google login, and the secret is what protects the sheet.
3. Click **Deploy** and copy the **Web app URL** (it ends in `/exec`).
4. Optional check: open that URL in a browser. It should say the web app is running.

If you change the script later, use **Deploy > Manage deployments > Edit > Version: New version** so the URL stays the same.

## 5. Connect Unity (every contributor)

1. Open **Tools > BlockDiggerz > Asset Tracker > Settings**.
2. Paste the **Web App URL** and **Sync Secret** and click **Save**. Both are stored in `UserSettings/`, which is never committed.
3. Click **Sync**. The status bar reports what was written. **Open Sheet** now opens the sheet.

## Day to day

- **New art or audio file:** put it in `Assets/_Project/<Feature>/Art|Audio/` or `Assets/_Project/Shared/<Area>/Art|Audio/`. It is added as Placeholder automatically. Subfolders are fine.
- **Planned asset:** add a row in the sheet's Art or Audio tab with at least a Name, or click **Add Planned** in Unity. Its status is Missing until a file is linked.
- **File for a planned asset arrives:** select the row in the Unity window and drop the file on **Linked Asset**. If the file was already auto-added, Unity offers to merge the two rows.
- **Who wins:** sheet edits to Name, Description, Requirements, Status and Notes win, unless the same field was changed in Unity since the last sync. Unity always writes ID, Area, Asset Path, Asset Type and Updated In Unity.
- **Deleting:** deleting a planned row in the sheet removes it from Unity on the next sync. A row whose file exists in the project comes back. To drop a tracked file, delete or move the file, or use **Remove Entry** in Unity.
- Extra columns you add to a tab, such as Assignee or Due, are kept. Sync briefly rewrites each tab, so avoid editing the sheet during a sync.

## Troubleshooting

| Message | Fix |
|---|---|
| "did not return JSON" | Wrong URL, or **Who has access** is not **Anyone**. Some Google Workspace accounts don't allow Anyone; use a personal account for the sheet. |
| "Wrong sync secret" | Someone generated a new secret. Get the current one. |
| "Missing tab" or "has no ... column" | Run **Asset Tracker > Set up tabs** again. Renamed headers must be renamed back. |
