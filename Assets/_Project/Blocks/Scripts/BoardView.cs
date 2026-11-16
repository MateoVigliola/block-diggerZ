using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Draws the board with three tilemaps: a background grid of the visible area, a see-through ghost of the
    /// falling piece where it will land (bottom-up fade in its block colors), and the locked cells plus the falling
    /// piece, each block drawn with its <see cref="BlockColor"/> sprite (or the shared cell tile tinted with its
    /// color). Blocks about to be cleared are blended toward a
    /// flash color. One board cell is one tilemap cell. Redraws only when
    /// <see cref="BlocksGame.Version"/> changes. The board grid is moved so the visible board is centered on the
    /// camera's start position, again whenever <see cref="BlocksGame.SetBoardSize"/> changes the size.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField, Tooltip("Game whose board is drawn.")]
        private BlocksGame _game;

        [SerializeField, Tooltip("Tilemap for locked cells and the falling piece.")]
        private Tilemap _blocks;

        [SerializeField, Tooltip("Tilemap for the empty board background.")]
        private Tilemap _background;

        [SerializeField, Tooltip("Tile used for every cell; tinted per piece. Its flags must allow color changes.")]
        private TileBase _cellTile;

        [SerializeField, Tooltip("Tint of empty board cells.")]
        private Color _backgroundColor = new(0.12f, 0.12f, 0.16f, 1f);

        [Header("Landing ghost")]
        [SerializeField, Tooltip("Tilemap for the see-through ghost showing where the falling piece will land. Sort it between the background and the blocks.")]
        private Tilemap _ghost;

        [SerializeField, Tooltip("Tile for ghost cells; its sprite holds the bottom-up fade. Tinted with the piece color.")]
        private TileBase _ghostTile;

        [SerializeField, Tooltip("Show the landing ghost.")]
        private bool _showGhost = true;

        [SerializeField, Range(0f, 1f), Tooltip("Overall opacity of the landing ghost, multiplied with the fade in its sprite.")]
        private float _ghostOpacity = 0.6f;

        [Header("Blocks")]
        [SerializeField, Tooltip("Tint of a block with no color (only debug setups make these).")]
        private Color _unpaintedColor = new(0.6f, 0.6f, 0.65f, 1f);

        [SerializeField, Tooltip("Color blocks blend toward while they flash before being cleared.")]
        private Color _clearFlashColor = Color.white;

        [SerializeField, Range(0f, 1f), Tooltip("How far flashing blocks blend toward the flash color.")]
        private float _clearFlashAmount = 0.6f;

        [Header("Placement")]
        [SerializeField, Tooltip("Camera the board centers on. Empty uses the main camera. The camera itself never moves.")]
        private Camera _camera;

        private readonly HashSet<Vector2Int> _flashing = new();
        private readonly Dictionary<BlockColor, Tile> _colorTiles = new();
        private int _drawnVersion = -1;
        private Vector2 _anchor;

        /// <summary>
        /// Camera the board is drawn for; the HUD uses it to place panels beside the board. The board is centered on
        /// where this camera was at start, so moving or zooming the camera later (the touch layout fits it to the
        /// screen) never moves the board.
        /// </summary>
        public Camera ViewCamera => _camera;

        /// <summary>World rectangle of the visible board.</summary>
        public Rect BoardWorldRect { get; private set; }

        /// <summary>Increments whenever the board moves or changes size, so the HUD re-places its panels.</summary>
        public int LayoutVersion { get; private set; }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _anchor = _camera != null ? (Vector2)_camera.transform.position : Vector2.zero;
        }

        private void OnEnable()
        {
            if (_game != null)
            {
                _game.BoardResized += ApplyLayout;
            }
        }

        private void OnDisable()
        {
            if (_game != null)
            {
                _game.BoardResized -= ApplyLayout;
            }
        }

        private void Start()
        {
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            if (_game == null || _blocks == null)
            {
                return;
            }

            GridLayout grid = _blocks.layoutGrid;
            float cellSize = grid != null ? grid.cellSize.x : 1f;
            Vector2 origin = BoardLayout.CenteredOrigin(_anchor,_game.BoardWidth, _game.VisibleHeight, cellSize);
            if (grid != null)
            {
                Transform root = grid.transform;
                root.position = new Vector3(origin.x, origin.y, root.position.z);
            }

            BoardWorldRect = new Rect(origin, new Vector2(_game.BoardWidth, _game.VisibleHeight) * cellSize);
            LayoutVersion++;
            _blocks.ClearAllTiles();
            if (_ghost != null)
            {
                _ghost.ClearAllTiles();
            }

            DrawBackground();
            _drawnVersion = -1;
        }

        private void LateUpdate()
        {
            if (_game == null || _game.Board == null || _game.Version == _drawnVersion)
            {
                return;
            }

            _drawnVersion = _game.Version;
            DrawBlocks();
        }

        private void DrawBackground()
        {
            if (_background == null || _game == null)
            {
                return;
            }

            _background.ClearAllTiles();
            for (int y = 0; y < _game.VisibleHeight; y++)
            {
                for (int x = 0; x < _game.BoardWidth; x++)
                {
                    SetCell(_background, x, y, _backgroundColor);
                }
            }
        }

        private void DrawBlocks()
        {
            BoardGrid board = _game.Board;
            int visibleHeight = _game.VisibleHeight;
            _flashing.Clear();
            IReadOnlyList<Vector2Int> clearing = _game.ClearingCells;
            for (int i = 0; i < clearing.Count; i++)
            {
                _flashing.Add(clearing[i]);
            }

            for (int y = 0; y < visibleHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.IsOccupied(x, y))
                    {
                        BlockColor blockColor = board.GetColor(x, y);
                        Color color = BlockTint(blockColor);
                        if (_flashing.Contains(new Vector2Int(x, y)))
                        {
                            color = Color.Lerp(color, _clearFlashColor, _clearFlashAmount);
                        }

                        SetCell(_blocks, BlockTile(blockColor), x, y, color);
                    }
                    else
                    {
                        _blocks.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }

            if (_ghost != null)
            {
                _ghost.ClearAllTiles();
            }

            if (_game.ActivePiece == null)
            {
                return;
            }

            if (_showGhost && _ghost != null && _ghostTile != null && _game.State == BlocksGameState.Playing)
            {
                DrawPiece(_ghost, true, _game.LandingPosition, _ghostOpacity, visibleHeight);
            }

            DrawPiece(_blocks, false, _game.ActivePosition, 1f, visibleHeight);
        }

        private void DrawPiece(Tilemap tilemap, bool ghost, Vector2Int origin, float opacity, int visibleHeight)
        {
            IReadOnlyList<Vector2Int> cells = _game.ActiveCells;
            IReadOnlyList<BlockColor> colors = _game.ActiveBlockColors;
            for (int i = 0; i < cells.Count; i++)
            {
                int x = cells[i].x + origin.x;
                int y = cells[i].y + origin.y;
                if (y >= visibleHeight)
                {
                    continue;
                }

                BlockColor blockColor = i < colors.Count ? colors[i] : null;
                Color color = ghost ? GhostTint(blockColor) : BlockTint(blockColor);
                color.a *= opacity;
                SetCell(tilemap, ghost ? _ghostTile : BlockTile(blockColor), x, y, color);
            }
        }

        private Color BlockTint(BlockColor color)
        {
            return color != null ? color.DrawTint : _unpaintedColor;
        }

        private Color GhostTint(BlockColor color)
        {
            return color != null ? color.Tint : _unpaintedColor;
        }

        private TileBase BlockTile(BlockColor color)
        {
            if (color == null || color.Sprite == null)
            {
                return _cellTile;
            }

            if (!_colorTiles.TryGetValue(color, out Tile tile))
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = color.Id;
                tile.sprite = color.Sprite;
                tile.color = Color.white;
                tile.flags = TileFlags.None;
                tile.hideFlags = HideFlags.DontSave;
                _colorTiles.Add(color, tile);
            }

            return tile;
        }

        private void OnDestroy()
        {
            foreach (Tile tile in _colorTiles.Values)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }

            _colorTiles.Clear();
        }

        private void SetCell(Tilemap tilemap, int x, int y, Color color)
        {
            SetCell(tilemap, _cellTile, x, y, color);
        }

        private static void SetCell(Tilemap tilemap, TileBase tile, int x, int y, Color color)
        {
            var position = new Vector3Int(x, y, 0);
            tilemap.SetTile(position, tile);
            tilemap.SetTileFlags(position, TileFlags.None);
            tilemap.SetColor(position, color);
        }
    }
}
