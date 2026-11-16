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
- [ ] **Controls layout:** on Controls, expect a header (Action, Primary, Alternate) and one row per action with its name and two key buttons. Hard Drop, Rotate Counter Clockwise, Store Piece and Restart show a dash under Alternate. Column widths: `.controls-action`, `.controls-column` and `.controls-key` in Menus.uss.
  - Notes:
- [ ] **Alternate key:** click Hard Drop's Alternate dash, press F. Expect both W and F to hard drop.
  - Notes:
- [ ] **Clear a key:** click a key, press Backspace. Expect a dash. Clearing a Primary while the Alternate has a key moves the Alternate key into Primary. Clearing an action's only key leaves it unbound (Reset to defaults restores it). Backspace itself can't be bound.
  - Notes:
- [ ] **Take a key into an empty slot:** set Hard Drop's Alternate to A (Move Left's Primary). Expect Move Left to keep only Left Arrow, now shown as its Primary.
  - Notes:
- [ ] **Store hint follows:** rebind Store Piece, then continue. Expect the key under the STORE box to show the new key.
  - Notes:
- [ ] **Saved between sessions:** change a key and both sliders, stop Play, start again. Expect the same keys and slider values. Reset to defaults puts each page back.
  - Notes:
- [ ] **Sound sliders:** move Music and Sound effects. Expect values 0 to 100 (default 80). No audio changes yet; these are placeholders.
  - Notes:
- [ ] **Look and size:** are the panels, buttons, tabs and rows comfortable to read at your screen size? Tune Menus.uss.
  - Notes:

