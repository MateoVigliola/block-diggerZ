namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// How much faster the falling piece drops as the score grows: a flat step added to a speed multiplier for every
    /// full block of points (by default +0.1 per 200 points: 1.0, 1.1, 1.2 ...). The fall interval is divided by it.
    /// </summary>
    public static class GravitySpeed
    {
        /// <summary>
        /// The speed multiplier at <paramref name="score"/>: 1 plus <paramref name="increasePerStep"/> for every full
        /// <paramref name="pointsPerStep"/> points, capped at <paramref name="maxMultiplier"/> when it is above 0.
        /// Never below 1. A step size of 0 or less turns the speed-up off.
        /// </summary>
        public static float Multiplier(int score, int pointsPerStep, float increasePerStep, float maxMultiplier)
        {
            int steps = pointsPerStep > 0 && score > 0 ? score / pointsPerStep : 0;
            float multiplier = 1f + steps * increasePerStep;
            if (maxMultiplier > 0f && multiplier > maxMultiplier)
            {
                multiplier = maxMultiplier;
            }

            return multiplier < 1f ? 1f : multiplier;
        }
    }
}
