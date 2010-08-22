namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_INFO
    {
        public BASSInfo flags;
        public int hwsize;
        public int hwfree;
        public int freesam;
        public int free3d;
        public int minrate;
        public int maxrate;
        public bool eax;
        public int minbuf = 500;
        public int dsver;
        public int latency;
        public BASSInit initflags;
        public int speakers;
        public int freq;
        public override string ToString()
        {
            return string.Format("Speakers={0}, MinRate={1}, MaxRate={2}, DX={3}, EAX={4}", new object[] { speakers, minrate, maxrate, dsver, eax });
        }

        public bool SupportsContinuousRate
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_CONTINUOUSRATE) != BASSInfo.DSCAPS_NONE);
            }
        }
        public bool SupportsDirectSound
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_EMULDRIVER) == BASSInfo.DSCAPS_NONE);
            }
        }
        public bool IsCertified
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_CERTIFIED) != BASSInfo.DSCAPS_NONE);
            }
        }
        public bool SupportsMonoSamples
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_SECONDARYMONO) != BASSInfo.DSCAPS_NONE);
            }
        }
        public bool SupportsStereoSamples
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_SECONDARYSTEREO) != BASSInfo.DSCAPS_NONE);
            }
        }
        public bool Supports8BitSamples
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_SECONDARY8BIT) != BASSInfo.DSCAPS_NONE);
            }
        }
        public bool Supports16BitSamples
        {
            get
            {
                return ((flags & BASSInfo.DSCAPS_SECONDARY16BIT) != BASSInfo.DSCAPS_NONE);
            }
        }
    }
}

