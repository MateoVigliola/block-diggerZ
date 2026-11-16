namespace BlockDiggerz.Blocks
{
    /// <summary>Formats the run timer as minutes:seconds.</summary>
    public static class RunClock
    {
        private const int SecondsPerMinute = 60;

        /// <summary>Whole seconds elapsed (rounded down, never below 0).</summary>
        public static int WholeSeconds(float seconds)
        {
            return seconds > 0f ? (int)System.Math.Floor(seconds) : 0;
        }

        /// <summary>
        /// <paramref name="wholeSeconds"/> as "m:ss" (for example 0:07, 3:45, 72:00). Minutes keep counting past 59.
        /// </summary>
        public static string Format(int wholeSeconds)
        {
            int total = System.Math.Max(0, wholeSeconds);
            int minutes = total / SecondsPerMinute;
            int seconds = total % SecondsPerMinute;
            return minutes + ":" + seconds.ToString("00");
        }
    }
}
