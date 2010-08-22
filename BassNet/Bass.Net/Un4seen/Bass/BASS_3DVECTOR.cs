namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_3DVECTOR
    {
        public float x;
        public float y;
        public float z;
        public BASS_3DVECTOR()
        {
        }

        public BASS_3DVECTOR(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override string ToString()
        {
            return string.Format("X={0}, Y={1}, Z={2}", x, y, z);
        }
    }
}

