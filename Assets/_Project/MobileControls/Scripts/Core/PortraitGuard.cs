namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Rules for keeping the portrait touch layout upright on phones: when the screen counts as landscape, and
    /// when turning the phone sideways should pause the game.
    /// </summary>
    public static class PortraitGuard
    {
        /// <summary>True when the screen is wider than it is tall. A square screen counts as portrait.</summary>
        public static bool IsLandscape(int width, int height)
        {
            return width > height;
        }

        /// <summary>
        /// True when the screen just turned landscape (it was portrait or not known yet) while the touch controls
        /// were in play, so the game should pause until the phone is upright again.
        /// </summary>
        public static bool ShouldPause(bool wasKnown, bool wasLandscape, bool isLandscape, bool controlsShown)
        {
            return isLandscape && controlsShown && (!wasKnown || !wasLandscape);
        }
    }
}
