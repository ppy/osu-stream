using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;
using osum.Audio;
using System.Diagnostics;
using osum.GameModes;

namespace osum.Helpers
{
    public enum ClockTypes
    {
        Game,
        Mode,
        Audio,
        AudioInputAdjusted,
        Manual
    }

    public static class Clock
    {
        // measured in seconds
        private static double time = 0;

#if iOS
        //higher offset == notes appear earlier
        public const int UNIVERSAL_OFFSET_MP3 = 45;
        public const int UNIVERSAL_OFFSET_M4A = -8;
        public const int UNIVERSAL_OFFSET_INPUT = 16;//16 * 2; //roughly four frames
#else
        public const int UNIVERSAL_OFFSET_MP3 = 40;
        public const int UNIVERSAL_OFFSET_M4A = -10;
        public const int UNIVERSAL_OFFSET_INPUT = 0; //unknown
#endif

        public static bool UseMp3Offset = true;

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
            audioCheckFrame = 0;

            Update(true); //do an update now.
        }

        public static int ManualTime;

        public static void Start()
        {
            sw.Start();
        }

        /// <summary>
        /// Gets the current game time in milliseconds, accurate to many decimal places.
        /// </summary>
        public static double TimeAccurate
        {
            get { return (time * 1000); }
        }

        public static double ElapsedMilliseconds = ELAPSED_AT_SIXTY_FRAMES;
        public static float ElapsedRatioToSixty = 1;

        static double currentFrameAudioTime;

        /// <summary>
        /// Gets the current audio time, as according to the active BackgroundAudioPlayer.
        /// </summary>
        public static int AudioTime;

        /// <summary>
        /// Gets the current audio time, as according to the active BackgroundAudioPlayer.
        /// </summary>
        public static int AudioTimeInputAdjust;

        /// <summary>
        /// Gets the current time for a specific clock type.
        /// </summary>
        /// <param name="clock">The clock type in question.</param>
        /// <returns>The current time.</returns>
        public static int GetTime(ClockTypes clock)
        {
            switch (clock)
            {
                case ClockTypes.AudioInputAdjusted:
                    return AudioTimeInputAdjust;
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

        public static Stopwatch sw = new Stopwatch();
        static double swLast;
        static double swLastUpdate;

        static int audioCheckFrame;
        const int CHECK_AUDIO_FRAME_COUNT = 20;
        public const double ELAPSED_AT_SIXTY_FRAMES = 1000d/60;

        /// <summary>
        /// Update from a non-game loop. elapsed is elapsed game time. Hook your own audioTimeSource.
        /// </summary>
        /// <param name="elapsed"></param>
        public static void UpdateCustom(double elapsed)
        {
            ElapsedMilliseconds = elapsed * 1000;

            time += elapsed;
            Time = (int)Math.Round(time * 1000);
            ModeTime = Time;
            currentFrameAudioTime = AudioTimeSource.CurrentTime;
            AudioTime = (int)Math.Round(currentFrameAudioTime * 1000);
            AudioTimeInputAdjust = AudioTime;
        }

        public static void Update(bool ignoreFrame)
        {
            double swTime = (double)sw.ElapsedTicks / Stopwatch.Frequency;

            double elapsed = swTime - swLast;
            swLast = swTime;

            if (!ignoreFrame)
            {
                double elapsedSinceUpdate = swTime - swLastUpdate;
                if (elapsedSinceUpdate > 0.1) elapsedSinceUpdate = 1d/60;

                ElapsedMilliseconds = elapsedSinceUpdate * 1000;
                ElapsedRatioToSixty = (float)(ElapsedMilliseconds / ELAPSED_AT_SIXTY_FRAMES);
                swLastUpdate = swTime;

                modeTime += elapsedSinceUpdate;
                time += elapsedSinceUpdate;
            }

            Time = (int)Math.Round(time * 1000);
            ModeTime = (int)Math.Round(modeTime * 1000);

            int offset = UseMp3Offset ? UNIVERSAL_OFFSET_MP3 : UNIVERSAL_OFFSET_M4A;

            if (AudioTimeSource.IsElapsing)
            {
                currentFrameAudioTime += elapsed;

                if (audioCheckFrame % CHECK_AUDIO_FRAME_COUNT == 0)
                {
                    double sourceTime = AudioTimeSource.CurrentTime;

                    if (sourceTime == 0)
                    {
                        AudioTime = 0;
                        AudioTimeInputAdjust = 0;
                        return;
                    }
                    else
                    {
                        double inaccuracy = Math.Abs(currentFrameAudioTime - sourceTime);
                        if (inaccuracy > 0.05)
                            currentFrameAudioTime = sourceTime;
                        else if (inaccuracy > 0.005)
                            currentFrameAudioTime = currentFrameAudioTime * 0.6 + sourceTime * 0.4;
                        else
                        {
                            currentFrameAudioTime = currentFrameAudioTime * 0.95 + sourceTime * 0.05;
                            audioCheckFrame++;
                        }
                    }
                }
                else
                    audioCheckFrame++;
            }
            else
            {
                currentFrameAudioTime = AudioTimeSource.CurrentTime;
                audioCheckFrame = 0;
            }

            AudioTime = (int)Math.Round(currentFrameAudioTime * 1000) + offset;
            AudioTimeInputAdjust = AudioTime - UNIVERSAL_OFFSET_INPUT;
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

        internal static void SkipOccurred()
        {
            audioCheckFrame = 0;
        }
    }
}
