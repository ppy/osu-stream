namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_FLANGER
    {
        public float fWetDry;
        public float fSpeed;
        public BASSFXChan lChannel;
        public BASS_BFX_FLANGER()
        {
            fWetDry = 1f;
            fSpeed = 0.01f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_FLANGER(float WetDry, float Speed)
        {
            fWetDry = 1f;
            fSpeed = 0.01f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fWetDry = WetDry;
            fSpeed = Speed;
        }

        public BASS_BFX_FLANGER(float WetDry, float Speed, BASSFXChan chans)
        {
            fWetDry = 1f;
            fSpeed = 0.01f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fWetDry = WetDry;
            fSpeed = Speed;
            lChannel = chans;
        }

        public void Preset_Default()
        {
            fWetDry = 1f;
            fSpeed = 0.01f;
        }
    }
}

