using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlockDiggerz.DebugTools
{
    /// <summary>General debug controls (frame rate, time scale, scene reload). Lives on the DebugMenu prefab.</summary>
    public sealed class GameDebugControls : MonoBehaviour
    {
        private const string SectionTitle = "Game";
        private const float MaxTimeScale = 3f;
        private const float FrameTimeSmoothing = 0.1f;
        private const float MillisecondsPerSecond = 1000f;

        private float _smoothedFrameTime;

        private void OnEnable()
        {
            DebugMenu.Register(this, SectionTitle, section => section
                .Readout("FPS", FormatFrameRate)
                .Slider("Time scale", 0f, MaxTimeScale, () => Time.timeScale, v => Time.timeScale = v)
                .Button("Reset time scale", () => Time.timeScale = 1f)
                .Button("Reload scene", () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)));
        }

        private void OnDisable()
        {
            DebugMenu.Unregister(this);
        }

        private void Update()
        {
            float previous = _smoothedFrameTime <= 0f ? Time.unscaledDeltaTime : _smoothedFrameTime;
            _smoothedFrameTime = Mathf.Lerp(previous, Time.unscaledDeltaTime, FrameTimeSmoothing);
        }

        private string FormatFrameRate()
        {
            if (_smoothedFrameTime <= 0f)
            {
                return "-";
            }

            return $"{1f / _smoothedFrameTime:0} ({_smoothedFrameTime * MillisecondsPerSecond:0.0} ms)";
        }
    }
}
