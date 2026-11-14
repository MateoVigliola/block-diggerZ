using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Playtest
{
    /// <summary>
    /// Tools &gt; BlockDiggerz &gt; Playtest Checklist. Shows <see cref="PlaytestPaths.ChecklistFile"/> in two tabs:
    /// items still to check, and items already confirmed done. Ticking items and pressing Confirm marks them
    /// <c>[x]</c> in the file; notes typed here are written under the item.
    /// </summary>
    public class PlaytestChecklistWindow : EditorWindow
    {
        private const string MenuPath = "Tools/BlockDiggerz/Playtest Checklist";
        private const string WindowTitle = "Playtest Checklist";
        private const string PendingTabLabel = "To check";
        private const string DoneTabLabel = "Done";
        private const string NotesLabel = "Notes";
        private const string ReopenText = "Reopen";
        private const string UntitledSection = "Checklist";
        private const string PendingEmptyText = "Nothing left to check. New features add their items to " + PlaytestPaths.ChecklistFile + ".";
        private const string DoneEmptyText = "No confirmed items yet. Tick items in the To check tab and press Confirm checked.";
        private const string MissingFileText = "No checklist yet: " + PlaytestPaths.ChecklistFile + " is created after the first feature.";
        private const string ItemGoneMessage = "That item changed in the file; reloaded.";

        private const string SectionClass = "section";
        private const string SectionDescriptionClass = "section-description";
        private const string ItemRowClass = "item-row";
        private const string ItemRowSelectedClass = "item-row--selected";
        private const string ItemHeaderClass = "item-header";
        private const string ItemToggleClass = "item-toggle";
        private const string ItemTextClass = "item-text";
        private const string ItemDoneTextClass = "item-done-text";
        private const string ReopenButtonClass = "reopen-button";
        private const string NotesFieldClass = "notes-field";
        private const string EmptyLabelClass = "empty-label";
        private const string ErrorStatusClass = "status-bar--error";
        private const string FoldoutViewKeyPrefix = "playtest-section-";

        private static readonly Regex BoldPattern = new Regex(@"\*\*(.+?)\*\*");
        private static readonly Regex CodePattern = new Regex("`([^`]+)`");

        [SerializeField] private List<string> _selectedKeys = new List<string>();

        private PlaytestChecklistDocument _document;
        private DateTime _loadedWriteTime;

        private Tab _pendingTab;
        private Tab _doneTab;
        private ScrollView _pendingList;
        private ScrollView _doneList;
        private ToolbarSearchField _search;
        private Label _selectionLabel;
        private Button _confirmButton;
        private Button _selectNoneButton;
        private Label _statusBar;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<PlaytestChecklistWindow>(WindowTitle);
        }

        private void CreateGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PlaytestPaths.WindowUxml);
            if (tree == null)
            {
                rootVisualElement.Add(new Label("Missing UI file: " + PlaytestPaths.WindowUxml));
                return;
            }

            tree.CloneTree(rootVisualElement);
            rootVisualElement.style.flexGrow = 1;

            _pendingTab = rootVisualElement.Q<Tab>("pending-tab");
            _doneTab = rootVisualElement.Q<Tab>("done-tab");
            _pendingList = rootVisualElement.Q<ScrollView>("pending-list");
            _doneList = rootVisualElement.Q<ScrollView>("done-list");
            _search = rootVisualElement.Q<ToolbarSearchField>("search");
            _selectionLabel = rootVisualElement.Q<Label>("selection-label");
            _confirmButton = rootVisualElement.Q<Button>("confirm-button");
            _selectNoneButton = rootVisualElement.Q<Button>("select-none-button");
            _statusBar = rootVisualElement.Q<Label>("status-bar");

            rootVisualElement.Q<ToolbarButton>("reload-button").clicked += () => Reload(true);
            rootVisualElement.Q<ToolbarButton>("open-file-button").clicked += OpenFile;
            _search.RegisterValueChangedCallback(_ => Rebuild());
            _confirmButton.clicked += ConfirmSelected;
            _selectNoneButton.clicked += ClearSelection;

            Reload(false);
        }

        private void OnFocus()
        {
            if (_pendingList != null && PlaytestChecklistFile.LastWriteUtc != _loadedWriteTime)
            {
                Reload(false);
            }
        }

        private void Reload(bool announce)
        {
            _document = PlaytestChecklistFile.Load();
            _loadedWriteTime = PlaytestChecklistFile.LastWriteUtc;
            _selectedKeys.RemoveAll(key => _document.Find(key) == null || _document.Find(key).IsChecked);
            Rebuild();
            if (!PlaytestChecklistFile.Exists)
            {
                SetStatus(MissingFileText, false);
            }
            else if (announce)
            {
                SetStatus("Reloaded " + PlaytestPaths.ChecklistFile + ".", false);
            }
        }

        private void Rebuild()
        {
            _pendingList.Clear();
            _doneList.Clear();
            int pendingCount = 0;
            int doneCount = 0;
            string filter = _search.value?.Trim() ?? string.Empty;

            foreach (PlaytestSection section in _document.Sections)
            {
                Foldout pendingSection = null;
                Foldout doneSection = null;
                foreach (PlaytestItem item in section.Items)
                {
                    if (item.IsChecked)
                    {
                        doneCount++;
                    }
                    else
                    {
                        pendingCount++;
                    }

                    if (!Matches(item, filter))
                    {
                        continue;
                    }

                    if (item.IsChecked)
                    {
                        doneSection ??= AddSection(_doneList, section, DoneTabLabel);
                        doneSection.Add(BuildDoneRow(item));
                    }
                    else
                    {
                        pendingSection ??= AddSection(_pendingList, section, PendingTabLabel);
                        pendingSection.Add(BuildPendingRow(item));
                    }
                }
            }

            if (_pendingList.childCount == 0)
            {
                _pendingList.Add(EmptyLabel(PlaytestChecklistFile.Exists ? PendingEmptyText : MissingFileText));
            }

            if (_doneList.childCount == 0)
            {
                _doneList.Add(EmptyLabel(DoneEmptyText));
            }

            _pendingTab.label = PendingTabLabel + " (" + pendingCount + ")";
            _doneTab.label = DoneTabLabel + " (" + doneCount + ")";
            RefreshSelectionFooter();
        }

        private static bool Matches(PlaytestItem item, string filter)
        {
            if (filter.Length == 0)
            {
                return true;
            }

            return Contains(item.Text, filter) || Contains(item.Notes, filter) || Contains(item.Section, filter);
        }

        private static bool Contains(string text, string filter)
        {
            return text != null && text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Foldout AddSection(ScrollView list, PlaytestSection section, string tabKey)
        {
            string title = section.Title.Length > 0 ? section.Title : UntitledSection;
            var foldout = new Foldout { text = title, value = true, viewDataKey = FoldoutViewKeyPrefix + tabKey + title };
            foldout.AddToClassList(SectionClass);
            if (section.Description.Length > 0)
            {
                var description = new Label(ToRichText(section.Description)) { enableRichText = true };
                description.AddToClassList(SectionDescriptionClass);
                foldout.Add(description);
            }

            list.Add(foldout);
            return foldout;
        }

        private VisualElement BuildPendingRow(PlaytestItem item)
        {
            var row = new VisualElement();
            row.AddToClassList(ItemRowClass);
            var header = new VisualElement();
            header.AddToClassList(ItemHeaderClass);

            bool selected = _selectedKeys.Contains(item.Key);
            var toggle = new Toggle { value = selected, tooltip = "Tick, then press Confirm checked to move this item to Done" };
            toggle.AddToClassList(ItemToggleClass);
            row.EnableInClassList(ItemRowSelectedClass, selected);
            toggle.RegisterValueChangedCallback(evt => SetSelected(item.Key, evt.newValue, row));

            var text = new Label(ToRichText(item.Text)) { enableRichText = true };
            text.AddToClassList(ItemTextClass);
            text.RegisterCallback<ClickEvent>(_ => toggle.value = !toggle.value);

            header.Add(toggle);
            header.Add(text);
            row.Add(header);
            row.Add(BuildNotesField(item));
            return row;
        }

        private VisualElement BuildDoneRow(PlaytestItem item)
        {
            var row = new VisualElement();
            row.AddToClassList(ItemRowClass);
            var header = new VisualElement();
            header.AddToClassList(ItemHeaderClass);

            var text = new Label(ToRichText(item.Text)) { enableRichText = true };
            text.AddToClassList(ItemTextClass);
            text.AddToClassList(ItemDoneTextClass);

            var reopen = new Button(() => Reopen(item.Key)) { text = ReopenText, tooltip = "Move this item back to To check" };
            reopen.AddToClassList(ReopenButtonClass);

            header.Add(text);
            header.Add(reopen);
            row.Add(header);
            row.Add(BuildNotesField(item));
            return row;
        }

        private TextField BuildNotesField(PlaytestItem item)
        {
            var notes = new TextField(NotesLabel) { multiline = true, isDelayed = true, value = item.Notes };
            notes.AddToClassList(NotesFieldClass);
            notes.RegisterValueChangedCallback(evt => SaveNotes(item.Key, evt.newValue));
            return notes;
        }

        private void SetSelected(string key, bool selected, VisualElement row)
        {
            _selectedKeys.Remove(key);
            if (selected)
            {
                _selectedKeys.Add(key);
            }

            row.EnableInClassList(ItemRowSelectedClass, selected);
            RefreshSelectionFooter();
        }

        private void ClearSelection()
        {
            _selectedKeys.Clear();
            Rebuild();
        }

        private void RefreshSelectionFooter()
        {
            int count = _selectedKeys.Count;
            _selectionLabel.text = count == 0 ? "Tick the items you have tested." : count + " ticked";
            _confirmButton.text = count == 0 ? "Confirm checked" : "Confirm checked (" + count + ")";
            _confirmButton.SetEnabled(count > 0);
            _selectNoneButton.SetEnabled(count > 0);
        }

        private void ConfirmSelected()
        {
            var keys = new List<string>(_selectedKeys);
            int moved = 0;
            bool edited = PlaytestChecklistFile.Edit(document =>
            {
                foreach (string key in keys)
                {
                    if (document.SetChecked(key, true))
                    {
                        moved++;
                    }
                }

                return moved > 0;
            });

            _selectedKeys.Clear();
            Reload(false);
            int missing = keys.Count - moved;
            string message = edited ? "Moved " + moved + " item(s) to Done." : "Nothing was moved.";
            SetStatus(missing > 0 ? message + " " + missing + " item(s) were no longer in the file." : message, missing > 0);
        }

        private void Reopen(string key)
        {
            bool edited = PlaytestChecklistFile.Edit(document => document.SetChecked(key, false));
            Reload(false);
            SetStatus(edited ? "Moved the item back to To check." : ItemGoneMessage, !edited);
        }

        private void SaveNotes(string key, string notes)
        {
            bool edited = PlaytestChecklistFile.Edit(document => document.SetNotes(key, notes));
            if (!edited)
            {
                Reload(false);
                SetStatus(ItemGoneMessage, true);
                return;
            }

            _document = PlaytestChecklistFile.Load();
            _loadedWriteTime = PlaytestChecklistFile.LastWriteUtc;
            SetStatus("Notes saved.", false);
        }

        private void OpenFile()
        {
            if (!PlaytestChecklistFile.Exists)
            {
                SetStatus(MissingFileText, true);
                return;
            }

            EditorUtility.OpenWithDefaultApp(PlaytestChecklistFile.FullPath);
        }

        private static Label EmptyLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList(EmptyLabelClass);
            return label;
        }

        private static string ToRichText(string markdown)
        {
            string escaped = markdown.Replace("<", "<noparse><</noparse>");
            string bold = BoldPattern.Replace(escaped, "<b>$1</b>");
            return CodePattern.Replace(bold, "<i>$1</i>");
        }

        private void SetStatus(string message, bool isError)
        {
            _statusBar.text = message;
            _statusBar.EnableInClassList(ErrorStatusClass, isError);
        }
    }
}
