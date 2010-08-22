namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSInputType
    {
        BASS_INPUT_TYPE_ANALOG = 0xa000000,
        BASS_INPUT_TYPE_AUX = 0x9000000,
        BASS_INPUT_TYPE_CD = 0x5000000,
        BASS_INPUT_TYPE_DIGITAL = 0x1000000,
        BASS_INPUT_TYPE_ERROR = -1,
        BASS_INPUT_TYPE_LINE = 0x2000000,
        BASS_INPUT_TYPE_MASK = -16777216,
        BASS_INPUT_TYPE_MIC = 0x3000000,
        BASS_INPUT_TYPE_PHONE = 0x6000000,
        BASS_INPUT_TYPE_SPEAKER = 0x7000000,
        BASS_INPUT_TYPE_SYNTH = 0x4000000,
        BASS_INPUT_TYPE_UNDEF = 0,
        BASS_INPUT_TYPE_WAVE = 0x8000000
    }
}

