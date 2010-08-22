namespace Un4seen.Bass
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void DSPPROC(int handle, int channel, IntPtr buffer, int length, IntPtr user);
}

