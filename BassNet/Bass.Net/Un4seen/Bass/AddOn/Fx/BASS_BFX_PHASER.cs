namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_PHASER
    {
        public float fDryMix;
        public float fWetMix;
        public float fFeedback;
        public float fRate;
        public float fRange;
        public float fFreq;
        public BASSFXChan lChannel;
        public BASS_BFX_PHASER()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_PHASER(float DryMix, float WetMix, float Feedback, float Rate, float Range, float Freq)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fRate = Rate;
            fRange = Range;
            fFreq = Freq;
        }

        public BASS_BFX_PHASER(float DryMix, float WetMix, float Feedback, float Rate, float Range, float Freq, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fRate = Rate;
            fRange = Range;
            fFreq = Freq;
            lChannel = chans;
        }

        public void Preset_Default()
        {
            fDryMix = -1f;
            fWetMix = 1f;
            fFeedback = 0.06f;
            fRate = 0.2f;
            fRange = 6f;
            fFreq = 100f;
        }

        public void Preset_PhaseShift()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fRate = 1f;
            fRange = 4f;
            fFreq = 100f;
        }

        public void Preset_SlowInvertPhaseShiftWithFeedback()
        {
            fDryMix = 0.999f;
            fWetMix = -0.999f;
            fFeedback = -0.6f;
            fRate = 0.2f;
            fRange = 6f;
            fFreq = 100f;
        }

        public void Preset_BasicPhase()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fRate = 1f;
            fRange = 4.3f;
            fFreq = 50f;
        }

        public void Preset_PhaseWithFeedback()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fRate = 1f;
            fRange = 4.3f;
            fFreq = 50f;
        }

        public void Preset_MediumPhase()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fRate = 1f;
            fRange = 7f;
            fFreq = 100f;
        }

        public void Preset_FastPhase()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0f;
            fRate = 1f;
            fRange = 7f;
            fFreq = 400f;
        }

        public void Preset_InvertWithInvertFeedback()
        {
            fDryMix = 0.999f;
            fWetMix = -0.999f;
            fFeedback = -0.2f;
            fRate = 1f;
            fRange = 7f;
            fFreq = 200f;
        }

        public void Preset_TremoloWah()
        {
            fDryMix = 0.999f;
            fWetMix = 0.999f;
            fFeedback = 0.6f;
            fRate = 1f;
            fRange = 4f;
            fFreq = 60f;
        }
    }
}

