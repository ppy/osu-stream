namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_CHORUS
    {
        public float fDryMix;
        public float fWetMix;
        public float fFeedback;
        public float fMinSweep;
        public float fMaxSweep;
        public float fRate;
        public BASSFXChan lChannel;
        public BASS_BFX_CHORUS()
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
        }

        public BASS_BFX_CHORUS(float DryMix, float WetMix, float Feedback, float MinSweep, float MaxSweep, float Rate)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fMinSweep = MinSweep;
            fMaxSweep = MaxSweep;
            fRate = Rate;
        }

        public BASS_BFX_CHORUS(float DryMix, float WetMix, float Feedback, float MinSweep, float MaxSweep, float Rate, BASSFXChan chans)
        {
            lChannel = ~BASSFXChan.BASS_BFX_CHANNONE;
            fDryMix = DryMix;
            fWetMix = WetMix;
            fFeedback = Feedback;
            fMinSweep = MinSweep;
            fMaxSweep = MaxSweep;
            fRate = Rate;
            lChannel = chans;
        }

        public void Preset_Flanger()
        {
            fDryMix = 1f;
            fWetMix = 0.35f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 5f;
            fRate = 1f;
        }

        public void Preset_ExaggeratedChorusLTMPitchSshiftedVoices()
        {
            fDryMix = 0.7f;
            fWetMix = 0.25f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 200f;
            fRate = 50f;
        }

        public void Preset_Motocycle()
        {
            fDryMix = 0.9f;
            fWetMix = 0.45f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 100f;
            fRate = 25f;
        }

        public void Preset_Devil()
        {
            fDryMix = 0.9f;
            fWetMix = 0.35f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 50f;
            fRate = 200f;
        }

        public void Preset_WhoSayTTNManyVoices()
        {
            fDryMix = 0.9f;
            fWetMix = 0.35f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 400f;
            fRate = 200f;
        }

        public void Preset_BackChipmunk()
        {
            fDryMix = 0.9f;
            fWetMix = -0.2f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 400f;
            fRate = 400f;
        }

        public void Preset_Water()
        {
            fDryMix = 0.9f;
            fWetMix = -0.4f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 2f;
            fRate = 1f;
        }

        public void Preset_ThisIsTheAirplane()
        {
            fDryMix = 0.3f;
            fWetMix = 0.4f;
            fFeedback = 0.5f;
            fMinSweep = 1f;
            fMaxSweep = 10f;
            fRate = 5f;
        }
    }
}