## Colored blocks, color matches and score, 2026-11-14

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset` (Colors: Color Count, Min Color Match, Clear Step Duration; Scoring: Color Block Score). Colors are assets in `Assets/_Project/Blocks/Data/Colors/`, ordered in `Data/BlockPalette.asset` (the first Color Count are used). Flash look on the BlocksGame prefab's BoardView (Clear Flash Color, Clear Flash Amount, Unpainted Color). The debug menu Blocks section shows Block colors, Next and Stored colors, Score, Settling, and has Color match (3) and Color chain (2 steps) buttons.

- [ ] **Painted pieces:** play a few pieces. Expect each piece to have 2 or 3 colors, any repeated color on 2 touching blocks, never 3 of one color. T pieces always have 3 colors.
  - Notes:
- [ ] **Colors stick to blocks:** rotate a piece with E and Q, store it with Space and bring it back. Expect every block to keep its color. The ghost, NEXT and STORE show the same colors.
  - Notes:
- [ ] **Color match:** line up 3 same-colored blocks in a row or column. Expect them to flash, then disappear, blocks above them to fall, and SCORE to go up 30 (10 per block). No new piece falls until it's done.
  - Notes:
- [ ] **No bends:** make an L of 3 same-colored blocks. Expect nothing to happen.
  - Notes:
- [ ] **Long line and cross:** a line of 4 gives 40. A plus shape (3 across and 3 down sharing the middle) deletes 5 blocks and gives 60.
  - Notes:
- [ ] **Chains:** press Color chain (2 steps) in the debug menu. Expect a red line to flash and go, the blue blocks to fall and match, then 60 total. Each step flashes for Clear Step Duration.
  - Notes:
- [ ] **Score display:** SCORE above NEXT (no LINES any more), and on the game over panel. R restarts at 0.
  - Notes:
- [ ] **Pause while clearing:** press Esc during a flash. Expect everything to freeze, and Continue to finish the clear.
  - Notes:
- [ ] **Color count feel:** with 4 colors, do matches happen too often or too rarely? Try Color Count 5 or 6 (at least 3 is needed).
  - Notes:
- [ ] **Color readability:** can you tell the 4 colors apart quickly, and is the flash clear? Tune the color assets' Tint and the flash fields.
  - Notes:

## Block gravity (full rows no longer clear), 2026-11-14

Placed blocks fall in same-colored groups at twice the piece fall speed (`Assets/_Project/Blocks/Data/BlocksRules.asset`, Falling: Placed Fall Speed Multiplier, applied to Gravity Interval). The next piece waits until the board has settled. The debug menu Blocks section shows Settling (blocks falling, clearing, or a dash).

- [ ] **Pieces split on landing:** drop a piece so part of it hangs over a gap (for example a flat I on a single block). Expect each color group with nothing under it to keep falling on its own, and touching blocks of the same color to stay together.
  - Notes:
- [ ] **Groups across pieces:** land a block next to a same-colored block of another piece, then remove what holds one of them up. Expect them to fall together.
  - Notes:
- [ ] **Holes fill:** clear a color line above a hole. Expect everything above to fall all the way into the hole.
  - Notes:
- [ ] **No player piece while settling:** after a lock, expect no new piece (and store and moves to do nothing) until all blocks have stopped and every match has cleared. Then the next piece spawns.
  - Notes:
- [ ] **Full rows stay:** fill a whole row (Fill bottom row (I gap), then drop an I flat). Expect the row to stay; only color matches in it clear.
  - Notes:
- [ ] **Chains with gravity:** press Color chain (2 steps). Expect the red line to clear, the blue pair to fall one row, then the three blues to clear: 60 in total.
  - Notes:
- [ ] **Fall speed feel:** placed blocks fall at 2x the piece speed (0.5 s per row). Does waiting for long falls feel right? Tune Placed Fall Speed Multiplier.
  - Notes:
- [ ] **Pause while settling:** press Esc while blocks fall. Expect them to freeze, and Continue to carry on.
  - Notes:
- [ ] **Deleting before falling (added 2026-11-14):** make a match land while other blocks are still falling elsewhere. Expect the match to flash and clear right away, with the other blocks frozen in the air until it is gone, then falling resumes.
  - Notes:
- [ ] **Falling blocks don't stick (added 2026-11-14):** let a falling block slide past a resting block of its color. Expect it to keep falling, not to stop beside it.
  - Notes:
- [ ] **Falling blocks don't match (added 2026-11-14):** let a falling block pass right next to two same-colored resting blocks in a row. Expect no delete while it passes; it can only match where it lands.
  - Notes:

## Block color sprites, 2026-11-14

Each color asset in `Assets/_Project/Blocks/Data/Colors/` has a Sprite slot. Put the art in `Assets/_Project/Blocks/Art/` (tracked), import as Sprite (2D and UI) at 32 pixels per unit, and drag it into the color's Sprite field.

- [ ] **Board and falling piece:** with sprites set, expect each block drawn with its color's art, untinted. Colors without a sprite keep the tinted white block.
  - Notes:
- [ ] **NEXT and STORE:** expect the previews to show the same sprites, and the STORE box to dim them while locked.
  - Notes:
- [ ] **Ghost:** the landing ghost still uses the fade sprite tinted with the color's Tint. Does it still match your art? Adjust each color's Tint, or ask for the ghost to use the color sprites.
  - Notes:
- [ ] **Match flash:** matched blocks blend toward the flash color on top of your art. Check it reads well (BoardView: Clear Flash Color, Clear Flash Amount on the BlocksGame prefab).
  - Notes:

## Live board size and side UI, 2026-11-14

Debug menu (backquote), Blocks section: Board size readout and Board width -1 / +1, Board height -1 / +1, Board size reset buttons. Each change restarts the run. Limits 4-30 wide, 8-40 tall. Default size: `Assets/_Project/Blocks/Data/BlocksRules.asset` (Board Width, Visible Height). Gap between board and side UI: BlocksHud prefab, Side Gap. The camera doesn't move or zoom (camera features come later).

- [ ] **Centered:** make the board wider, narrower, taller and shorter. Expect it to stay centered on screen every time.
  - Notes:
- [ ] **Side UI follows:** expect STORE to stay just left of the board and SCORE / NEXT just right of it, both lined up with the board's top row, at every size.
  - Notes:
- [ ] **Window resize:** resize the Game view (or change its aspect) while playing. Expect the side UI to stay beside the board.
  - Notes:
- [ ] **Pieces at small sizes:** at 4 wide, expect every piece to spawn centered and rotate (with pushes) without getting stuck.
  - Notes:
- [ ] **Big boards:** at 30 x 40, the board runs off screen with this camera. Confirm that's fine until camera features exist.
  - Notes:
- [ ] **Side gap feel:** is the gap between board and side UI right? Tune Side Gap.
  - Notes:

## Level start columns, 2026-11-14

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset`, Level start: Start Column Min Height, Start Column Max Height, Start Fill Duration, Start Fill Fall Interval. Color Count is now 6. Starting blocks count as the BlocksGame prefab's Start Block Piece (empty: first piece of the set).

