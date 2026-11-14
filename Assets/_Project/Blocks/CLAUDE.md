# Blocks (core block dropper)

Namespace `BlockDiggerz.Blocks`, asmdef `BlockDiggerz.Blocks` (references `BlockDiggerz.Input`, `BlockDiggerz.DebugTools`).

## Pieces
- `PieceDefinition` (ScriptableObject, `Data/Pieces/Piece*.asset`): id, color, box size, spawn cells inside a square box (y up). Clockwise rotation turns the box: `(x, y) -> (y, box - 1 - x)`. Standard shapes use box 3, I uses 4, O uses 2.
- `PieceSet` (`Data/StandardPieceSet.asset`): the 7 pieces. `PieceBag` deals each piece once per bag.
- Tags: `PieceTag` assets (`Data/Tags/`). Every piece lists its tags; only `Basic` for now. Board cells keep their `PieceDefinition`, so tags survive locking. Compare tags by asset reference.

## Rules and loop
- `BlocksRules` (`Data/BlocksRules.asset`): 10 x 20 board plus 2 hidden rows, gravity, soft drop, lock delay, auto shift delay and repeat, seed.
- `BlocksGame`: spawn centered with the top row on the top visible row; simple rotation, clockwise (E/Up) or counter-clockwise (Q), refused if blocked, no kicks; hard drop locks at once; full rows clear; spawn overlap means game over; R restarts. Reads input only through `GameInput`.
- `BoardView` draws with three tilemaps (Background order 0, Ghost order 1, Blocks order 2), redrawing when `BlocksGame.Version` changes. The Ghost layer is a see-through copy of the falling piece at `BlocksGame.LandingPosition` (from `BoardGrid.LandingOffset`), tinted with the piece color; its sprite `Art/GhostCell.png` holds the bottom-up fade. `BlocksHud` (UI Toolkit) shows lines and the game over panel.

## Debug menu section "Blocks"
Readouts: State, Piece, Position, Rotation, Landing, Tags, Next, Lines, Pieces locked. Toggle: Pause gravity. Buttons: Clear board, Fill bottom row (I gap), Game over, Restart run, Spawn I/O/T/S/Z/J/L.

## Tests
- `Tests/EditMode`: board, rotation, bag, piece data and tags.
- `Tests/PlayMode/BlocksInputTests`: keyboard-driven controls through the Game scene.
