using UnityEngine;

namespace BlockDiggerz.Blocks
{
    /// <summary>
    /// Lock delay for one falling piece. Time counts only while the piece rests on the floor or stack. A sideways
    /// move or rotation while resting can extend it by a bonus (never past the full delay), a limited number of
    /// times per piece. Reaching a row lower than the piece has been before restarts both the time and the
    /// extension count.
    /// </summary>
    public sealed class LockDelayTimer
    {
        /// <summary>Seconds the piece has spent resting since the timer last restarted, minus bonuses.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Extensions used since the timer last restarted.</summary>
        public int ExtensionsUsed { get; private set; }

        /// <summary>Lowest row the piece's bottom cells have reached.</summary>
        public int LowestRow { get; private set; }

        /// <summary>Starts the timer for a new piece whose bottom cells are on <paramref name="row"/>.</summary>
        public void Reset(int row)
        {
            Elapsed = 0f;
            ExtensionsUsed = 0;
            LowestRow = row;
        }

        /// <summary>Restarts time and the extension count if <paramref name="row"/> is lower than any row so far.</summary>
        public void ReachRow(int row)
        {
            if (row >= LowestRow)
            {
                return;
            }

            LowestRow = row;
            Elapsed = 0f;
            ExtensionsUsed = 0;
        }

        /// <summary>Adds resting time and returns true once it reaches <paramref name="lockDelay"/>.</summary>
        public bool Tick(float deltaTime, float lockDelay)
        {
            Elapsed += deltaTime;
            return Elapsed >= lockDelay;
        }

        /// <summary>
        /// Gives back up to <paramref name="bonus"/> seconds (never below zero elapsed, so never past the full delay)
        /// if fewer than <paramref name="maxExtensions"/> have been used. Returns true if it extended.
        /// </summary>
        public bool TryExtend(float bonus, int maxExtensions)
        {
            if (ExtensionsUsed >= maxExtensions)
            {
                return false;
            }

            ExtensionsUsed++;
            Elapsed = Mathf.Max(0f, Elapsed - bonus);
            return true;
        }
    }
}
