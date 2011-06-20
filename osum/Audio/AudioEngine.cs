using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;
using osum.Helpers;
using osum.GameplayElements.Beatmaps;

namespace osum.Audio
{
    enum OsuSamples
    {
        HitNormal,
        HitWhistle,
        HitClap,
        HitFinish,
        MenuHit,
        MenuClick,
        SliderTick,
        SliderSlide,
        MenuBack,
        SpinnerBonus,
        SpinnerSpin
    }

    internal static class AudioEngine
    {
        static Dictionary<string, int> loadedSamples = new Dictionary<string, int>();

        internal static SoundEffectPlayer Effect;
        internal static BackgroundAudioPlayer Music;


        /// <summary>
        /// Initializes the audio subsystem using specific implementations for sound effects and music modules.
        /// </summary>
        /// <param name="effect">The effect player.</param>
        /// <param name="music">The music player.</param>
        internal static void Initialize(SoundEffectPlayer effect, BackgroundAudioPlayer music)
        {
            Effect = effect;
            Music = music;

            foreach (SampleSet set in Enum.GetValues(typeof(SampleSet)))
            {
                if (set == SampleSet.None)
                    continue;
                foreach (OsuSamples s in Enum.GetValues(typeof(OsuSamples)))
                    LoadSample(s, set);
            }
        }

        static Dictionary<OsuSamples, int> lastPlayedTimes = new Dictionary<OsuSamples, int>();

        internal static Source PlaySample(OsuSamples sample, SampleSet set = SampleSet.Soft, float volume = 1)
        {
            int buffer = LoadSample(sample, set);
            if (buffer < 0) return null;

            if (AudioEngine.Effect == null)
                return null;

            int lastPlayed = -1;
            if (lastPlayedTimes.TryGetValue(sample, out lastPlayed))
                if (Math.Abs(Clock.AudioTime - lastPlayed) < 45)
                    return null;
            lastPlayedTimes[sample] = Clock.AudioTime;

            return AudioEngine.Effect.PlayBuffer(buffer, volume);
        }

        internal static void Reset()
        {
            lastPlayedTimes.Clear();
        }

        internal static int LoadSample(OsuSamples sample, SampleSet set = SampleSet.Soft)
        {
            int buffer;

            string filename = null;

            string setName = set.ToString().ToLower();
            string sampleName = sample.ToString().ToLower();

            switch (sample)
            {
                case OsuSamples.HitClap:
                case OsuSamples.HitFinish:
                case OsuSamples.HitNormal:
                case OsuSamples.HitWhistle:
                case OsuSamples.SliderTick:
                case OsuSamples.SliderSlide:
                    filename = setName + "-" + sampleName;
                    break;
                default:
                    filename = sampleName;
                    break;
            }

            if (filename == null) return -1;

            if (!loadedSamples.TryGetValue(filename, out buffer))
            {
                if (AudioEngine.Effect != null)
                    buffer = AudioEngine.Effect.Load("Skins/Default/" + filename + ".wav");
                loadedSamples.Add(filename, buffer);
            }

            return buffer;
        }
    }
}
