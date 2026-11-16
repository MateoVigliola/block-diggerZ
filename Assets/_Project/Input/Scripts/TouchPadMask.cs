namespace BlockDiggerz.Input
{
    /// <summary>Bit operations on the button mask of <see cref="TouchPadState"/>.</summary>
    public static class TouchPadMask
    {
        /// <summary>No button pressed.</summary>
        public const uint None = 0u;

        /// <summary>The bit of <paramref name="button"/>.</summary>
        public static uint Bit(TouchPadButton button)
        {
            return 1u << (int)button;
        }

        /// <summary>
        /// <paramref name="mask"/> with <paramref name="button"/> pressed or released; other buttons are kept.
        /// </summary>
        public static uint Set(uint mask, TouchPadButton button, bool pressed)
        {
            return pressed ? mask | Bit(button) : mask & ~Bit(button);
        }

        /// <summary>True when <paramref name="button"/> is pressed in <paramref name="mask"/>.</summary>
        public static bool IsPressed(uint mask, TouchPadButton button)
        {
            return (mask & Bit(button)) != 0u;
        }
    }
}
