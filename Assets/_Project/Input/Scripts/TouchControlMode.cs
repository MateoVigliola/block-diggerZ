namespace BlockDiggerz.Input
{
    /// <summary>How the on-screen touch controls move and rotate the piece. Chosen in Settings.</summary>
    public enum TouchControlMode
    {
        /// <summary>Drag on the board to move the piece under the finger, tap a board half to rotate.</summary>
        Gestures,

        /// <summary>Tap or hold a board half to move, CCW and CW buttons to rotate.</summary>
        Buttons
    }
}
