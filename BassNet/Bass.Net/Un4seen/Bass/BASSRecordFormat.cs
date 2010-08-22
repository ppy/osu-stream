namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSRecordFormat
    {
        WAVE_FORMAT_1M08 = 1,
        WAVE_FORMAT_1M16 = 4,
        WAVE_FORMAT_1S08 = 2,
        WAVE_FORMAT_1S16 = 8,
        WAVE_FORMAT_2M08 = 0x10,
        WAVE_FORMAT_2M16 = 0x40,
        WAVE_FORMAT_2S08 = 0x20,
        WAVE_FORMAT_2S16 = 0x80,
        WAVE_FORMAT_48M08 = 0x1000,
        WAVE_FORMAT_48M16 = 0x4000,
        WAVE_FORMAT_48S08 = 0x2000,
        WAVE_FORMAT_48S16 = 0x8000,
        WAVE_FORMAT_4M08 = 0x100,
        WAVE_FORMAT_4M16 = 0x400,
        WAVE_FORMAT_4S08 = 0x200,
        WAVE_FORMAT_4S16 = 0x800,
        WAVE_FORMAT_96M08 = 0x10000,
        WAVE_FORMAT_96M16 = 0x40000,
        WAVE_FORMAT_96S08 = 0x20000,
        WAVE_FORMAT_96S16 = 0x80000,
        WAVE_FORMAT_UNKNOWN = 0
    }
}

