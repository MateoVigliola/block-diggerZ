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
    /// (<see cref="LockDelayTimer"/>) and ends the run when a new piece can't spawn. Every piece's blocks are
    /// painted with <see cref="PiecePainter"/>. After a piece locks the board settles before the next piece spawns:
    /// same-colored groups with space below fall one row per <see cref="BlocksRules.PlacedFallInterval"/>
    /// (<see cref="BlockGravity"/>; a falling group never sticks to resting blocks of its color). After every change,
    /// color matches among resting blocks (<see cref="ClearResolver"/>) flash for
    /// <see cref="BlocksRules.ClearStepDuration"/> and are removed before anything falls further; falling blocks
    /// never match. Full rows
    /// don't clear. Reads input only through <see cref="GameInput"/>.
    /// </summary>
    public sealed class BlocksGame : MonoBehaviour
    {
        [SerializeField, Tooltip("Board size and timing.")]
        private BlocksRules _rules;

        [SerializeField, Tooltip("Pieces the bag deals.")]
        private PieceSet _pieceSet;

        [SerializeField, Tooltip("Block colors; a run uses the first Color Count of them (Blocks Rules).")]
        private BlockPalette _palette;

        [SerializeField, Tooltip("Piece the starting rubble blocks count as (for tags). Empty uses the first piece of the piece set.")]
        private PieceDefinition _startBlockPiece;

        [SerializeField, Tooltip("Dirt looks mixed into the starting columns (each an obstacle Block Color); each Dirt block picks one evenly. Empty means no Dirt. Counts and placement are in Blocks Rules.")]
        private List<BlockColor> _startDirt = new();

        private readonly List<List<BlockColor>> _startColumns = new();
        private int _startColumnsPlaced;
        private float _startFillTimer;
        private bool _startingFill;

        [SerializeField, Tooltip("Input wrapper the game reads.")]
        private GameInput _input;

        private readonly List<BlockColor> _activeColors = new();
        private readonly List<BlockColor> _paintBuffer = new();
        private readonly ClearStep _clearStep = new();
        private ClearResolver _resolver;
        private System.Random _paintRandom;
        private PaintedPiece _active;
        private PaintedPiece _next;
        private PaintedPiece _resumePiece;
        private bool _settling;
        private bool _flashing;
        private bool _fallingChecked;
        private bool _hasFalling;
        private bool[] _fallState;
        private float _settleTimer;
        private PieceBag _bag;
        private int _heldDirection;
        private float _shiftTimer;
        private bool _autoShifting;
        private float _fallTimer;
        private readonly LockDelayTimer _lock = new();
        private readonly PieceStore _store = new();

        /// <summary>Raised after a color match is deleted, with the scored block count.</summary>
        public event Action<int> ColorsCleared;

        /// <summary>Raised when the run ends.</summary>
        public event Action GameOver;

        /// <summary>Raised when the level is won (every starting Dirt block cleared).</summary>
        public event Action Won;

        /// <summary>
        /// Seconds of play this run: counts from the first piece spawn (after the drop-in) until the run is won or
        /// lost, scaled time (so it stops while paused).
        /// </summary>
        public float RunTime { get; private set; }

        /// <summary>Dirt blocks the level started with (counted when the starting columns are rolled).</summary>
        public int StartDirtCount { get; private set; }

        /// <summary>Current speed multiplier of the falling piece, from the score (<see cref="GravitySpeed"/>).</summary>
        public float GravitySpeedMultiplier =>
            GravitySpeed.Multiplier(Score, _rules.SpeedUpScoreStep, _rules.SpeedUpPerStep, _rules.MaxSpeedMultiplier);

        /// <summary>Seconds per row the falling piece drops on its own: gravity interval divided by the speed multiplier.</summary>
        public float PieceFallInterval => _rules.GravityInterval / GravitySpeedMultiplier;

        private bool _clockRunning;

        /// <summary>Raised when a new run starts.</summary>
        public event Action RunStarted;

        /// <summary>Raised after <see cref="SetBoardSize"/> changes the board size, before the new run starts.</summary>
        public event Action BoardResized;

        /// <summary>Columns of the current board (from <see cref="BlocksRules.BoardWidth"/> until changed).</summary>
        public int BoardWidth { get; private set; }

        /// <summary>Visible rows of the current board (from <see cref="BlocksRules.VisibleHeight"/> until changed).</summary>
        public int VisibleHeight { get; private set; }

        /// <summary>Board size and timing in use.</summary>
        public BlocksRules Rules => _rules;

        /// <summary>Pieces the bag deals.</summary>
        public PieceSet PieceSet => _pieceSet;

        /// <summary>Locked cells.</summary>
        public BoardGrid Board { get; private set; }

        /// <summary>Whether the run is in progress or over.</summary>
        public BlocksGameState State { get; private set; } = BlocksGameState.Playing;

        /// <summary>Block colors in play this run (the first <see cref="BlocksRules.ColorCount"/> of the palette).</summary>
        public IReadOnlyList<BlockColor> ActiveColors => _activeColors;

        /// <summary>Shape of the falling piece, or null while clearing or between runs.</summary>
        public PieceDefinition ActivePiece => _active?.Definition;

        /// <summary>The falling piece with its block colors, or null.</summary>
        public PaintedPiece ActivePainted => _active;

        /// <summary>Colors of the falling piece's blocks, in the order of <see cref="ActiveCells"/>.</summary>
        public IReadOnlyList<BlockColor> ActiveBlockColors => _active != null ? _active.Colors : Array.Empty<BlockColor>();

        /// <summary>Points this run.</summary>
        public int Score { get; private set; }

        /// <summary>Blocks deleted by color matches this run (not counting blasted blocks).</summary>
        public int ColorBlocksCleared { get; private set; }

        /// <summary>Blocks removed by match blasts this run (<see cref="MatchBlast"/>), Dirt included.</summary>
        public int BlocksBlasted { get; private set; }

        /// <summary>
        /// True from a lock until the board has settled: placed blocks are falling or matches are flashing, and the
        /// player has no piece.
        /// </summary>
        public bool IsSettling => _settling;

        /// <summary>True while the starting columns drop in at the start of a run (no player piece yet).</summary>
        public bool IsStartingFill => _settling && _startingFill;

        /// <summary>Starting blocks this run that had no color safe from making a line (placed anyway).</summary>
        public int StartBlocksWithoutSafeColor { get; private set; }

        private bool IsStartColumnsPending => _startingFill && _startColumnsPlaced < _startColumns.Count;

        /// <summary>True while placed blocks with space below are falling.</summary>
        public bool IsBlocksFalling => _settling && !_flashing && _fallingChecked && _hasFalling;

        /// <summary>True while matched blocks flash before they are removed.</summary>
        public bool IsClearing => _settling && _flashing;

        /// <summary>Cells about to be removed by the current clear step (empty when not clearing).</summary>
        public IReadOnlyList<Vector2Int> ClearingCells => IsClearing ? _clearStep.Cells : Array.Empty<Vector2Int>();

        /// <summary>Rotation state of the falling piece (0 = spawn, 1 = one clockwise turn, and so on).</summary>
        public int ActiveRotation { get; private set; }

        /// <summary>Board position of the falling piece's box (its bottom-left corner).</summary>
        public Vector2Int ActivePosition { get; private set; }

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

        /// <summary>The piece that spawns next, already painted.</summary>
        public PaintedPiece NextPiece => _next;

        /// <summary>The stored piece, or null if nothing is stored.</summary>
        public PaintedPiece StoredPiece => _store.Stored;

        /// <summary>False from a store until the next piece locks.</summary>
        public bool CanStore => _store.CanStore;

        /// <summary>Input wrapper the game reads; the HUD uses it for key hints.</summary>
        public GameInput GameInput => _input;

        /// <summary>Cells of the falling piece in its current rotation, relative to <see cref="ActivePosition"/>.</summary>
        public IReadOnlyList<Vector2Int> ActiveCells => ActivePiece != null ? ActivePiece.GetCells(ActiveRotation) : Array.Empty<Vector2Int>();

        private void Awake()
        {
            Vector2Int size = BoardLayout.ClampSize(_rules.BoardWidth, _rules.VisibleHeight);
            CreateBoard(size.x, size.y);
            _bag = new PieceBag(_pieceSet.Pieces, PickSeed());
            _resolver = new ClearResolver(_rules.ColorBlockScore, _rules.MinColorMatch, _rules.Blast);
        }

        /// <summary>
        /// Changes the board to <paramref name="width"/> columns and <paramref name="visibleHeight"/> visible rows
        /// (clamped to <see cref="BoardLayout"/> limits, plus the rules' buffer rows) and starts a new run. Levels
        /// call this at level start; the debug menu calls it to tune sizes.
        /// </summary>
        public void SetBoardSize(int width, int visibleHeight)
        {
            Vector2Int size = BoardLayout.ClampSize(width, visibleHeight);
            if (Board != null && size.x == BoardWidth && size.y == VisibleHeight)
            {
                return;
            }

            CreateBoard(size.x, size.y);
            BoardResized?.Invoke();
            StartRun();
        }

        private void CreateBoard(int width, int visibleHeight)
        {
            BoardWidth = width;
            VisibleHeight = visibleHeight;
            Board = new BoardGrid(width, visibleHeight + _rules.BufferRows);
            _fallState = new bool[Board.Width * Board.Height];
            _active = null;
            _settling = false;
            _flashing = false;
            _fallingChecked = false;
            Version++;
        }

        private void Start()
        {
            StartRun();
        }

        private void Update()
        {
            if (State != BlocksGameState.Playing)
            {
                if (_input != null && _input.RestartPressed)
                {
                    StartRun();
                }

                return;
            }

            if (_clockRunning)
            {
                RunTime += Time.deltaTime;
            }

            if (_settling)
            {
                UpdateSettling(Time.deltaTime);
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
            Array.Clear(_fallState, 0, _fallState.Length);
            int seed = PickSeed();
            _bag.Reset(seed);
            _paintRandom = new System.Random(seed);
            _palette?.GetActive(_rules.ColorCount, _activeColors);
            PiecesLocked = 0;
            Score = 0;
            ColorBlocksCleared = 0;
            BlocksBlasted = 0;
            RunTime = 0f;
            _clockRunning = false;
            _settling = false;
            _flashing = false;
            _fallingChecked = false;
            _resumePiece = null;
            _clearStep.Reset();
            _store.Reset();
            State = BlocksGameState.Playing;
            ResetTimers();
            RunStarted?.Invoke();
            BeginStartFill();
            _next = Paint(_bag.Next());
            if (_startingFill)
            {
                BeginSettling(null);
            }
            else
            {
                SpawnPiece(TakeNext());
            }
        }

        private void BeginStartFill()
        {
            _startColumnsPlaced = 0;
            _startFillTimer = 0f;
            int forced = StartColumnsGenerator.Generate(BoardWidth, _rules.StartColumnMinHeight,
                _rules.StartColumnMaxHeight, VisibleHeight, _rules.MinColorMatch, _activeColors, _startDirt,
                _rules.StartDirt, _paintRandom, _startColumns);
            StartBlocksWithoutSafeColor = forced;
            _startingFill = false;
            StartDirtCount = 0;
            foreach (List<BlockColor> column in _startColumns)
            {
                _startingFill |= column.Count > 0;
                foreach (BlockColor color in column)
                {
                    StartDirtCount += color != null && color.IsObstacle ? 1 : 0;
                }
            }
        }

        private void UpdateStartFill(float deltaTime)
        {
            if (!_startingFill || _startColumnsPlaced >= _startColumns.Count)
            {
                return;
            }

            _startFillTimer += deltaTime;
            float interval = _startColumns.Count > 0 ? _rules.StartFillDuration / _startColumns.Count : 0f;
            while (_startColumnsPlaced < _startColumns.Count && _startFillTimer >= _startColumnsPlaced * interval)
            {
                DropStartColumn(_startColumnsPlaced);
                _startColumnsPlaced++;
            }
        }

        private void DropStartColumn(int x)
        {
            List<BlockColor> column = _startColumns[x];
            PieceDefinition piece = _startBlockPiece != null ? _startBlockPiece : _pieceSet.Pieces[0];
            int bottom = VisibleHeight - column.Count;
            for (int i = 0; i < column.Count; i++)
            {
                Board.Set(x, bottom + i, piece, column[i]);
                _fallState[(bottom + i) * Board.Width + x] = true;
            }

            BoardChanged();
        }

        /// <summary>Paints <paramref name="piece"/> with this run's colors.</summary>
        public PaintedPiece Paint(PieceDefinition piece)
        {
            if (piece == null)
            {
                return null;
            }

            _paintRandom ??= new System.Random(PickSeed());
            PiecePainter.Paint(piece.GetCells(0), _activeColors, _paintRandom, _paintBuffer);
            return new PaintedPiece(piece, _paintBuffer);
        }

        /// <summary>
        /// Paints <paramref name="piece"/> and makes it the falling piece at the spawn position. Debug and tests.
        /// </summary>
        public void SpawnPiece(PieceDefinition piece)
        {
            SpawnPiece(Paint(piece));
        }

        /// <summary>
        /// Replaces the falling piece with <paramref name="piece"/> at the spawn position. Ends the run if it
        /// doesn't fit. Ignored while clearing.
        /// </summary>
        public void SpawnPiece(PaintedPiece piece)
        {
            if (piece == null || State != BlocksGameState.Playing || _settling)
            {
                return;
            }

            _clockRunning = true;
            PieceDefinition definition = piece.Definition;
            _active = piece;
            ActiveRotation = 0;
            ActivePosition = SpawnPosition(definition);
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

            if (!_store.TryStore(_active, out PaintedPiece released))
            {
                return false;
            }

            SpawnPiece(released ?? TakeNext());
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
            Array.Clear(_fallState, 0, _fallState.Length);
            BoardChanged();
        }

        /// <summary>
        /// Fills one row except the given columns. Colors step through the active colors, shifted by the row, so
        /// filled rows never form a color match by themselves. Debug and tests.
        /// </summary>
        public void FillRow(int row, int gapStart, int gapLength, PieceDefinition filler)
        {
            for (int x = 0; x < Board.Width; x++)
            {
                bool inGap = x >= gapStart && x < gapStart + gapLength;
                BlockColor color = _activeColors.Count > 0 ? _activeColors[(x + row) % _activeColors.Count] : null;
                Board.Set(x, row, inGap ? null : filler, color);
            }

            BoardChanged();
        }

        /// <summary>Puts one block on the board. Debug and tests; call <see cref="ClearBoardNow"/> to resolve.</summary>
        public void PlaceBlock(int x, int y, PieceDefinition piece, BlockColor color)
        {
            Board.Set(x, y, piece, color);
            BoardChanged();
        }

        /// <summary>
        /// Settles the board as if a piece had just landed (blocks fall, matches flash and clear, chains). The
        /// falling piece is set aside and comes back afterwards. Debug and tests.
        /// </summary>
        public void ClearBoardNow()
        {
            if (State != BlocksGameState.Playing || _settling)
            {
                return;
            }

            PaintedPiece resume = _active;
            _active = null;
            BeginSettling(resume);
        }

        /// <summary>Wins the level at once, as if all Dirt were cleared. Debug and tests.</summary>
        public void ForceWin()
        {
            if (State == BlocksGameState.Playing)
            {
                WinLevel();
            }
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
            int x = (BoardWidth - piece.BoxSize) / 2;
            int y = VisibleHeight - 1 - piece.GetTopRow(0);
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

            float gravity = PieceFallInterval;
            float interval = softDrop ? Mathf.Min(_rules.SoftDropInterval, gravity) : gravity;
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
            Board.Place(ActiveCells, ActivePosition, ActivePiece, ActiveBlockColors);
            PiecesLocked++;
            _store.OnPieceLocked();
            _active = null;
            Version++;
            BeginSettling(null);
        }

        private void BeginSettling(PaintedPiece resume)
        {
            _resumePiece = resume;
            _settling = true;
            _flashing = false;
            _settleTimer = 0f;
            _fallingChecked = false;
            _clearStep.Reset();
            Version++;
            UpdateSettling(0f);
        }

        private void UpdateSettling(float deltaTime)
        {
            _settleTimer += deltaTime;
            UpdateStartFill(deltaTime);
            float fallInterval = _startingFill ? _rules.StartFillFallInterval : _rules.PlacedFallInterval;
            while (_settling)
            {
                if (_flashing)
                {
                    if (_settleTimer < _rules.ClearStepDuration)
                    {
                        return;
                    }

                    _settleTimer -= _rules.ClearStepDuration;
                    ApplyClearStep();
                    continue;
                }

                if (!_fallingChecked)
                {
                    _hasFalling = BlockGravity.UpdateFalling(Board, _fallState);
                    _fallingChecked = true;
                    if (_resolver.TryFindStep(Board, _clearStep, _fallState))
                    {
                        _flashing = true;
                        _settleTimer = 0f;
                        Version++;
                        continue;
                    }
                }

                if (!_hasFalling)
                {
                    if (!IsStartColumnsPending)
                    {
                        FinishSettling();
                    }

                    return;
                }

                if (_settleTimer < fallInterval)
                {
                    return;
                }

                _settleTimer -= fallInterval;
                BlockGravity.Step(Board, _fallState);
                BoardChanged();
            }
        }

        private void ApplyClearStep()
        {
            _resolver.Apply(Board, _clearStep);
            Score += _clearStep.Score;
            ColorBlocksCleared += _clearStep.MatchedBlocks;
            BlocksBlasted += _clearStep.BlastedBlocks;
            int scored = _clearStep.ScoredBlocks;
            _clearStep.Reset();
            _flashing = false;
            _settleTimer = 0f;
            BoardChanged();
            ColorsCleared?.Invoke(scored);
        }

        private void FinishSettling()
        {
            _settling = false;
            _startingFill = false;
            _flashing = false;
            _clearStep.Reset();
            if (LevelGoal.IsCleared(StartDirtCount, Board.CountObstacles()))
            {
                _resumePiece = null;
                WinLevel();
                return;
            }

            PaintedPiece piece = _resumePiece ?? TakeNext();
            _resumePiece = null;
            SpawnPiece(piece);
        }

        private void BoardChanged()
        {
            _fallingChecked = false;
            Version++;
        }

        private PaintedPiece TakeNext()
        {
            PaintedPiece piece = _next;
            _next = Paint(_bag.Next());
            return piece;
        }

        private void EndRun()
        {
            State = BlocksGameState.GameOver;
            _clockRunning = false;
            Version++;
            GameOver?.Invoke();
        }

        private void WinLevel()
        {
            State = BlocksGameState.Won;
            _clockRunning = false;
            _active = null;
            _settling = false;
            _flashing = false;
            Version++;
            Won?.Invoke();
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
