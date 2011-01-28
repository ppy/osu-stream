using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;

namespace osum.Helpers
{
    public enum ClockTypes
    {
        Game,
        Mode,
        Audio
    }

    public static class Clock
    {
        // measured in seconds
        private static double time = 0;
        private static double zero = 0;

#if IPHONE
        private const int UNIVERSAL_OFFSET = 55;
#else
        private const int UNIVERSAL_OFFSET = 20;
#endif

        /// <summary>
        /// Get the current game time in milliseconds.
        /// </summary>
        public static int Time
        {
            get { return (int)(time * 1000); }
        }

        private static int lastModeLoadTime;
        public static int ModeTime { get { return Time - lastModeLoadTime; } }

        public static void ModeLoadComplete()
        {
            lastModeLoadTime = Time;
        }


        /// <summary>
        /// Gets the current game time in milliseconds, accurate to many decimal places.
        /// </summary>
        public static double TimeAccurate
        {
            get { return (time * 1000); }
        }

        static double currentFrameAudioTime;
        static int currentFrameAudioTimeOffset;
        

        /// <summary>
        /// Gets the current audio time, as according to the active BackgroundAudioPlayer.
        /// </summary>
        public static int AudioTime
        {
            get { return currentFrameAudioTimeOffset; }
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
                case ClockTypes.Mode:
                    return Clock.ModeTime;
            }
        }

        public static void Update(double elapsed)
        {
            time += elapsed;

            currentFrameAudioTime += elapsed;

            double sourceTime = AudioTimeSource.CurrentTime;

            if (sourceTime == 0)
                currentFrameAudioTimeOffset = 0;
            else
            {
                double inaccuracy = Math.Abs(currentFrameAudioTime - sourceTime);
                if (inaccuracy > 0.03)
					currentFrameAudioTime = sourceTime;

                currentFrameAudioTimeOffset = (int)(currentFrameAudioTime * 1000) + UNIVERSAL_OFFSET;
            }
        }

        public static ITimeSource AudioTimeSource { private get; set; }
    }
}
