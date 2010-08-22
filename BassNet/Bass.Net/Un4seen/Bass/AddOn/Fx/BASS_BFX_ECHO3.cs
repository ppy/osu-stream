namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_ECHO3
    {
        public float fDryMix;
        public float fWetMix;
        public float fDelay;
        public BASSFXChan lChannel;
        public BASS_BFX_ECHO3()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_ECHO3(float DryMix, float WetMix, float Delay)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fDelay = Delay;
        }

        public BASS_BFX_ECHO3(float DryMix, float WetMix, float Delay, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fDelay = Delay;
            lChannel = chans;
        }

        public void Preset_SmallEcho()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fDelay = 0.2f;
        }

        public void Preset_DoubleKick()
        {
            fDryMix = 0.5f;
            fWetMix = 0.599f;
            fDelay = 0.5f;
        }

        public void Preset_LongEcho()
        {
            fDryMix = 0.999f;
            fWetMix = 0.699f;
            fDelay = 0.9f;
        }
    }
}

