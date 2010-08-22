namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_PEAKEQ
    {
        public int lBand;
        public float fFreq;
        public float fBandwidth;
        public float fQ;
        public float fCenter;
        public float fGain;
        public BASSFXChan lChannel;
        public BASS_BFX_PEAKEQ()
        {
            fFreq = 44100f;
            fBandwidth = 1f;
            fQ = 0.1f;
            fCenter = 1000f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_PEAKEQ(int Band, float Freq, float Bandwidth, float Q, float Center, float Gain)
        {
            fFreq = 44100f;
            fBandwidth = 1f;
            fQ = 0.1f;
            fCenter = 1000f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            lBand = Band;
            fFreq = Freq;
            fBandwidth = Bandwidth;
            fQ = Q;
            fCenter = Center;
            fGain = Gain;
        }

        public BASS_BFX_PEAKEQ(int Band, float Freq, float Bandwidth, float Q, float Center, float Gain, BASSFXChan chans)
        {
            fFreq = 44100f;
            fBandwidth = 1f;
            fQ = 0.1f;
            fCenter = 1000f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            lBand = Band;
            fFreq = Freq;
            fBandwidth = Bandwidth;
            fQ = Q;
            fCenter = Center;
            fGain = Gain;
            lChannel = chans;
        }
    }
}

