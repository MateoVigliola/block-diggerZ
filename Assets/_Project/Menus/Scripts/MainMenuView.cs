using BlockDiggerz.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace BlockDiggerz.Menus
{
    /// <summary>
    /// The main menu (MainMenu scene): Play, Settings and Quit, plus the shared settings panel. Esc goes back from
    /// Settings. Loads saved bindings at startup and keeps gameplay keys off while the menu is up.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuView : MonoBehaviour
    {
        /// <summary>Name of the element holding the main buttons.</summary>
        public const string RootPageName = "main-root";

        /// <summary>Name of the settings panel instance.</summary>
        public const string SettingsPageName = "settings-page";

        private const string PlayButtonName = "play-button";
        private const string SettingsButtonName = "settings-button";
        private const string QuitButtonName = "quit-button";

        [SerializeField, Tooltip("Input wrapper; supplies Esc and the actions to rebind.")]
        private GameInput _input;

        [SerializeField, Tooltip("Scene loaded by Play. Must be in Build Settings.")]
        private string _gameScene = "Game";

        private readonly MenuNavigator _navigator = new();
        private VisualElement _rootPage;
        private VisualElement _settingsPage;
        private Button _playButton;
        private SettingsPanelController _settingsPanel;

        /// <summary>The page currently shown.</summary>
        public MenuPage CurrentPage => _navigator.Current;

        private GameSettings _settings;

        private void Awake()
        {
            _settings = new GameSettings(new PlayerPrefsKeyValueStore());
            _settings.LoadBindings(_input.Actions);
        }

        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _rootPage = root.Q<VisualElement>(RootPageName);
            _settingsPage = root.Q<VisualElement>(SettingsPageName);
            _playButton = MenuUi.BindButton(root, PlayButtonName, Play);
            MenuUi.BindButton(root, SettingsButtonName, OpenSettings);
            MenuUi.BindButton(root, QuitButtonName, Application.Quit);

            if (_settingsPage != null)
            {
                _settingsPanel = new SettingsPanelController(_settingsPage, _input, _settings);
                _settingsPanel.BackRequested += Back;
            }

            ShowCurrentPage();
        }

        private void Start()
        {
            _input.SetGameplayEnabled(false);
        }

        private void OnDisable()
        {
            _settingsPanel?.Dispose();
            _settingsPanel = null;
        }

        private void Update()
        {
            if (_input.PausePressed && (_settingsPanel == null || !_settingsPanel.ConsumesPause))
            {
                Back();
            }
        }

        private void Play()
        {
            SceneManager.LoadScene(_gameScene);
        }

        private void OpenSettings()
        {
            _navigator.Open(MenuPage.Settings);
            ShowCurrentPage();
        }

        private void Back()
        {
            if (_navigator.Back())
            {
                ShowCurrentPage();
            }
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
                _playButton?.Focus();
            }
        }
    }
}
