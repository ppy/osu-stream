namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BASS_CHANNELINFO_INTERNAL
    {
        public int freq;
        public int chans;
        public BASSFlag flags;
        public BASSChannelType ctype;
        public int origres;
        public int plugin;
        public int sample;
        public IntPtr filename;
    }
}

