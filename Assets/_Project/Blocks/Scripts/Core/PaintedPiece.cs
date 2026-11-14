using System.Collections.Generic;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// A piece shape plus the color of each of its blocks. Colors are indexed like the shape's cells
    /// (<see cref="PieceDefinition.GetCells"/> keeps the same cell order in every rotation), so blocks keep their
    /// colors when the piece rotates or is stored.
    /// </summary>
    public sealed class PaintedPiece
    {
        private readonly BlockColor[] _colors;

        /// <summary>Wraps <paramref name="definition"/> with one color per cell.</summary>
        public PaintedPiece(PieceDefinition definition, IReadOnlyList<BlockColor> colors)
        {
            Definition = definition;
            _colors = new BlockColor[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                _colors[i] = colors[i];
            }
        }

        /// <summary>The piece shape and tags.</summary>
        public PieceDefinition Definition { get; }

        /// <summary>Color of each cell, in cell order.</summary>
        public IReadOnlyList<BlockColor> Colors => _colors;
    }
}
