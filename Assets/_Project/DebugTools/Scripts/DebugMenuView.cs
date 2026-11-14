using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace BlockDiggerz.DebugTools
{
    /// <summary>
    /// Shows the debug menu: one foldout per section registered with <see cref="DebugMenu"/>.
    /// Lives on the DebugMenu prefab next to a UIDocument (source asset DebugMenu.uxml). The game keeps running
    /// while the menu is open. Removes itself in release builds.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DebugMenuView : MonoBehaviour
    {
        private const string PanelName = "debug-panel";
        private const string SectionListName = "debug-sections";
        private const string SectionClass = "debug-section";
        private const string ButtonClass = "debug-button";
        private const string ReadoutClass = "debug-readout";
        private const string EmptyMessage = "No debug controls registered in this scene.";

        [SerializeField, Tooltip("Action that opens and closes the menu (Debug/ToggleDebugMenu in GameControls). " +
                                 "Keep it in a map that is never disabled.")]
        private InputActionReference _toggleAction;

        [SerializeField, Tooltip("Start with the menu open.")]
        private bool _startOpen;

        [SerializeField, Min(0.05f), Tooltip("Seconds between refreshes of readouts and bound values.")]
        private float _refreshInterval = 0.25f;

#if UNITY_EDITOR || DEBUG
        private readonly List<System.Action> _refreshers = new();
        private UIDocument _document;
        private VisualElement _panel;
        private ScrollView _sectionList;
        private InputAction _boundAction;
        private float _nextRefresh;
        private bool _rebuildPending = true;

        /// <summary>True while the menu is shown.</summary>
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement root = _document.rootVisualElement;
            _panel = root.Q<VisualElement>(PanelName);
            _sectionList = root.Q<ScrollView>(SectionListName);

            DebugMenu.SectionsChanged += OnSectionsChanged;

            if (_toggleAction != null && _toggleAction.action != null)
            {
                _boundAction = _toggleAction.action;
                _boundAction.performed += OnToggle;
                _boundAction.Enable();
            }

            SetOpen(_startOpen);
        }

        private void OnDisable()
        {
            DebugMenu.SectionsChanged -= OnSectionsChanged;
            if (_boundAction == null)
            {
                return;
            }

            _boundAction.performed -= OnToggle;
            _boundAction = null;
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (_rebuildPending)
            {
                Rebuild();
            }

            if (Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + _refreshInterval;
                foreach (System.Action refresh in _refreshers)
                {
                    refresh();
                }
            }
        }

        /// <summary>Shows or hides the menu.</summary>
        public void SetOpen(bool open)
        {
            IsOpen = open;
            if (_panel == null)
            {
                return;
            }

            _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (open)
            {
                Rebuild();
                _sectionList?.Q<Foldout>()?.Focus();
            }
        }

        private void OnToggle(InputAction.CallbackContext context)
        {
            SetOpen(!IsOpen);
        }

        private void OnSectionsChanged()
        {
            _rebuildPending = true;
        }

        private void Rebuild()
        {
            _rebuildPending = false;
            if (_sectionList == null)
            {
                return;
            }

            _sectionList.Clear();
            _refreshers.Clear();

            var foldouts = new Dictionary<string, Foldout>();
            foreach (DebugSection section in DebugMenu.Sections)
            {
                if (!foldouts.TryGetValue(section.Title, out Foldout foldout))
                {
                    foldout = new Foldout { text = section.Title, value = false };
                    foldout.AddToClassList(SectionClass);
                    foldouts.Add(section.Title, foldout);
                    _sectionList.Add(foldout);
                }

                foreach (DebugControl control in section.Controls)
                {
                    foldout.Add(CreateControl(control));
                }
            }

            if (DebugMenu.Sections.Count == 0)
            {
                _sectionList.Add(new Label(EmptyMessage));
            }
        }

        private VisualElement CreateControl(DebugControl control)
        {
            switch (control.Kind)
            {
                case DebugControlKind.Button:
                    return CreateButton(control);
                case DebugControlKind.Toggle:
                    return CreateToggle(control);
                case DebugControlKind.Slider:
                    return CreateSlider(control);
                default:
                    return CreateReadout(control);
            }
        }

        private static VisualElement CreateButton(DebugControl control)
        {
            var button = new Button(() => control.OnClick?.Invoke()) { text = control.Label };
            button.AddToClassList(ButtonClass);
            return button;
        }

        private VisualElement CreateToggle(DebugControl control)
        {
            var toggle = new Toggle(control.Label);
            toggle.SetValueWithoutNotify(control.GetBool?.Invoke() ?? false);
            toggle.RegisterValueChangedCallback(e => control.SetBool?.Invoke(e.newValue));
            _refreshers.Add(() => toggle.SetValueWithoutNotify(control.GetBool?.Invoke() ?? false));
            return toggle;
        }

        private VisualElement CreateSlider(DebugControl control)
        {
            var slider = new Slider(control.Label, control.Min, control.Max) { showInputField = true };
            slider.SetValueWithoutNotify(control.GetFloat?.Invoke() ?? control.Min);
            slider.RegisterValueChangedCallback(e => control.SetFloat?.Invoke(e.newValue));
            _refreshers.Add(() =>
            {
                if (slider.focusController?.focusedElement is VisualElement focused && slider.Contains(focused))
                {
                    return;
                }

                slider.SetValueWithoutNotify(control.GetFloat?.Invoke() ?? control.Min);
            });
            return slider;
        }

        private VisualElement CreateReadout(DebugControl control)
        {
            var row = new Label();
            row.AddToClassList(ReadoutClass);
            void Refresh() => row.text = $"{control.Label}: {control.GetText?.Invoke()}";
            Refresh();
            _refreshers.Add(Refresh);
            return row;
        }
#else
        private void Awake()
        {
            Destroy(gameObject);
        }
#endif
    }
}
