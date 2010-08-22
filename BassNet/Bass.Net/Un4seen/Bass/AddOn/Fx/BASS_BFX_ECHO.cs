namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_BFX_ECHO
    {
        public float fLevel;
        public int lDelay;
        public BASS_BFX_ECHO()
        {
            lDelay = 0x4b0;
        }

        public BASS_BFX_ECHO(float Level, int Delay)
        {
            lDelay = 0x4b0;
            fLevel = Level;
            lDelay = Delay;
        }
    }
}

