namespace Un4seen.Bass
{
    using System;

    public sealed class BASS_DEVICEINFO
    {
        internal BASS_DEVICEINFO_INTERNAL _internal;
        public string driver = string.Empty;
        public BASSDeviceInfo flags;
        public string name = string.Empty;

        public override string ToString()
        {
            return name;
        }

        public bool IsDefault
        {
            get
            {
                return ((flags & BASSDeviceInfo.BASS_DEVICE_DEFAULT) != BASSDeviceInfo.BASS_DEVICE_NONE);
            }
        }

        public bool IsEnabled
        {
            get
            {
                return ((flags & BASSDeviceInfo.BASS_DEVICE_ENABLED) != BASSDeviceInfo.BASS_DEVICE_NONE);
            }
        }

        public bool IsInitialized
        {
            get
            {
                return ((flags & BASSDeviceInfo.BASS_DEVICE_INIT) != BASSDeviceInfo.BASS_DEVICE_NONE);
            }
        }
    }
}

