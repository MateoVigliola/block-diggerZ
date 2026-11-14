using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// UI Toolkit HUD for the block dropper: the store panel (left of the board), the lines counter and next piece
    /// (right of the board) and the game over panel. Element names match BlocksHud.uxml. Updates elements only when
    /// the value they show changes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class BlocksHud : MonoBehaviour
    {
        /// <summary>Name of the label showing lines cleared.</summary>
        public const string LinesValueName = "lines-value";

        /// <summary>Name of the game over panel.</summary>
        public const string GameOverPanelName = "game-over";

        /// <summary>Name of the label on the game over panel showing the final line count.</summary>
        public const string FinalLinesName = "final-lines";

        /// <summary>Name of the next piece preview box.</summary>
        public const string NextPreviewName = "next-preview";

        /// <summary>Name of the store panel.</summary>
        public const string StorePanelName = "store";

        /// <summary>Name of the stored piece preview box.</summary>
        public const string StorePreviewName = "store-preview";

        /// <summary>Name of the label showing the store key.</summary>
        public const string StoreKeyName = "store-key";

        /// <summary>USS class added to the store panel while storing is locked.</summary>
        public const string StoreLockedClass = "hud-store--locked";

        [SerializeField, Tooltip("Game the HUD shows.")]
        private BlocksGame _game;

        private UIDocument _document;
        private Label _linesValue;
        private Label _finalLines;
        private VisualElement _gameOverPanel;
        private VisualElement _storePanel;
        private PiecePreviewView _nextPreview;
        private PiecePreviewView _storePreview;
        private int _shownLines = -1;
        private BlocksGameState _shownState;
        private bool _stateShown;
        private bool _shownCanStore;
        private bool _piecesShown;

        /// <summary>True while the game over panel is visible.</summary>
        public bool IsGameOverShown => _gameOverPanel != null && _gameOverPanel.resolvedStyle.display == DisplayStyle.Flex;

        /// <summary>The piece shown in the next box.</summary>
        public PieceDefinition ShownNextPiece => _nextPreview?.Shown;

        /// <summary>The piece shown in the store box.</summary>
        public PieceDefinition ShownStoredPiece => _storePreview?.Shown;

        /// <summary>True while the store panel is dimmed because storing is locked.</summary>
        public bool IsStoreDimmed => _storePanel != null && _storePanel.ClassListContains(StoreLockedClass);

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement root = _document.rootVisualElement;
            _linesValue = root.Q<Label>(LinesValueName);
            _finalLines = root.Q<Label>(FinalLinesName);
            _gameOverPanel = root.Q<VisualElement>(GameOverPanelName);
            _storePanel = root.Q<VisualElement>(StorePanelName);
            _nextPreview = new PiecePreviewView(root.Q<VisualElement>(NextPreviewName));
            _storePreview = new PiecePreviewView(root.Q<VisualElement>(StorePreviewName));
            _shownLines = -1;
            _stateShown = false;
            _piecesShown = false;

            Label storeKey = root.Q<Label>(StoreKeyName);
            if (storeKey != null && _game != null && _game.GameInput != null)
            {
                string keys = _game.GameInput.StorePieceBindingText;
                if (!string.IsNullOrEmpty(keys))
                {
                    storeKey.text = keys;
                }
            }
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            UpdateLines();
            UpdateState();
            UpdatePieces();
        }

        private void UpdateLines()
        {
            if (_game.TotalLinesCleared == _shownLines)
            {
                return;
            }

            _shownLines = _game.TotalLinesCleared;
            string text = _shownLines.ToString();
            if (_linesValue != null)
            {
                _linesValue.text = text;
            }

            if (_finalLines != null)
            {
                _finalLines.text = text;
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
            PieceDefinition next = _game.NextPiece;
            if (!_piecesShown || next != _nextPreview.Shown)
            {
                _nextPreview.Show(next);
            }

            PieceDefinition stored = _game.StoredPiece;
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
