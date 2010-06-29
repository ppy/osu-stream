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

        public static int AudioTime
        {
            get { return (int)((time - zero) * 1000); }
        }

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
            zero = time;
        }

        public static void Update(double elapsed)
        {
            time += elapsed;
        }
    }
}
