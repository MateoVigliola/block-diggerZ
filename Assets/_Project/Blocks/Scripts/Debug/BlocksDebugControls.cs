using System.Text;
using BlockDiggerz.DebugTools;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// The "Blocks" debug menu section: readouts of the run and the falling piece, a gravity pause, and buttons to
    /// spawn a given piece, clear or fill the board and end or restart the run. Sits on the BlocksGame prefab.
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

        /// <summary>Prefix of the per-piece spawn buttons, followed by the piece id.</summary>
        public const string SpawnLabelPrefix = "Spawn ";

        private const string IPieceId = "I";
        private const int BottomRow = 0;
        private const int IPieceLength = 4;

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
                    .Readout("Next", () => _game.NextPiece != null ? _game.NextPiece.Id : "-")
                    .Readout("Lines", () => _game.TotalLinesCleared.ToString())
                    .Readout("Pieces locked", () => _game.PiecesLocked.ToString())
                    .Toggle(PauseGravityLabel, () => _game.GravityPaused, v => _game.GravityPaused = v)
                    .Button(ClearBoardLabel, _game.ClearBoard)
                    .Button(FillBottomRowLabel, FillBottomRowWithIGap)
                    .Button(GameOverLabel, _game.ForceGameOver)
                    .Button(RestartLabel, _game.StartRun);

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
