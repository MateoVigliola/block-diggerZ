using UnityEditor;

namespace BlockDiggerz.Blocks.Tests
{
    /// <summary>Loads the real Blocks data assets for EditMode tests.</summary>
    internal static class BlocksTestAssets
    {
        public const string PieceSetPath = "Assets/_Project/Blocks/Data/StandardPieceSet.asset";
        public const string BasicTagPath = "Assets/_Project/Blocks/Data/Tags/Basic.asset";
        public const string RulesPath = "Assets/_Project/Blocks/Data/BlocksRules.asset";

        public static PieceSet PieceSet => AssetDatabase.LoadAssetAtPath<PieceSet>(PieceSetPath);

        public static PieceTag BasicTag => AssetDatabase.LoadAssetAtPath<PieceTag>(BasicTagPath);

        public static BlocksRules Rules => AssetDatabase.LoadAssetAtPath<BlocksRules>(RulesPath);

        public static PieceDefinition Piece(string id) => PieceSet.FindById(id);
    }
}
