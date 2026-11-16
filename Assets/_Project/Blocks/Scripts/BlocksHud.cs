using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// UI Toolkit HUD for the block dropper: the store panel (just left of the board), score and next piece
    /// (just right of the board, both following the board as it moves or resizes), the run timer above the store
    /// panel, the game over panel (score, Restart and Main menu buttons) and the win panel (time, score, New level
    /// and Main menu buttons). In the portrait touch layout (<see cref="SetPortraitLayout"/>) the panels sit in
    /// slots the touch controls give instead.
    /// Element names match BlocksHud.uxml. Updates elements only when
    /// the value they show changes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BlocksHud : MonoBehaviour
    {
        /// <summary>Name of the label showing the score.</summary>
        public const string ScoreValueName = "score-value";

        /// <summary>Name of the label on the game over panel showing the final score.</summary>
        public const string FinalScoreName = "final-score";

        /// <summary>Name of the game over panel.</summary>
        public const string GameOverPanelName = "game-over";

        /// <summary>Name of the next piece preview box.</summary>
        public const string NextPreviewName = "next-preview";

        /// <summary>Name of the store panel.</summary>
        public const string StorePanelName = "store";

        /// <summary>Name of the SCORE / NEXT column right of the board.</summary>
        public const string StatsPanelName = "stats";

        /// <summary>Name of the stored piece preview box.</summary>
        public const string StorePreviewName = "store-preview";

        /// <summary>Name of the label showing the store key.</summary>
        public const string StoreKeyName = "store-key";

        /// <summary>USS class added to the store panel while storing is locked.</summary>
        public const string StoreLockedClass = "hud-store--locked";

        /// <summary>Name of the label showing the run timer (above STORE).</summary>
        public const string TimerValueName = "timer-value";

        /// <summary>Name of the win panel.</summary>
        public const string WinPanelName = "win";

        /// <summary>Name of the label on the win panel showing the final time.</summary>
        public const string WinTimeName = "win-time";

        /// <summary>Name of the label on the win panel showing the final score.</summary>
        public const string WinScoreName = "win-score";

        /// <summary>Name of the win panel button that starts a new level.</summary>
        public const string NewLevelButtonName = "win-new-level";

        /// <summary>Name of the win panel button that loads the main menu.</summary>
        public const string MainMenuButtonName = "win-main-menu";

        /// <summary>Name of the "Clear the dirt!" message shown in the middle of the screen while the level start drops in.</summary>
        public const string LevelIntroName = "level-intro";

        /// <summary>Name of the game over panel button that starts a new run.</summary>
        public const string RestartButtonName = "game-over-restart";

        /// <summary>Name of the game over panel button that loads the main menu.</summary>
        public const string GameOverMainMenuButtonName = "game-over-main-menu";

        /// <summary>
        /// USS class on the HUD root in the portrait touch layout: STORE above TIME, no key hints, panels placed in
        /// fixed slots (<see cref="SetPanelSlots"/>) instead of beside the board.
        /// </summary>
        public const string PortraitClass = "hud-root--portrait";

        private const int PortraitLayoutVersion = int.MinValue;

        [SerializeField, Tooltip("Game the HUD shows.")]
        private BlocksGame _game;

        [SerializeField, Tooltip("Scene the win panel's Main menu button loads. Must be in Build Settings.")]
        private string _mainMenuScene = "MainMenu";

        private Label _timerValue;
        private Label _winTime;
        private Label _winScore;
        private VisualElement _winPanel;
        private Button _newLevelButton;
        private Button _mainMenuButton;
        private Button _restartButton;
        private Button _gameOverMainMenuButton;
        private bool _portrait;
        private bool _hasSlots;
        private Rect _storeSlot;
        private Rect _statsSlot;
        private int _shownSeconds = -1;
        private VisualElement _levelIntro;
        private bool _levelIntroShown;
        private bool _levelIntroKnown;

        [SerializeField, Min(0f), Tooltip("Gap in UI pixels (at 1920 x 1080) between the board's edge and the STORE panel on the left and the SCORE / NEXT column on the right.")]
        private float _sideGap = 32f;

        private VisualElement _root;
        private VisualElement _statsPanel;
        private BoardView _boardView;
        private int _placedLayoutVersion = -1;
        private Vector2 _placedPanelSize;

        private UIDocument _document;
        private Label _scoreValue;
        private Label _finalScore;
        private int _shownScore = -1;
        private VisualElement _gameOverPanel;
        private VisualElement _storePanel;
        private Label _storeKey;
        private PiecePreviewView _nextPreview;
        private PiecePreviewView _storePreview;
        private BlocksGameState _shownState;
        private bool _stateShown;
        private bool _shownCanStore;
        private bool _piecesShown;

        /// <summary>True while the game over panel is visible.</summary>
        public bool IsGameOverShown => _gameOverPanel != null && _gameOverPanel.resolvedStyle.display == DisplayStyle.Flex;

        /// <summary>True while the win panel is visible.</summary>
        public bool IsWinShown => _winPanel != null && _winPanel.resolvedStyle.display == DisplayStyle.Flex;

        /// <summary>True while the "Clear the dirt!" level start message is visible.</summary>
        public bool IsLevelIntroShown => _levelIntro != null && _levelIntro.resolvedStyle.display == DisplayStyle.Flex;

        /// <summary>The piece shown in the next box.</summary>
        public PaintedPiece ShownNextPiece => _nextPreview?.Shown;

        /// <summary>The piece shown in the store box.</summary>
        public PaintedPiece ShownStoredPiece => _storePreview?.Shown;

        /// <summary>True while the store panel is dimmed because storing is locked.</summary>
        public bool IsStoreDimmed => _storePanel != null && _storePanel.ClassListContains(StoreLockedClass);

        /// <summary>True while the portrait touch layout is on (<see cref="SetPortraitLayout"/>).</summary>
        public bool IsPortrait => _portrait;

        /// <summary>
        /// Switches the portrait touch layout on or off. Off puts the STORE and SCORE / NEXT panels back beside the
        /// board.
        /// </summary>
        public void SetPortraitLayout(bool portrait)
        {
            _portrait = portrait;
            _root?.EnableInClassList(PortraitClass, portrait);
            if (!portrait)
            {
                _hasSlots = false;
                ClearSlotStyles();
            }

            _placedLayoutVersion = -1;
        }

        /// <summary>
        /// Portrait layout only: places the STORE panel at the top left of <paramref name="storeSlot"/> and the
        /// SCORE / NEXT panel at the top right of <paramref name="statsSlot"/>, both in panel coordinates.
        /// </summary>
        public void SetPanelSlots(Rect storeSlot, Rect statsSlot)
        {
            _hasSlots = true;
            _storeSlot = storeSlot;
            _statsSlot = statsSlot;
            _placedLayoutVersion = -1;
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement root = _document.rootVisualElement;
            _scoreValue = root.Q<Label>(ScoreValueName);
            _finalScore = root.Q<Label>(FinalScoreName);
            _shownScore = -1;
            _gameOverPanel = root.Q<VisualElement>(GameOverPanelName);
            _storePanel = root.Q<VisualElement>(StorePanelName);
            _statsPanel = root.Q<VisualElement>(StatsPanelName);
            _root = root;
            _boardView = _game != null ? _game.GetComponent<BoardView>() : null;
            _placedLayoutVersion = -1;
            _nextPreview = new PiecePreviewView(root.Q<VisualElement>(NextPreviewName));
            _storePreview = new PiecePreviewView(root.Q<VisualElement>(StorePreviewName));
            _stateShown = false;
            _piecesShown = false;

            _storeKey = root.Q<Label>(StoreKeyName);
            if (_game != null && _game.GameInput != null)
            {
                _game.GameInput.BindingsChanged += RefreshStoreKey;
            }

            RefreshStoreKey();

            _timerValue = root.Q<Label>(TimerValueName);
            _winTime = root.Q<Label>(WinTimeName);
            _winScore = root.Q<Label>(WinScoreName);
            _winPanel = root.Q<VisualElement>(WinPanelName);
            _shownSeconds = -1;
            _levelIntro = root.Q<VisualElement>(LevelIntroName);
            _levelIntroKnown = false;
            _newLevelButton = root.Q<Button>(NewLevelButtonName);
            _mainMenuButton = root.Q<Button>(MainMenuButtonName);
            if (_newLevelButton != null)
            {
                _newLevelButton.clicked += OnNewLevelClicked;
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.clicked += OnMainMenuClicked;
            }

            _restartButton = root.Q<Button>(RestartButtonName);
            _gameOverMainMenuButton = root.Q<Button>(GameOverMainMenuButtonName);
            if (_restartButton != null)
            {
                _restartButton.clicked += OnNewLevelClicked;
            }

            if (_gameOverMainMenuButton != null)
            {
                _gameOverMainMenuButton.clicked += OnMainMenuClicked;
            }

            _root.EnableInClassList(PortraitClass, _portrait);
        }

        private void OnNewLevelClicked()
        {
            if (_game != null)
            {
                _game.StartRun();
            }
        }

        private void OnMainMenuClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(_mainMenuScene);
        }

        private void OnDisable()
        {
            if (_newLevelButton != null)
            {
                _newLevelButton.clicked -= OnNewLevelClicked;
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.clicked -= OnMainMenuClicked;
            }

            if (_restartButton != null)
            {
                _restartButton.clicked -= OnNewLevelClicked;
            }

            if (_gameOverMainMenuButton != null)
            {
                _gameOverMainMenuButton.clicked -= OnMainMenuClicked;
            }

            if (_game != null && _game.GameInput != null)
            {
                _game.GameInput.BindingsChanged -= RefreshStoreKey;
            }
        }

        private void RefreshStoreKey()
        {
            if (_storeKey == null || _game == null || _game.GameInput == null)
            {
                return;
            }

            string keys = _game.GameInput.StorePieceBindingText;
            if (!string.IsNullOrEmpty(keys))
            {
                _storeKey.text = keys;
            }
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            UpdatePlacement();
            UpdateScore();
            UpdateTimer();
            UpdateState();
            UpdatePieces();
            UpdateLevelIntro();
        }

        private void UpdateLevelIntro()
        {
            bool show = _game.IsStartingFill;
            if (_levelIntroKnown && show == _levelIntroShown)
            {
                return;
            }

            _levelIntroKnown = true;
            _levelIntroShown = show;
            if (_levelIntro != null)
            {
                _levelIntro.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void UpdatePlacement()
        {
            if (_portrait)
            {
                PlaceInSlots();
                return;
            }

            if (_boardView == null || _boardView.ViewCamera == null || _root?.panel == null)
            {
                return;
            }

            var panelSize = new Vector2(_root.layout.width, _root.layout.height);
            if (float.IsNaN(panelSize.x) || panelSize.x <= 0f)
            {
                return;
            }

            if (_placedLayoutVersion == _boardView.LayoutVersion && _placedPanelSize == panelSize)
            {
                return;
            }

            _placedLayoutVersion = _boardView.LayoutVersion;
            _placedPanelSize = panelSize;
            Rect board = _boardView.BoardWorldRect;
            Camera camera = _boardView.ViewCamera;
            Vector2 topLeft = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, new Vector3(board.xMin, board.yMax, 0f), camera);
            Vector2 topRight = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, new Vector3(board.xMax, board.yMax, 0f), camera);

            if (_storePanel != null)
            {
                _storePanel.style.right = panelSize.x - topLeft.x + _sideGap;
                _storePanel.style.top = topLeft.y;
            }

            if (_statsPanel != null)
            {
                _statsPanel.style.left = topRight.x + _sideGap;
                _statsPanel.style.top = topRight.y;
            }
        }

        private void PlaceInSlots()
        {
            if (!_hasSlots || _placedLayoutVersion == PortraitLayoutVersion || _root?.panel == null)
            {
                return;
            }

            float panelWidth = _root.layout.width;
            if (float.IsNaN(panelWidth) || panelWidth <= 0f)
            {
                return;
            }

            _placedLayoutVersion = PortraitLayoutVersion;
            if (_storePanel != null)
            {
                _storePanel.style.left = _storeSlot.xMin;
                _storePanel.style.right = StyleKeyword.Auto;
                _storePanel.style.top = _storeSlot.yMin;
            }

            if (_statsPanel != null)
            {
                _statsPanel.style.left = StyleKeyword.Auto;
                _statsPanel.style.right = panelWidth - _statsSlot.xMax;
                _statsPanel.style.top = _statsSlot.yMin;
            }
        }

        private void ClearSlotStyles()
        {
            if (_storePanel != null)
            {
                _storePanel.style.left = StyleKeyword.Null;
            }

            if (_statsPanel != null)
            {
                _statsPanel.style.right = StyleKeyword.Null;
            }
        }

        private void UpdateScore()
        {
            if (_game.Score == _shownScore)
            {
                return;
            }

            _shownScore = _game.Score;
            string text = _shownScore.ToString();
            if (_scoreValue != null)
            {
                _scoreValue.text = text;
            }

            if (_finalScore != null)
            {
                _finalScore.text = text;
            }

            if (_winScore != null)
            {
                _winScore.text = text;
            }
        }

        private void UpdateTimer()
        {
            int seconds = RunClock.WholeSeconds(_game.RunTime);
            if (seconds == _shownSeconds)
            {
                return;
            }

            _shownSeconds = seconds;
            string text = RunClock.Format(seconds);
            if (_timerValue != null)
            {
                _timerValue.text = text;
            }

            if (_winTime != null)
            {
                _winTime.text = text;
            }
        }

        private void UpdateState()
        {
            if (_stateShown && _game.State == _shownState)
            {
                return;
            }

            _stateShown = true;
            _shownState = _game.State;
            if (_gameOverPanel != null)
            {
                _gameOverPanel.style.display = _shownState == BlocksGameState.GameOver ? DisplayStyle.Flex : DisplayStyle.None;
            }

            bool won = _shownState == BlocksGameState.Won;
            if (_winPanel != null)
            {
                _winPanel.style.display = won ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (won)
            {
                _newLevelButton?.Focus();
            }
        }

        private void UpdatePieces()
        {
            PaintedPiece next = _game.NextPiece;
            if (!_piecesShown || next != _nextPreview.Shown)
            {
                _nextPreview.Show(next);
            }

            PaintedPiece stored = _game.StoredPiece;
            if (!_piecesShown || stored != _storePreview.Shown)
            {
                _storePreview.Show(stored);
            }

            bool canStore = _game.CanStore;
            if (!_piecesShown || canStore != _shownCanStore)
            {
                _shownCanStore = canStore;
                _storePanel?.EnableInClassList(StoreLockedClass, !canStore);
            }

            _piecesShown = true;
        }
    }
}
