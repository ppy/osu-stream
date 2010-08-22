namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_ECHO2
    {
        public float fDryMix;
        public float fWetMix;
        public float fFeedback;
        public float fDelay;
        public BASSFXChan lChannel;
        public BASS_BFX_ECHO2()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_ECHO2(float DryMix, float WetMix, float Feedback, float Delay)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fDelay = Delay;
        }

        public BASS_BFX_ECHO2(float DryMix, float WetMix, float Feedback, float Delay, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fDelay = Delay;
            lChannel = chans;
        }

        public void Preset_SmallEcho()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fDelay = 0.2f;
        }

        public void Preset_ManyEchoes()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0.7f;
            fDelay = 0.5f;
        }

        public void Preset_ReverseEchoes()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = -0.7f;
            fDelay = 0.8f;
        }
    }
}

