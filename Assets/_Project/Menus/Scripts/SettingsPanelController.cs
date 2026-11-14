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
    /// Sound tabs, keyboard rebinding with key swap on conflict, volume sliders, reset buttons and Back.
    /// Plain class owned by the menu screen component, which forwards Esc through <see cref="ConsumesPause"/>.
    /// </summary>
    public sealed class SettingsPanelController : IDisposable
    {
        /// <summary>Name of the Controls tab button.</summary>
        public const string ControlsTabName = "tab-controls";

        /// <summary>Name of the Sound tab button.</summary>
        public const string SoundTabName = "tab-sound";

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
        private const string ActionLabelClass = "controls-action";
        private const string KeyButtonClass = "controls-key";
        private const string ListeningClass = "controls-key--listening";
        private const string ListeningText = "Press a key";
        private const string KeyboardPath = "<Keyboard>";
        private const string CancelPath = "<Keyboard>/escape";
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
            foreach (InputAction action in _input.GameplayActions)
            {
                var row = new VisualElement();
                row.AddToClassList(RowClass);
                var label = new Label(ActionDisplayNames.ToLabel(action.name));
                label.AddToClassList(ActionLabelClass);
                row.Add(label);

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (!BindingSwapper.IsKeyboardBinding(action.bindings[i]))
                    {
                        continue;
                    }

                    int index = i;
                    var button = new Button();
                    button.AddToClassList(KeyButtonClass);
                    button.clicked += () => StartRebind(action, index, button);
                    row.Add(button);
                    _keyButtons.Add((button, action, index));
                }

                _controlsList.Add(row);
            }

            RefreshKeyLabels();
        }

        private void RefreshKeyLabels()
        {
            foreach ((Button button, InputAction action, int index) in _keyButtons)
            {
                button.text = action.GetBindingDisplayString(index);
                button.RemoveFromClassList(ListeningClass);
            }
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
                .OnApplyBinding((op, path) => BindingSwapper.Assign(_input.GameplayActions, action, index, path))
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

        private void ResetVolumes()
        {
            _settings.ResetVolumes();
            _musicSlider?.SetValueWithoutNotify(_settings.MusicVolume);
            _sfxSlider?.SetValueWithoutNotify(_settings.SfxVolume);
        }
    }
}
