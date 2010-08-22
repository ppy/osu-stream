namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSInput
    {
        BASS_INPUT_NONE = 0,
        BASS_INPUT_OFF = 0x10000,
        BASS_INPUT_ON = 0x20000
    }
}