- [ ] **Drop in:** start a run. Expect columns to drop in one at a time from left to right, each falling fast from the top, taking about 3 seconds in all. No piece appears until they all rest.
  - Notes:
- [ ] **Heights:** expect every column to hold 1 to 4 blocks, different each run.
  - Notes:
- [ ] **No deletes at start:** expect no straight line of 3 same-colored blocks in the starting rubble, so nothing flashes or clears and the score stays 0.
  - Notes:
- [ ] **Every start:** press R after a game over, and resize the board from the debug menu. Expect fresh rubble each time.
  - Notes:
- [ ] **Six colors:** expect pieces and rubble to use all six colors. Are matches still frequent enough to play? Tune Color Count.
  - Notes:
- [ ] **Pace:** is 3 seconds of drop-in too long or too short, and does the fast fall read well? Tune Start Fill Duration and Start Fill Fall Interval.
  - Notes:
- [ ] **Pause during drop-in:** press Esc while columns drop. Expect them to freeze, and Continue to carry on.
  - Notes:

## Dirt obstacles, 2026-11-16

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset`, Level start Dirt: Start Dirt Min Per Column (1), Start Dirt Max Per Column (3), Start Dirt Spread (0.75, bell curve width), Start Dirt Bottom Falloff (0.5; 0 = always at the bottom, 1 = mixed in evenly). Dirt looks are the BlocksGame prefab's Start Dirt list (`Assets/_Project/Blocks/Data/Dirt/DirtA-D.asset`). The debug menu shows a Dirt blocks count.

- [ ] **Dirt in every column:** start a run. Expect every column to get 1 to 3 Dirt blocks on top of its 1 to 4 colored blocks, 2 most often, with the four Dirt looks mixed.
  - Notes:
- [ ] **Mostly at the bottom:** expect Dirt to sit on the floor most of the time, sometimes between or above colored blocks. Too scattered or too tidy? Tune Start Dirt Bottom Falloff.
  - Notes:
- [ ] **Dirt never clears:** line up 3 or more Dirt in a row or column (they often touch along the floor). Expect nothing to flash or clear.
  - Notes:
- [ ] **Dirt breaks lines:** expect two same-colored blocks with Dirt between them never to count as a line.
  - Notes:
- [ ] **Dirt falls alone:** clear colored blocks under a Dirt block. Expect it to drop as a single block, not dragging touching Dirt with it.
  - Notes:
- [ ] **Only at level start:** play several pieces. Expect Dirt to appear only in the starting drop-in, never on pieces or in NEXT and STORE.
  - Notes:
- [ ] **Looks:** Dirt sprites now import at 32 pixels per unit like the colored blocks. Do they fit the cells?
  - Notes:
- [ ] **Amount:** does 1 to 3 per column feel right, and does the curve favor 2 enough? Tune Start Dirt Spread (smaller means more 2s).
  - Notes:

## Match blasts, 2026-11-16

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset`, Match blasts: Horizontal Blast Base (1), Horizontal Blast Per Extra Block (1), Vertical Blast Base (1), Vertical Blast Per Extra Block (1). The debug menu shows a Blocks blasted count; "Color chain (2 steps)" was rebuilt so the blast doesn't break it.

- [ ] **Row of 3:** match 3 in a row with blocks on both sides. Expect the 3 plus 1 block left and 1 block right to flash and vanish.
  - Notes:
- [ ] **Row of 4 and 5:** expect 2 cells each side for 4, 3 each side for 5.
  - Notes:
- [ ] **Column of 3:** match 3 stacked. Expect the 3 plus 1 block directly below to vanish; nothing above is blasted (it falls instead).
  - Notes:
- [ ] **Column of 4 and 5:** expect 2 cells below for 4, 3 below for 5.
  - Notes:
- [ ] **Digging Dirt:** make a match next to or above Dirt. Expect the Dirt to be blasted away; this is the only way to remove Dirt.
  - Notes:
- [ ] **Gaps use up reach:** match 3 in a row with an empty cell right beside it and a block after that. Expect that block to stay.
  - Notes:
- [ ] **Score:** expect only the matched blocks to score (10 each); blasted blocks give nothing.
  - Notes:
