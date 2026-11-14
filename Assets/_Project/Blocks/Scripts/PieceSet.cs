using System.Collections.Generic;
using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>The pieces a run draws from. The randomizer deals each piece once per bag, in a shuffled order.</summary>
    [CreateAssetMenu(menuName = "BlockDiggerz/Blocks/Piece Set", fileName = "PieceSet")]
    public sealed class PieceSet : ScriptableObject
    {
        [SerializeField, Tooltip("Pieces in the bag. Each appears once per bag.")]
        private List<PieceDefinition> _pieces = new();

        /// <summary>Pieces in the set.</summary>
        public IReadOnlyList<PieceDefinition> Pieces => _pieces;

        /// <summary>The piece with the given id, or null.</summary>
        public PieceDefinition FindById(string id)
        {
            for (int i = 0; i < _pieces.Count; i++)
            {
                if (_pieces[i] != null && _pieces[i].Id == id)
                {
                    return _pieces[i];
                }
            }

            return null;
        }
    }
}
