using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// UI Toolkit HUD for the block dropper: the lines cleared counter and the game over panel.
    /// Element names match BlocksHud.uxml.
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

        [SerializeField, Tooltip("Game the HUD shows.")]
        private BlocksGame _game;

        private UIDocument _document;
        private Label _linesValue;
        private Label _finalLines;
        private VisualElement _gameOverPanel;
        private int _shownLines = -1;
        private BlocksGameState _shownState;
        private bool _stateShown;

        /// <summary>True while the game over panel is visible.</summary>
        public bool IsGameOverShown => _gameOverPanel != null && _gameOverPanel.resolvedStyle.display == DisplayStyle.Flex;

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
            _shownLines = -1;
            _stateShown = false;
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            if (_game.TotalLinesCleared != _shownLines)
            {
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

            if (!_stateShown || _game.State != _shownState)
            {
                _stateShown = true;
                _shownState = _game.State;
                if (_gameOverPanel != null)
                {
                    _gameOverPanel.style.display = _shownState == BlocksGameState.GameOver ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
    }
}
