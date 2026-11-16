# MobileControls (portrait touch controls)

Namespace `BlockDiggerz.MobileControls`, asmdef `BlockDiggerz.MobileControls` (references `BlockDiggerz.Input`, `BlockDiggerz.Blocks`, `BlockDiggerz.DebugTools`, `Unity.InputSystem`). Target: Web builds on itch.io and CrazyGames played in mobile browsers.

## When it shows
- `TouchControls.IsMobilePlatform` (`UnityEngine.Device.Application.isMobilePlatform`, so the Device Simulator counts) or `ForceShow` (serialized `_forceShow`, debug toggle). Mobile platforms only: desktops with touchscreens get the desktop layout.
- Active: connects the touch pad, saves and fits the camera, switches the HUD to `BlocksHud.SetPortraitLayout(true)`. Inactive: the reverse (camera restored).
- Shown (visibility) only while `BlocksGame.State` is Playing and `GameInput.GameplayEnabled` (hidden under the pause menu, game over and win panels). Hiding releases every held button.

## Input path
- Buttons press a virtual Input System device, `BlockDiggerz.Input.TouchPadDevice` (layout `BlockDiggerzTouchPad`, state `TouchPadState`, one bit per `TouchPadButton`), through `TouchPad` (queues state events). GameControls binds each button in group `Touch` (MoveLeft, MoveRight, HardDrop, RotateClockwise, RotateCounterClockwise, StorePiece, Pause), so `BlocksGame` and `PauseMenuView` read touch through `GameInput` like keys, auto repeat included. Touch bindings are left out of key hints (`GameInput.TouchGroup`) and the rebinding page (keyboard group only).
- `TouchPressManipulator`: one per button element; a pointer down captures that pointer and holds the button until it lifts, cancels or loses capture; USS class `mobile-pressed` while held. Several elements can be held at once (multi-touch).
- Two modes, `GameInput.TouchMode` (`BlockDiggerz.Input.TouchControlMode`), picked in Settings (Controls tab, row shown only while `GameInput.TouchLayoutInUse`) and saved by `GameSettings`; default Gestures (`GameInput.DefaultTouchMode`). The menus apply the saved mode to `GameInput` in their `OnEnable`. `TouchControls.UpdateMode` toggles USS class `mobile-gestures` on `mobile-root` and releases everything on a switch.
- Buttons mode: `move-left` / `move-right` are the left and right halves of `board-area` (invisible, over the board). Tapping or holding one is a held MoveLeft / MoveRight button, auto repeat included. CCW / DROP / CW bottom row.
- Gestures mode: `gesture-area` (absolute, fills `board-area`; the move halves and rotate buttons are hidden by USS, DROP spans the bottom row) carries a `TouchGestureManipulator` following one finger through `Core/GestureTracker`. A finger that travels `_dragThreshold` panel pixels is a drag: each frame the piece steps one column toward the column under the finger (`Core/DragFollow`: finger cell from `BoardPanelRect`, piece center from `ActiveCells`, step when more than half a cell away). A finger that lifts sooner, within `_tapMaxDuration`, is a tap: left half CCW, right half CW (queued, at most `_maxQueuedTaps` per direction). `Core/GesturePulses` turns both into one-frame touch pad presses with a release frame between (`TouchPad.SetButtons`), so moves never auto repeat and the game still reads everything through `GameInput`.
- No soft drop on touch (decided).

## Layout
- `UI/MobileControls.uxml` + `.uss`, on the same `BlocksPanelSettings` as the HUD (one panel, sort order above the HUD). Top band: `store-slot` (STORE tap area; the HUD's STORE box and TIME are placed in it) and `pause-button` with `stats-slot` under it (HUD SCORE / NEXT placed there, right-aligned). Middle: `board-area`, full screen width, holding the two move halves. Bottom row: `rotate-ccw`, `hard-drop`, `rotate-cw`.
- Board height on touch (`Core/MobileBoardRows`): once per activation, the first usable fit picks the visible rows that fit `board-area` with the board at full width (padding included), clamped to `_minMobileRows`..`BlocksRules.VisibleHeight`, and calls `BlocksGame.SetBoardSize` (restarts the run). Turning the layout off from Update puts the rules' height back (another restart); disabling the component does not. Screen size changes afterwards do not resize the board (no mid-run restarts when a mobile browser bar hides); the board then just fits by height.
- Camera fit (`Core/BoardFit`, `CameraFit`): the board never moves; the camera zooms and moves so `BoardView.BoardWorldRect` plus `_boardPadding` (0, so it touches both edges) fills `board-area`, placed by `_boardAlignment` (centered by default). Refits on geometry changes, board resizes (`BoardView.LayoutVersion`) and screen size changes, then hands the slot rects to `BlocksHud.SetPanelSlots`.

## Orientation (phones only, `IsMobilePlatform`; never on desktop with Force touch controls)
- `_requestPortrait`: on activation and whenever `Screen.fullScreen` turns on, sets the autorotate flags to portrait only and `Screen.orientation = Portrait`. On the Web this is the browser's `screen.orientation.lock`, which works only in fullscreen on supporting browsers (Android Chrome); iOS Safari ignores it. The Web player dedups repeat requests of the same lock.
- `_guardPortrait` (`Core/PortraitGuard`): while the screen is wider than tall, `rotate-overlay` (outside `mobile-root`, pickable, blocks taps) covers everything with "Turn your phone upright to play". Turning sideways while the controls are shown pulses the touch pad's Pause button for one frame, so the pause menu opens. The mobile row count waits until the phone is upright.
- The main fix for itch.io is the page's embed settings (see Builds in the root CLAUDE.md): itch's own fullscreen launcher picks the orientation.

## Prefab and scene
- `Prefabs/MobileControls.prefab`: UIDocument (MobileControls.uxml, BlocksPanelSettings, sort order 1), `TouchControls`, `TouchControlsDebugControls`. The Game scene has one instance wired to BlocksGame, BlocksHud, BoardView and GameInput.

## Debug menu section "Touch controls"
Toggle: Force touch controls. Readouts: Mobile platform, Active, Shown, Mode, Dragging, Touch pad, Pressed, Board on screen, Screen orientation, Rotate message.

## Tests
- `Tests/EditMode`: `BoardFitTests`, `MobileBoardRowsTests`, `PortraitGuardTests`, `TouchPadTests` (mask, device controls per bit, `SetButtons`, GameControls touch bindings), `TouchControlsLayoutTests` (UXML names, pickable touch targets, DROP below the board area, move halves and gesture area inside the board area, CCW / DROP / CW bottom row, rotate overlay outside the root), `GestureTrackerTests`, `DragFollowTests`, `GesturePulsesTests`.
- Not covered automatically: real touch events, multi-touch, the mode switch on screen, drag feel, the camera fit and board resize on screen, the HUD in portrait. See the playtest checklist.
