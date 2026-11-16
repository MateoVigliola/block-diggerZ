namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Turns sideways finger travel into whole column steps for the relative drag: every
    /// <c>stepSize</c> of travel is one step, the remainder carries over to the next move, so slow and fast drags
    /// move the same distance.
    /// </summary>
    public sealed class DragStepper
    {
        /// <summary>Travel not yet turned into a step (same sign as the direction of travel).</summary>
        public float Remainder { get; private set; }

        /// <summary>
        /// Adds <paramref name="delta"/> of travel and returns the whole steps it completes: positive is right,
        /// negative is left. Returns 0 when <paramref name="stepSize"/> is not positive.
        /// </summary>
        public int Feed(float delta, float stepSize)
        {
            if (stepSize <= 0f)
            {
                return 0;
            }

            Remainder += delta;
            int steps = (int)(Remainder / stepSize);
            Remainder -= steps * stepSize;
            return steps;
        }

        /// <summary>Forgets the remainder (a new drag, or the piece was blocked).</summary>
        public void Reset()
        {
            Remainder = 0f;
        }
    }
}