- [ ] **Chains:** expect blasts to open space, blocks to fall into it and new matches to blast again. Press the debug "Color chain (2 steps)" button and expect 2 clear steps.
  - Notes:
- [ ] **Power:** do blasts feel too strong or too weak, sideways and downward? Tune the four Match blasts values.
  - Notes:

## Timer, speed-up and Dirt win, 2026-11-16

Tunables in `Assets/_Project/Blocks/Data/BlocksRules.asset`, Speed-up: Speed Up Score Step (200), Speed Up Per Step (0.1), Max Speed Multiplier (0 = no cap). Main menu scene name on the BlocksHud prefab (Main Menu Scene). Debug menu: Run time, Gravity speed readouts and a Win now button.

- [ ] **Timer:** start a run. Expect TIME above STORE, right-aligned like SCORE on the other side, showing 0:00 through the drop-in and starting when the first piece appears, in minutes:seconds.
  - Notes:
- [ ] **Timer pauses and stops:** press Esc: expect the timer to freeze. Lose or win: expect it to stop. Press R: expect 0:00 again.
  - Notes:
- [ ] **Speed-up:** score 200 points. Expect the piece to fall a bit faster (debug Gravity speed x1.1), x1.2 at 400, and so on. Soft drop and settling blocks keep their speed.
  - Notes:
- [ ] **Speed curve:** does the game get too fast too soon, or not soon enough? Tune Speed Up Score Step, Speed Up Per Step and Max Speed Multiplier.
  - Notes:
- [ ] **Win by digging:** blast away every Dirt block. Expect the board to finish settling, then a LEVEL CLEAR screen with your Time and Score, and no new piece.
  - Notes:
- [ ] **Win buttons:** on the win screen, expect New level to start a fresh run (new board, timer 0:00, score 0) and Main menu to load the main menu. R also starts a new level. Try mouse and keyboard (New level starts focused).
  - Notes:
- [ ] **Win now:** press the debug Win now button mid-run. Expect the win screen at once.
  - Notes:
- [ ] **Readability:** is the win screen clear and does it look right over the board? Do the TIME and STORE boxes still fit beside the board at small board heights?
  - Notes:

## Clear the dirt! message, 2026-11-16

Look in `Assets/_Project/Blocks/UI/BlocksHud.uss` (`.hud-level-intro-text`: font size, color, outline, shadow); the text itself is in `Assets/_Project/Blocks/UI/BlocksHud.uxml` (`level-intro`). How long it shows follows Start Fill Duration in `Assets/_Project/Blocks/Data/BlocksRules.asset`.

- [ ] **Shows at level start:** start a run. Expect "Clear the dirt!" in the middle of the screen while the starting columns drop in, gone as soon as the first piece appears.
  - Notes:
- [ ] **Every new level:** press R, use New level on the win screen and change the board size from the debug menu. Expect the message each time the columns drop in again.
  - Notes:
- [ ] **Readability:** is the message easy to read over the board and dropping blocks, and does it stay on screen long enough? Tune the USS style or Start Fill Duration.
  - Notes:

## Pause menu after a mid-play recompile, 2026-11-16

Fix for a NullReferenceException in `SettingsPanelController` when the pause menu re-enabled after scripts recompiled during Play mode. No tunables.

- [ ] **Pause after recompile:** play the Game scene, then save any script change (or let another session edit one) while still playing. After the recompile, press Esc. Expect the pause menu to open with no NullReferenceException in the Console, and Settings to show the sliders and key rows.
  - Notes:
- [ ] **Bindings kept:** rebind a key in Settings, recompile mid-play as above, then open Settings again. Expect the new key to still show and work.
  - Notes:
- [ ] **Main menu too:** in the MainMenu scene, recompile mid-play, then open Settings. Expect no exception.
  - Notes:

## Mobile touch controls, 2026-11-16

Portrait touch layout for mobile browsers (itch.io, CrazyGames). In the Editor, turn it on with the debug menu (backquote, section Touch controls, Force touch controls) or the Force Show box on the MobileControls object in the Game scene, and try it in the Device Simulator (Game view dropdown, Simulator), where it turns on by itself for phones. A mouse works like one finger. Layout sizes are in `Assets/_Project/MobileControls/UI/MobileControls.uss` (top band height, strip width, button sizes); drag and board tunables are on the TouchControls component of `Assets/_Project/MobileControls/Prefabs/MobileControls.prefab` (Drag Cells Per Column, Board Padding, Board Alignment). HUD portrait styles are the `.hud-root--portrait` rules in `Assets/_Project/Blocks/UI/BlocksHud.uss`.

