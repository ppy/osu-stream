namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSInfo
    {
        DSCAPS_CERTIFIED = 0x40,
        DSCAPS_CONTINUOUSRATE = 0x10,
        DSCAPS_EMULDRIVER = 0x20,
        DSCAPS_NONE = 0,
        DSCAPS_SECONDARY16BIT = 0x800,
        DSCAPS_SECONDARY8BIT = 0x400,
        DSCAPS_SECONDARYMONO = 0x100,
        DSCAPS_SECONDARYSTEREO = 0x200
    }
}

