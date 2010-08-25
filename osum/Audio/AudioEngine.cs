using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        internal static void PlaySample(OsuSamples sample)
        {
            int buffer;

            if (!loadedSamples.TryGetValue(sample, out buffer))
            {
                buffer = GameBase.Instance.soundEffectPlayer.Load("Skins/Default/normal-" + sample.ToString().ToLower() + ".wav");
                loadedSamples.Add(sample, buffer);
            }

            GameBase.Instance.soundEffectPlayer.PlayBuffer(buffer);
        }
    }
}
