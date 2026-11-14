using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BlockDiggerz.AssetTracking.Tests
{
    public class SheetSyncMergeTests
    {
        private const string Secret = "secret";
        private const string Guid = "0123456789abcdef0123456789abcdef";
        private const string ArtPath = "Assets/_Project/Board/Art/block.png";

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
        public void SheetOnlyRowBecomesPlannedEntry()
        {
            PullSummary summary = SheetSyncMerge.ApplyPull(_database, new[]
            {
                Row("S-1", "Audio", "Line clear", "Missing")
            });

            Assert.That(summary.Added, Is.EqualTo(1));
            TrackedAssetEntry entry = _database.FindById("S-1");
            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Audio));
            Assert.That(entry.IsLinked, Is.False);
            Assert.That(entry.SyncedOnce);
            Assert.That(entry.DirtyFields, Is.EqualTo(SheetField.None));
        }

        [Test]
        public void SheetEditsUpdateSyncedEntry()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();
            SheetRow row = Row(entry.Id, "Art", "Red Block", "Needs Replacement");
            row.description = "Basic red block";

            PullSummary summary = SheetSyncMerge.ApplyPull(_database, new[] { row });

            Assert.That(summary.Updated, Is.EqualTo(1));
            Assert.That(entry.Name, Is.EqualTo("Red Block"));
            Assert.That(entry.Description, Is.EqualTo("Basic red block"));
            Assert.That(entry.Status, Is.EqualTo(AssetStatus.NeedsReplacement));
        }

        [Test]
        public void FieldChangedInUnityWinsOverSheet()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();
            entry.SetStatus(AssetStatus.Missing, false);

            SheetSyncMerge.ApplyPull(_database, new[] { Row(entry.Id, "Art", "Red Block", "Final") });

            Assert.That(entry.Status, Is.EqualTo(AssetStatus.Missing));
            Assert.That(entry.Name, Is.EqualTo("Red Block"));
        }

        [Test]
        public void LinkedEntryKeepsCategoryFromItsFolder()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();

            SheetSyncMerge.ApplyPull(_database, new[] { Row(entry.Id, "Audio", "block", "Placeholder") });

            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Art));
        }

        [Test]
        public void PlannedEntryMovedToOtherTabInSheetFollows()
        {
            var entry = new TrackedAssetEntry("P-1", AssetCategory.Art, "Theme");
            entry.MarkSynced();
            _database.Add(entry);

            SheetRow row = Row("P-1", "Audio", "Theme", "Missing");
            row.area = "Shared/Gameplay";
            SheetSyncMerge.ApplyPull(_database, new[] { row });

            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Audio));
            Assert.That(entry.Area, Is.EqualTo("Shared/Gameplay"));
        }

        [Test]
        public void UnknownStatusKeepsLocalStatus()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();

            PullSummary summary = SheetSyncMerge.ApplyPull(_database, new[] { Row(entry.Id, "Art", "block", "Done!!") });

            Assert.That(summary.UnknownStatuses, Is.EqualTo(1));
            Assert.That(entry.Status, Is.EqualTo(AssetStatus.Placeholder));
        }

        [Test]
        public void PlannedRowDeletedInSheetIsForgotten()
        {
            var entry = new TrackedAssetEntry("P-1", AssetCategory.Art, "Theme");
            entry.MarkSynced();
            _database.Add(entry);

            PullSummary summary = SheetSyncMerge.ApplyPull(_database, new SheetRow[0]);

            Assert.That(summary.Forgotten, Is.EqualTo(1));
            Assert.That(_database.Entries, Is.Empty);
            Assert.That(_database.RemovedIds, Is.Empty);
        }

        [Test]
        public void LinkedRowDeletedInSheetIsRestoredInFull()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();

            PullSummary summary = SheetSyncMerge.ApplyPull(_database, new SheetRow[0]);

            Assert.That(summary.Restored, Is.EqualTo(1));
            Assert.That(_database.Entries.Count, Is.EqualTo(1));
            PushRow pushed = SheetSyncMerge.BuildPush(_database, Secret).rows.Single();
            Assert.That(pushed.writeFields.Length, Is.EqualTo(5));
        }

        [Test]
        public void NewEntryIsNotForgottenBeforeFirstPush()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, "Texture2D");

            SheetSyncMerge.ApplyPull(_database, new SheetRow[0]);

            Assert.That(_database.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemovedEntryIsNotPulledBackAndIsDeletedOnPush()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();
            _database.Remove(entry);

            SheetSyncMerge.ApplyPull(_database, new[] { Row(entry.Id, "Art", "block", "Placeholder") });
            PushRequest push = SheetSyncMerge.BuildPush(_database, Secret);

            Assert.That(_database.Entries, Is.Empty);
            Assert.That(push.deleteIds, Is.EqualTo(new[] { entry.Id }));
        }

        [Test]
        public void PushWritesOnlyDirtySheetFieldsOfSyncedEntries()
        {
            TrackedAssetEntry entry = SyncedLinkedEntry();
            entry.SetStatus(AssetStatus.Missing, false);

            PushRow pushed = SheetSyncMerge.BuildPush(_database, Secret).rows.Single();

            Assert.That(pushed.writeFields, Is.EqualTo(new[] { SheetSyncMerge.StatusKey }));
            Assert.That(pushed.status, Is.EqualTo(AssetStatusNames.Missing));
            Assert.That(pushed.assetPath, Is.EqualTo(ArtPath));
            Assert.That(pushed.category, Is.EqualTo("Art"));
        }

        [Test]
        public void CompletePushClearsDirtyStateAndRemovedIds()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, "Texture2D");
            var removed = new TrackedAssetEntry("P-1", AssetCategory.Art, "Old");
            removed.MarkSynced();
            _database.Add(removed);
            _database.Remove(removed);

            SheetSyncMerge.CompletePush(_database);

            TrackedAssetEntry entry = _database.FindByGuid(Guid);
            Assert.That(entry.SyncedOnce);
            Assert.That(entry.DirtyFields, Is.EqualTo(SheetField.None));
            Assert.That(_database.RemovedIds, Is.Empty);
        }

        [Test]
        public void StatusNamesRoundTrip()
        {
            foreach (AssetStatus status in System.Enum.GetValues(typeof(AssetStatus)))
            {
                Assert.That(AssetStatusNames.TryParse(AssetStatusNames.ToSheet(status), out AssetStatus parsed));
                Assert.That(parsed, Is.EqualTo(status));
            }

            Assert.That(AssetStatusNames.TryParse(" needs replacement ", out AssetStatus loose));
            Assert.That(loose, Is.EqualTo(AssetStatus.NeedsReplacement));
        }

        private TrackedAssetEntry SyncedLinkedEntry()
        {
            TrackedAssetUpdater.ApplyImported(_database, ArtPath, Guid, "Texture2D");
            SheetSyncMerge.CompletePush(_database);
            return _database.FindByGuid(Guid);
        }

        private static SheetRow Row(string id, string category, string name, string status)
        {
            return new SheetRow
            {
                id = id,
                category = category,
                name = name,
                area = string.Empty,
                description = string.Empty,
                requirements = string.Empty,
                status = status,
                notes = string.Empty
            };
        }
    }
}
