using System.Text;
using BlockDiggerz.DebugTools;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// The "Blocks" debug menu section: readouts of the run and the falling piece, a gravity pause, and buttons to
    /// spawn a given piece, clear or fill the board, set up a color match or chain, and end or restart the run.
    /// Sits on the BlocksGame prefab.
    /// Labels are the CLI's keys (`unity command debugmenu do --section Blocks --label "Spawn I"`).
    /// </summary>
    public sealed class BlocksDebugControls : MonoBehaviour
    {
        /// <summary>Section title in the debug menu and the CLI.</summary>
        public const string SectionTitle = "Blocks";

        /// <summary>Label of the gravity pause toggle.</summary>
        public const string PauseGravityLabel = "Pause gravity";

        /// <summary>Label of the clear board button.</summary>
        public const string ClearBoardLabel = "Clear board";

        /// <summary>Label of the button that fills the bottom row except where a flat I piece lands.</summary>
        public const string FillBottomRowLabel = "Fill bottom row (I gap)";

        /// <summary>Label of the game over button.</summary>
        public const string GameOverLabel = "Game over";

        /// <summary>Label of the restart button.</summary>
        public const string RestartLabel = "Restart run";

        /// <summary>Label of the store piece button (same rules as the store key).</summary>
        public const string StorePieceLabel = "Store piece";

        /// <summary>Prefix of the per-piece spawn buttons, followed by the piece id.</summary>
        public const string SpawnLabelPrefix = "Spawn ";

        /// <summary>
        /// Label of the button that clears the board, puts three same-colored blocks at the bottom left and clears
        /// them (30 points).
        /// </summary>
        public const string ColorMatchLabel = "Color match (3)";

        /// <summary>
        /// Label of the button that clears the board and sets up a color match whose falling blocks make a second
        /// match (a chain, 60 points in total).
        /// </summary>
        public const string ColorChainLabel = "Color chain (2 steps)";

        /// <summary>Label of the button that makes the board one column narrower (restarts the run).</summary>
        public const string WidthDownLabel = "Board width -1";

        /// <summary>Label of the button that makes the board one column wider (restarts the run).</summary>
        public const string WidthUpLabel = "Board width +1";

        /// <summary>Label of the button that makes the board one row shorter (restarts the run).</summary>
        public const string HeightDownLabel = "Board height -1";

        /// <summary>Label of the button that makes the board one row taller (restarts the run).</summary>
        public const string HeightUpLabel = "Board height +1";

        /// <summary>Label of the button that puts the board back to the size in Blocks Rules (restarts the run).</summary>
        public const string ResetSizeLabel = "Board size reset";

        private const string IPieceId = "I";
        private const int BottomRow = 0;
        private const int IPieceLength = 4;
        private const int MatchLength = 3;
        private const string NoValue = "-";

        [SerializeField, Tooltip("Game the section controls.")]
        private BlocksGame _game;

#if UNITY_EDITOR || DEBUG
        private void OnEnable()
        {
            DebugMenu.Register(this, SectionTitle, section =>
            {
                section
                    .Readout("State", () => _game.State.ToString())
                    .Readout("Piece", () => _game.ActivePiece != null ? _game.ActivePiece.Id : "-")
                    .Readout("Position", () => $"{_game.ActivePosition.x},{_game.ActivePosition.y}")
                    .Readout("Rotation", () => _game.ActiveRotation.ToString())
                    .Readout("Landing", () => $"{_game.LandingPosition.x},{_game.LandingPosition.y}")
                    .Readout("Lock timer", () => $"{_game.LockElapsed:0.00}/{_game.Rules.LockDelay:0.00}s{(_game.IsResting ? " resting" : string.Empty)}")
                    .Readout("Extensions used", () => $"{_game.LockExtensionsUsed}/{_game.Rules.MaxLockExtensions}")
                    .Readout("Tags", DescribeTags)
                    .Readout("Block colors", () => DescribeColors(_game.ActivePainted))
                    .Readout("Next", () => DescribePiece(_game.NextPiece))
                    .Readout("Stored", () => DescribePiece(_game.StoredPiece))
                    .Readout("Can store", () => _game.CanStore ? "yes" : "no")
                    .Readout("Score", () => _game.Score.ToString())
                    .Readout("Color blocks cleared", () => _game.ColorBlocksCleared.ToString())
                    .Readout("Settling", DescribeSettling)
                    .Readout("Colors in play", () => _game.ActiveColors.Count.ToString())
                    .Readout("Pieces locked", () => _game.PiecesLocked.ToString())
                    .Toggle(PauseGravityLabel, () => _game.GravityPaused, v => _game.GravityPaused = v)
                    .Readout("Board size", () => $"{_game.BoardWidth} x {_game.VisibleHeight}")
                    .Button(WidthDownLabel, () => _game.SetBoardSize(_game.BoardWidth - 1, _game.VisibleHeight))
                    .Button(WidthUpLabel, () => _game.SetBoardSize(_game.BoardWidth + 1, _game.VisibleHeight))
                    .Button(HeightDownLabel, () => _game.SetBoardSize(_game.BoardWidth, _game.VisibleHeight - 1))
                    .Button(HeightUpLabel, () => _game.SetBoardSize(_game.BoardWidth, _game.VisibleHeight + 1))
                    .Button(ResetSizeLabel, () => _game.SetBoardSize(_game.Rules.BoardWidth, _game.Rules.VisibleHeight))
                    .Button(ClearBoardLabel, _game.ClearBoard)
                    .Button(FillBottomRowLabel, FillBottomRowWithIGap)
                    .Button(ColorMatchLabel, SetUpColorMatch)
                    .Button(ColorChainLabel, SetUpColorChain)
                    .Button(GameOverLabel, _game.ForceGameOver)
                    .Button(RestartLabel, _game.StartRun)
                    .Button(StorePieceLabel, () => _game.TryStorePiece());

                foreach (PieceDefinition piece in _game.PieceSet.Pieces)
                {
                    if (piece != null)
                    {
                        section.Button(SpawnLabelPrefix + piece.Id, () => _game.SpawnPiece(piece));
                    }
                }
            });
        }

        private void OnDisable()
        {
            DebugMenu.Unregister(this);
        }

        private void FillBottomRowWithIGap()
        {
            PieceDefinition iPiece = _game.PieceSet.FindById(IPieceId);
            PieceDefinition filler = _game.PieceSet.Pieces.Count > 0 ? _game.PieceSet.Pieces[0] : null;
            int gapStart = iPiece != null ? _game.SpawnPosition(iPiece).x : 0;
            _game.FillRow(BottomRow, gapStart, IPieceLength, filler);
        }

        private void SetUpColorMatch()
        {
            if (!TryGetFillerAndColors(out PieceDefinition filler, out BlockColor first, out _))
            {
                return;
            }

            _game.ClearBoard();
            for (int x = 0; x < MatchLength; x++)
            {
                _game.PlaceBlock(x, BottomRow, filler, first);
            }

            _game.ClearBoardNow();
        }

        private void SetUpColorChain()
        {
            if (!TryGetFillerAndColors(out PieceDefinition filler, out BlockColor first, out BlockColor second))
            {
                return;
            }

            _game.ClearBoard();
            for (int x = 0; x < MatchLength; x++)
            {
                _game.PlaceBlock(x, BottomRow, filler, first);
            }

            _game.PlaceBlock(MatchLength, BottomRow, filler, second);
            for (int x = 1; x < MatchLength; x++)
            {
                _game.PlaceBlock(x, BottomRow + 1, filler, second);
            }

            _game.ClearBoardNow();
        }

        private bool TryGetFillerAndColors(out PieceDefinition filler, out BlockColor first, out BlockColor second)
        {
            filler = _game.PieceSet.Pieces.Count > 0 ? _game.PieceSet.Pieces[0] : null;
            first = _game.ActiveColors.Count > 0 ? _game.ActiveColors[0] : null;
            second = _game.ActiveColors.Count > 1 ? _game.ActiveColors[1] : null;
            return filler != null && first != null && second != null;
        }

        private string DescribeSettling()
        {
            if (_game.IsClearing)
            {
                return $"clearing {_game.ClearingCells.Count} blocks";
            }

            if (_game.IsBlocksFalling)
            {
                return "blocks falling";
            }

            return _game.IsSettling ? "checking" : NoValue;
        }

        private static string DescribePiece(PaintedPiece piece)
        {
            return piece != null ? $"{piece.Definition.Id} ({DescribeColors(piece)})" : NoValue;
        }

        private static string DescribeColors(PaintedPiece piece)
        {
            if (piece == null)
            {
                return NoValue;
            }

            var text = new StringBuilder();
            foreach (BlockColor color in piece.Colors)
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }

                text.Append(color != null ? color.Id : NoValue);
            }

            return text.ToString();
        }

        private string DescribeTags()
        {
            if (_game.ActivePiece == null || _game.ActivePiece.Tags.Count == 0)
            {
                return "-";
            }

            var text = new StringBuilder();
            foreach (PieceTag tag in _game.ActivePiece.Tags)
            {
                if (tag == null)
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text.Append(", ");
                }

                text.Append(tag.DisplayName);
            }

            return text.ToString();
        }
#endif
    }
}
