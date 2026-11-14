using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Draws one piece inside a HUD preview box (a square VisualElement). Creates its cell elements once and only
    /// moves and tints them (one color per block) when the shown piece changes. Cell look comes from the "hud-piece-cell" USS class.
    /// </summary>
    public sealed class PiecePreviewView
    {
        /// <summary>USS class of each preview cell.</summary>
        public const string CellClass = "hud-piece-cell";

        private const int MaxCells = PiecePreviewLayout.GridSize * PiecePreviewLayout.GridSize;
        private const float Percent = 100f;

        private readonly VisualElement _box;
        private readonly List<VisualElement> _cells = new();
        private readonly List<Vector2> _positions = new();

        /// <summary>Wraps a preview box element; does nothing if it is null.</summary>
        public PiecePreviewView(VisualElement box)
        {
            _box = box;
        }

        /// <summary>The piece currently drawn, or null.</summary>
        public PaintedPiece Shown { get; private set; }

        /// <summary>Draws <paramref name="piece"/> in spawn orientation, or clears the box for null.</summary>
        public void Show(PaintedPiece piece)
        {
            Shown = piece;
            if (_box == null)
            {
                return;
            }

            if (piece == null)
            {
                _positions.Clear();
            }
            else
            {
                PiecePreviewLayout.Layout(piece.Definition.GetCells(0), _positions);
            }

            EnsureCells(_positions.Count);
            float cellPercent = Percent / PiecePreviewLayout.GridSize;
            for (int i = 0; i < _cells.Count; i++)
            {
                VisualElement cell = _cells[i];
                bool used = i < _positions.Count;
                cell.style.display = used ? DisplayStyle.Flex : DisplayStyle.None;
                if (!used)
                {
                    continue;
                }

                cell.style.left = Length.Percent(_positions[i].x * cellPercent);
                cell.style.top = Length.Percent(_positions[i].y * cellPercent);
                cell.style.width = Length.Percent(cellPercent);
                cell.style.height = Length.Percent(cellPercent);
                BlockColor color = i < piece.Colors.Count ? piece.Colors[i] : null;
                cell.style.unityBackgroundImageTintColor = color != null ? color.DrawTint : Color.white;
                cell.style.backgroundImage = color != null && color.Sprite != null
                    ? new StyleBackground(color.Sprite)
                    : new StyleBackground(StyleKeyword.Null);
            }
        }

        private void EnsureCells(int count)
        {
            while (_cells.Count < Mathf.Min(count, MaxCells))
            {
                var cell = new VisualElement { pickingMode = PickingMode.Ignore };
                cell.AddToClassList(CellClass);
                _box.Add(cell);
                _cells.Add(cell);
            }
        }
    }
}
