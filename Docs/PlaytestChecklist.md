# Playtest Checklist

Each feature adds a section here. Write your notes under each item; Claude reads them next session and turns them into fixes or tuning changes.

## Asset Tracker (editor tool), 2026-11-14

Editor tooling, so this is a hands-on check of the workflow rather than game feel. Setup steps: `Assets/_Project/AssetTracking/Sheet/SETUP.md`.

- [x] **Sheet setup:** follow SETUP.md steps 1 to 4. Expect Art and Audio tabs with grey Unity columns, blue people columns, a Status dropdown and status colors.
  - Notes:
- [x] **Connect and first sync:** paste the Web App URL and secret in Tools > BlockDiggerz > Asset Tracker > Settings, Save, click Sync. Expect a status bar message (not red) like "Synced: 0 rows written..." and Open Sheet becoming clickable.
  - Notes:
- [x] **Auto-add:** create `Assets/_Project/Test/Art/` and drop a PNG in it. Expect a Placeholder row in the window (Area "Test", Sync "New"). Sync, and it appears in the Art tab with its Asset Path.
  - Notes:
- [x] **Planned row from the sheet:** add a row in the Audio tab with only a Name. Sync. Expect it in Unity as Missing, and an ID filled in the sheet.
  - Notes:
- [x] **Sheet edits win:** change Description and Status of a row in the sheet, Sync. Expect Unity to show the new values.
  - Notes:
- [x] **Unity edits win for changed fields:** change Notes in the Unity window (Sync column shows "Changed"), Sync. Expect the sheet to show the new Notes.
  - Notes:
- [x] **Link a file to a planned row:** select the planned Audio row, drop an audio clip on Linked Asset. Expect status Placeholder and the path filled; if the clip was already auto-added, a merge prompt.
  - Notes:
- [x] **Delete a file:** delete the test PNG in Unity. Expect its row to stay, marked Missing, in Unity and (after Sync) in the sheet.
  - Notes:
- [x] **Clean up:** Remove Entry for the test rows, delete `Assets/_Project/Test/`, Sync. Expect the rows gone from the sheet.
  - Notes:
- [x] **Window layout:** is the list and details split comfortable, are the filters and buttons where you expect them? Layout lives in `Assets/_Project/AssetTracking/UI/AssetTrackerWindow.uxml` and `.uss`.
  - Notes:

## Block Dropper (core loop), 2026-11-14

Open `Assets/Scenes/Game.unity` and press Play. Tunables live in `Assets/_Project/Blocks/Data/BlocksRules.asset` unless noted. The debug menu (backquote key) has a Blocks section to spawn a given piece, pause gravity, fill a row or force game over.

- [x] **Move:** tap A / D and the Left / Right arrows. Expect one column per tap, stopping at the walls.
  - Notes:
- [x] **Hold to slide:** hold a move key. Expect a short pause, then a fast slide to the wall. Tune `Auto Shift Delay` (pause) and `Auto Repeat Interval` (slide speed).
  - Notes:
- [x] **Gravity:** leave a piece alone. Expect one row per second. Tune `Gravity Interval`.
  - Notes:
- [x] **Soft drop:** hold S / Down arrow. Expect a much faster fall that stops on release. Tune `Soft Drop Interval`.
  - Notes:
- [x] **Hard drop:** press W. Expect the piece to land and lock at once and the next piece to spawn.
  - Notes:
- [x] **Lock delay:** let a piece land without hard dropping. Expect a brief moment to slide it sideways before it locks. Tune `Lock Delay`.
  - Notes:
- [x] **Rotate:** press E / Up arrow. Expect a clockwise turn. Against a wall or the stack the turn is refused (simple rotation, no wall kicks).
  - Notes:
- [x] **Rotate counter-clockwise (added 2026-11-14):** press Q. Expect a counter-clockwise turn; Q then E returns the piece to where it started. Refused against walls or the stack, like E. Rebind in `Assets/_Project/Input/Data/GameControls.inputactions` (Gameplay > RotateCounterClockwise).
  - Notes:
- [x] **Pieces:** play a few bags. Expect all seven pieces (I O T S Z J L) in their colors, each once per 7 pieces. Colors: `Assets/_Project/Blocks/Data/Pieces/Piece*.asset`.
  - Notes:
- [x] **Line clear:** fill a row. Expect it to vanish, rows above to drop and LINES to count up (also 2 to 4 rows at once).
  - Notes:
