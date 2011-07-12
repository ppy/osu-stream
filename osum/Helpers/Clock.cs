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
        public const int UNIVERSAL_OFFSET = 45;
#else
        public const int UNIVERSAL_OFFSET = 60;
#endif

        /// <summary>
        /// Get the current game time in milliseconds.
        /// </summary>
        public static int Time;

        private static double modeTime;
        public static int ModeTime;

        public static void ModeTimeReset()
        {
            modeTime = 0;
            ModeTime = 0;
        }

        public static int ManualTime;


        /// <summary>
        /// Gets the current game time in milliseconds, accurate to many decimal places.
        /// </summary>
        public static double TimeAccurate
        {
            get { return (time * 1000); }
        }

        public static double ElapsedMilliseconds = 1000 / 60f;

        static double currentFrameAudioTime;

        /// <summary>
        /// Gets the current audio time, as according to the active BackgroundAudioPlayer.
        /// </summary>
        public static int AudioTime;

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

        public static bool AudioLeadingIn;
        public static bool AudioLeadingInRunning;

        public static void BeginLeadIn(int leadInStartTime)
        {
            currentFrameAudioTime = leadInStartTime / 1000d;
            AudioLeadingIn = true;
            AudioLeadingInRunning = true;
        }

        public static void AbortLeadIn()
        {
            if (AudioLeadingIn)
            {
                AudioLeadingIn = false;
                AudioLeadingInRunning = false;
                currentFrameAudioTime = AudioTime = 0;
            }
        }

        public static void Update(double elapsed)
        {
            if (elapsed > 0.1) elapsed = 1d/60;
            //let's disregard slow frames for mode time calculations.

            ElapsedMilliseconds = elapsed * 1000;

            modeTime += elapsed;
            time += elapsed;

            Time = (int)Math.Round(time * 1000);
            ModeTime = (int)Math.Round(modeTime * 1000);

            if (AudioLeadingIn && AudioLeadingInRunning && elapsed < 0.1)
            {
                currentFrameAudioTime += elapsed;

                if (currentFrameAudioTime + UNIVERSAL_OFFSET / 1000f >= AudioTimeSource.CurrentTime)
                {
                    if (AudioEngine.Music != null)
                        AudioEngine.Music.Play();
                    AudioLeadingIn = false;
                }
            }

            if (AudioTimeSource.IsElapsing)
            {
                currentFrameAudioTime += elapsed;
                double sourceTime = AudioTimeSource.CurrentTime;

                if (sourceTime == 0)
                {
                    AudioTime = 0;
                    return;
                }
                else
                {
                    double inaccuracy = Math.Abs(currentFrameAudioTime - sourceTime);
                    if (inaccuracy > 0.05)
                        currentFrameAudioTime = sourceTime;
                    if (inaccuracy > 0.005)
                        currentFrameAudioTime = sourceTime * 0.01 + currentFrameAudioTime * 0.99;
                }
            }

            AudioTime = (int)(currentFrameAudioTime * 1000) + UNIVERSAL_OFFSET;
        }

        public static ITimeSource AudioTimeSource { get; set; }

        internal static void IncrementManual(float rate = 1)
        {
            ManualTime += (int)(ElapsedMilliseconds * rate);
        }

        internal static void ResetManual()
        {
            ManualTime = 0;
        }
    }
}
