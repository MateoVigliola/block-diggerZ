using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// A label for a kind of piece (Basic for now; later special pieces such as explosive or drill). Pieces list
    /// their tags in <see cref="PieceDefinition"/>, and cells locked into the board keep their piece, so game rules
    /// and upgrades can ask "does this piece or cell have tag X". Compare tags by asset reference, not by name.
    /// </summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Piece Tag", fileName = "PieceTag")]
    public sealed class PieceTag : ScriptableObject
    {
        [field: SerializeField, Tooltip("Name shown in UI and the debug menu.")]
        public string DisplayName { get; private set; } = "Basic";

        [field: SerializeField, TextArea, Tooltip("What the tag means for game rules. For designers; not shown in game yet.")]
        public string Description { get; private set; } = string.Empty;
    }
}