- [x] **Top-out and restart:** stack to the top. Expect a GAME OVER panel with the line count; R starts a fresh run.
  - Notes:
- [x] **Scene reload:** debug menu, Game section, Reload scene. Expect controls to keep working afterwards.
  - Notes:
- [x] **Look:** board framing, cell sprite (`Assets/_Project/Blocks/Art/BlockCell.png`), HUD position and sizes (`Assets/_Project/Blocks/UI/BlocksHud.uss`), camera size (Main Camera in the Game scene).
  - Notes:
- [x] **Tags:** debug menu, Blocks section, Tags readout shows Basic for every piece. Tag asset: `Assets/_Project/Blocks/Data/Tags/Basic.asset`.
  - Notes:

## Playtest Checklist window (editor tool), 2026-11-14

Open Tools > BlockDiggerz > Playtest Checklist. It reads and writes this file. Layout: `Assets/_Project/Playtest/UI/PlaytestChecklistWindow.uxml` and `.uss`.

- [x] **Tabs:** expect a To check tab and a Done tab, each showing its item count, with items grouped under collapsible feature headings.
  - Notes:
- [x] **Tick and confirm:** tick two items (click the box or the text). Expect them highlighted green and the button to read Confirm checked (2). Press it. Expect both to move to the Done tab and show `[x]` in this file.
  - Notes:
- [x] **Clear:** tick a few items, press Clear. Expect all ticks removed and nothing moved.
  - Notes:
- [x] **Reopen:** in the Done tab, press Reopen on an item. Expect it back in To check and `[ ]` in this file.
  - Notes:
- [x] **Notes:** type a note (several lines too) under an item and click elsewhere. Expect "Notes saved." and the text under that item's Notes line here.
  - Notes: tetetetet
- [ ] **Outside edits:** edit this file in a text editor (add an item or a note), save, then click back into the window. Expect the change to show without pressing Reload.
  - Notes:
- [x] **Search:** type in the search box. Expect only items whose text, notes or feature name match.
  - Notes: tetete
- [x] **Readability:** are the rows, highlight color, note fields and footer comfortable to use?
  - Notes:

## Landing ghost, 2026-11-14

A see-through copy of the falling piece shows where it will land, in the piece color, each cell fading from bottom to top. Settings on the BoardView component of `Assets/_Project/Blocks/Prefabs/BlocksGame.prefab` (Landing ghost header).

- [ ] **Follows the piece:** move and rotate a piece. Expect the ghost to stay directly below it, resting on the floor or the stack, and to match its shape and color.
  - Notes:
- [ ] **Matches hard drop:** press W. Expect the piece to lock exactly where the ghost was.
  - Notes:
- [ ] **Overlap:** soft drop until the piece reaches the ghost. Expect the solid piece to cover the ghost with no flicker.
  - Notes:
- [ ] **Readability:** can you tell the ghost from real blocks at a glance? Tune `Ghost Opacity` on BoardView, or the fade itself in `Assets/_Project/Blocks/Art/GhostCell.png` (solid outline, bottom about 100% to top about 12%). Untick `Show Ghost` to compare without it.
  - Notes:
- [ ] **Game over:** top out. Expect the ghost to disappear on the game over screen.
  - Notes:

