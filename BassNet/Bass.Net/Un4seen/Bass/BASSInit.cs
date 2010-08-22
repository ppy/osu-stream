namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSInit
    {
        BASS_DEVICE_3D = 4,
        BASS_DEVICE_8BITS = 1,
        BASS_DEVICE_CPSPEAKERS = 0x400,
        BASS_DEVICE_DEFAULT = 0,
        BASS_DEVICE_LATENCY = 0x100,
        BASS_DEVICE_MONO = 2,
        BASS_DEVICE_NOSPEAKER = 0x1000,
        BASS_DEVICE_SPEAKERS = 0x800
    }
}

