using BlockDiggerz.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// Pause menu in the Game scene. Esc pauses (time stops, gameplay keys off) and shows Continue, Settings and
    /// Main menu. Esc goes back from Settings, and Esc on the pause page continues. Works on the game over screen
    /// too. Loads saved bindings at startup so the Game scene can be played directly.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class PauseMenuView : MonoBehaviour
    {
        /// <summary>Name of the full-screen overlay shown while paused.</summary>
        public const string OverlayName = "pause-overlay";

        /// <summary>Name of the element holding the pause buttons.</summary>
        public const string RootPageName = "pause-root";

        /// <summary>Name of the settings panel instance.</summary>
        public const string SettingsPageName = "settings-page";

        private const string ContinueButtonName = "continue-button";
        private const string SettingsButtonName = "settings-button";
        private const string MainMenuButtonName = "main-menu-button";
        private const float NormalTimeScale = 1f;

        [SerializeField, Tooltip("Input wrapper; supplies Esc, gameplay switching and the actions to rebind.")]
        private GameInput _input;

        [SerializeField, Tooltip("Scene loaded by Main menu. Must be in Build Settings.")]
        private string _mainMenuScene = "MainMenu";

        private readonly MenuNavigator _navigator = new();
        private VisualElement _overlay;
        private VisualElement _rootPage;
        private VisualElement _settingsPage;
        private Button _continueButton;
        private SettingsPanelController _settingsPanel;
        private float _timeScaleBeforePause = NormalTimeScale;

        /// <summary>True while the pause menu is open.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>The page currently shown while paused.</summary>
        public MenuPage CurrentPage => _navigator.Current;

        private GameSettings _settings;

        private void Awake()
        {
            Time.timeScale = NormalTimeScale;
        }

        private void OnEnable()
        {
            _settings = GameSettings.LoadIfMissing(_settings, new PlayerPrefsKeyValueStore(), _input.Actions);

            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _overlay = root.Q<VisualElement>(OverlayName);
            _rootPage = root.Q<VisualElement>(RootPageName);
            _settingsPage = root.Q<VisualElement>(SettingsPageName);
            _continueButton = MenuUi.BindButton(root, ContinueButtonName, Resume);
            MenuUi.BindButton(root, SettingsButtonName, OpenSettings);
            MenuUi.BindButton(root, MainMenuButtonName, GoToMainMenu);

            if (_settingsPage != null)
            {
                _settingsPanel = new SettingsPanelController(_settingsPage, _input, _settings);
                _settingsPanel.BackRequested += Back;
            }

            MenuUi.SetVisible(_overlay, IsPaused);
        }

        private void OnDisable()
        {
            _settingsPanel?.Dispose();
            _settingsPanel = null;
        }

        private void OnDestroy()
        {
            if (IsPaused)
            {
                Time.timeScale = _timeScaleBeforePause;
            }
        }

        private void Update()
        {
            if (!_input.PausePressed || (_settingsPanel != null && _settingsPanel.ConsumesPause))
            {
                return;
            }

            if (!IsPaused)
            {
                Pause();
            }
            else
            {
                Back();
            }
        }

        /// <summary>Opens the pause menu: stops time and switches gameplay keys off.</summary>
        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;
            _input.SetGameplayEnabled(false);
            _navigator.ResetToRoot();
            MenuUi.SetVisible(_overlay, true);
            ShowCurrentPage();
        }

        /// <summary>Closes the pause menu and restores time and gameplay keys.</summary>
        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            _settingsPanel?.Dispose();
            IsPaused = false;
            Time.timeScale = _timeScaleBeforePause;
            _input.SetGameplayEnabled(true);
            MenuUi.SetVisible(_overlay, false);
        }

        private void Back()
        {
            if (_navigator.Back())
            {
                ShowCurrentPage();
            }
            else
            {
                Resume();
            }
        }

        private void OpenSettings()
        {
            _navigator.Open(MenuPage.Settings);
            ShowCurrentPage();
        }

        private void GoToMainMenu()
        {
            Resume();
            SceneManager.LoadScene(_mainMenuScene);
        }

        private void ShowCurrentPage()
        {
            bool settings = _navigator.Current == MenuPage.Settings;
            MenuUi.SetVisible(_rootPage, !settings);
            MenuUi.SetVisible(_settingsPage, settings);
            if (settings)
            {
                _settingsPanel?.Show();
            }
            else
            {
                _continueButton?.Focus();
            }
        }
    }
}