## Wall push and lock delay extension, 2026-11-14

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset` (Falling and Rotation push headers). The debug menu Blocks section shows Lock timer and Extensions used.

- [ ] **Push off a wall:** stand an I piece up, move it against the left wall, rotate. Expect it to lie flat, pushed 2 cells right. Same on the right wall, pushed left. Tune `Max Sideways Push`.
  - Notes:
- [ ] **Push off blocks:** rotate a T or L right next to a stack of blocks. Expect it to turn and shift away from the blocks instead of refusing.
  - Notes:
- [ ] **Push up:** rotate a piece resting in a one-row dip so the turned shape would poke into the floor or stack. Expect it to pop up 1 row. Tune `Max Up Push`.
  - Notes:
- [ ] **No room:** wedge a piece where no push within the limits fits. Expect the rotation to do nothing.
  - Notes:
- [ ] **Extend by rotating:** let a piece land and keep rotating it. Expect it to stay unlocked longer than 0.5 s, then lock after about 15 rotations. Tune `Lock Extension Bonus` and `Max Lock Extensions`.
  - Notes:
- [ ] **Extend by sliding:** let a piece land on a flat stack and tap left and right. Expect the same extension as rotating.
  - Notes:
- [ ] **No banking:** rotate quickly several times right after landing, then stop. Expect the piece to lock no later than 0.5 s after your last input.
  - Notes:
- [ ] **Fresh count lower down:** use up extensions on a ledge, then slide off so the piece drops a row. Expect a full new lock delay and extension count there.
  - Notes:
- [ ] **Hard drop:** press W. Expect an instant lock, with no extension or delay.
  - Notes:

## Next piece and store piece, 2026-11-14

Layout and look in `Assets/_Project/Blocks/UI/BlocksHud.uss` (`.hud-piece-box` size, `.hud-store` position, `.hud-store--locked` dim opacity). Store key: `Assets/_Project/Input/Data/GameControls.inputactions` (Gameplay > StorePiece). The debug menu Blocks section shows Next, Stored and Can store, and has a Store piece button.

- [ ] **Next preview:** play a few pieces. Expect a NEXT box below the LINES count showing the piece that spawns next, in its color and starting orientation, centered in the box.
  - Notes:
- [ ] **First store:** press Space. Expect the falling piece to appear in the STORE box (left of the board) and the next piece to spawn at the top.
  - Notes:
- [ ] **Store once per piece:** press Space again right away. Expect nothing to happen and the STORE box to look dimmed.
  - Notes:
- [ ] **Unlock after placing:** place the piece. Expect the STORE box to brighten; Space now swaps, with the stored piece entering at the top in its starting rotation.
  - Notes:
- [ ] **Key hint:** the STORE box shows the key under it (Space). Rebind StorePiece and restart Play. Expect the hint to follow the new key.
  - Notes:
- [ ] **Restart:** store a piece, top out, press R. Expect an empty STORE box and storing unlocked.
  - Notes:
- [ ] **Readability:** are the NEXT and STORE boxes easy to read at a glance, and is the dimmed look clear? Tune the box size, colors and dim opacity in BlocksHud.uss.
  - Notes:

## Main menu, pause menu and settings, 2026-11-14

Open `Assets/Scenes/MainMenu.unity` and press Play (the Game scene can still be played directly too). Layouts: `Assets/_Project/Menus/UI/` (MainMenu.uxml, PauseMenu.uxml, SettingsPanel.uxml, Menus.uss). Scene names on the MainMenu and PauseMenu prefabs (`Game Scene`, `Main Menu Scene`).

- [ ] **Main menu:** expect BLOCKDIGGERZ with Play, Settings and Quit. Play starts a run. Quit closes a built game (does nothing in the Editor).
  - Notes:
- [ ] **Keyboard navigation:** use arrow keys or Tab and Enter on the menus. Expect a visible highlight and buttons that activate.
  - Notes:
- [ ] **Pause:** press Esc during a run. Expect PAUSED with Continue, Settings and Main menu, the piece frozen, and game keys (move, rotate, drop, store) doing nothing.
  - Notes:
- [ ] **Continue:** press Esc again or click Continue. Expect the run to carry on exactly where it was, including the lock timer.
  - Notes:
- [ ] **Back with Esc:** open Settings from the pause menu, press Esc. Expect the pause menu; Esc again continues the game. From the main menu, Esc in Settings returns to the main menu.
  - Notes:
- [ ] **Main menu from pause:** click Main menu. Expect the main menu; Play starts a fresh run.
  - Notes:
- [ ] **Game over:** top out, press Esc. Expect the pause menu; Continue returns to the game over screen where R restarts.
  - Notes:
- [ ] **Tabs:** in Settings, switch between Controls and Sound. Expect only one page visible and the selected tab highlighted.
  - Notes:
- [ ] **Rebind:** on Controls, click Rotate Clockwise's key, press F. Expect F to rotate in game. Click a key and press Esc instead. Expect no change.
  - Notes:
- [ ] **Swap on conflict:** set Rotate Clockwise to W (hard drop's key). Expect hard drop to take the old rotate key, and both rows to update.
  - Notes:
- [ ] **Store hint follows:** rebind Store Piece, then continue. Expect the key under the STORE box to show the new key.
  - Notes:
- [ ] **Saved between sessions:** change a key and both sliders, stop Play, start again. Expect the same keys and slider values. Reset to defaults puts each page back.
  - Notes:
- [ ] **Sound sliders:** move Music and Sound effects. Expect values 0 to 100 (default 80). No audio changes yet; these are placeholders.
  - Notes:
- [ ] **Look and size:** are the panels, buttons, tabs and rows comfortable to read at your screen size? Tune Menus.uss.
  - Notes:
