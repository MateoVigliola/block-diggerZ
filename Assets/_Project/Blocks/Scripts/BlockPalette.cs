using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Ordered list of every block color. A run uses the first N colors, where N is
    /// <see cref="BlocksRules.ColorCount"/> (later set by a difficulty mode), so put colors in the order they should
    /// be added as difficulty rises.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Block Palette", fileName = "BlockPalette")]
    public sealed class BlockPalette : ScriptableObject
    {
        [SerializeField, Tooltip("Every block color, in the order they become active.")]
        private List<BlockColor> _colors = new();

        /// <summary>Every color in the palette.</summary>
        public IReadOnlyList<BlockColor> Colors => _colors;

        /// <summary>Writes the first <paramref name="count"/> colors (fewer if the palette is shorter) into <paramref name="result"/>.</summary>
        public void GetActive(int count, List<BlockColor> result)
        {
            result.Clear();
            for (int i = 0; i < _colors.Count && result.Count < count; i++)
            {
                if (_colors[i] != null)
                {
                    result.Add(_colors[i]);
                }
            }
        }
    }
}
