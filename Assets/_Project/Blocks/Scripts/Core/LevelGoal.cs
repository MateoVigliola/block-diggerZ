namespace BlockDiggerz.Blocks
{
    /// <summary>The level's win condition: every Dirt block the level started with has been cleared.</summary>
    public static class LevelGoal
    {
        /// <summary>
        /// True when the level started with Dirt (<paramref name="startDirt"/> above 0) and none is left. A level
        /// without Dirt can't be won this way.
        /// </summary>
        public static bool IsCleared(int startDirt, int dirtLeft)
        {
            return startDirt > 0 && dirtLeft <= 0;
        }
    }
}
