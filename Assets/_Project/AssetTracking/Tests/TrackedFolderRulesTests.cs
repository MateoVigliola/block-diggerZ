using NUnit.Framework;

namespace BlockDiggerz.AssetTracking.Tests
{
    public class TrackedFolderRulesTests
    {
        [TestCase("Assets/_Project/Board/Art/block.png", AssetCategory.Art, "Board")]
        [TestCase("Assets/_Project/Board/Audio/clear.wav", AssetCategory.Audio, "Board")]
        [TestCase("Assets/_Project/Board/Art/Blocks/red.png", AssetCategory.Art, "Board")]
        [TestCase("Assets/_Project/Shared/UI/Art/button.png", AssetCategory.Art, "Shared/UI")]
        [TestCase("Assets/_Project/Shared/Gameplay/Audio/Music/theme.ogg", AssetCategory.Audio, "Shared/Gameplay")]
        public void TrackedPathsAreClassified(string path, AssetCategory category, string area)
        {
            Assert.That(TrackedFolderRules.TryClassify(path, out AssetCategory actualCategory, out string actualArea));
            Assert.That(actualCategory, Is.EqualTo(category));
            Assert.That(actualArea, Is.EqualTo(area));
        }

        [TestCase("Assets/_Project/Board/Scripts/Board.cs")]
        [TestCase("Assets/_Project/Board/Art")]
        [TestCase("Assets/_Project/Shared/Art/button.png")]
        [TestCase("Assets/Welcome/Art/logo.png")]
        [TestCase("Packages/com.unity.foo/Art/a.png")]
        [TestCase("")]
        public void OtherPathsAreIgnored(string path)
        {
            Assert.That(TrackedFolderRules.TryClassify(path, out _, out _), Is.False);
        }
    }
}
