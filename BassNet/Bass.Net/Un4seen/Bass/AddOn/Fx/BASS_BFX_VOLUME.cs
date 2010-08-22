namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_VOLUME
    {
        public BASSFXChan lChannel;
        public float fVolume;
        public BASS_BFX_VOLUME()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fVolume = 1f;
        }

        public BASS_BFX_VOLUME(float Volume)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fVolume = 1f;
            fVolume = Volume;
        }

        public BASS_BFX_VOLUME(float Volume, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fVolume = 1f;
            fVolume = Volume;
            lChannel = chans;
        }
    }
}

