namespace Un4seen.Bass
{
    using System;

    public sealed class BASS_CHANNELINFO
    {
        internal BASS_CHANNELINFO_INTERNAL _internal;
        public int chans;
        public BASSChannelType ctype;
        public string filename = string.Empty;
        public BASSFlag flags;
        public int freq;
        public int origres;
        public int plugin;
        public int sample;

        public override string ToString()
        {
            return string.Format("{0}, {1}Hz, {2}, {3}bit", new object[] { Utils.BASSChannelTypeToString(ctype), freq, Utils.ChannelNumberToString(chans), (origres == 0) ? 0x10 : origres });
        }

        public bool Is32bit
        {
            get
            {
                return ((flags & BASSFlag.BASS_SAMPLE_FLOAT) != BASSFlag.BASS_DEFAULT);
            }
        }

        public bool Is8bit
        {
            get
            {
                return ((flags & BASSFlag.BASS_FX_BPM_BKGRND) != BASSFlag.BASS_DEFAULT);
            }
        }

        public bool IsDecodingChannel
        {
            get
            {
                return ((flags & BASSFlag.BASS_MUSIC_DECODE) != BASSFlag.BASS_DEFAULT);
            }
        }
    }
}

