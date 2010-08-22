namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_LPF
    {
        public float fFreq;
        public float fResonance;
        public float fCutOffFreq;
        public BASSFXChan lChannel;
        public BASS_BFX_LPF()
        {
            fFreq = 44100f;
            fResonance = 2f;
            fCutOffFreq = 200f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_LPF(float Freq, float Resonance, float CutOffFreq)
        {
            fFreq = 44100f;
            fResonance = 2f;
            fCutOffFreq = 200f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fFreq = Freq;
            fResonance = Resonance;
            fCutOffFreq = CutOffFreq;
        }

        public BASS_BFX_LPF(float Freq, float Resonance, float CutOffFreq, BASSFXChan chans)
        {
            fFreq = 44100f;
            fResonance = 2f;
            fCutOffFreq = 200f;
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fFreq = Freq;
            fResonance = Resonance;
            fCutOffFreq = CutOffFreq;
            lChannel = chans;
        }
    }
}

