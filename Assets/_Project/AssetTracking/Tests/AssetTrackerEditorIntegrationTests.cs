using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace BlockDiggerz.AssetTracking.Tests
{
    /// <summary>
    /// Runs the real import watcher and window against the project database. Cleans up the temporary
    /// feature folder and its tracker entries afterwards.
    /// </summary>
    public class AssetTrackerEditorIntegrationTests
    {
        private const string TempFeature = "TrackerTestTemp";
        private const string TempFeatureFolder = TrackedFolderRules.ProjectRoot + "/" + TempFeature;
        private const string TempArtFolder = TempFeatureFolder + "/Art";
        private const string TempTexturePath = TempArtFolder + "/test_block.asset";
        private const int MaxWaitFrames = 30;

        [TearDown]
        public void TearDown()
        {
            AssetTrackerDatabase database = AssetTrackerDatabase.LoadOrCreate();
            AssetDatabase.DeleteAsset(TempFeatureFolder);

            for (int i = database.Entries.Count - 1; i >= 0; i--)
            {
                if (database.Entries[i].Area == TempFeature)
                {
                    database.Forget(database.Entries[i]);
                }
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
        }

        [UnityTest]
        public IEnumerator NewFileInFeatureArtFolderIsTrackedAutomatically()
        {
            AssetDatabase.CreateFolder(TrackedFolderRules.ProjectRoot, TempFeature);
            AssetDatabase.CreateFolder(TempFeatureFolder, "Art");
            var texture = new Texture2D(4, 4);
            AssetDatabase.CreateAsset(texture, TempTexturePath);
            string guid = AssetDatabase.AssetPathToGUID(TempTexturePath);

            AssetTrackerDatabase database = AssetTrackerDatabase.LoadOrCreate();
            for (int frame = 0; frame < MaxWaitFrames && database.FindByGuid(guid) == null; frame++)
            {
                yield return null;
            }

            TrackedAssetEntry entry = database.FindByGuid(guid);
            Assert.That(entry, Is.Not.Null, "Import watcher did not add the new file");
            Assert.That(entry.Area, Is.EqualTo(TempFeature));
            Assert.That(entry.Category, Is.EqualTo(AssetCategory.Art));
            Assert.That(entry.Status, Is.EqualTo(AssetStatus.Placeholder));
            Assert.That(entry.AssetType, Is.EqualTo(nameof(Texture2D)));
        }

        [Test]
        public void WindowOpensWithListAndToolbar()
        {
            AssetTrackerWindow.Open();
            var window = EditorWindow.GetWindow<AssetTrackerWindow>();
            try
            {
                Assert.That(window.rootVisualElement.Q<MultiColumnListView>("entry-list"), Is.Not.Null);
                Assert.That(window.rootVisualElement.Q("sync"), Is.Not.Null);
            }
            finally
            {
                window.Close();
            }
        }
    }
}
