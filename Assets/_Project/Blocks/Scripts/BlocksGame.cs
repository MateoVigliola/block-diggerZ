using System;
using System.Collections.Generic;
using BlockDiggerz.Input;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Runs the block dropper: spawns pieces from the bag, applies gravity, soft drop, hard drop, sideways moves with
    /// auto repeat and clockwise or counter-clockwise rotation (a blocked rotation is pushed clear by
    /// <see cref="RotationPush"/>), locks pieces after a lock delay that moves and rotations can extend
    /// (<see cref="LockDelayTimer"/>), clears full rows and ends the run when a new piece can't spawn.
    /// Reads input only through <see cref="GameInput"/>.
    /// </summary>
    public sealed class BlocksGame : MonoBehaviour
    {
        [SerializeField, Tooltip("Board size and timing.")]
        private BlocksRules _rules;

        [SerializeField, Tooltip("Pieces the bag deals.")]
        private PieceSet _pieceSet;

        [SerializeField, Tooltip("Input wrapper the game reads.")]
        private GameInput _input;

        private PieceBag _bag;
        private int _heldDirection;
        private float _shiftTimer;
        private bool _autoShifting;
        private float _fallTimer;
        private readonly LockDelayTimer _lock = new();
        private readonly PieceStore _store = new();

        /// <summary>Raised after rows are cleared, with how many.</summary>
        public event Action<int> LinesCleared;

        /// <summary>Raised when the run ends.</summary>
        public event Action GameOver;

        /// <summary>Raised when a new run starts.</summary>
        public event Action RunStarted;

        /// <summary>Board size and timing in use.</summary>
        public BlocksRules Rules => _rules;

        /// <summary>Pieces the bag deals.</summary>
        public PieceSet PieceSet => _pieceSet;

        /// <summary>Locked cells.</summary>
        public BoardGrid Board { get; private set; }

        /// <summary>Whether the run is in progress or over.</summary>
        public BlocksGameState State { get; private set; } = BlocksGameState.Playing;

        /// <summary>The falling piece, or null between runs.</summary>
        public PieceDefinition ActivePiece { get; private set; }

        /// <summary>Rotation state of the falling piece (0 = spawn, 1 = one clockwise turn, and so on).</summary>
        public int ActiveRotation { get; private set; }

        /// <summary>Board position of the falling piece's box (its bottom-left corner).</summary>
        public Vector2Int ActivePosition { get; private set; }

        /// <summary>Rows cleared this run.</summary>
        public int TotalLinesCleared { get; private set; }

        /// <summary>Pieces locked this run.</summary>
        public int PiecesLocked { get; private set; }

        /// <summary>When true the piece does not fall on its own; soft and hard drop still work. Debug and tests.</summary>
        public bool GravityPaused { get; set; }

        /// <summary>Increments whenever the board or falling piece changes, so views redraw only then.</summary>
        public int Version { get; private set; }

        /// <summary>Seconds the falling piece has rested toward locking, after extensions.</summary>
        public float LockElapsed => _lock.Elapsed;

        /// <summary>Lock delay extensions the falling piece has used since it last reached a lower row.</summary>
        public int LockExtensionsUsed => _lock.ExtensionsUsed;

        /// <summary>True while the falling piece rests on the floor or the stack.</summary>
        public bool IsResting => ActivePiece != null && !Board.Fits(ActiveCells, ActivePosition + Vector2Int.down);

        /// <summary>Box position where the falling piece would land if hard dropped now.</summary>
        public Vector2Int LandingPosition => ActivePiece != null ? Board.LandingOffset(ActiveCells, ActivePosition) : ActivePosition;

        /// <summary>The piece the bag deals next.</summary>
        public PieceDefinition NextPiece => _bag?.Peek();

        /// <summary>The stored piece, or null if nothing is stored.</summary>
        public PieceDefinition StoredPiece => _store.Stored;

        /// <summary>False from a store until the next piece locks.</summary>
        public bool CanStore => _store.CanStore;

        /// <summary>Input wrapper the game reads; the HUD uses it for key hints.</summary>
        public GameInput GameInput => _input;

        /// <summary>Cells of the falling piece in its current rotation, relative to <see cref="ActivePosition"/>.</summary>
        public IReadOnlyList<Vector2Int> ActiveCells => ActivePiece != null ? ActivePiece.GetCells(ActiveRotation) : Array.Empty<Vector2Int>();

        private void Awake()
        {
            Board = new BoardGrid(_rules.BoardWidth, _rules.TotalHeight);
            _bag = new PieceBag(_pieceSet.Pieces, PickSeed());
        }

        private void Start()
        {
            StartRun();
        }

        private void Update()
        {
            if (State == BlocksGameState.GameOver)
            {
                if (_input != null && _input.RestartPressed)
                {
                    StartRun();
                }

                return;
            }

            if (ActivePiece == null || _input == null)
            {
                return;
            }

            if (_input.StorePiecePressed)
            {
                TryStorePiece();
            }

            float deltaTime = Time.deltaTime;
            UpdateSideways(deltaTime);

            if (_input.RotateClockwisePressed)
            {
                TryRotateClockwise();
            }

            if (_input.RotateCounterClockwisePressed)
            {
                TryRotateCounterClockwise();
            }

            if (_input.HardDropPressed)
            {
                HardDrop();
                return;
            }

            UpdateFalling(deltaTime, _input.SoftDropHeld);
        }

        /// <summary>Empties the board, resets counters and the bag, and spawns the first piece.</summary>
        public void StartRun()
        {
            Board.Clear();
            _bag.Reset(PickSeed());
            TotalLinesCleared = 0;
            PiecesLocked = 0;
            _store.Reset();
            State = BlocksGameState.Playing;
            ResetTimers();
            RunStarted?.Invoke();
            SpawnPiece(_bag.Next());
        }

        /// <summary>
        /// Replaces the falling piece with <paramref name="piece"/> at the spawn position. Ends the run if it
        /// doesn't fit.
        /// </summary>
        public void SpawnPiece(PieceDefinition piece)
        {
            if (piece == null || State == BlocksGameState.GameOver)
            {
                return;
            }

            ActivePiece = piece;
            ActiveRotation = 0;
            ActivePosition = SpawnPosition(piece);
            ResetTimers();
            Version++;

            if (!Board.Fits(ActiveCells, ActivePosition))
            {
                EndRun();
            }
        }

        /// <summary>
        /// Moves the falling piece by one step if the target is free. A sideways move while resting extends the
        /// lock delay.
        /// </summary>
        public bool TryMove(Vector2Int step)
        {
            if (ActivePiece == null || State != BlocksGameState.Playing)
            {
                return false;
            }

            Vector2Int target = ActivePosition + step;
            if (!Board.Fits(ActiveCells, target))
            {
                return false;
            }

            bool sidewaysWhileResting = step.y == 0 && IsResting;
            ActivePosition = target;
            AfterPieceChanged(sidewaysWhileResting);
            return true;
        }

        /// <summary>Rotates the falling piece a quarter turn clockwise, pushing it clear if the turn is blocked.</summary>
        public bool TryRotateClockwise()
        {
            return TryRotate(1);
        }

        /// <summary>Rotates the falling piece a quarter turn counter-clockwise, pushing it clear if the turn is blocked.</summary>
        public bool TryRotateCounterClockwise()
        {
            return TryRotate(-1);
        }

        private bool TryRotate(int quarterTurns)
        {
            if (ActivePiece == null || State != BlocksGameState.Playing)
            {
                return false;
            }

            int rotation = PieceDefinition.Wrap(ActiveRotation + quarterTurns);
            if (!RotationPush.TryFind(Board, ActivePiece.GetCells(rotation), ActivePosition,
                    _rules.MaxSidewaysPush, _rules.MaxUpPush, out Vector2Int position))
            {
                return false;
            }

            bool wasResting = IsResting;
            ActiveRotation = rotation;
            ActivePosition = position;
            AfterPieceChanged(wasResting);
            return true;
        }

        private void AfterPieceChanged(bool extendLock)
        {
            if (extendLock)
            {
                _lock.TryExtend(_rules.LockExtensionBonus, _rules.MaxLockExtensions);
            }

            _lock.ReachRow(BottomRow());
            Version++;
        }

        private int BottomRow()
        {
            IReadOnlyList<Vector2Int> cells = ActiveCells;
            int bottom = int.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                bottom = Mathf.Min(bottom, cells[i].y);
            }

            return ActivePosition.y + (cells.Count > 0 ? bottom : 0);
        }

        /// <summary>
        /// Stores the falling piece and brings in the stored one at the spawn spot (or the next piece from the bag
        /// if nothing was stored). Locked from a store until the next piece locks.
        /// </summary>
        public bool TryStorePiece()
        {
            if (ActivePiece == null || State != BlocksGameState.Playing)
            {
                return false;
            }

            if (!_store.TryStore(ActivePiece, out PieceDefinition released))
            {
                return false;
            }

            SpawnPiece(released != null ? released : _bag.Next());
            return true;
        }

        /// <summary>Drops the falling piece as far as it goes and locks it at once.</summary>
        public void HardDrop()
        {
            if (ActivePiece == null || State != BlocksGameState.Playing)
            {
                return;
            }

            ActivePosition = LandingPosition;
            Version++;
            LockPiece();
        }

        /// <summary>Empties the board without ending the run. Debug and tests.</summary>
        public void ClearBoard()
        {
            Board.Clear();
            Version++;
        }

        /// <summary>Fills one row except the given columns. Debug and tests.</summary>
        public void FillRow(int row, int gapStart, int gapLength, PieceDefinition filler)
        {
            for (int x = 0; x < Board.Width; x++)
            {
                bool inGap = x >= gapStart && x < gapStart + gapLength;
                Board.Set(x, row, inGap ? null : filler);
            }

            Version++;
        }

        /// <summary>Ends the run as if the stack had reached the top. Debug and tests.</summary>
        public void ForceGameOver()
        {
            if (State == BlocksGameState.Playing)
            {
                EndRun();
            }
        }

        /// <summary>Box position where <paramref name="piece"/> spawns: centered, its top row on the top visible row.</summary>
        public Vector2Int SpawnPosition(PieceDefinition piece)
        {
            int x = (_rules.BoardWidth - piece.BoxSize) / 2;
            int y = _rules.VisibleHeight - 1 - piece.GetTopRow(0);
            return new Vector2Int(x, y);
        }

        private void UpdateSideways(float deltaTime)
        {
            int direction = (_input.MoveRightHeld ? 1 : 0) - (_input.MoveLeftHeld ? 1 : 0);
            if (direction != _heldDirection)
            {
                _heldDirection = direction;
                _shiftTimer = 0f;
                _autoShifting = false;
                if (direction != 0)
                {
                    TryMove(new Vector2Int(direction, 0));
                }

                return;
            }

            if (direction == 0)
            {
                return;
            }

            _shiftTimer += deltaTime;
            float threshold = _autoShifting ? _rules.AutoRepeatInterval : _rules.AutoShiftDelay;
            while (_shiftTimer >= threshold)
            {
                _shiftTimer -= threshold;
                _autoShifting = true;
                threshold = _rules.AutoRepeatInterval;
                if (!TryMove(new Vector2Int(direction, 0)))
                {
                    _shiftTimer = 0f;
                    break;
                }
            }
        }

        private void UpdateFalling(float deltaTime, bool softDrop)
        {
            if (IsResting)
            {
                if (_lock.Tick(deltaTime, _rules.LockDelay))
                {
                    LockPiece();
                }

                return;
            }

            if (GravityPaused && !softDrop)
            {
                _fallTimer = 0f;
                return;
            }

            float interval = softDrop ? Mathf.Min(_rules.SoftDropInterval, _rules.GravityInterval) : _rules.GravityInterval;
            _fallTimer = Mathf.Min(_fallTimer, interval) + deltaTime;
            while (_fallTimer >= interval)
            {
                _fallTimer -= interval;
                if (!TryMove(Vector2Int.down))
                {
                    _fallTimer = 0f;
                    break;
                }
            }
        }

        private void LockPiece()
        {
            Board.Place(ActiveCells, ActivePosition, ActivePiece);
            PiecesLocked++;
            _store.OnPieceLocked();
            ActivePiece = null;
            Version++;

            int cleared = Board.ClearFullRows();
            if (cleared > 0)
            {
                TotalLinesCleared += cleared;
                LinesCleared?.Invoke(cleared);
            }

            SpawnPiece(_bag.Next());
        }

        private void EndRun()
        {
            State = BlocksGameState.GameOver;
            Version++;
            GameOver?.Invoke();
        }

        private void ResetTimers()
        {
            _fallTimer = 0f;
            _lock.Reset(BottomRow());
        }

        private int PickSeed()
        {
            return _rules.RandomSeed != 0 ? _rules.RandomSeed : Environment.TickCount;
        }
    }
}
