using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.AssetTracking.Tests
{
    public class TrackedAssetUpdaterTests
    {
        private const string Guid = "0123456789abcdef0123456789abcdef";
        private const string ArtPath = "Assets/_Project/Board/Art/block.png";
        private const string Texture = "Texture2D";

        private AssetTrackerDatabase _database;

        [SetUp]
        public void SetUp()
        {
            _database = ScriptableObject.CreateInstance<AssetTrackerDatabase>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_database);
        }

        [Test]
        public void ImportInTrackedFolderAddsPlaceholder()
        {
            Assert.That(TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, Texture));

            TrackedAssetEntry entry = _database.FindByGuid(Guid);
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Name, Is.EqualTo("block"));
            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Art));
            Assert.That(entry.Area, Is.EqualTo("Board"));
            Assert.That(entry.Status, Is.EqualTo(AssetStatus.Placeholder));
            Assert.That(entry.AssetPath, Is.EqualTo(ArtPath));
            Assert.That(entry.SyncedOnce, Is.False);
        }

        [Test]
        public void ImportOutsideTrackedFoldersIsIgnored()
        {
            Assert.That(TrackedAssetUpdater.ApplyImported(_database, "Assets/_Project/Board/Scripts/a.png", Guid, Texture),
                Is.False);
            Assert.That(_database.Entries, Is.Empty);
        }

        [Test]
        public void ReimportDoesNotDuplicateOrOverwriteText()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, Texture);
            TrackedAssetEntry entry = _database.FindByGuid(Guid);
            entry.SetSheetField(SheetField.Name, "Red Block", true);

            Assert.That(TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, Texture), Is.False);
            Assert.That(_database.Entries.Count, Is.EqualTo(1));
            Assert.That(entry.Name, Is.EqualTo("Red Block"));
        }

        [Test]
        public void MoveUpdatesPathCategoryAndArea()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, Texture);
            const string moved = "Assets/_Project/Shared/UI/Audio/block.png";

            Assert.That(TrackedAssetUpdater.ApplyMoved(_database, moved, Guid, Texture));

            TrackedAssetEntry entry = _database.FindByGuid(Guid);
            Assert.That(entry.AssetPath, Is.EqualTo(moved));
            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Audio));
            Assert.That(entry.Area, Is.EqualTo("Shared/UI"));
        }

        [Test]
        public void DeleteKeepsRowAndMarksMissing()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, Texture);
            TrackedAssetEntry entry = _database.FindByGuid(Guid);
            entry.MarkSynced();

            Assert.That(TrackedAssetUpdater.ApplyDeleted(_database, ArtPath));

            Assert.That(_database.Entries.Count, Is.EqualTo(1));
            Assert.That(entry.IsLinked, Is.False);
            Assert.That(entry.Status, Is.EqualTo(AssetStatus.Missing));
            Assert.That(entry.IsDirty(SheetField.Status), "Missing status must be pushed to the sheet");
        }
    }
}
