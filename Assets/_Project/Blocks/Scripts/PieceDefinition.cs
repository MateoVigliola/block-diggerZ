using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// One piece shape. Cells are given in its spawn orientation inside a square box of <see cref="BoxSize"/> cells
    /// (x to the right, y up, origin bottom-left). Rotating clockwise turns the whole box a quarter turn, so the
    /// standard pieces use a 3x3 box, the I piece a 4x4 box and the O piece a 2x2 box.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Piece Definition", fileName = "Piece")]
    public sealed class PieceDefinition : ScriptableObject
    {
        /// <summary>Number of rotation states (spawn, right, upside down, left).</summary>
        public const int RotationCount = 4;

        [field: SerializeField, Tooltip("Short stable id, used by debug controls such as \"Spawn T\".")]
        public string Id { get; private set; } = "T";

        [field: SerializeField, Tooltip("Color of the piece's cells on the board.")]
        public Color Color { get; private set; } = Color.white;

        [field: SerializeField, Min(1), Tooltip("Side of the square box the cells sit in; the piece rotates around the box center.")]
        public int BoxSize { get; private set; } = 3;

        [SerializeField, Tooltip("Filled cells in spawn orientation, inside the box (0,0 is bottom-left).")]
        private Vector2Int[] _cells = System.Array.Empty<Vector2Int>();

        [SerializeField, Tooltip("Tags describing what kind of piece this is. Every piece has at least Basic for now.")]
        private List<PieceTag> _tags = new();

        private Vector2Int[][] _rotations;

        /// <summary>Tags of this piece.</summary>
        public IReadOnlyList<PieceTag> Tags => _tags;

        /// <summary>True if the piece carries <paramref name="tag"/>.</summary>
        public bool HasTag(PieceTag tag)
        {
            return tag != null && _tags.Contains(tag);
        }

        /// <summary>Cells for a rotation state (0 = spawn, 1 = one clockwise turn, and so on).</summary>
        public IReadOnlyList<Vector2Int> GetCells(int rotation)
        {
            if (_rotations == null)
            {
                BuildRotations();
            }

            return _rotations[Wrap(rotation)];
        }

        /// <summary>Highest filled row inside the box for a rotation state.</summary>
        public int GetTopRow(int rotation)
        {
            IReadOnlyList<Vector2Int> cells = GetCells(rotation);
            int top = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                top = Mathf.Max(top, cells[i].y);
            }

            return top;
        }

        /// <summary>Wraps any rotation index into 0..3.</summary>
        public static int Wrap(int rotation)
        {
            return ((rotation % RotationCount) + RotationCount) % RotationCount;
        }

        /// <summary>Turns a box cell a quarter turn clockwise inside a box of the given size.</summary>
        public static Vector2Int RotateClockwise(Vector2Int cell, int boxSize)
        {
            return new Vector2Int(cell.y, boxSize - 1 - cell.x);
        }

        private void BuildRotations()
        {
            _rotations = new Vector2Int[RotationCount][];
            _rotations[0] = (Vector2Int[])_cells.Clone();
            for (int r = 1; r < RotationCount; r++)
            {
                Vector2Int[] previous = _rotations[r - 1];
                var next = new Vector2Int[previous.Length];
                for (int i = 0; i < previous.Length; i++)
                {
                    next[i] = RotateClockwise(previous[i], BoxSize);
                }

                _rotations[r] = next;
            }
        }

        private void OnValidate()
        {
            _rotations = null;
        }
    }
}
