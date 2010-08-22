namespace Un4seen.Bass
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public delegate bool RECORDPROC(int handle, IntPtr buffer, int length, IntPtr user);
}

