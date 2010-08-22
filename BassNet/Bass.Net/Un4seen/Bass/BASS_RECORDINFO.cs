namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_RECORDINFO
    {
        public BASSRecordInfo flags;
        public BASSRecordFormat formats;
        public int inputs;
        public bool singlein;
        public int freq;
        public override string ToString()
        {
            return string.Format("Inputs={0}, SingleIn={1}", inputs, singlein);
        }

        public bool SupportsDirectSound
        {
            get
            {
                return ((flags & BASSRecordInfo.DSCAPS_EMULDRIVER) == BASSRecordInfo.DSCAPS_NONE);
            }
        }
        public bool IsCertified
        {
            get
            {
                return ((flags & BASSRecordInfo.DSCAPS_CERTIFIED) != BASSRecordInfo.DSCAPS_NONE);
            }
        }
    }
}

