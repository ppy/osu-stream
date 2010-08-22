namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSVam
    {
        BASS_VAM_HARDWARE = 1,
        BASS_VAM_SOFTWARE = 2,
        BASS_VAM_TERM_DIST = 8,
        BASS_VAM_TERM_PRIO = 0x10,
        BASS_VAM_TERM_TIME = 4
    }
}

