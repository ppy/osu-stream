using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Helpers
{
    public enum ClockTypes
    {
        Game,
        Audio
    }

    public static class Clock
    {
        // measured in seconds
        private static double time = 0;
        private static double zero = 0;

        /// <summary>
        /// Get the current game time in milliseconds.
        /// </summary>
        public static int Time
        {
            get { return (int)(time * 1000); }
        }

        /// <summary>
        /// Gets the current game time in milliseconds, accurate to many decimal places.
        /// </summary>
        public static double TimeAccurate
        {
            get { return (time * 1000); }
        }

        /// <summary>
        /// Gets the current audio time, as according to the active BackgroundAudioPlayer.
        /// </summary>
        public static int AudioTime
        {
            get { return (int)((time - zero) * 1000); }
        }

        /// <summary>
        /// Gets the current time for a specific clock type.
        /// </summary>
        /// <param name="clock">The clock type in question.</param>
        /// <returns>The current time.</returns>
        public static int GetTime(ClockTypes clock)
        {
            switch (clock)
            {
                case ClockTypes.Audio:
                    return Clock.AudioTime;

                default:
                case ClockTypes.Game:
                    return Clock.Time;
            }
        }

        public static void ResetAudioTime()
        {
            //todo: this shouldn't really happen. audio time is never really reset.
            zero = time;
        }

        public static void Update(double elapsed)
        {
            time += elapsed;
        }
    }
}
