# MobileControls (portrait touch controls)

Namespace `BlockDiggerz.MobileControls`, asmdef `BlockDiggerz.MobileControls` (references `BlockDiggerz.Input`, `BlockDiggerz.Blocks`, `BlockDiggerz.DebugTools`, `Unity.InputSystem`). Target: Web builds on itch.io and CrazyGames played in mobile browsers.

## When it shows
- `TouchControls.IsMobilePlatform` (`UnityEngine.Device.Application.isMobilePlatform`, so the Device Simulator counts) or `ForceShow` (serialized `_forceShow`, debug toggle). Mobile platforms only: desktops with touchscreens get the desktop layout.
- Active: connects the touch pad, saves and fits the camera, switches the HUD to `BlocksHud.SetPortraitLayout(true)`. Inactive: the reverse (camera restored).
- Shown (visibility) only while `BlocksGame.State` is Playing and `GameInput.GameplayEnabled` (hidden under the pause menu, game over and win panels). Hiding releases every held button.

## Input path
- Buttons press a virtual Input System device, `BlockDiggerz.Input.TouchPadDevice` (layout `BlockDiggerzTouchPad`, state `TouchPadState`, one bit per `TouchPadButton`), through `TouchPad` (queues state events). GameControls binds each button in group `Touch` (MoveLeft, MoveRight, HardDrop, RotateClockwise, RotateCounterClockwise, StorePiece, Pause), so `BlocksGame` and `PauseMenuView` read touch through `GameInput` like keys, auto repeat included. Touch bindings are left out of key hints (`GameInput.TouchGroup`) and the rebinding page (keyboard group only).
- `TouchPressManipulator`: one per button element; a pointer down captures that pointer and holds the button until it lifts, cancels or loses capture; USS class `mobile-pressed` while held. Several elements can be held at once (multi-touch).
- Drag (`BoardDragManipulator` on `board-area`): only starts inside the board's on-screen rect (`BoardPanelRect`); sideways travel goes through `Core/DragStepper` (one column per `DragStepSize` = board cell width times `_dragCellsPerColumn`, remainder carried) and calls `BlocksGame.TryMove` directly. A blocked step drops the remainder.
- No soft drop on touch (decided).

## Layout
- `UI/MobileControls.uxml` + `.uss`, on the same `BlocksPanelSettings` as the HUD (one panel, sort order above the HUD). Top band: `store-slot` (STORE tap area; the HUD's STORE box and TIME are placed in it) and `pause-button` with `stats-slot` under it (HUD SCORE / NEXT placed there, right-aligned). Middle: full-height `move-left` / `move-right` strips with `board-area` and the bottom row (`rotate-ccw`, `hard-drop`, `rotate-cw`) between them.
- Camera fit (`Core/BoardFit`, `CameraFit`): the board never moves; the camera zooms and moves so `BoardView.BoardWorldRect` plus `_boardPadding` fills `board-area`, placed by `_boardAlignment` (bottom center by default, so DROP sits right under the board). Refits on geometry changes, board resizes (`BoardView.LayoutVersion`) and screen size changes, then hands the slot rects to `BlocksHud.SetPanelSlots`.

## Prefab and scene
- `Prefabs/MobileControls.prefab`: UIDocument (MobileControls.uxml, BlocksPanelSettings, sort order 1), `TouchControls`, `TouchControlsDebugControls`. The Game scene has one instance wired to BlocksGame, BlocksHud, BoardView and GameInput.

## Debug menu section "Touch controls"
Toggle: Force touch controls. Readouts: Mobile platform, Active, Shown, Touch pad, Pressed, Board on screen, Drag step.

## Tests
- `Tests/EditMode`: `DragStepperTests`, `BoardFitTests`, `TouchPadTests` (mask, device controls per bit, GameControls touch bindings), `TouchControlsLayoutTests` (UXML names, pickable touch targets, DROP below the board area).
- Not covered automatically: real touch events, multi-touch, the camera fit on screen, the HUD in portrait. See the playtest checklist.