- [ ] **Shows only on mobile:** play on desktop with the force toggle off. Expect the normal desktop layout. In the Device Simulator with a phone selected (or with the toggle on), expect the portrait layout.
  - Notes:
- [ ] **Layout:** expect the STORE box top left with TIME under it, the pause button top right with SCORE and NEXT under it, a left and a right arrow strip down both edges, and CCW, DROP, CW right under the board. The board should fill the space between the strips. Try a tall phone (19.5:9) and a 16:9 phone.
  - Notes:
- [ ] **Arrow strips:** tap a strip to move one column. Hold it: the piece should keep moving after the same delay and repeat rate as holding a key (Auto Shift Delay and Auto Repeat Interval in `Assets/_Project/Blocks/Data/BlocksRules.asset`).
  - Notes:
- [ ] **Drag on the board:** put a finger on the board and slide sideways. Expect the piece to move one column per cell of finger travel, from wherever you touch on the board. Touching outside the board (above it or beside it) should not drag. If it feels too slow or too twitchy, tune Drag Cells Per Column.
  - Notes:
- [ ] **Buttons:** CCW and CW rotate, DROP hard drops, tapping the STORE box stores the piece (and does nothing while STORE is dimmed). There is no soft drop on touch.
  - Notes:
- [ ] **Two fingers:** hold a strip while tapping a rotate button, or drag with one finger and tap DROP with the other. Expect both to work.
  - Notes:
- [ ] **Pause:** tap the pause button. Expect the pause menu, with the touch controls hidden until you Continue. Check that the pause menu and Settings are usable on a portrait phone screen.
  - Notes:
- [ ] **Game over and win:** top out, then win a level. Expect the touch controls to hide, and the game over panel to have Restart and Main menu buttons that work by tap (and by mouse on desktop).
  - Notes:
- [ ] **Back to desktop:** turn the force toggle off mid-run. Expect the camera, board and HUD to go back to the desktop layout.
  - Notes:
- [ ] **Store dimming:** on desktop, store a piece. Expect only the STORE box to dim now, not the TIME above it.
  - Notes:
- [ ] **Real phone:** upload a Web build to itch.io (or host it locally) and open it in a phone browser. Expect the same layout as the simulator and responsive touches. Check landscape too.
  - Notes:
- [ ] **Readability and feel:** are the strips, buttons and labels big enough and easy to hit? Is the board big enough? Tune the sizes in the USS.
  - Notes:

## Full-width mobile board and tap to move, 2026-11-16

