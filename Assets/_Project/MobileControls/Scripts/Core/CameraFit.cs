using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>Orthographic size and position that place the board in a screen area (<see cref="BoardFit"/>).</summary>
    public readonly struct CameraFit
    {
        /// <summary>Creates a fit.</summary>
        public CameraFit(float orthographicSize, Vector2 position)
        {
            OrthographicSize = orthographicSize;
            Position = position;
            IsValid = true;
        }

        /// <summary>Half the visible world height.</summary>
        public float OrthographicSize { get; }

        /// <summary>World position of the camera (x and y; keep its z).</summary>
        public Vector2 Position { get; }

        /// <summary>False when the inputs had no area to fit into.</summary>
        public bool IsValid { get; }
    }
}
