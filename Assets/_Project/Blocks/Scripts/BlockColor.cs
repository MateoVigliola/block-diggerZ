using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// One block color. Blocks of the same color asset match each other; three or more in a straight row or
    /// column are deleted. A color can have its own block sprite; without one, blocks use the shared white block
    /// sprite tinted with <see cref="Tint"/>. Obstacle colors (Dirt) never match.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Block Color", fileName = "BlockColor")]
    public sealed class BlockColor : ScriptableObject
    {
        [field: SerializeField, Tooltip("Short stable id, used in debug readouts and tests (for example \"Red\").")]
        public string Id { get; private set; } = "Red";

        [field: SerializeField, Tooltip("The color of this block. Tints the shared block sprite when no Sprite is set, and always tints the landing ghost.")]
        public Color Tint { get; private set; } = Color.white;

        [field: SerializeField, Tooltip("Optional block art for this color, drawn untinted on the board, the falling piece and the NEXT and STORE previews. Import as Sprite at 32 pixels per unit.")]
        public Sprite Sprite { get; private set; }

        [field: SerializeField, Tooltip("Obstacle blocks (such as Dirt) never match, not even with each other, and always fall as single blocks.")]
        public bool IsObstacle { get; private set; }

        /// <summary>Tint to draw a block of this color with: white when it has its own sprite, else <see cref="Tint"/>.</summary>
        public Color DrawTint => Sprite != null ? Color.white : Tint;

        /// <summary>True if blocks of <paramref name="color"/> can be part of a color match (painted, not an obstacle).</summary>
        public static bool CanMatch(BlockColor color) => color != null && !color.IsObstacle;
    }
}
