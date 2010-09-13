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
        HitFinish,
        MenuHit
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

            string filename = null;

            switch (sample)
            {
                case OsuSamples.HitClap:
                    filename = "normal-hitclap";
                    break;
                case OsuSamples.HitFinish:
                    filename = "normal-hitfinish";
                    break;
                case OsuSamples.HitNormal:
                    filename = "normal-hitnormal";
                    break;
                case OsuSamples.HitWhistle:
                    filename = "normal-hitwhistle";
                    break;
                case OsuSamples.MenuHit:
                    filename = "menuhit";
                    break;
            }

            if (filename == null) return;

            if (!loadedSamples.TryGetValue(sample, out buffer))
            {
                buffer = AudioEngine.Effect.Load("Skins/Default/" + filename + ".wav");
                loadedSamples.Add(sample, buffer);
            }

            AudioEngine.Effect.PlayBuffer(buffer);
        }





    }
}
    