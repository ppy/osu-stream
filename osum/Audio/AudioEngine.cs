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
        SpinnerSpin,
        stream_down,
        stream_up,
        count3,
        count2,
        count1,
        countgo,
        miss,
        PRELOAD_END,
        fail,
        menuwhoosh,
        MainMenu_Intro,
        RankingBam,
        RankingBam2
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
                {
                    if (s == OsuSamples.PRELOAD_END)
                        break;
                    LoadSample(s, set);
                }
            }

            Effect.Volume = GameBase.Config.GetValue<int>("VolumeEffect", 100) / 100f;
            Music.MaxVolume = GameBase.Config.GetValue<int>("VolumeMusic", 100) / 100f;
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
                if (Math.Abs(Clock.Time - lastPlayed) < 40)
                    return null;
            lastPlayedTimes[sample] = Clock.Time;

            Source src = AudioEngine.Effect.PlayBuffer(buffer, volume);

            if (sample > OsuSamples.PRELOAD_END)
                src.Disposable = true;

            return src;
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
                bool oneShot = sample > OsuSamples.PRELOAD_END;

                if (AudioEngine.Effect != null)
                    buffer = AudioEngine.Effect.Load("Skins/Default/" + filename + ".wav");
                if (!oneShot)
                    loadedSamples.Add(filename, buffer);
            }

            return buffer;
        }

        internal static void Suspend()
        {
            if (Effect != null)
            {
                Effect.UnloadAll();
            }
        }
    }
}
