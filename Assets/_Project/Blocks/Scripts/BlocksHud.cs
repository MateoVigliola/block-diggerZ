using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// UI Toolkit HUD for the block dropper: the store panel (just left of the board), score and next piece
    /// (just right of the board, both following the board as it moves or resizes) and the game over panel.
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

        [SerializeField, Tooltip("Game the HUD shows.")]
        private BlocksGame _game;

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

        /// <summary>The piece shown in the next box.</summary>
        public PaintedPiece ShownNextPiece => _nextPreview?.Shown;

        /// <summary>The piece shown in the store box.</summary>
        public PaintedPiece ShownStoredPiece => _storePreview?.Shown;

        /// <summary>True while the store panel is dimmed because storing is locked.</summary>
        public bool IsStoreDimmed => _storePanel != null && _storePanel.ClassListContains(StoreLockedClass);

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
        }

        private void OnDisable()
        {
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
            UpdateState();
            UpdatePieces();
        }

        private void UpdatePlacement()
        {
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
