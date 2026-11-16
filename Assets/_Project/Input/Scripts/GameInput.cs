using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Input
{
    /// <summary>
    /// The single wrapper the game reads input through. Each action is an <see cref="InputActionReference"/> into
    /// the GameControls actions asset, so default bindings are edited in that asset and code only asks questions
    /// such as "is move left held" or "was hard drop pressed this frame". Gameplay actions can be switched off
    /// (for menus) while the pause action stays on.
    /// </summary>
    public sealed class GameInput : MonoBehaviour
    {
        private const string BindingTextSeparator = " | ";

        /// <summary>Binding group of the on-screen touch controls (<see cref="TouchPadDevice"/>); left out of key hints.</summary>
        public const string TouchGroup = "Touch";

        /// <summary>Touch control mode before the player picks one in Settings.</summary>
        public const TouchControlMode DefaultTouchMode = TouchControlMode.Gestures;

        [SerializeField, Tooltip("Held to move the piece left (auto repeats after a delay).")]
        private InputActionReference _moveLeft;

        [SerializeField, Tooltip("Held to move the piece right (auto repeats after a delay).")]
        private InputActionReference _moveRight;

        [SerializeField, Tooltip("Held to make the piece fall faster.")]
        private InputActionReference _softDrop;

        [SerializeField, Tooltip("Pressed to drop the piece to the bottom and lock it at once.")]
        private InputActionReference _hardDrop;

        [SerializeField, Tooltip("Pressed to rotate the piece clockwise.")]
        private InputActionReference _rotateClockwise;

        [SerializeField, Tooltip("Pressed to rotate the piece counter-clockwise.")]
        private InputActionReference _rotateCounterClockwise;

        [SerializeField, Tooltip("Pressed to store the falling piece, or swap it with the stored one.")]
        private InputActionReference _storePiece;

        [SerializeField, Tooltip("Pressed on the game over screen to start a new run.")]
        private InputActionReference _restart;

        [SerializeField, Tooltip("Pressed to open or close the pause menu, and to go back in menus. Not rebindable.")]
        private InputActionReference _pause;

        private static int _activeInstances;
        private readonly List<InputAction> _gameplayActions = new();

        /// <summary>Raised after bindings change (rebinding or reset), so key hints can refresh.</summary>
        public event Action BindingsChanged;

        /// <summary>How the touch controls work (gestures or buttons). The menus apply the saved choice.</summary>
        public TouchControlMode TouchMode { get; private set; } = DefaultTouchMode;

        /// <summary>
        /// True when the touch layout can be in use: on mobile platforms, or while the touch pad device is
        /// connected (touch controls forced on for testing).
        /// </summary>
        public static bool TouchLayoutInUse =>
            UnityEngine.Device.Application.isMobilePlatform || InputSystem.GetDevice<TouchPadDevice>() != null;

        /// <summary>True while the move left action is held.</summary>
        public bool MoveLeftHeld => IsHeld(_moveLeft);

        /// <summary>True while the move right action is held.</summary>
        public bool MoveRightHeld => IsHeld(_moveRight);

        /// <summary>True while the soft drop action is held.</summary>
        public bool SoftDropHeld => IsHeld(_softDrop);

        /// <summary>True on the frame the hard drop action was pressed.</summary>
        public bool HardDropPressed => WasPressed(_hardDrop);

        /// <summary>True on the frame the rotate clockwise action was pressed.</summary>
        public bool RotateClockwisePressed => WasPressed(_rotateClockwise);

        /// <summary>True on the frame the rotate counter-clockwise action was pressed.</summary>
        public bool RotateCounterClockwisePressed => WasPressed(_rotateCounterClockwise);

        /// <summary>True on the frame the store piece action was pressed.</summary>
        public bool StorePiecePressed => WasPressed(_storePiece);

        /// <summary>
        /// Display text of the store piece bindings (for example "Space | LB"), for UI hints. Unbound key slots are
        /// left out.
        /// </summary>
        public string StorePieceBindingText => BindingText(_storePiece);

        /// <summary>True on the frame the restart action was pressed.</summary>
        public bool RestartPressed => WasPressed(_restart);

        /// <summary>True on the frame the pause action was pressed.</summary>
        public bool PausePressed => WasPressed(_pause);

        /// <summary>
        /// Gameplay actions in display order; the controls page lets players rebind these. Refilled on demand
        /// because a script recompile during Play mode empties the list without calling Awake again.
        /// </summary>
        public IReadOnlyList<InputAction> GameplayActions
        {
            get
            {
                if (_gameplayActions.Count == 0)
                {
                    CollectGameplayActions();
                }

                return _gameplayActions;
            }
        }

        /// <summary>The actions asset every action belongs to; binding overrides are saved from and loaded into it.</summary>
        public InputActionAsset Actions => _pause != null && _pause.action != null ? _pause.action.actionMap.asset : null;

        /// <summary>False while menus have switched gameplay actions off.</summary>
        public bool GameplayEnabled { get; private set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _activeInstances = 0;
        }

        private void Awake()
        {
            CollectGameplayActions();
        }

        private void CollectGameplayActions()
        {
            _gameplayActions.Clear();
            AddGameplay(_moveLeft);
            AddGameplay(_moveRight);
            AddGameplay(_softDrop);
            AddGameplay(_hardDrop);
            AddGameplay(_rotateClockwise);
            AddGameplay(_rotateCounterClockwise);
            AddGameplay(_storePiece);
            AddGameplay(_restart);
        }

        /// <summary>
        /// The actions live in a shared asset, and on a scene load the new instance is enabled before the old one is
        /// disabled. Counting instances keeps the old one from switching off the actions the new one relies on.
        /// </summary>
        private void OnEnable()
        {
            _activeInstances++;
            GameplayEnabled = true;
            SetGameplayActions(true);
            SetEnabled(_pause, true);
        }

        private void OnDisable()
        {
            _activeInstances = Mathf.Max(0, _activeInstances - 1);
            if (_activeInstances == 0)
            {
                SetGameplayActions(false);
                SetEnabled(_pause, false);
            }
        }

        /// <summary>Switches gameplay actions on or off; the pause action is not affected.</summary>
        public void SetGameplayEnabled(bool enabled)
        {
            GameplayEnabled = enabled;
            SetGameplayActions(enabled);
        }

        /// <summary>Call after changing binding overrides so listeners refresh their key hints.</summary>
        public void NotifyBindingsChanged()
        {
            BindingsChanged?.Invoke();
        }

        /// <summary>Sets <see cref="TouchMode"/>; the touch controls switch layout on their next frame.</summary>
        public void SetTouchMode(TouchControlMode mode)
        {
            TouchMode = mode;
        }

        private void AddGameplay(InputActionReference reference)
        {
            if (reference != null && reference.action != null)
            {
                _gameplayActions.Add(reference.action);
            }
        }

        private void SetGameplayActions(bool enabled)
        {
            SetEnabled(_moveLeft, enabled);
            SetEnabled(_moveRight, enabled);
            SetEnabled(_softDrop, enabled);
            SetEnabled(_hardDrop, enabled);
            SetEnabled(_rotateClockwise, enabled);
            SetEnabled(_rotateCounterClockwise, enabled);
            SetEnabled(_storePiece, enabled);
            SetEnabled(_restart, enabled);
        }

        private static void SetEnabled(InputActionReference reference, bool enabled)
        {
            if (reference == null || reference.action == null)
            {
                return;
            }

            if (enabled)
            {
                reference.action.Enable();
            }
            else
            {
                reference.action.Disable();
            }
        }

        private static bool IsHeld(InputActionReference reference)
        {
            return reference != null && reference.action != null && reference.action.IsPressed();
        }

        private static bool WasPressed(InputActionReference reference)
        {
            return reference != null && reference.action != null && reference.action.WasPressedThisFrame();
        }

        private static bool IsTouchBinding(InputBinding binding)
        {
            return !string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(TouchGroup);
        }

        private static string BindingText(InputActionReference reference)
        {
            if (reference == null || reference.action == null)
            {
                return string.Empty;
            }

            InputAction action = reference.action;
            var parts = new List<string>();
            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                if (binding.isPartOfComposite || string.IsNullOrEmpty(binding.effectivePath) || IsTouchBinding(binding))
                {
                    continue;
                }

                parts.Add(action.GetBindingDisplayString(i));
            }

            return string.Join(BindingTextSeparator, parts);
        }
    }
}
