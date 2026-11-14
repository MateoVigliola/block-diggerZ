using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BlockDiggerz.AssetTracking
{
    /// <summary>
    /// Tools &gt; BlockDiggerz &gt; Asset Tracker. Lists every tracked art and audio asset, edits entries,
    /// links planned rows to files, rescans the tracked folders and syncs with the Google Sheet.
    /// </summary>
    public class AssetTrackerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/BlockDiggerz/Asset Tracker";
        private const string WindowTitle = "Asset Tracker";
        private const string UndoName = "Edit Asset Tracker";
        private const string AllTabsChoice = "All tabs";
        private const string AllStatusesChoice = "All statuses";
        private const string NewPlannedName = "New asset";
        private const string OpenSettingsClass = "settings-panel--open";
        private const string ErrorStatusClass = "status-bar--error";
        private const string CellLabelClass = "cell-label";
        private const string StatusClassPrefix = "status-";
        private const string SyncStateNew = "New";
        private const string SyncStateChanged = "Changed";

        private const string NameColumn = "name";
        private const string CategoryColumn = "category";
        private const string AreaColumn = "area";
        private const string StatusColumn = "status";
        private const string PathColumn = "path";
        private const string SyncColumn = "sync";

        private static readonly HashSet<AssetTrackerWindow> OpenWindows = new HashSet<AssetTrackerWindow>();

        private readonly List<TrackedAssetEntry> _visible = new List<TrackedAssetEntry>();

        private AssetTrackerDatabase _database;
        private TrackedAssetEntry _selected;
        private bool _syncing;

        private DropdownField _categoryFilter;
        private DropdownField _statusFilter;
        private ToolbarSearchField _search;
        private ToolbarButton _syncButton;
        private ToolbarButton _rescanButton;
        private ToolbarButton _addButton;
        private ToolbarButton _openSheetButton;
        private VisualElement _settingsPanel;
        private TextField _webAppUrlField;
        private TextField _secretField;
        private MultiColumnListView _list;
        private Label _emptyDetails;
        private VisualElement _detailFields;
        private TextField _nameField;
        private DropdownField _categoryField;
        private TextField _areaField;
        private DropdownField _statusField;
        private TextField _descriptionField;
        private TextField _requirementsField;
        private TextField _notesField;
        private ObjectField _assetField;
        private Label _pathLabel;
        private Label _syncStateLabel;
        private Button _pingButton;
        private Label _statusBar;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<AssetTrackerWindow>(WindowTitle);
        }

        /// <summary>Redraws every open tracker window, for example after the import watcher changed entries.</summary>
        public static void RefreshOpenWindows()
        {
            foreach (AssetTrackerWindow window in OpenWindows)
            {
                window.RefreshList();
            }
        }

        private void OnEnable()
        {
            OpenWindows.Add(this);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            OpenWindows.Remove(this);
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void CreateGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetTrackerPaths.WindowUxml);
            if (tree == null)
            {
                rootVisualElement.Add(new Label("Missing UI file: " + AssetTrackerPaths.WindowUxml));
                return;
            }

            tree.CloneTree(rootVisualElement);
            rootVisualElement.style.flexGrow = 1;
            _database = AssetTrackerDatabase.LoadOrCreate();

            QueryElements();
            SetUpToolbar();
            SetUpSettings();
            SetUpList();
            SetUpDetails();

            RefreshList();
            BindDetails();
            SetStatus(_database.Entries.Count + " tracked assets.", false);
        }

        private void QueryElements()
        {
            VisualElement root = rootVisualElement;
            _categoryFilter = root.Q<DropdownField>("category-filter");
            _statusFilter = root.Q<DropdownField>("status-filter");
            _search = root.Q<ToolbarSearchField>("search");
            _syncButton = root.Q<ToolbarButton>("sync");
            _rescanButton = root.Q<ToolbarButton>("rescan");
            _addButton = root.Q<ToolbarButton>("add-planned");
            _openSheetButton = root.Q<ToolbarButton>("open-sheet");
            _settingsPanel = root.Q<VisualElement>("settings-panel");
            _webAppUrlField = root.Q<TextField>("web-app-url");
            _secretField = root.Q<TextField>("secret");
            _list = root.Q<MultiColumnListView>("entry-list");
            _emptyDetails = root.Q<Label>("empty-details");
            _detailFields = root.Q<VisualElement>("detail-fields");
            _nameField = root.Q<TextField>("entry-name");
            _categoryField = root.Q<DropdownField>("entry-category");
            _areaField = root.Q<TextField>("entry-area");
            _statusField = root.Q<DropdownField>("entry-status");
            _descriptionField = root.Q<TextField>("entry-description");
            _requirementsField = root.Q<TextField>("entry-requirements");
            _notesField = root.Q<TextField>("entry-notes");
            _assetField = root.Q<ObjectField>("entry-asset");
            _pathLabel = root.Q<Label>("entry-path");
            _syncStateLabel = root.Q<Label>("entry-sync-state");
            _pingButton = root.Q<Button>("ping-asset");
            _statusBar = root.Q<Label>("status-bar");
        }

        private void SetUpToolbar()
        {
            var categoryChoices = new List<string> { AllTabsChoice };
            categoryChoices.AddRange(Enum.GetNames(typeof(AssetCategory)));
            _categoryFilter.choices = categoryChoices;
            _categoryFilter.SetValueWithoutNotify(AllTabsChoice);
            _categoryFilter.RegisterValueChangedCallback(_ => RefreshList());

            var statusChoices = new List<string> { AllStatusesChoice };
            statusChoices.AddRange(AssetStatusNames.All);
            _statusFilter.choices = statusChoices;
            _statusFilter.SetValueWithoutNotify(AllStatusesChoice);
            _statusFilter.RegisterValueChangedCallback(_ => RefreshList());

            _search.RegisterValueChangedCallback(_ => RefreshList());
            _addButton.clicked += AddPlanned;
            _rescanButton.clicked += Rescan;
            _syncButton.clicked += Sync;
            _openSheetButton.clicked += () => Application.OpenURL(AssetTrackerSettings.instance.SheetUrl);
            UpdateOpenSheetButton();
        }

        private void SetUpSettings()
        {
            AssetTrackerSettings settings = AssetTrackerSettings.instance;
            _webAppUrlField.SetValueWithoutNotify(settings.WebAppUrl);
            _secretField.SetValueWithoutNotify(settings.Secret);

            var toggle = rootVisualElement.Q<ToolbarToggle>("settings-toggle");
            toggle.RegisterValueChangedCallback(evt => _settingsPanel.EnableInClassList(OpenSettingsClass, evt.newValue));
            if (!settings.IsConfigured)
            {
                toggle.value = true;
            }

            rootVisualElement.Q<Button>("save-settings").clicked += () =>
            {
                AssetTrackerSettings.instance.SetConnection(_webAppUrlField.value, _secretField.value);
                SetStatus("Sheet connection saved.", false);
            };
            rootVisualElement.Q<Button>("copy-script").clicked += CopyAppsScript;
            rootVisualElement.Q<Button>("open-guide").clicked += () =>
                AssetDatabase.OpenAsset(AssetDatabase.LoadMainAssetAtPath(AssetTrackerPaths.SetupGuide));
        }

        private void SetUpList()
        {
            SetUpColumn(NameColumn, entry => entry.Name);
            SetUpColumn(CategoryColumn, entry => entry.Category.ToString());
            SetUpColumn(AreaColumn, entry => entry.Area);
            SetUpColumn(StatusColumn, entry => AssetStatusNames.ToSheet(entry.Status));
            SetUpColumn(PathColumn, entry => entry.AssetPath);
            SetUpColumn(SyncColumn, SyncState);

            Column statusColumn = _list.columns[StatusColumn];
            Action<VisualElement, int> bindText = statusColumn.bindCell;
            statusColumn.bindCell = (element, index) =>
            {
                bindText(element, index);
                foreach (AssetStatus status in Enum.GetValues(typeof(AssetStatus)))
                {
                    element.EnableInClassList(StatusClass(status), _visible[index].Status == status);
                }
            };

            _list.itemsSource = _visible;
            _list.selectionChanged += _ =>
            {
                _selected = _list.selectedIndex >= 0 && _list.selectedIndex < _visible.Count
                    ? _visible[_list.selectedIndex]
                    : null;
                BindDetails();
            };
        }

        private void SetUpColumn(string columnName, Func<TrackedAssetEntry, string> text)
        {
            Column column = _list.columns[columnName];
            column.makeCell = () =>
            {
                var label = new Label();
                label.AddToClassList(CellLabelClass);
                return label;
            };
            column.bindCell = (element, index) => ((Label)element).text = text(_visible[index]);
        }

        private void SetUpDetails()
        {
            _categoryField.choices = new List<string>(Enum.GetNames(typeof(AssetCategory)));
            _statusField.choices = new List<string>(AssetStatusNames.All);
            _assetField.objectType = typeof(Object);

            RegisterText(_nameField, SheetField.Name);
            RegisterText(_descriptionField, SheetField.Description);
            RegisterText(_requirementsField, SheetField.Requirements);
            RegisterText(_notesField, SheetField.Notes);

            _statusField.RegisterValueChangedCallback(evt =>
            {
                if (_selected == null || !AssetStatusNames.TryParse(evt.newValue, out AssetStatus status))
                {
                    return;
                }

                RecordUndo();
                _selected.SetStatus(status, false);
                OnEdited();
            });

            _categoryField.RegisterValueChangedCallback(evt => SetPlacement(evt.newValue, _selected?.Area));
            _areaField.RegisterValueChangedCallback(evt => SetPlacement(_selected?.Category.ToString(), evt.newValue));
            _assetField.RegisterValueChangedCallback(evt => OnAssetChanged(evt.newValue));
            _pingButton.clicked += () =>
            {
                if (_selected != null && _selected.IsLinked)
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(_selected.AssetPath));
                }
            };
            rootVisualElement.Q<Button>("remove-entry").clicked += RemoveSelected;
        }

        private void RegisterText(TextField field, SheetField sheetField)
        {
            field.RegisterValueChangedCallback(evt =>
            {
                if (_selected == null)
                {
                    return;
                }

                RecordUndo();
                _selected.SetSheetField(sheetField, evt.newValue, false);
                OnEdited();
            });
        }

        private void SetPlacement(string categoryName, string area)
        {
            if (_selected == null || _selected.IsLinked || !Enum.TryParse(categoryName, out AssetCategory category))
            {
                return;
            }

            RecordUndo();
            _selected.SetPlacement(category, area);
            OnEdited();
        }

        private void OnAssetChanged(Object asset)
        {
            if (_selected == null)
            {
                return;
            }

            if (asset == null)
            {
                if (_selected.IsLinked)
                {
                    RecordUndo();
                    TrackedAssetUpdater.MarkMissing(_selected);
                    OnEdited();
                }

                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid) || guid == _selected.AssetGuid)
            {
                BindDetails();
                return;
            }

            TrackedAssetEntry other = _database.FindByGuid(guid);
            if (other != null && other != _selected)
            {
                string message = $"{path} is already tracked as \"{other.Name}\".\n\n" +
                                 $"Link it to \"{_selected.Name}\" instead? The \"{other.Name}\" entry is removed, " +
                                 "and its sheet row is deleted on the next sync.";
                if (!EditorUtility.DisplayDialog(WindowTitle, message, "Link here", "Cancel"))
                {
                    BindDetails();
                    return;
                }
            }

            RecordUndo();
            if (other != null && other != _selected)
            {
                _database.Remove(other);
            }

            _selected.Link(guid, path, AssetTrackerImportWatcher.AssetTypeName(path));
            if (TrackedFolderRules.TryClassify(path, out AssetCategory category, out string area))
            {
                _selected.SetPlacement(category, area);
            }

            if (_selected.Status == AssetStatus.Missing)
            {
                _selected.SetStatus(AssetStatus.Placeholder, false);
            }

            OnEdited();
        }

        private void AddPlanned()
        {
            AssetCategory category = Enum.TryParse(_categoryFilter.value, out AssetCategory filtered)
                ? filtered
                : AssetCategory.Art;

            RecordUndo();
            var entry = new TrackedAssetEntry(TrackedAssetEntry.NewId(), category, NewPlannedName);
            _database.Add(entry);
            _selected = entry;
            _search.SetValueWithoutNotify(string.Empty);
            _statusFilter.SetValueWithoutNotify(AllStatusesChoice);
            OnEdited();
            _nameField.Focus();
        }

        private void RemoveSelected()
        {
            if (_selected == null)
            {
                return;
            }

            string message = $"Remove \"{_selected.Name}\" from the tracker?" +
                             (_selected.SyncedOnce ? "\n\nIts sheet row is deleted on the next sync." : string.Empty) +
                             "\n\nThe asset file itself is not touched. Undo is available until the next sync.";
            if (!EditorUtility.DisplayDialog(WindowTitle, message, "Remove", "Cancel"))
            {
                return;
            }

            RecordUndo();
            _database.Remove(_selected);
            _selected = null;
            OnEdited();
        }

        private void Rescan()
        {
            RecordUndo();
            AssetTrackerScanner.ScanResult result = AssetTrackerScanner.Rescan(_database);
            RefreshList();
            BindDetails();
            SetStatus($"Rescan: {result.Added} added, {result.Updated} updated, {result.MarkedMissing} marked Missing.",
                false);
        }

        private async void Sync()
        {
            AssetTrackerSettings settings = AssetTrackerSettings.instance;
            if (!settings.IsConfigured)
            {
                rootVisualElement.Q<ToolbarToggle>("settings-toggle").value = true;
                SetStatus("Set the Web App URL and Sync Secret first (see Setup Guide).", true);
                return;
            }

            SetSyncing(true);
            SetStatus("Syncing with the sheet...", false);
            try
            {
                AssetDatabase.SaveAssetIfDirty(_database);
                string summary = await AssetTrackerSync.SyncAsync(_database, settings);
                SetStatus(summary, false);
            }
            catch (Exception exception)
            {
                SetStatus("Sync failed: " + exception.Message, true);
                Debug.LogException(exception);
            }
            finally
            {
                SetSyncing(false);
                UpdateOpenSheetButton();
                if (_selected != null && _database.FindById(_selected.Id) == null)
                {
                    _selected = null;
                }

                RefreshList();
                BindDetails();
            }
        }

        private void SetSyncing(bool syncing)
        {
            _syncing = syncing;
            _syncButton.SetEnabled(!syncing);
            _rescanButton.SetEnabled(!syncing);
            _addButton.SetEnabled(!syncing);
            _detailFields.SetEnabled(!syncing);
        }

        private void CopyAppsScript()
        {
            if (!File.Exists(AssetTrackerPaths.AppsScript))
            {
                SetStatus("Apps Script not found at " + AssetTrackerPaths.AppsScript, true);
                return;
            }

            EditorGUIUtility.systemCopyBuffer = File.ReadAllText(AssetTrackerPaths.AppsScript);
            SetStatus("Apps Script copied. Paste it into Extensions > Apps Script in the sheet.", false);
        }

        private void RefreshList()
        {
            if (_list == null || _database == null)
            {
                return;
            }

            _visible.Clear();
            for (int i = 0; i < _database.Entries.Count; i++)
            {
                if (Matches(_database.Entries[i]))
                {
                    _visible.Add(_database.Entries[i]);
                }
            }

            _list.RefreshItems();
            int index = _selected == null ? -1 : _visible.IndexOf(_selected);
            if (index >= 0)
            {
                _list.SetSelectionWithoutNotify(new[] { index });
            }
            else
            {
                _list.ClearSelection();
            }
        }

        private bool Matches(TrackedAssetEntry entry)
        {
            if (_categoryFilter.value != AllTabsChoice && entry.Category.ToString() != _categoryFilter.value)
            {
                return false;
            }

            if (_statusFilter.value != AllStatusesChoice && AssetStatusNames.ToSheet(entry.Status) != _statusFilter.value)
            {
                return false;
            }

            string search = _search.value;
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return Contains(entry.Name, search) || Contains(entry.Area, search) || Contains(entry.AssetPath, search)
                   || Contains(entry.Description, search);
        }

        private void BindDetails()
        {
            if (_detailFields == null)
            {
                return;
            }

            bool hasSelection = _selected != null;
            _emptyDetails.style.display = hasSelection ? DisplayStyle.None : DisplayStyle.Flex;
            _detailFields.style.display = hasSelection ? DisplayStyle.Flex : DisplayStyle.None;
            if (!hasSelection)
            {
                return;
            }

            _nameField.SetValueWithoutNotify(_selected.Name);
            _categoryField.SetValueWithoutNotify(_selected.Category.ToString());
            _areaField.SetValueWithoutNotify(_selected.Area);
            _statusField.SetValueWithoutNotify(AssetStatusNames.ToSheet(_selected.Status));
            _descriptionField.SetValueWithoutNotify(_selected.Description);
            _requirementsField.SetValueWithoutNotify(_selected.Requirements);
            _notesField.SetValueWithoutNotify(_selected.Notes);
            _assetField.SetValueWithoutNotify(_selected.IsLinked
                ? AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(_selected.AssetGuid))
                : null);

            _categoryField.SetEnabled(!_selected.IsLinked);
            _areaField.SetEnabled(!_selected.IsLinked);
            _pingButton.SetEnabled(_selected.IsLinked);
            _pathLabel.text = _selected.IsLinked
                ? _selected.AssetPath + " (" + _selected.AssetType + ")"
                : "No file yet. Drop an asset on Linked Asset when it exists.";
            _syncStateLabel.text = DescribeSyncState(_selected);
        }

        private void OnEdited()
        {
            EditorUtility.SetDirty(_database);
            RefreshList();
            BindDetails();
        }

        private void OnUndoRedo()
        {
            if (_syncing || _database == null)
            {
                return;
            }

            if (_selected != null && _database.FindById(_selected.Id) == null)
            {
                _selected = null;
            }

            RefreshList();
            BindDetails();
        }

        private void RecordUndo()
        {
            Undo.RecordObject(_database, UndoName);
        }

        private void UpdateOpenSheetButton()
        {
            _openSheetButton.SetEnabled(!string.IsNullOrEmpty(AssetTrackerSettings.instance.SheetUrl));
        }

        private void SetStatus(string message, bool isError)
        {
            _statusBar.text = message;
            _statusBar.EnableInClassList(ErrorStatusClass, isError);
        }

        private static string SyncState(TrackedAssetEntry entry)
        {
            if (!entry.SyncedOnce)
            {
                return SyncStateNew;
            }

            return entry.DirtyFields == SheetField.None ? string.Empty : SyncStateChanged;
        }

        private static string DescribeSyncState(TrackedAssetEntry entry)
        {
            if (!entry.SyncedOnce)
            {
                return "Not in the sheet yet. The next sync adds it.";
            }

            if (entry.DirtyFields == SheetField.None)
            {
                return "In the sheet. Sheet edits to Name, Description, Requirements, Status and Notes win.";
            }

            return "Changed here: " + entry.DirtyFields + ". The next sync writes these to the sheet.";
        }

        private static string StatusClass(AssetStatus status)
        {
            return StatusClassPrefix + status.ToString().ToLowerInvariant();
        }

        private static bool Contains(string text, string search)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
