namespace Un4seen.Bass
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class BASS_PLUGININFO
    {
        public int formatc;
        public BASS_PLUGINFORM[] formats;
        public int version;

        private BASS_PLUGININFO()
        {
        }

        public BASS_PLUGININFO(IntPtr pluginInfoPtr)
        {
            if (pluginInfoPtr != IntPtr.Zero)
            {
                BASS_PLUGININFO _plugininfo = (BASS_PLUGININFO)Marshal.PtrToStructure(pluginInfoPtr, typeof(BASS_PLUGININFO));
                version = _plugininfo.version;
                formatc = _plugininfo.formatc;
                formats = new BASS_PLUGINFORM[formatc];
                ReadArrayStructure(formatc, pluginInfoPtr);
            }
        }

        internal BASS_PLUGININFO(int Version, BASS_PLUGINFORM[] Formats)
        {
            version = Version;
            formatc = Formats.Length;
            formats = Formats;
        }

        internal BASS_PLUGININFO(int ver, int count, IntPtr fPtr)
        {
            version = ver;
            formatc = count;
            if (fPtr != IntPtr.Zero)
            {
                formats = new BASS_PLUGINFORM[count];
                ReadArrayStructure(formatc, fPtr);
            }
        }

        private unsafe void ReadArrayStructure(int count, IntPtr p)
        {
            for (int i = 0; i < count; i++)
            {
                formats[i] = (BASS_PLUGINFORM) Marshal.PtrToStructure(p, typeof(BASS_PLUGINFORM));
                p = new IntPtr(p.ToInt64() + Marshal.SizeOf(formats[i]));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", version, formatc);
        }
    }
}

