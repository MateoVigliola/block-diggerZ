using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BlockDiggerz.Testing
{
    /// <summary>
    /// Base for PlayMode tests that drive the game through real input devices: state events queued on the keyboard
    /// go through the Input System and the game's actions exactly like hardware. Use code (or the debug menu) for
    /// setup only, never for the behaviour under test.
    /// Time runs at a fixed 60 frames per second of game time (Time.captureDeltaTime), so frame counts map to
    /// game seconds no matter how fast the Editor renders.
    /// InputTestFixture is not used on purpose: it resets the Input System after the game enabled its actions.
    /// </summary>
    public abstract class InputTestBase
    {
        /// <summary>Game seconds per test frame.</summary>
        protected const float FrameTime = 1f / 60f;

        private InputSettings.EditorInputBehaviorInPlayMode _savedEditorBehavior;
        private InputSettings.BackgroundBehavior _savedBackground;

        /// <summary>The keyboard the tests type on.</summary>
        protected Keyboard Keyboard { get; private set; }

        [SetUp]
        public void SetUpDevices()
        {
            _savedEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            _savedBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Keyboard = Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
            Time.timeScale = 1f;
            Time.captureDeltaTime = FrameTime;
        }

        [UnityTearDown]
        public IEnumerator TearDownDevices()
        {
            if (Keyboard != null)
            {
                InputSystem.QueueStateEvent(Keyboard, new KeyboardState());
            }

            yield return null;
            Time.timeScale = 1f;
            Time.captureDeltaTime = 0f;
            InputSystem.settings.editorInputBehaviorInPlayMode = _savedEditorBehavior;
            InputSystem.settings.backgroundBehavior = _savedBackground;
            OnTearDown();
        }

        /// <summary>Project hook: reset pause state, singletons and so on.</summary>
        protected virtual void OnTearDown()
        {
        }

        /// <summary>Presses a button through a queued state event.</summary>
        protected static void Press(ButtonControl button)
        {
            Write(button, 1f);
        }

        /// <summary>Releases a button through a queued state event.</summary>
        protected static void Release(ButtonControl button)
        {
            Write(button, 0f);
        }

        /// <summary>Press, hold for two frames, release, wait two frames.</summary>
        protected static IEnumerator Tap(ButtonControl button)
        {
            Press(button);
            yield return null;
            yield return null;
            Release(button);
            yield return null;
            yield return null;
        }

        /// <summary>Holds a button for the given number of frames, then releases it and waits a frame.</summary>
        protected static IEnumerator Hold(ButtonControl button, int frames)
        {
            Press(button);
            for (int i = 0; i < frames; i++)
            {
                yield return null;
            }

            Release(button);
            yield return null;
        }

        /// <summary>Waits (unscaled real time) until the condition holds, else fails the test.</summary>
        protected static IEnumerator WaitFor(Func<bool> condition, float timeout, string what)
        {
            float elapsed = 0f;
            while (!condition())
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed > timeout)
                {
                    Assert.Fail($"Timed out after {timeout}s waiting for: {what}");
                }

                yield return null;
            }
        }

        /// <summary>Waits a number of frames.</summary>
        protected static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return null;
            }
        }

        private static void Write(ButtonControl control, float value)
        {
            using (StateEvent.From(control.device, out InputEventPtr eventPtr))
            {
                control.WriteValueIntoEvent(value, eventPtr);
                InputSystem.QueueEvent(eventPtr);
            }
        }
    }
}
