namespace osu_Tencho.Helpers
{
    internal class RollingTime
    {
        private readonly int size;
        private readonly int minDelayMs;
        private readonly long[] time;

        /// <summary>
        /// Make a new object that keeps track of time for a number of events.
        /// </summary>
        /// <param name="size">The number of events to track.</param>
        /// <param name="minDelayMs">The time between the number of events.</param>
        internal RollingTime(int size, int minDelayMs)
        {
            this.size = size;
            this.minDelayMs = minDelayMs;
            time = new long[size];
            UpdateTime();
        }

        internal bool IsWithinAllowable
        {
            get { return (time[0] == 0 && time[1] == 0) || time[0] + minDelayMs < time[size - 1]; }
        }

        internal long TimeToNext
        {
            get {
                long timeToNext = time[0] + minDelayMs - Tencho.CurrentTime;
                if (timeToNext < 0)
                    timeToNext = 0;
                return timeToNext;
            }
        }

        internal long TimeOfNewest
        {
            get {
                return time[size - 1];
            }
        }

        internal void UpdateTime()
        {
            for (int i = 0; i < size - 1; i++)
                time[i] = time[i + 1];
            time[size - 1] = Tencho.CurrentTime;
        }
    }
}