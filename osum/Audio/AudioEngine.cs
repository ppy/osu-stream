using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;

namespace osum.Audio
{
    enum OsuSamples
    {
        HitNormal,
        HitWhistle,
        HitClap,
        HitFinish
    }

    internal static class AudioEngine
    {
        static Dictionary<OsuSamples, int> loadedSamples = new Dictionary<OsuSamples, int>();
        
        internal static ISoundEffectPlayer Effect;
        internal static IBackgroundAudioPlayer Music;


        /// <summary>
        /// Initializes the audio subsystem using specific implementations for sound effects and music modules.
        /// </summary>
        /// <param name="effect">The effect player.</param>
        /// <param name="music">The music player.</param>
        internal static void Initialize(ISoundEffectPlayer effect, IBackgroundAudioPlayer music)
        {
            Effect = effect;
            Music = music;
        }

        internal static void PlaySample(OsuSamples sample)
        {
            int buffer;

            if (!loadedSamples.TryGetValue(sample, out buffer))
            {
                buffer = AudioEngine.Effect.Load("Skins/Default/normal-" + sample.ToString().ToLower() + ".wav");
                loadedSamples.Add(sample, buffer);
            }

            AudioEngine.Effect.PlayBuffer(buffer);
        }

    }
}
    