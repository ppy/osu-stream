namespace Un4seen.Bass
{
    
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class BASS_PLUGINFORM
    {
        public BASSChannelType ctype;
        [MarshalAs(UnmanagedType.LPStr)]
        public string name;
        [MarshalAs(UnmanagedType.LPStr)]
        public string exts;
        public BASS_PLUGINFORM()
        {
            name = string.Empty;
            exts = string.Empty;
        }

        public BASS_PLUGINFORM(string Name, string Extensions, BASSChannelType ChannelType)
        {
            name = string.Empty;
            exts = string.Empty;
            ctype = ChannelType;
            name = Name;
            exts = Extensions;
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", name, exts);
        }
    }
}