Drag gestures and the side strips are gone on mobile. The board spans the screen width, its visible rows are cut to what fits (between Min Mobile Rows and the rules' Visible Height), and tapping or holding the left or right half of the board moves the piece. Desktop is unchanged and the rules board is 10 x 20 again (it was 15 wide). Tunables: Board Padding, Min Mobile Rows and Board Alignment on the TouchControls component of `Assets/_Project/MobileControls/Prefabs/MobileControls.prefab`; top band, bottom row and button sizes in `Assets/_Project/MobileControls/UI/MobileControls.uss`; Board Width / Visible Height in `Assets/_Project/Blocks/Data/BlocksRules.asset`.

- [ ] **Board edge to edge:** in the Device Simulator (or with Force touch controls on), expect the board to touch both sides of the screen, STORE / TIME and pause / SCORE / NEXT above it, and CCW, DROP, CW below it, with nothing overlapping. Try a 19.5:9 and a 16:9 phone; the 16:9 one should get fewer rows (about 12, the 19.5:9 one about 15).
  - Notes:
- [ ] **Tap to move:** tap the left half of the board to move one column left, the right half to move right. Hold to keep moving with the keyboard's repeat delay and rate. Swiping should do nothing beyond that press.
  - Notes:
- [ ] **Run restart on layout switch:** the run restarts once when the touch layout turns on (the board gets its mobile height) and once more when it turns off. On a phone this happens during the opening drop-in, so it should not be noticeable.
  - Notes:
- [ ] **Desktop board:** on desktop expect 10 columns and 20 rows.
  - Notes:
- [ ] **Feel:** is the shorter mobile board too cramped? To fit more rows, shrink the top band or the bottom row in the USS.
  - Notes:

## Portrait on phones, 2026-11-16

The previous itch.io build opened in landscape on a phone. Fix in two parts: itch's embed settings (Edit game, Embed options: tick "Mobile friendly", orientation Portrait, then save), and in game a portrait lock request plus a rotate message. Tunables: Request Portrait and Guard Portrait on the TouchControls component of `Assets/_Project/MobileControls/Prefabs/MobileControls.prefab`; message look in `.mobile-rotate-overlay` / `.mobile-rotate-overlay-text` in `Assets/_Project/MobileControls/UI/MobileControls.uss`.

- [ ] **itch settings:** after setting Mobile friendly + Portrait on itch, open the page on a phone and tap Run game. Expect the game fullscreen and upright with the touch layout.
  - Notes:
- [ ] **Android lock:** on Android Chrome, once the game is fullscreen, rotate the phone sideways. Expect the screen to stay portrait (the browser lock). If it still rotates, expect the rotate message instead.
  - Notes:
- [ ] **Rotate message:** on any phone (iPhone Safari included) or in the Device Simulator with a phone turned sideways (rotate button in the Simulator toolbar), expect a dark screen with "Turn your phone upright to play", and the game paused underneath. Turn it upright: the message goes and the pause menu is there; tap Continue.
  - Notes:
- [ ] **Starting sideways:** open the game with the phone already sideways. Expect the message right away; turning upright should give the full-width board with the right row count for portrait.
  - Notes:
- [ ] **Desktop untouched:** on desktop, and in the Editor with Force touch controls on, expect no rotate message and no orientation change.
  - Notes:

## Gesture touch controls, 2026-11-17

Touch controls now have two modes, picked in Settings, Controls tab, "Touch controls" row (only shown on mobile, or with Force touch controls on in the debug menu). Gestures is the default; Buttons is the previous layout, unchanged. Debug menu, Touch controls section, shows Mode and Dragging. Tunables: Drag Threshold, Tap Max Duration and Max Queued Taps on the TouchControls component of `Assets/_Project/MobileControls/Prefabs/MobileControls.prefab`; gesture layout rules (`.mobile-gestures ...`) in `Assets/_Project/MobileControls/UI/MobileControls.uss`; settings row look in `.touch-mode-row` / `.touch-mode-option` in `Assets/_Project/Menus/UI/Menus.uss`.

- [ ] **Default mode:** on a fresh install (or after clearing PlayerPrefs), start a run on a phone or the Device Simulator. Expect gesture mode: no CCW / CW buttons, DROP spanning the bottom row, STORE and pause as before.
  - Notes:
- [ ] **Drag to move:** put a finger on the board and drag sideways. Expect the piece to slide column by column until it sits under the finger, and to follow as the finger moves. Dragging toward a wall or the stack stops it there. Lifting the finger leaves the piece where it is.
  - Notes:
- [ ] **Tap to rotate:** tap the left half of the board. Expect a counter-clockwise turn; the right half turns clockwise. Two quick taps should give two turns. A long press without moving should do nothing.
  - Notes:
- [ ] **Tap vs drag:** small finger wobbles during a tap should still rotate, not move. If taps turn into drags too often, raise Drag Threshold; if drags feel slow to start, lower it. If quick taps get missed, raise Tap Max Duration.
  - Notes:
- [ ] **Drop while dragging:** drag with one finger and tap DROP with another. Expect the piece to hard drop and the next piece to follow the dragging finger.
  - Notes:
- [ ] **Switch mode:** pause, Settings, Controls, pick Buttons, go back. Expect the previous layout (tap halves to move, CCW / DROP / CW). Pick Gestures again and the layout switches back. The choice should survive a reload and a return to the main menu.
  - Notes:
- [ ] **Desktop:** on desktop without Force touch controls, expect no Touch controls row in Settings.
  - Notes:
- [ ] **Feel:** does the piece follow the finger fast enough? It moves one column every other frame (about 30 columns a second at 60 fps). Is the finger hiding the piece too much?
  - Notes:
