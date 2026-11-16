using System.Collections.Generic;
using BlockDiggerz.Blocks;
using BlockDiggerz.Input;
using UnityEngine;
using UnityEngine.UIElements;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Portrait touch controls for mobile browsers: the left and right halves of the board area move the piece
    /// (held, with the keyboard's auto repeat), CCW / HARD DROP / CW under the board, STORE in the top left and
    /// pause in the top right. They press buttons of a virtual <see cref="TouchPadDevice"/>, so the game reads them
    /// through <see cref="GameInput"/> like keys. While active the board spans the full width of the board area:
    /// the visible height is cut to the rows that fit (<see cref="MobileBoardRows"/>, once per activation, which
    /// restarts the run), the camera is fitted to the board, and the HUD switches to its portrait layout. Shown only
    /// on mobile platforms (<see cref="IsMobilePlatform"/>) or when forced for testing. Hidden while the game is
    /// not playing or gameplay input is off (pause menu). Element names match MobileControls.uxml.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TouchControls : MonoBehaviour
    {
        /// <summary>Name of the root element.</summary>
        public const string RootName = "mobile-root";

        /// <summary>Name of the left half of the board area (move left).</summary>
        public const string MoveLeftName = "move-left";

        /// <summary>Name of the right half of the board area (move right).</summary>
        public const string MoveRightName = "move-right";

        /// <summary>Name of the hard drop button.</summary>
        public const string HardDropName = "hard-drop";

        /// <summary>Name of the rotate clockwise button.</summary>
        public const string RotateClockwiseName = "rotate-cw";

        /// <summary>Name of the rotate counter-clockwise button.</summary>
        public const string RotateCounterClockwiseName = "rotate-ccw";

        /// <summary>Name of the STORE tap area in the top left; the HUD's STORE panel is placed in it.</summary>
        public const string StoreSlotName = "store-slot";

        /// <summary>Name of the pause button in the top right.</summary>
        public const string PauseName = "pause-button";

        /// <summary>Name of the area under the pause button the HUD's SCORE / NEXT panel is placed in.</summary>
        public const string StatsSlotName = "stats-slot";

        /// <summary>Name of the area the board is fitted into; it holds the two move halves.</summary>
        public const string BoardAreaName = "board-area";

        /// <summary>
        /// Name of the full-screen "turn your phone upright" message shown on phones held sideways. It sits outside
        /// the root, so it stays up while the controls are hidden under the pause menu.
        /// </summary>
        public const string RotateOverlayName = "rotate-overlay";

        [SerializeField, Tooltip("Game the controls move pieces in.")]
        private BlocksGame _game;

        [SerializeField, Tooltip("HUD switched to its portrait layout while the touch controls are active.")]
        private BlocksHud _hud;

        [SerializeField, Tooltip("Board view; its camera is fitted so the board fills the board area.")]
        private BoardView _boardView;

        [SerializeField, Tooltip("Input wrapper; the controls hide while it has gameplay input off (pause menu).")]
        private GameInput _input;

        [SerializeField, Tooltip("Show the touch controls on desktop too, for testing in the Editor or Device Simulator. The debug menu toggles this.")]
        private bool _forceShow;

        [SerializeField, Min(0f), Tooltip("Empty space in world units (1 = one cell) kept around the board when it is fitted into the board area. 0 lets the board touch both screen edges.")]
        private float _boardPadding;

        [SerializeField, Min(BoardLayout.MinHeight), Tooltip("Fewest visible rows on the touch layout. On short screens the board keeps this many rows and gets narrower than the screen instead.")]
        private int _minMobileRows = 12;

        [SerializeField, Tooltip("On phones, ask the browser to lock the screen to portrait when the controls turn on and when the game goes fullscreen. Browsers only allow it in fullscreen, and iOS Safari never does; the rotate message covers the rest.")]
        private bool _requestPortrait = true;

        [SerializeField, Tooltip("On phones held sideways, cover the screen with the rotate message and pause the game.")]
        private bool _guardPortrait = true;

        [SerializeField, Tooltip("Where the board sits in the board area when it doesn't fill it: x 0 left to 1 right, y 0 bottom to 1 top. The default centers it.")]
        private Vector2 _boardAlignment = new(0.5f, 0.5f);

        private readonly TouchPad _touchPad = new();
        private readonly List<TouchPressManipulator> _presses = new();
        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _boardArea;
        private VisualElement _storeSlot;
        private VisualElement _statsSlot;
        private VisualElement _rotateOverlay;
        private bool _active;
        private bool _mobileSizeApplied;
        private bool _landscape;
        private bool _landscapeKnown;
        private bool _wasFullScreen;
        private bool _pauseReleasePending;
        private bool _shown;
        private bool _shownKnown;
        private bool _layoutDirty;
        private int _fittedLayoutVersion = -1;
        private Vector2 _fittedViewport;
        private Rect _boardPanelRect;
        private bool _cameraSaved;
        private float _savedOrthographicSize;
        private Vector3 _savedCameraPosition;

        /// <summary>
        /// True on phones and tablets, including mobile web browsers. Reads the Device Simulator's device in the
        /// Editor.
        /// </summary>
        public static bool IsMobilePlatform => UnityEngine.Device.Application.isMobilePlatform;

        /// <summary>Show the controls on desktop too (testing). Takes effect on the next frame.</summary>
        public bool ForceShow
        {
            get => _forceShow;
            set => _forceShow = value;
        }

        /// <summary>True while the touch layout is on (mobile platform or forced).</summary>
        public bool IsActive => _active;

        /// <summary>True while the controls are visible and take touches.</summary>
        public bool IsShown => _active && _shown;

        /// <summary>The virtual device the controls press.</summary>
        public TouchPad TouchPad => _touchPad;

        /// <summary>True while the rotate message covers the screen.</summary>
        public bool IsRotateMessageShown => _active && _landscapeKnown && _landscape && UsesPortraitGuard;

        private bool UsesPortraitGuard => _guardPortrait && IsMobilePlatform;

        /// <summary>The board's rectangle in panel coordinates after the last fit.</summary>
        public Rect BoardPanelRect => _boardPanelRect;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _root = _document.rootVisualElement.Q<VisualElement>(RootName);
            _boardArea = _document.rootVisualElement.Q<VisualElement>(BoardAreaName);
            _storeSlot = _document.rootVisualElement.Q<VisualElement>(StoreSlotName);
            _statsSlot = _document.rootVisualElement.Q<VisualElement>(StatsSlotName);
            _rotateOverlay = _document.rootVisualElement.Q<VisualElement>(RotateOverlayName);

            AddPress(MoveLeftName, TouchPadButton.MoveLeft);
            AddPress(MoveRightName, TouchPadButton.MoveRight);
            AddPress(HardDropName, TouchPadButton.HardDrop);
            AddPress(RotateClockwiseName, TouchPadButton.RotateClockwise);
            AddPress(RotateCounterClockwiseName, TouchPadButton.RotateCounterClockwise);
            AddPress(StoreSlotName, TouchPadButton.StorePiece);
            AddPress(PauseName, TouchPadButton.Pause);

            _boardArea?.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            _active = false;
            _shownKnown = false;
            SetRootDisplay(false);
        }

        private void OnDisable()
        {
            Deactivate(false);
            _boardArea?.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            foreach (TouchPressManipulator press in _presses)
            {
                press.target?.RemoveManipulator(press);
            }

            _presses.Clear();
        }

        private void AddPress(string elementName, TouchPadButton button)
        {
            VisualElement element = _document.rootVisualElement.Q<VisualElement>(elementName);
            if (element == null)
            {
                return;
            }

            var press = new TouchPressManipulator(_touchPad, button);
            element.AddManipulator(press);
            _presses.Add(press);
        }

        private void Update()
        {
            bool wantActive = IsMobilePlatform || _forceShow;
            if (wantActive != _active)
            {
                if (wantActive)
                {
                    Activate();
                }
                else
                {
                    Deactivate(true);
                }
            }

            if (!_active)
            {
                return;
            }

            UpdateShown();
            UpdateOrientation();
            UpdateFit();
        }

        private void Activate()
        {
            _active = true;
            _touchPad.Connect();
            SaveCamera();
            SetHudPortrait(true);
            SetRootDisplay(true);
            _shownKnown = false;
            _layoutDirty = true;
            _landscapeKnown = false;
            _wasFullScreen = Screen.fullScreen;
            RequestPortrait();
        }

        /// <summary>
        /// Turns the touch layout off. <paramref name="restoreBoardSize"/> puts the rules' visible height back
        /// (restarting the run); it is false while disabling, so tearing down the scene never restarts a run.
        /// </summary>
        private void Deactivate(bool restoreBoardSize)
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            ReleaseAll();
            _touchPad.Disconnect();
            RestoreCamera();
            if (restoreBoardSize)
            {
                RestoreBoardSize();
            }

            _mobileSizeApplied = false;
            _landscapeKnown = false;
            _pauseReleasePending = false;
            SetRotateOverlay(false);
            SetHudPortrait(false);
            SetRootDisplay(false);
        }

        private void UpdateShown()
        {
            bool show = _game != null && _game.State == BlocksGameState.Playing && (_input == null || _input.GameplayEnabled);
            if (_shownKnown && show == _shown)
            {
                return;
            }

            _shownKnown = true;
            _shown = show;
            if (!show)
            {
                ReleaseAll();
            }

            if (_root != null)
            {
                _root.style.visibility = show ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void UpdateOrientation()
        {
            if (_pauseReleasePending)
            {
                _pauseReleasePending = false;
                _touchPad.SetPressed(TouchPadButton.Pause, false);
            }

            bool fullScreen = Screen.fullScreen;
            if (fullScreen != _wasFullScreen)
            {
                _wasFullScreen = fullScreen;
                if (fullScreen)
                {
                    RequestPortrait();
                }
            }

            bool landscape = UsesPortraitGuard && PortraitGuard.IsLandscape(Screen.width, Screen.height);
            if (_landscapeKnown && landscape == _landscape)
            {
                return;
            }

            if (PortraitGuard.ShouldPause(_landscapeKnown, _landscape, landscape, IsShown))
            {
                _touchPad.SetPressed(TouchPadButton.Pause, true);
                _pauseReleasePending = true;
            }

            _landscapeKnown = true;
            _landscape = landscape;
            SetRotateOverlay(landscape);
        }

        /// <summary>
        /// Asks for a portrait lock on phones. On the Web this becomes the browser's screen orientation lock, which
        /// only takes effect in fullscreen on browsers that support it; elsewhere the request is ignored.
        /// </summary>
        private void RequestPortrait()
        {
            if (!_requestPortrait || !IsMobilePlatform)
            {
                return;
            }

            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void SetRotateOverlay(bool visible)
        {
            if (_rotateOverlay != null)
            {
                _rotateOverlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void ReleaseAll()
        {
            foreach (TouchPressManipulator press in _presses)
            {
                press.Cancel();
            }

            _touchPad.ReleaseAll();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _layoutDirty = true;
        }

        private void UpdateFit()
        {
            if (_boardView == null || _boardView.ViewCamera == null)
            {
                return;
            }

            Camera camera = _boardView.ViewCamera;
            var viewport = new Vector2(camera.pixelWidth, camera.pixelHeight);
            if (!_layoutDirty && _fittedLayoutVersion == _boardView.LayoutVersion && _fittedViewport == viewport)
            {
                return;
            }

            if (ApplyFit(camera, viewport))
            {
                _layoutDirty = false;
                _fittedLayoutVersion = _boardView.LayoutVersion;
                _fittedViewport = viewport;
            }
        }

        private bool ApplyFit(Camera camera, Vector2 viewport)
        {
            IPanel panel = _document.rootVisualElement.panel;
            if (panel == null || _boardArea == null)
            {
                return false;
            }

            Rect panelRect = panel.visualTree.layout;
            Rect area = _boardArea.worldBound;
            if (!IsUsable(panelRect) || !IsUsable(area))
            {
                return false;
            }

            if (ApplyMobileBoardSize(area))
            {
                return false;
            }

            float scaleX = viewport.x / panelRect.width;
            float scaleY = viewport.y / panelRect.height;
            var target = new Rect(area.xMin * scaleX, viewport.y - area.yMax * scaleY, area.width * scaleX, area.height * scaleY);
            Rect board = _boardView.BoardWorldRect;
            CameraFit fit = BoardFit.Fit(viewport, target, board, _boardPadding, _boardAlignment);
            if (!fit.IsValid)
            {
                return false;
            }

            camera.orthographicSize = fit.OrthographicSize;
            Vector3 position = camera.transform.position;
            camera.transform.position = new Vector3(fit.Position.x, fit.Position.y, position.z);

            Vector2 topLeft = RuntimePanelUtils.CameraTransformWorldToPanel(panel, new Vector3(board.xMin, board.yMax, 0f), camera);
            Vector2 bottomRight = RuntimePanelUtils.CameraTransformWorldToPanel(panel, new Vector3(board.xMax, board.yMin, 0f), camera);
            _boardPanelRect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);

            if (_hud != null && _storeSlot != null && _statsSlot != null)
            {
                _hud.SetPanelSlots(_storeSlot.worldBound, _statsSlot.worldBound);
            }

            return true;
        }

        private static bool IsUsable(Rect rect)
        {
            return !float.IsNaN(rect.width) && !float.IsNaN(rect.height) && rect.width > 0f && rect.height > 0f;
        }

        /// <summary>
        /// Once per activation, cuts the visible height to the rows that fit the board area at full width (waiting
        /// while the phone is held sideways, so a landscape screen never picks the row count). Returns
        /// true when the board was resized (the run restarts and the fit runs again on the next frame).
        /// </summary>
        private bool ApplyMobileBoardSize(Rect area)
        {
            if (_mobileSizeApplied || _game == null || _game.Rules == null || IsRotateMessageShown)
            {
                return false;
            }

            _mobileSizeApplied = true;
            int rows = MobileBoardRows.Fit(area.width, area.height, _game.BoardWidth, _boardPadding, _minMobileRows,
                _game.Rules.VisibleHeight);
            if (rows == _game.VisibleHeight)
            {
                return false;
            }

            _game.SetBoardSize(_game.BoardWidth, rows);
            return true;
        }

        private void RestoreBoardSize()
        {
            if (_mobileSizeApplied && _game != null && _game.Rules != null)
            {
                _game.SetBoardSize(_game.BoardWidth, _game.Rules.VisibleHeight);
            }
        }

        private void SetHudPortrait(bool portrait)
        {
            if (_hud != null)
            {
                _hud.SetPortraitLayout(portrait);
            }
        }

        private void SetRootDisplay(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void SaveCamera()
        {
            Camera camera = _boardView != null ? _boardView.ViewCamera : null;
            if (camera == null || _cameraSaved)
            {
                return;
            }

            _cameraSaved = true;
            _savedOrthographicSize = camera.orthographicSize;
            _savedCameraPosition = camera.transform.position;
        }

        private void RestoreCamera()
        {
            Camera camera = _boardView != null ? _boardView.ViewCamera : null;
            if (camera == null || !_cameraSaved)
            {
                return;
            }

            _cameraSaved = false;
            camera.orthographicSize = _savedOrthographicSize;
            camera.transform.position = _savedCameraPosition;
            _fittedLayoutVersion = -1;
        }
    }
}
