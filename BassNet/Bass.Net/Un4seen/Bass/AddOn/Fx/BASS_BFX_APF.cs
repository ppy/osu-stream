namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_APF
    {
        public float fGain;
        public float fDelay;
        public BASSFXChan lChannel;
        public BASS_BFX_APF()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_APF(float Gain, float Delay)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fGain = Gain;
            fDelay = Delay;
        }

        public BASS_BFX_APF(float Gain, float Delay, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fGain = Gain;
            fDelay = Delay;
            lChannel = chans;
        }

        public void Preset_Default()
        {
            fGain = -0.5f;
            fDelay = 0.5f;
        }

        public void Preset_SmallRever()
        {
            fGain = 0.799f;
            fDelay = 0.2f;
        }

        public void Preset_RobotVoice()
        {
            fGain = 0.6f;
            fDelay = 0.05f;
        }

        public void Preset_LongReverberation()
        {
            fGain = 0.599f;
            fDelay = 1.3f;
        }
    }
}

