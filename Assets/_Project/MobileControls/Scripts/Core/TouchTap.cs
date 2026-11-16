namespace BlockDiggerz.MobileControls
{
    /// <summary>Result of a finger lifting off the gesture area.</summary>
    public enum TouchTap
    {
        /// <summary>Not a tap (a drag, a long press, or no finger tracked).</summary>
        None,

        /// <summary>A tap on the left half: rotate counter-clockwise.</summary>
        Left,

        /// <summary>A tap on the right half: rotate clockwise.</summary>
        Right
    }
}
