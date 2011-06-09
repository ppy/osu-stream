using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;
using osum.Audio;

namespace osum.Helpers
{
    public enum ClockTypes
    {
        Game,
        Mode,
        Audio,
        Manual
    }

    public static class Clock
    {
        // measured in seconds
        private static double time = 0;

#if iOS
        private const int UNIVERSAL_OFFSET = 45;
#else
        private const int UNIVERSAL_OFFSET = 45;
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

        public static void ModeTimeReset()
        {
            lastModeLoadTime = Time;
        }

        public static int ManualTime;


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
                    return AudioTime;
                default:
                case ClockTypes.Game:
                    return Time;
                case ClockTypes.Mode:
                    return ModeTime;
                case ClockTypes.Manual:
                    return ManualTime;
            }
        }

        static bool LeadingIn;

        public static void BeginLeadIn(int leadInStartTime)
        {
            currentFrameAudioTime = leadInStartTime / 1000d;
            LeadingIn = true;
        }

        public static void AbortLeadIn()
        {
            LeadingIn = false;
            currentFrameAudioTime = AudioTimeSource.CurrentTime;
        }

        public static void Update(double elapsed)
        {
            time += elapsed;

            if (LeadingIn && elapsed < 0.1)
            {
                currentFrameAudioTime += elapsed;

                if (currentFrameAudioTime + UNIVERSAL_OFFSET / 1000f >= AudioTimeSource.CurrentTime)
                {
                    AudioEngine.Music.Play();
                    LeadingIn = false;
                }
            }

            if (AudioTimeSource.IsElapsing)
            {
                currentFrameAudioTime += elapsed;
                double sourceTime = AudioTimeSource.CurrentTime;

                if (sourceTime == 0)
                {
                    currentFrameAudioTimeOffset = 0;
                    return;
                }
                else
                {
                    double inaccuracy = Math.Abs(currentFrameAudioTime - sourceTime);
                    if (inaccuracy > 0.03)
                        currentFrameAudioTime = sourceTime;
                }
            }

            currentFrameAudioTimeOffset = (int)(currentFrameAudioTime * 1000) + UNIVERSAL_OFFSET;
        }

        public static ITimeSource AudioTimeSource { get; set; }

        internal static void IncrementManual(float rate = 1)
        {
            ManualTime += (int)(GameBase.ElapsedMilliseconds * rate);
        }

        internal static void ResetManual()
        {
            ManualTime = 0;
        }
    }
}
