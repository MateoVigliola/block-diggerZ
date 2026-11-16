using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks.Tests
{
    /// <summary>
    /// Builds boards from text for color tests. Rows are given top to bottom; '.' is empty, 'X' an unpainted
    /// block, letters R, B, G, Y are blocks of four test colors and D is an obstacle (Dirt) block.
    /// </summary>
    internal sealed class ColorTestBoard
    {
        public const char Empty = '.';
        public const char Unpainted = 'X';
        public const char Dirt = 'D';
        private const string Letters = "RBGYD";
        private const string ObstacleField = "<IsObstacle>k__BackingField";

        public readonly List<BlockColor> Colors = new();
        public readonly PieceDefinition Piece;

        public ColorTestBoard()
        {
            foreach (char letter in Letters)
            {
                var color = ScriptableObject.CreateInstance<BlockColor>();
                color.name = letter.ToString();
                if (letter == Dirt)
                {
                    var serialized = new UnityEditor.SerializedObject(color);
                    serialized.FindProperty(ObstacleField).boolValue = true;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                Colors.Add(color);
            }

            Piece = ScriptableObject.CreateInstance<PieceDefinition>();
        }

        public BlockColor this[char letter] => Colors[Letters.IndexOf(letter)];

        public BoardGrid Build(params string[] rowsTopDown)
        {
            int width = rowsTopDown[0].Length;
            var board = new BoardGrid(width, rowsTopDown.Length);
            for (int r = 0; r < rowsTopDown.Length; r++)
            {
                int y = rowsTopDown.Length - 1 - r;
                for (int x = 0; x < width; x++)
                {
                    char c = rowsTopDown[r][x];
                    if (c == Empty)
                    {
                        continue;
                    }

                    board.Set(x, y, Piece, c == Unpainted ? null : this[c]);
                }
            }

            return board;
        }

        public string[] Describe(BoardGrid board)
        {
            var rows = new string[board.Height];
            for (int y = 0; y < board.Height; y++)
            {
                var chars = new char[board.Width];
                for (int x = 0; x < board.Width; x++)
                {
                    BlockColor color = board.GetColor(x, y);
                    chars[x] = !board.IsOccupied(x, y) ? Empty : color == null ? Unpainted : Letters[Colors.IndexOf(color)];
                }

                rows[board.Height - 1 - y] = new string(chars);
            }

            return rows;
        }

        public void Destroy()
        {
            foreach (BlockColor color in Colors)
            {
                Object.DestroyImmediate(color);
            }

            Object.DestroyImmediate(Piece);
        }
    }
}
