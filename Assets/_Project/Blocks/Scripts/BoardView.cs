using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Draws the board with two tilemaps: a background grid of the visible area, and the locked cells plus the
    /// falling piece tinted with their piece color. One board cell is one tilemap cell. Redraws only when
    /// <see cref="BlocksGame.Version"/> changes.
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

        private int _drawnVersion = -1;

        private void Start()
        {
            DrawBackground();
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
            for (int y = 0; y < _game.Rules.VisibleHeight; y++)
            {
                for (int x = 0; x < _game.Rules.BoardWidth; x++)
                {
                    SetCell(_background, x, y, _backgroundColor);
                }
            }
        }

        private void DrawBlocks()
        {
            BoardGrid board = _game.Board;
            int visibleHeight = _game.Rules.VisibleHeight;
            for (int y = 0; y < visibleHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    PieceDefinition piece = board.Get(x, y);
                    if (piece != null)
                    {
                        SetCell(_blocks, x, y, piece.Color);
                    }
                    else
                    {
                        _blocks.SetTile(new Vector3Int(x, y, 0), null);
                    }
                }
            }

            if (_game.ActivePiece == null)
            {
                return;
            }

            IReadOnlyList<Vector2Int> cells = _game.ActiveCells;
            Vector2Int origin = _game.ActivePosition;
            for (int i = 0; i < cells.Count; i++)
            {
                int x = cells[i].x + origin.x;
                int y = cells[i].y + origin.y;
                if (y < visibleHeight)
                {
                    SetCell(_blocks, x, y, _game.ActivePiece.Color);
                }
            }
        }

        private void SetCell(Tilemap tilemap, int x, int y, Color color)
        {
            var position = new Vector3Int(x, y, 0);
            tilemap.SetTile(position, _cellTile);
            tilemap.SetTileFlags(position, TileFlags.None);
            tilemap.SetColor(position, color);
        }
    }
}
