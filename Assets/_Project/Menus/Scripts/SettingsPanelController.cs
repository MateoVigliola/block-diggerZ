using System;
using System.Collections.Generic;
using BlockDiggerz.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Drives the shared settings panel (SettingsPanel.uxml) inside the main menu and the pause menu: Controls and
    /// Sound tabs, keyboard rebinding (one row per action with Primary and Alternate key buttons; a pressed key
    /// swaps with any slot that used it, Backspace clears the slot), volume sliders, reset buttons and Back.
    /// Plain class owned by the menu screen component, which forwards Esc through <see cref="ConsumesPause"/>.
    /// </summary>
    public sealed class SettingsPanelController : IDisposable
    {
        /// <summary>Name of the Controls tab button.</summary>
        public const string ControlsTabName = "tab-controls";

        /// <summary>Name of the Sound tab button.</summary>
        public const string SoundTabName = "tab-sound";

        /// <summary>Name of the touch control mode row (shown only while the touch layout can be in use).</summary>
        public const string TouchModeRowName = "touch-mode-row";

        /// <summary>Name of the button that picks gesture touch controls.</summary>
        public const string TouchGesturesName = "touch-mode-gestures";

        /// <summary>Name of the button that picks button touch controls.</summary>
        public const string TouchButtonsName = "touch-mode-buttons";

        private const string ControlsPageName = "page-controls";
        private const string SoundPageName = "page-sound";
        private const string ControlsListName = "controls-list";
        private const string ControlsResetName = "controls-reset";
        private const string MusicSliderName = "music-slider";
        private const string SfxSliderName = "sfx-slider";
        private const string SoundResetName = "sound-reset";
        private const string BackButtonName = "settings-back";
        private const string SelectedTabClass = "settings-tab--selected";
        private const string RowClass = "controls-row";
        private const string HeaderRowClass = "controls-row--header";
        private const string ActionLabelClass = "controls-action";
        private const string ColumnLabelClass = "controls-column";
        private const string KeyButtonClass = "controls-key";
        private const string EmptyKeyClass = "controls-key--empty";
        private const string ListeningClass = "controls-key--listening";
        private const string ActionHeaderText = "Action";
        private const string PrimaryHeaderText = "Primary";
        private const string AlternateHeaderText = "Alternate";
        private const string ListeningText = "Press a key";
        private const string EmptyKeyText = "-";
        private const string KeyboardPath = "<Keyboard>";
        private const string CancelPath = "<Keyboard>/escape";
        private const string ClearPath = "<Keyboard>/backspace";
        private const string AnyKeyPath = "<Keyboard>/anyKey";

        private readonly GameInput _input;
        private readonly GameSettings _settings;
        private readonly Button _controlsTab;
        private readonly Button _soundTab;
        private readonly VisualElement _controlsPage;
        private readonly VisualElement _soundPage;
        private readonly ScrollView _controlsList;
        private readonly Slider _musicSlider;
        private readonly Slider _sfxSlider;
        private readonly VisualElement _touchModeRow;
        private readonly Button _touchGestures;
        private readonly Button _touchButtons;
        private readonly List<(Button button, InputAction action, int index)> _keyButtons = new();
        private InputActionRebindingExtensions.RebindingOperation _rebind;
        private int _lastRebindEndFrame = -1;

        /// <summary>Raised when the Back button is pressed.</summary>
        public event Action BackRequested;

        /// <summary>Binds the panel under <paramref name="root"/>.</summary>
        public SettingsPanelController(VisualElement root, GameInput input, GameSettings settings)
        {
            _input = input;
            _settings = settings;
            Root = root;
            _controlsTab = root.Q<Button>(ControlsTabName);
            _soundTab = root.Q<Button>(SoundTabName);
            _controlsPage = root.Q<VisualElement>(ControlsPageName);
            _soundPage = root.Q<VisualElement>(SoundPageName);
            _controlsList = root.Q<ScrollView>(ControlsListName);
            _musicSlider = root.Q<Slider>(MusicSliderName);
            _sfxSlider = root.Q<Slider>(SfxSliderName);
            _touchModeRow = root.Q<VisualElement>(TouchModeRowName);
            _touchGestures = root.Q<Button>(TouchGesturesName);
            _touchButtons = root.Q<Button>(TouchButtonsName);
            OnClick(_touchGestures, () => SetTouchMode(TouchControlMode.Gestures));
            OnClick(_touchButtons, () => SetTouchMode(TouchControlMode.Buttons));

            OnClick(_controlsTab, () => SelectTab(true));
            OnClick(_soundTab, () => SelectTab(false));
            OnClick(root.Q<Button>(ControlsResetName), ResetBindings);
            OnClick(root.Q<Button>(SoundResetName), ResetVolumes);
            OnClick(root.Q<Button>(BackButtonName), () => BackRequested?.Invoke());

            SetUpSlider(_musicSlider, _settings.MusicVolume, _settings.SetMusicVolume);
            SetUpSlider(_sfxSlider, _settings.SfxVolume, _settings.SetSfxVolume);
            BuildControlRows();
            SelectTab(true);
        }

        /// <summary>The panel's root element.</summary>
        public VisualElement Root { get; }

        /// <summary>True while waiting for the player to press a new key.</summary>
        public bool IsRebinding => _rebind != null;

        /// <summary>
        /// True while rebinding, or on the frame a rebind ended: Esc then belongs to the rebind (cancel), not to
        /// menu navigation.
        /// </summary>
        public bool ConsumesPause => IsRebinding || _lastRebindEndFrame == Time.frameCount;

        /// <summary>Shows the Controls tab and refreshes values; call when the panel opens.</summary>
        public void Show()
        {
            SelectTab(true);
            RefreshKeyLabels();
            MenuUi.SetVisible(_touchModeRow, GameInput.TouchLayoutInUse);
            RefreshTouchMode();
            _musicSlider?.SetValueWithoutNotify(_settings.MusicVolume);
            _sfxSlider?.SetValueWithoutNotify(_settings.SfxVolume);
            _controlsTab?.Focus();
        }

        /// <summary>Cancels a rebind in progress.</summary>
        public void Dispose()
        {
            _rebind?.Cancel();
            _rebind?.Dispose();
            _rebind = null;
        }

        private void SelectTab(bool controls)
        {
            _controlsTab?.EnableInClassList(SelectedTabClass, controls);
            _soundTab?.EnableInClassList(SelectedTabClass, !controls);
            MenuUi.SetVisible(_controlsPage, controls);
            MenuUi.SetVisible(_soundPage, !controls);
        }

        private static void OnClick(Button button, Action action)
        {
            if (button != null)
            {
                button.clicked += action;
            }
        }

        private static void SetUpSlider(Slider slider, float value, Action<float> save)
        {
            if (slider == null)
            {
                return;
            }

            slider.lowValue = GameSettings.MinVolume;
            slider.highValue = GameSettings.MaxVolume;
            slider.SetValueWithoutNotify(value);
            slider.RegisterValueChangedCallback(e => save(e.newValue));
        }

        private void BuildControlRows()
        {
            if (_controlsList == null || _input == null)
            {
                return;
            }

            _controlsList.Clear();
            _keyButtons.Clear();
            _controlsList.Add(BuildHeaderRow());
            foreach (InputAction action in _input.GameplayActions)
            {
                var row = new VisualElement();
                row.AddToClassList(RowClass);
                var label = new Label(ActionDisplayNames.ToLabel(action.name));
                label.AddToClassList(ActionLabelClass);
                row.Add(label);

                for (int slot = 0; slot < BindingSwapper.SlotCount; slot++)
                {
                    int index = BindingSwapper.GetBindingIndex(action, slot);
                    var button = new Button();
                    button.AddToClassList(KeyButtonClass);
                    row.Add(button);
                    if (index == BindingSwapper.NoBinding)
                    {
                        button.text = EmptyKeyText;
                        button.AddToClassList(EmptyKeyClass);
                        button.SetEnabled(false);
                        continue;
                    }

                    button.clicked += () => StartRebind(action, index, button);
                    _keyButtons.Add((button, action, index));
                }

                _controlsList.Add(row);
            }

            RefreshKeyLabels();
        }

        private static VisualElement BuildHeaderRow()
        {
            var header = new VisualElement();
            header.AddToClassList(RowClass);
            header.AddToClassList(HeaderRowClass);
            AddHeaderLabel(header, ActionHeaderText, ActionLabelClass);
            AddHeaderLabel(header, PrimaryHeaderText, ColumnLabelClass);
            AddHeaderLabel(header, AlternateHeaderText, ColumnLabelClass);
            return header;
        }

        private static void AddHeaderLabel(VisualElement header, string text, string className)
        {
            var label = new Label(text);
            label.AddToClassList(className);
            header.Add(label);
        }

        private void RefreshKeyLabels()
        {
            foreach ((Button button, InputAction action, int index) in _keyButtons)
            {
                bool empty = BindingSwapper.IsEmpty(action.bindings[index]);
                button.text = empty ? EmptyKeyText : action.GetBindingDisplayString(index);
                button.EnableInClassList(EmptyKeyClass, empty);
                button.RemoveFromClassList(ListeningClass);
            }
        }

        private void ApplyKey(InputAction action, int index, string path)
        {
            if (string.Equals(path, ClearPath, StringComparison.OrdinalIgnoreCase))
            {
                BindingSwapper.Clear(action, index);
                return;
            }

            BindingSwapper.Assign(_input.GameplayActions, action, index, path);
        }

        private void StartRebind(InputAction action, int index, Button button)
        {
            if (IsRebinding)
            {
                return;
            }

            bool wasEnabled = action.enabled;
            action.Disable();
            button.text = ListeningText;
            button.AddToClassList(ListeningClass);

            _rebind = action.PerformInteractiveRebinding(index)
                .WithControlsHavingToMatchPath(KeyboardPath)
                .WithControlsExcluding(AnyKeyPath)
                .WithCancelingThrough(CancelPath)
                .OnApplyBinding((op, path) => ApplyKey(action, index, path))
                .OnComplete(op => FinishRebind(action, wasEnabled, true))
                .OnCancel(op => FinishRebind(action, wasEnabled, false))
                .Start();
        }

        private void FinishRebind(InputAction action, bool wasEnabled, bool applied)
        {
            _rebind?.Dispose();
            _rebind = null;
            _lastRebindEndFrame = Time.frameCount;
            if (wasEnabled)
            {
                action.Enable();
            }

            if (applied)
            {
                _settings.SaveBindings(_input.Actions);
                _input.NotifyBindingsChanged();
            }

            RefreshKeyLabels();
        }

        private void ResetBindings()
        {
            Dispose();
            _settings.ResetBindings(_input.Actions);
            _input.NotifyBindingsChanged();
            RefreshKeyLabels();
        }

        private void SetTouchMode(TouchControlMode mode)
        {
            _settings.SetTouchMode(mode);
            _input?.SetTouchMode(mode);
            RefreshTouchMode();
        }

        private void RefreshTouchMode()
        {
            _touchGestures?.EnableInClassList(SelectedTabClass, _settings.TouchMode == TouchControlMode.Gestures);
            _touchButtons?.EnableInClassList(SelectedTabClass, _settings.TouchMode == TouchControlMode.Buttons);
        }

        private void ResetVolumes()
        {
            _settings.ResetVolumes();
            _musicSlider?.SetValueWithoutNotify(_settings.MusicVolume);
            _sfxSlider?.SetValueWithoutNotify(_settings.SfxVolume);
        }
    }
}
