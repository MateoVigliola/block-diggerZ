# Blocks (core block dropper)

Namespace `BlockDiggerz.Blocks`, asmdef `BlockDiggerz.Blocks` (references `BlockDiggerz.Input`, `BlockDiggerz.DebugTools`).

## Pieces
- `PieceDefinition` (ScriptableObject, `Data/Pieces/Piece*.asset`): id, color, box size, spawn cells inside a square box (y up). Clockwise rotation turns the box: `(x, y) -> (y, box - 1 - x)`. Standard shapes use box 3, I uses 4, O uses 2.
- `PieceSet` (`Data/StandardPieceSet.asset`): the 7 pieces. `PieceBag` deals each piece once per bag.
- Tags: `PieceTag` assets (`Data/Tags/`). Every piece lists its tags; only `Basic` for now. Board cells keep their `PieceDefinition`, so tags survive locking. Compare tags by asset reference.

## Rules and loop
- `BlocksRules` (`Data/BlocksRules.asset`): 10 x 20 board plus 2 hidden rows, gravity, soft drop, lock delay, auto shift delay and repeat, seed.
- `BlocksGame`: spawn centered with the top row on the top visible row; rotation clockwise (E/Up) or counter-clockwise (Q); hard drop (W) locks at once; full rows clear; spawn overlap means game over; R restarts. Reads input only through `GameInput`.
- Rotation push (`Core/RotationPush`): a blocked rotation tries offsets in order (1 sideways away from what blocks, 1 the other way, 2 away, 2 the other way, then 1 up, then 1 up plus 1 sideways each way) and takes the first that fits; never down. Limits: `MaxSidewaysPush`, `MaxUpPush`.
- Lock delay (`Core/LockDelayTimer`): counts only while resting. A sideways move or rotation while resting gives back `LockExtensionBonus` (never past the full `LockDelay`), at most `MaxLockExtensions` times. Reaching a lower row than before restarts time and count.
- Store (`Core/PieceStore`): Space (or gamepad left shoulder) stores the falling piece; the stored piece (or the next bag piece the first time) spawns at the top. Locked from a store until the next piece locks. Reset on a new run.
- `BoardView` draws with three tilemaps (Background order 0, Ghost order 1, Blocks order 2), redrawing when `BlocksGame.Version` changes. The Ghost layer is a see-through copy of the falling piece at `BlocksGame.LandingPosition` (from `BoardGrid.LandingOffset`), tinted with the piece color; its sprite `Art/GhostCell.png` holds the bottom-up fade. `BlocksHud` (UI Toolkit) shows the STORE box left of the board (dimmed with `hud-store--locked` while locked, key hint from `GameInput.StorePieceBindingText`), LINES and the NEXT box right of the board, and the game over panel. Piece boxes are drawn by `PiecePreviewView`, centered by `Core/PiecePreviewLayout` in a 4 x 4 grid.

## Debug menu section "Blocks"
Readouts: State, Piece, Position, Rotation, Landing, Lock timer, Extensions used, Tags, Next, Stored, Can store, Lines, Pieces locked. Toggle: Pause gravity. Buttons: Clear board, Fill bottom row (I gap), Game over, Restart run, Store piece, Spawn I/O/T/S/Z/J/L.

## Tests
- `Tests/EditMode`: board, rotation, bag, piece data and tags, rotation push, lock delay timer, piece store, preview layout.
- `Tests/PlayMode/BlocksInputTests`: keyboard-driven controls through the Game scene.
