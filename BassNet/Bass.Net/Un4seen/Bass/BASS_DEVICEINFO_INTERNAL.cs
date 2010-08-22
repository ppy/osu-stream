namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct BASS_DEVICEINFO_INTERNAL
    {
        public IntPtr name;
        public IntPtr driver;
        public BASSDeviceInfo flags;
    }
}

