namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSData
    {
        BASS_DATA_AVAILABLE = 0,
        BASS_DATA_FFT_INDIVIDUAL = 0x10,
        BASS_DATA_FFT_NOWINDOW = 0x20,
        BASS_DATA_FFT1024 = -2147483646,
        BASS_DATA_FFT2048 = -2147483645,
        BASS_DATA_FFT256 = -2147483648,
        BASS_DATA_FFT4096 = -2147483644,
        BASS_DATA_FFT512 = -2147483647,
        BASS_DATA_FFT8192 = -2147483643,
        BASS_DATA_FLOAT = 0x40000000
    }
}

