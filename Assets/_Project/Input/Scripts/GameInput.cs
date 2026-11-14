using UnityEngine;
using UnityEngine.InputSystem;

namespace BlockDiggerz.Input
{
    /// <summary>
    /// The single wrapper the game reads input through. Each gameplay action is an
    /// <see cref="InputActionReference"/> into the GameControls actions asset, so bindings are edited in that asset
    /// and code only asks questions such as "is move left held" or "was hard drop pressed this frame".
    /// </summary>
    public sealed class GameInput : MonoBehaviour
    {
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

        /// <summary>Display text of the store piece bindings (for example "Space"), for UI hints.</summary>
        public string StorePieceBindingText =>
            _storePiece != null && _storePiece.action != null ? _storePiece.action.GetBindingDisplayString() : string.Empty;

        /// <summary>True on the frame the restart action was pressed.</summary>
        public bool RestartPressed => WasPressed(_restart);

        private static int _activeInstances;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _activeInstances = 0;
        }

        /// <summary>
        /// The actions live in a shared asset, and on a scene load the new instance is enabled before the old one is
        /// disabled. Counting instances keeps the old one from switching off the actions the new one relies on.
        /// </summary>
        private void OnEnable()
        {
            _activeInstances++;
            SetEnabled(true);
        }

        private void OnDisable()
        {
            _activeInstances = Mathf.Max(0, _activeInstances - 1);
            if (_activeInstances == 0)
            {
                SetEnabled(false);
            }
        }

        private void SetEnabled(bool enabled)
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
    }
}
