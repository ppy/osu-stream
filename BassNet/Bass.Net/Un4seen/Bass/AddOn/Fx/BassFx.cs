namespace Un4seen.Bass.AddOn.Fx
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public sealed class BassFx
    {
        private static int _myModuleHandle = 0;
        private static string _myModuleName = "bass_fx.dll";
        public const int BASSFXVERSION = 0x204;

        private BassFx()
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatCallbackReset(int handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatCallbackSet(int handle, BPMBEATPROC proc, IntPtr user);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatDecodeGet(int channel, double startSec, double endSec, BASSFXBpm flags, BPMBEATPROC proc, IntPtr user);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatFree(int handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatGetParameters(int handle, ref float bandwidth, ref float centerfreq, ref float beat_rtime);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatGetParameters(int handle, [In, Out, MarshalAs(UnmanagedType.AsAny)] object bandwidth, [In, Out, MarshalAs(UnmanagedType.AsAny)] object centerfreq, [In, Out, MarshalAs(UnmanagedType.AsAny)] object beat_rtime);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_BeatSetParameters(int handle, float bandwidth, float centerfreq, float beat_rtime);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_CallbackReset(int handle);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_CallbackSet(int handle, BPMPROC proc, double period, int minMaxBPM, BASSFXBpm flags, IntPtr user);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern float BASS_FX_BPM_DecodeGet(int channel, double startSec, double endSec, int minMaxBPM, BASSFXBpm flags, BPMPROCESSPROC proc);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FX_BPM_Free(int handle);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern float BASS_FX_BPM_Translate(int handle, float val2tran, BASSFXBpmTrans trans);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_FX_GetVersion();
        public static Version BASS_FX_GetVersion(int fieldcount)
        {
            if (fieldcount < 1)
            {
                fieldcount = 1;
            }
            if (fieldcount > 4)
            {
                fieldcount = 4;
            }
            int num = BASS_FX_GetVersion();
            Version version = new Version(2, 3);
            switch (fieldcount)
            {
                case 1:
                    return new Version((num >> 0x18) & 0xff, 0);

                case 2:
                    return new Version((num >> 0x18) & 0xff, (num >> 0x10) & 0xff);

                case 3:
                    return new Version((num >> 0x18) & 0xff, (num >> 0x10) & 0xff, (num >> 8) & 0xff);

                case 4:
                    return new Version((num >> 0x18) & 0xff, (num >> 0x10) & 0xff, (num >> 8) & 0xff, num & 0xff);
            }
            return version;
        }

        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_FX_ReverseCreate(int channel, float dec_block, BASSFlag flags);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_FX_ReverseGetSource(int channel);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_FX_TempoCreate(int channel, BASSFlag flags);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern float BASS_FX_TempoGetRateRatio(int chan);
        [DllImport("bass_fx.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_FX_TempoGetSource(int channel);
        public static bool FreeMe()
        {
            return Utils.FreeLib(ref _myModuleHandle);
        }

        public static bool LoadMe()
        {
            return Utils.LoadLib(_myModuleName, ref _myModuleHandle);
        }

        public static bool LoadMe(string path)
        {
            return Utils.LoadLib(Path.Combine(path, _myModuleName), ref _myModuleHandle);
        }
    }
}

