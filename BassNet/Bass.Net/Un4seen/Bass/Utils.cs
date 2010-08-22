using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Un4seen.Bass
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class Utils
    {
        private static readonly ModuleBuilder _builder;
        private static readonly Hashtable _methodLookup;
        private static Random _autoRandomizer = new Random();

        static Utils()
        {
            AssemblyName name = new AssemblyName();
            name.Name = "dynamicBASSNET";
            _builder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run).DefineDynamicModule(
                    "BASSNETdynmod");
            _methodLookup = new Hashtable();
        }

        private Utils()
        {
        }

        public static bool Is64Bit
        {
            get
            {
                if (IntPtr.Size != 8)
                {
                    return false;
                }
                return true;
            }
        }

        public static short AbsSignMax(short val1, short val2)
        {
            if (val1 != -32768)
            {
                if (val2 == -32768)
                {
                    return val2;
                }
                if (Math.Abs(val1) < Math.Abs(val2))
                {
                    return val2;
                }
            }
            return val1;
        }

        public static float AbsSignMax(float val1, float val2)
        {
            if (Math.Abs(val1) < Math.Abs(val2))
            {
                return val2;
            }
            return val1;
        }

        public static string BASSAddOnGetPluginFileFilter(Hashtable plugins, string allFormatName)
        {
            return BASSAddOnGetPluginFileFilter(plugins, allFormatName, true);
        }

        public static string BASSAddOnGetPluginFileFilter(Hashtable plugins, string allFormatName, bool includeBASS)
        {
            string name = string.Empty;
            string exts = string.Empty;
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            if (includeBASS)
            {
                foreach (BASS_PLUGINFORM bass_pluginform in Bass.BASS_PluginGetInfo(0).formats)
                {
                    name = bass_pluginform.name;
                    exts = bass_pluginform.exts;
                    builder.Append("|" + name + "|" + exts);
                    builder2.Append(";" + exts);
                }
            }
            if (plugins != null)
            {
                foreach (int num in plugins.Keys)
                {
                    foreach (BASS_PLUGINFORM bass_pluginform2 in Bass.BASS_PluginGetInfo(num).formats)
                    {
                        name = bass_pluginform2.name;
                        exts = bass_pluginform2.exts;
                        builder.Append("|" + name + "|" + exts);
                        builder2.Append(";" + exts);
                    }
                }
            }
            if ((allFormatName != string.Empty) && (allFormatName != null))
            {
                builder.Insert(0, allFormatName + "|" + builder2.ToString() + "|");
            }
            if (builder[0] == '|')
            {
                builder.Remove(0, 1);
            }
            return builder.ToString();
        }

        public static string BASSAddOnGetSupportedFileExtensions(string file)
        {
            string supportedStreamExtensions = Bass.SupportedStreamExtensions;
            if (!string.IsNullOrEmpty(file))
            {
                if (file.ToLower() == "music")
                {
                    return Bass.SupportedMusicExtensions;
                }
            }
            return supportedStreamExtensions;
        }

        public static string BASSAddOnGetSupportedFileExtensions(Hashtable plugins, bool includeBASS)
        {
            StringBuilder builder = new StringBuilder();
            if (includeBASS)
            {
                builder.Append(BASSAddOnGetSupportedFileExtensions(null));
                builder.Append(";");
            }
            if (plugins != null)
            {
                foreach (string str in plugins.Values)
                {
                    builder.Append(BASSAddOnGetSupportedFileExtensions(str));
                    builder.Append(";");
                }
            }
            if ((builder.Length > 0) && (builder[builder.Length - 1] == ';'))
            {
                builder.Remove(builder.Length, 1);
            }
            return builder.ToString();
        }

        public static string BASSAddOnGetSupportedFileFilter(Hashtable plugins, string allFormatName)
        {
            return BASSAddOnGetSupportedFileFilter(plugins, allFormatName, true);
        }

        public static string BASSAddOnGetSupportedFileFilter(Hashtable plugins, string allFormatName, bool includeBASS)
        {
            string str = string.Empty;
            string str2 = string.Empty;
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            if (includeBASS)
            {
                str = BASSAddOnGetSupportedFileName(null);
                str2 = BASSAddOnGetSupportedFileExtensions(null);
                builder.Append(str + " (" + str2 + ")|" + str2);
                builder2.Append(str2);
                str = BASSAddOnGetSupportedFileName("music");
                str2 = BASSAddOnGetSupportedFileExtensions("music");
                builder.Append("|" + str + " (" + str2 + ")|" + str2);
                builder2.Append(";" + str2);
            }
            if (plugins != null)
            {
                foreach (string str3 in plugins.Values)
                {
                    str = BASSAddOnGetSupportedFileName(str3);
                    str2 = BASSAddOnGetSupportedFileExtensions(str3);
                    builder.Append("|" + str + " (" + str2 + ")|" + str2);
                    builder2.Append(";" + str2);
                }
            }
            if ((allFormatName != string.Empty) && (allFormatName != null))
            {
                builder.Insert(0, allFormatName + "|" + builder2.ToString() + "|");
            }
            if (builder[0] == '|')
            {
                builder.Remove(0, 1);
            }
            return builder.ToString();
        }

        public static string BASSAddOnGetSupportedFileName(string file)
        {
            string supportedStreamName = Bass.SupportedStreamName;
            if ((file != null) && (file != string.Empty))
            {
                if (file.ToLower() == "music")
                {
                    return "MOD Music";
                }
            }
            return supportedStreamName;
        }

        public static bool BASSAddOnIsFileSupported(Hashtable plugins, string filename)
        {
            if ((filename == null) || (filename == string.Empty))
            {
                return false;
            }
            string str = Path.GetExtension(filename).ToLower();
            if (BASSAddOnGetSupportedFileExtensions(null).ToLower().IndexOf(str) >= 0)
            {
                return true;
            }
            if (BASSAddOnGetSupportedFileExtensions("music").ToLower().IndexOf(str) >= 0)
            {
                return true;
            }
            if (plugins != null)
            {
                foreach (string str3 in plugins.Values)
                {
                    if (BASSAddOnGetSupportedFileExtensions(str3).ToLower().IndexOf(str) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string BASSChannelTypeToString(BASSChannelType ctype)
        {
            string str = "???";
            if ((ctype & BASSChannelType.BASS_CTYPE_STREAM_WAV) > BASSChannelType.BASS_CTYPE_UNKNOWN)
            {
                ctype = BASSChannelType.BASS_CTYPE_STREAM_WAV;
            }
            BASSChannelType type = ctype;
            if (type <= BASSChannelType.BASS_CTYPE_STREAM_FLAC)
            {
                if (type <= BASSChannelType.BASS_CTYPE_STREAM_CD)
                {
                    if (type <= BASSChannelType.BASS_CTYPE_MUSIC_MO3)
                    {
                        switch (type)
                        {
                            case BASSChannelType.BASS_CTYPE_SAMPLE:
                                return "sample";

                            case BASSChannelType.BASS_CTYPE_RECORD:
                                return "recording";

                            case BASSChannelType.BASS_CTYPE_MUSIC_MO3:
                                return "MO3";
                        }
                        return str;
                    }
                    switch (type)
                    {
                        case BASSChannelType.BASS_CTYPE_STREAM:
                            return "custom stream";

                        case (BASSChannelType.BASS_CTYPE_STREAM | BASSChannelType.BASS_CTYPE_SAMPLE):
                            return str;

                        case BASSChannelType.BASS_CTYPE_STREAM_OGG:
                            return "OGG";

                        case BASSChannelType.BASS_CTYPE_STREAM_MP1:
                            return "MP1";

                        case BASSChannelType.BASS_CTYPE_STREAM_MP2:
                            return "MP2";

                        case BASSChannelType.BASS_CTYPE_STREAM_MP3:
                            return "MP3";

                        case BASSChannelType.BASS_CTYPE_STREAM_AIFF:
                            return "AIFF";

                        case BASSChannelType.BASS_CTYPE_STREAM_CD:
                            return "CDA";
                    }
                    return str;
                }
                if (type <= BASSChannelType.BASS_CTYPE_STREAM_WV_LH)
                {
                    switch (type)
                    {
                        case BASSChannelType.BASS_CTYPE_STREAM_WMA:
                            return "WMA";

                        case BASSChannelType.BASS_CTYPE_STREAM_WMA_MP3:
                            return "MP3";

                        case BASSChannelType.BASS_CTYPE_STREAM_WV:
                            return "Wavpack";

                        case BASSChannelType.BASS_CTYPE_STREAM_WV_H:
                            return "Wavpack";

                        case BASSChannelType.BASS_CTYPE_STREAM_WV_L:
                            return "Wavpack";

                        case BASSChannelType.BASS_CTYPE_STREAM_WV_LH:
                            return "Wavpack";
                    }
                    return str;
                }
                switch (type)
                {
                    case BASSChannelType.BASS_CTYPE_STREAM_OFR:
                        return "Optimfrog";

                    case BASSChannelType.BASS_CTYPE_STREAM_APE:
                        return "APE";

                    case BASSChannelType.BASS_CTYPE_STREAM_FLAC:
                        return "FLAC";
                }
                return str;
            }
            if (type <= BASSChannelType.BASS_CTYPE_STREAM_ALAC)
            {
                if (type <= BASSChannelType.BASS_CTYPE_STREAM_MP4)
                {
                    switch (type)
                    {
                        case BASSChannelType.BASS_CTYPE_STREAM_AAC:
                            return "AAC";

                        case BASSChannelType.BASS_CTYPE_STREAM_MP4:
                            return "MP4";

                        case BASSChannelType.BASS_CTYPE_STREAM_MPC:
                            return "MPC";
                    }
                    return str;
                }
                switch (type)
                {
                    case BASSChannelType.BASS_CTYPE_STREAM_SPX:
                        return "Speex";

                    case BASSChannelType.BASS_CTYPE_STREAM_MIDI:
                        return "MIDI";

                    case BASSChannelType.BASS_CTYPE_STREAM_ALAC:
                        return "ALAC";
                }
                return str;
            }
            if (type <= BASSChannelType.BASS_CTYPE_STREAM_AC3)
            {
                switch (type)
                {
                    case BASSChannelType.BASS_CTYPE_STREAM_TTA:
                        return "TTA";

                    case BASSChannelType.BASS_CTYPE_STREAM_AC3:
                        return "AC3";
                }
                return str;
            }
            switch (type)
            {
                case BASSChannelType.BASS_CTYPE_MUSIC_MOD:
                    return "MOD";

                case BASSChannelType.BASS_CTYPE_MUSIC_MTM:
                    return "MTM";

                case BASSChannelType.BASS_CTYPE_MUSIC_S3M:
                    return "S3M";

                case BASSChannelType.BASS_CTYPE_MUSIC_XM:
                    return "XM";

                case BASSChannelType.BASS_CTYPE_MUSIC_IT:
                    return "IT";

                case BASSChannelType.BASS_CTYPE_STREAM_WAV:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV_PCM:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV_FLOAT:
                    return "WAV";

                case (BASSChannelType.BASS_CTYPE_STREAM_WAV | BASSChannelType.BASS_CTYPE_STREAM_OGG):
                    return str;
            }
            return str;
        }

        public static float BPM2Seconds(float bpm)
        {
            if (bpm != 0f)
            {
                return (60f/bpm);
            }
            return -1f;
        }

        public static string ChannelNumberToString(int chans)
        {
            string str = chans.ToString();
            switch (chans)
            {
                case 1:
                    return "mono";

                case 2:
                    return "stereo";

                case 3:
                    return "2.1";

                case 4:
                    return "2.2";

                case 5:
                    return "4.1";

                case 6:
                    return "5.1";

                case 7:
                    return "5.2";

                case 8:
                    return "7.1";
            }
            return str;
        }

        public static double DBToLevel(double dB, double maxLevel)
        {
            return (maxLevel*Math.Pow(10.0, dB/20.0));
        }

        public static int DBToLevel(double dB, int maxLevel)
        {
            return (int) Math.Round((double) (maxLevel*Math.Pow(10.0, dB/20.0)));
        }

        public static long DecodeAllData(int channel, bool autoFree)
        {
            long num = 0L;
            byte[] buffer = new byte[0x20000];
            while (Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                num += Bass.BASS_ChannelGetData(channel, buffer, 0x20000);
            }
            if (autoFree)
            {
                Bass.BASS_StreamFree(channel);
            }
            return num;
        }

        public static bool DetectCuePoints(string filename, float blockSize, ref double cueInPos, ref double cueOutPos,
                                           double dBIn, double dBOut, int findZeroCrossing)
        {
            int handle =
                Bass.BASS_StreamCreateFile(filename, 0L, 0L, BASSFlag.BASS_MUSIC_DECODE | BASSFlag.BASS_SAMPLE_OVER_POS);
            if (handle == 0)
            {
                return false;
            }
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            if (!Bass.BASS_ChannelGetInfo(handle, info))
            {
                return false;
            }
            if (dBIn > 0.0)
            {
                dBIn = 0.0;
            }
            else if (dBIn < -90.0)
            {
                dBIn = -90.0;
            }
            if (dBOut > 0.0)
            {
                dBOut = 0.0;
            }
            else if (dBOut < -90.0)
            {
                dBOut = -90.0;
            }
            if (blockSize > 30f)
            {
                blockSize = 30f;
            }
            else if (blockSize < 0.1f)
            {
                blockSize = 0.1f;
            }
            short num2 = (short) DBToLevel(dBIn, 0x7fff);
            short num3 = (short) DBToLevel(dBOut, 0x7fff);
            long num4 = Bass.BASS_ChannelGetLength(handle);
            long pos = 0L;
            long num6 = num4;
            int length = (int) Bass.BASS_ChannelSeconds2Bytes(handle, (double) blockSize);
            short[] buffer = new short[length/2];
            int num8 = 0;
            int num9 = 0;
            long num10 = 0L;
            bool flag = false;
            while (!flag && (num10 < num4))
            {
                num9 = Bass.BASS_ChannelGetData(handle, buffer, length);
                pos = num10;
                num8 = 0;
                while (!flag && (num8 < num9))
                {
                    if (ScanSampleLevel(buffer, num8, info.chans) < num2)
                    {
                        num8 += info.chans;
                    }
                    else
                    {
                        flag = true;
                        pos = num10 + num8;
                    }
                }
                if (!flag)
                {
                    num10 += num9;
                    if (num9 == 0)
                    {
                        num10 = num4;
                        pos = num10;
                    }
                }
            }
            if (flag && (pos < num4))
            {
                if (findZeroCrossing == 1)
                {
                    while ((num8 > 0) && !IsZeroCrossingPos(buffer, num8, num8 - info.chans, info.chans))
                    {
                        num8 -= info.chans;
                        pos -= info.chans;
                    }
                    if (pos < 0L)
                    {
                        pos = 0L;
                    }
                }
                else if (findZeroCrossing == 2)
                {
                    while ((num8 > 0) && (ScanSampleLevel(buffer, num8, info.chans) > (num2/2)))
                    {
                        num8 -= info.chans;
                        pos -= info.chans;
                    }
                    if (pos < 0L)
                    {
                        pos = 0L;
                    }
                }
            }
            else
            {
                pos = 0L;
            }
            num9 = 0;
            num10 = num4;
            flag = false;
            while (!flag && (num10 > 0L))
            {
                Bass.BASS_ChannelSetPosition(handle, ((num10 - length) >= 0L) ? (num10 - length) : 0L);
                num9 = Bass.BASS_ChannelGetData(handle, buffer, length);
                num6 = num10;
                num8 = num9;
                while (!flag && (num8 > 0))
                {
                    if (ScanSampleLevel(buffer, num8 - info.chans, info.chans) < num3)
                    {
                        num8 -= info.chans;
                    }
                    else
                    {
                        flag = true;
                        num6 = num10 - num8;
                    }
                }
                if (!flag)
                {
                    num10 -= num9;
                    if (num9 == 0)
                    {
                        num10 = 0L;
                        num6 = num4;
                    }
                }
            }
            if (flag && (num6 > 0L))
            {
                if (findZeroCrossing == 1)
                {
                    while ((num8 < num9) && !IsZeroCrossingPos(buffer, num8, num8 + info.chans, info.chans))
                    {
                        num8 += info.chans;
                        num6 += info.chans;
                    }
                }
                else if (findZeroCrossing == 2)
                {
                    while ((num8 < num9) && (ScanSampleLevel(buffer, num8, info.chans) > (num3/2)))
                    {
                        num8 += info.chans;
                        num6 += info.chans;
                    }
                }
            }
            else
            {
                num6 = num4;
            }
            cueInPos = Bass.BASS_ChannelBytes2Seconds(handle, pos);
            cueOutPos = Bass.BASS_ChannelBytes2Seconds(handle, num6);
            Bass.BASS_StreamFree(handle);
            return true;
        }

        public static void DMACopyMemory(IntPtr destination, IntPtr source, long length)
        {
            DMACopyMemory(destination, source, new IntPtr(length));
        }

        [DllImport("kernel32.dll", EntryPoint="CopyMemory", CharSet=CharSet.Auto)]
        private static extern void DMACopyMemory(IntPtr destination, IntPtr source, IntPtr length);

        public static void DMAFillMemory(IntPtr destination, long length, byte fill)
        {
            DMAFillMemory(destination, new IntPtr(length), fill);
        }

        [DllImport("kernel32.dll", EntryPoint="FillMemory", CharSet=CharSet.Auto)]
        private static extern void DMAFillMemory(IntPtr destination, IntPtr length, byte fill);

        public static void DMAMoveMemory(IntPtr destination, IntPtr source, long length)
        {
            DMAMoveMemory(destination, source, new IntPtr(length));
        }

        [DllImport("kernel32.dll", EntryPoint="MoveMemory", CharSet=CharSet.Auto)]
        private static extern void DMAMoveMemory(IntPtr destination, IntPtr source, IntPtr length);

        public static void DMAZeroMemory(IntPtr destination, long length)
        {
            DMAZeroMemory(destination, new IntPtr(length));
        }

        [DllImport("kernel32.dll", EntryPoint="ZeroMemory", CharSet=CharSet.Auto)]
        private static extern void DMAZeroMemory(IntPtr destination, IntPtr length);

        public static int FFTFrequency2Index(int frequency, int length, int samplerate)
        {
            int num = (int) Math.Round((double) ((length*frequency)/((double) samplerate)));
            if (num > ((length/2) - 1))
            {
                num = (length/2) - 1;
            }
            return num;
        }

        public static int FFTIndex2Frequency(int index, int length, int samplerate)
        {
            return (int) Math.Round((double) ((index*samplerate)/((double) length)));
        }

        public static string FixTimespan(double seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString();
        }

        public static string FixTimespan(double seconds, string format)
        {
            DateTime time = DateTime.Today.AddSeconds(seconds);
            switch (format)
            {
                case "HHMM":
                    return time.ToString("HH:mm");

                case "HHMMSS":
                    return time.ToString("HH:mm:ss");

                case "MMSS":
                    return time.ToString("mm:ss");

                case "MMSSFFF":
                    return time.ToString("mm:ss.fff");

                case "MMSSFF":
                    return time.ToString("mm:ss.ff");

                case "MMSSF":
                    return time.ToString("mm:ss.f");

                case "HHMMSSFFF":
                    return time.ToString("HH:mm:ss.fff");

                case "HHMMSSFF":
                    return time.ToString("HH:mm:ss.ff");

                case "HHMMSSF":
                    return time.ToString("HH:mm:ss.f");
            }
            return time.ToString(format);
        }

        internal static bool FreeLib(ref int handle)
        {
            if (handle != 0)
            {
                return LIBFreeLibrary(handle);
            }
            return true;
        }

        public static int GetLevel(byte[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = 0;
            short num3 = 0;
            int num4 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num4 = Math.Abs((int) ((buffer[i] - 0x80)*0x100));
                if (num4 > 0x7fff)
                {
                    num4 = 0x7fff;
                }
                if ((i%2) == 0)
                {
                    if (num4 > num2)
                    {
                        num2 = (short) num4;
                    }
                }
                else if (num4 > num3)
                {
                    num3 = (short) num4;
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
            }
            return MakeLong(num2, num3);
        }

        public static int GetLevel(short[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = 0;
            short num3 = 0;
            int num4 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num4 = Math.Abs((int) buffer[i]);
                if (num4 > 0x7fff)
                {
                    num4 = 0x7fff;
                }
                if ((i%2) == 0)
                {
                    if (num4 > num2)
                    {
                        num2 = (short) num4;
                    }
                }
                else if (num4 > num3)
                {
                    num3 = (short) num4;
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
            }
            return MakeLong(num2, num3);
        }

        public static int GetLevel(float[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = 0;
            short num3 = 0;
            int num4 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num4 = (int) Math.Round((double) Math.Abs((float) (buffer[i]*32768f)));
                if (num4 > 0x7fff)
                {
                    num4 = 0x7fff;
                }
                if ((i%2) == 0)
                {
                    if (num4 > num2)
                    {
                        num2 = (short) num4;
                    }
                }
                else if (num4 > num3)
                {
                    num3 = (short) num4;
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
            }
            return MakeLong(num2, num3);
        }

        public static unsafe int GetLevel(IntPtr buffer, int chans, int bps, int startIndex, int length)
        {
            if (buffer == IntPtr.Zero)
            {
                return 0;
            }
            if (((bps == 0x10) || (bps == 0x20)) || (bps == 8))
            {
                bps /= 8;
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            short num = 0;
            short num2 = 0;
            int num3 = 0;
            int num4 = startIndex + length;
            if (bps == 2)
            {
                short* numPtr = (short*) buffer;
                for (int i = startIndex; i < num4; i++)
                {
                    num3 = Math.Abs((int) numPtr[i]);
                    if (num3 > 0x7fff)
                    {
                        num3 = 0x7fff;
                    }
                    if ((i%2) == 0)
                    {
                        if (num3 > num)
                        {
                            num = (short) num3;
                        }
                    }
                    else if (num3 > num2)
                    {
                        num2 = (short) num3;
                    }
                }
            }
            else if (bps == 4)
            {
                float* numPtr2 = (float*) buffer;
                for (int j = startIndex; j < num4; j++)
                {
                    num3 = (int) Math.Round((double) Math.Abs((float) (numPtr2[j*4]*32768f)));
                    if (num3 > 0x7fff)
                    {
                        num3 = 0x7fff;
                    }
                    if ((j%2) == 0)
                    {
                        if (num3 > num)
                        {
                            num = (short) num3;
                        }
                    }
                    else if (num3 > num2)
                    {
                        num2 = (short) num3;
                    }
                }
            }
            else
            {
                byte* numPtr3 = (byte*) buffer;
                for (int k = startIndex; k < num4; k++)
                {
                    num3 = Math.Abs((int) ((numPtr3[k] - 0x80)*0x100));
                    if (num3 > 0x7fff)
                    {
                        num3 = 0x7fff;
                    }
                    if ((k%2) == 0)
                    {
                        if (num3 > num)
                        {
                            num = (short) num3;
                        }
                    }
                    else if (num3 > num2)
                    {
                        num2 = (short) num3;
                    }
                }
            }
            if (chans == 1)
            {
                num2 = num = Math.Max(num, num2);
            }
            return MakeLong(num, num2);
        }

        public static long GetLevel2(byte[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0L;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = -32768;
            short num3 = -32768;
            short num4 = 0x7fff;
            short num5 = 0x7fff;
            short num6 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num6 = (short) ((buffer[i] - 0x80)*0x100);
                if ((i%2) == 0)
                {
                    if (num6 > num2)
                    {
                        num2 = num6;
                    }
                    if (num6 < num4)
                    {
                        num4 = num6;
                    }
                }
                else
                {
                    if (num6 > num3)
                    {
                        num3 = num6;
                    }
                    if (num6 < num5)
                    {
                        num5 = num6;
                    }
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
                num5 = num4 = Math.Min(num4, num5);
            }
            return MakeLong64(MakeLong(num4, num2), MakeLong(num5, num3));
        }

        public static long GetLevel2(short[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0L;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = -32768;
            short num3 = -32768;
            short num4 = 0x7fff;
            short num5 = 0x7fff;
            short num6 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num6 = buffer[i];
                if ((i%2) == 0)
                {
                    if (num6 > num2)
                    {
                        num2 = num6;
                    }
                    if (num6 < num4)
                    {
                        num4 = num6;
                    }
                }
                else
                {
                    if (num6 > num3)
                    {
                        num3 = num6;
                    }
                    if (num6 < num5)
                    {
                        num5 = num6;
                    }
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
                num5 = num4 = Math.Min(num4, num5);
            }
            return MakeLong64(MakeLong(num4, num2), MakeLong(num5, num3));
        }

        public static long GetLevel2(float[] buffer, int chans, int startIndex, int length)
        {
            if (buffer == null)
            {
                return 0L;
            }
            int num = buffer.Length;
            if ((startIndex > (num - 1)) || (startIndex < 0))
            {
                startIndex = 0;
            }
            if ((length > num) || (length < 0))
            {
                length = num;
            }
            if ((startIndex + length) > num)
            {
                length = num - startIndex;
            }
            short num2 = -32768;
            short num3 = -32768;
            short num4 = 0x7fff;
            short num5 = 0x7fff;
            int num6 = 0;
            num = startIndex + length;
            for (int i = startIndex; i < num; i++)
            {
                num6 = (int) Math.Round((double) (buffer[i]*32768.0));
                if (num6 > 0x7fff)
                {
                    num6 = 0x7fff;
                }
                else if (num6 < -32768)
                {
                    num6 = -32768;
                }
                if ((i%2) == 0)
                {
                    if (num6 > num2)
                    {
                        num2 = (short) num6;
                    }
                    if (num6 < num4)
                    {
                        num4 = (short) num6;
                    }
                }
                else
                {
                    if (num6 > num3)
                    {
                        num3 = (short) num6;
                    }
                    if (num6 < num5)
                    {
                        num5 = (short) num6;
                    }
                }
            }
            if (chans == 1)
            {
                num3 = num2 = Math.Max(num2, num3);
                num5 = num4 = Math.Min(num4, num5);
            }
            return MakeLong64(MakeLong(num4, num2), MakeLong(num5, num3));
        }

        public static unsafe long GetLevel2(IntPtr buffer, int chans, int bps, int startIndex, int length)
        {
            if (buffer == IntPtr.Zero)
            {
                return 0L;
            }
            if (((bps == 0x10) || (bps == 0x20)) || (bps == 8))
            {
                bps /= 8;
            }
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            short num = -32768;
            short num2 = -32768;
            short num3 = 0x7fff;
            short num4 = 0x7fff;
            int num5 = 0;
            int num6 = startIndex + length;
            if (bps == 2)
            {
                short* numPtr = (short*) buffer;
                for (int i = startIndex; i < num6; i++)
                {
                    num5 = numPtr[i];
                    if ((i%2) == 0)
                    {
                        if (num5 > num)
                        {
                            num = (short) num5;
                        }
                        if (num5 < num3)
                        {
                            num3 = (short) num5;
                        }
                    }
                    else
                    {
                        if (num5 > num2)
                        {
                            num2 = (short) num5;
                        }
                        if (num5 < num4)
                        {
                            num4 = (short) num5;
                        }
                    }
                }
            }
            else if (bps == 4)
            {
                float* numPtr2 = (float*) buffer;
                for (int j = startIndex; j < num6; j++)
                {
                    num5 = (int) Math.Round((double) (numPtr2[j*4]*32768f));
                    if (num5 > 0x7fff)
                    {
                        num5 = 0x7fff;
                    }
                    else if (num5 < -32768)
                    {
                        num5 = -32768;
                    }
                    if ((j%2) == 0)
                    {
                        if (num5 > num)
                        {
                            num = (short) num5;
                        }
                        if (num5 < num3)
                        {
                            num3 = (short) num5;
                        }
                    }
                    else
                    {
                        if (num5 > num2)
                        {
                            num2 = (short) num5;
                        }
                        if (num5 < num4)
                        {
                            num4 = (short) num5;
                        }
                    }
                }
            }
            else
            {
                byte* numPtr3 = (byte*) buffer;
                for (int k = startIndex; k < num6; k++)
                {
                    num5 = (numPtr3[k] - 0x80)*0x100;
                    if ((k%2) == 0)
                    {
                        if (num5 > num)
                        {
                            num = (short) num5;
                        }
                        if (num5 < num3)
                        {
                            num3 = (short) num5;
                        }
                    }
                    else
                    {
                        if (num5 > num2)
                        {
                            num2 = (short) num5;
                        }
                        if (num5 < num4)
                        {
                            num4 = (short) num5;
                        }
                    }
                }
            }
            if (chans == 1)
            {
                num2 = num = Math.Max(num, num2);
                num4 = num3 = Math.Min(num3, num4);
            }
            return MakeLong64(MakeLong(num3, num), MakeLong(num4, num2));
        }

        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static short HighWord(int dWord)
        {
            return (short) ((dWord >> 0x10) & 0xffff);
        }

        public static int HighWord(long qWord)
        {
            return (int) (((ulong) (qWord >> 0x20)) & 0xffffffffL);
        }

        public static int HighWord32(int dWord)
        {
            return ((dWord >> 0x10) & 0xffff);
        }

        public static object IntAsObject(IntPtr ptr, Type structureType)
        {
            return Marshal.PtrToStructure(ptr, structureType);
        }

        public static string IntPtrAsStringAnsi(IntPtr ansiPtr)
        {
            if (ansiPtr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ansiPtr);
            }
            return null;
        }

        public static string IntPtrAsStringUnicode(IntPtr unicodePtr)
        {
            if (unicodePtr != IntPtr.Zero)
            {
                return Marshal.PtrToStringUni(unicodePtr);
            }
            return null;
        }

        public static string IntPtrAsStringUtf8(IntPtr utf8Ptr)
        {
            if (!(utf8Ptr != IntPtr.Zero))
            {
                return null;
            }
            UTF8Encoding encoding = new UTF8Encoding();
            string str = Marshal.PtrToStringAnsi(utf8Ptr);
            if (str.Length != 0)
            {
                byte[] destination = new byte[str.Length];
                Marshal.Copy(utf8Ptr, destination, 0, str.Length);
                str = encoding.GetString(destination);
            }
            return str;
        }

        public static unsafe string[] IntPtrToArrayNullTermAnsi(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                ArrayList list = new ArrayList();
                while (true)
                {
                    string str = Marshal.PtrToStringAnsi(pointer);
                    if (str.Length == 0)
                    {
                        break;
                    }
                    list.Add(str);
                    pointer = new IntPtr((pointer.ToInt64() + str.Length) + 1);
                }
                if (list.Count > 0)
                {
                    return (string[]) list.ToArray(typeof (string));
                }
            }
            return null;
        }

        public static unsafe short[] IntPtrToArrayNullTermInt16(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                int index = 0;
                short* numPtr = (short*) pointer;
                while (numPtr[index] != 0)
                {
                    index++;
                }
                if (index > 0)
                {
                    short[] destination = new short[index];
                    Marshal.Copy(pointer, destination, 0, index);
                    return destination;
                }
            }
            return null;
        }

        public static unsafe int[] IntPtrToArrayNullTermInt32(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                int index = 0;
                int* numPtr = (int*) pointer;
                while (numPtr[index] != 0)
                {
                    index++;
                }
                if (index > 0)
                {
                    int[] destination = new int[index];
                    Marshal.Copy(pointer, destination, 0, index);
                    return destination;
                }
            }
            return null;
        }

        public static unsafe string[] IntPtrToArrayNullTermUnicode(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                ArrayList list = new ArrayList();
                while (true)
                {
                    string str = Marshal.PtrToStringUni(pointer);
                    if (str.Length == 0)
                    {
                        break;
                    }
                    list.Add(str);
                    pointer = new IntPtr((pointer.ToInt64() + (2*str.Length)) + 2);
                }
                if (list.Count > 0)
                {
                    return (string[]) list.ToArray(typeof (string));
                }
            }
            return null;
        }

        public static unsafe string[] IntPtrToArrayNullTermUtf8(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                ArrayList list = new ArrayList();
                UTF8Encoding encoding = new UTF8Encoding();
                string str = string.Empty;
                while (true)
                {
                    str = Marshal.PtrToStringAnsi(pointer);
                    if (str.Length == 0)
                    {
                        break;
                    }
                    byte[] destination = new byte[str.Length];
                    Marshal.Copy(pointer, destination, 0, str.Length);
                    pointer = new IntPtr((pointer.ToInt64() + str.Length) + 1);
                    str = encoding.GetString(destination);
                    list.Add(str);
                }
                if (list.Count > 0)
                {
                    return (string[]) list.ToArray(typeof (string));
                }
            }
            return null;
        }

        private static bool IsZeroCrossingPos(short[] buffer, int pos1, int pos2, int chans)
        {
            bool flag = false;
            try
            {
                if (chans > 1)
                {
                    short num = buffer[pos1];
                    short num2 = buffer[pos1 + 1];
                    short num3 = buffer[pos2];
                    short num4 = buffer[pos2 + 1];
                    if ((((num < 0) || (num3 > 0)) && ((num2 < 0) || (num4 > 0))) &&
                        (((num >= 0) || (num3 <= 0)) && ((num2 >= 0) || (num4 <= 0))))
                    {
                        return flag;
                    }
                    return true;
                }
                short num5 = buffer[pos1];
                short num6 = buffer[pos2];
                if (((num5 < 0) || (num6 > 0)) && ((num5 < 0) || (num6 > 0)))
                {
                    return flag;
                }
                flag = true;
            }
            catch
            {
            }
            return flag;
        }

        public static double LevelToDB(double level, double maxLevel)
        {
            return (20.0*Math.Log10(level/maxLevel));
        }

        public static double LevelToDB(int level, int maxLevel)
        {
            return (20.0*Math.Log10(((double) level)/((double) maxLevel)));
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", EntryPoint="FreeLibrary", CharSet=CharSet.Auto)]
        public static extern bool LIBFreeLibrary(int hModule);

        [DllImport("kernel32.dll", EntryPoint="LoadLibrary", CharSet=CharSet.Auto)]
        public static extern int LIBLoadLibrary([In, MarshalAs(UnmanagedType.LPTStr)] string fileName);

        internal static bool LoadLib(string moduleName, ref int handle)
        {
            if (handle == 0)
            {
                handle = LIBLoadLibrary(moduleName);
            }
            return (handle != 0);
        }

        public static short LowWord(int dWord)
        {
            return (short) (dWord & 0xffff);
        }

        public static int LowWord(long qWord)
        {
            return (int) (((ulong) qWord) & 0xffffffffL);
        }

        public static int LowWord32(int dWord)
        {
            return (dWord & 0xffff);
        }

        public static int MakeLong(short lowWord, short highWord)
        {
            return ((highWord << 0x10) | (lowWord & 0xffff));
        }

        public static int MakeLong(int lowWord, int highWord)
        {
            return ((highWord << 0x10) | (lowWord & 0xffff));
        }

        public static long MakeLong64(int lowWord, int highWord)
        {
            return ((highWord << 0x20) | (lowWord & ((long) 0xffffffffL)));
        }

        public static long MakeLong64(long lowWord, long highWord)
        {
            return ((highWord << 0x20) | (lowWord & ((long) 0xffffffffL)));
        }

        public static short MakeWord(byte lowByte, byte highByte)
        {
            return (short) ((highByte << 8) | lowByte);
        }

        public static double SampleDither(double sample, double factor, double max)
        {
            return (sample += (((_autoRandomizer.NextDouble() - _autoRandomizer.NextDouble())*factor)/max));
        }

        public static byte[] SampleTo16Bit(byte sample)
        {
            byte[] buffer = new byte[2];
            int num = (sample - 0x80)*0x100;
            if (num > 0x7fff)
            {
                num = 0x7fff;
            }
            else if (num < -32768)
            {
                num = -32768;
            }
            for (int i = 0; i < 2; i++)
            {
                buffer[i] = (byte) (num >> (i*8));
            }
            return buffer;
        }

        public static byte[] SampleTo16Bit(short sample)
        {
            byte[] buffer = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                buffer[i] = (byte) (sample >> (i*8));
            }
            return buffer;
        }

        public static byte[] SampleTo16Bit(float sample)
        {
            byte[] buffer = new byte[2];
            int num = (int) (sample*32768f);
            if (num > 0x7fff)
            {
                num = 0x7fff;
            }
            else if (num < -32768)
            {
                num = -32768;
            }
            for (int i = 0; i < 2; i++)
            {
                buffer[i] = (byte) (num >> (i*8));
            }
            return buffer;
        }

        public static short SampleTo16Bit(byte[] sample)
        {
            int length = sample.Length;
            if (length < 2)
            {
                return 0;
            }
            int num2 = 0;
            int num3 = 0;
            for (int i = length - 2; i < length; i++)
            {
                num2 |= sample[i] << (num3*8);
                num3++;
            }
            return (short) num2;
        }

        public static byte[] SampleTo24Bit(byte sample)
        {
            byte[] buffer = new byte[3];
            int num = (sample - 0x80)*0x10000;
            if (num > 0x7fffff)
            {
                num = 0x7fffff;
            }
            else if (num < -8388608)
            {
                num = -8388608;
            }
            for (int i = 0; i < 3; i++)
            {
                buffer[i] = (byte) (num >> (i*8));
            }
            return buffer;
        }

        public static byte[] SampleTo24Bit(short sample)
        {
            byte[] buffer = new byte[3];
            int num = sample*0x100;
            if (num > 0x7fffff)
            {
                num = 0x7fffff;
            }
            else if (num < -8388608)
            {
                num = -8388608;
            }
            for (int i = 0; i < 3; i++)
            {
                buffer[i] = (byte) (num >> (i*8));
            }
            return buffer;
        }

        public static int SampleTo24Bit(byte[] sample)
        {
            int length = sample.Length;
            if (length < 3)
            {
                return 0;
            }
            int num2 = 0;
            int num3 = 0;
            for (int i = length - 3; i < length; i++)
            {
                num2 |= sample[i] << (num3*8);
                num3++;
            }
            while (num2 > 0x7fffff)
            {
                num2 -= 0x800000;
            }
            return num2;
        }

        public static byte[] SampleTo24Bit(float sample)
        {
            byte[] buffer = new byte[3];
            int num = (int) (sample*8388608f);
            if (num > 0x7fffff)
            {
                num = 0x7fffff;
            }
            else if (num < -8388608)
            {
                num = -8388608;
            }
            for (int i = 0; i < 3; i++)
            {
                buffer[i] = (byte) (num >> (i*8));
            }
            return buffer;
        }

        public static float SampleTo32Bit(byte[] sample)
        {
            int length = sample.Length;
            if (length == 1)
            {
                return SampleTo32Bit(sample[0]);
            }
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                num2 |= sample[i] << (i*8);
            }
            if (sample[length - 1] > 0x7f)
            {
                num2 -= (int) (Math.Pow(256.0, (double) length)/2.0);
                return (-1f + ((float) (((double) num2)/(Math.Pow(256.0, (double) length)/2.0))));
            }
            return (float) (((double) num2)/(Math.Pow(256.0, (double) length)/2.0));
        }

        public static float SampleTo32Bit(byte sample)
        {
            return ((sample - 128f)/128f);
        }

        public static float SampleTo32Bit(short sample)
        {
            return (((float) sample)/32768f);
        }

        public static byte[] SampleTo8Bit(short sample)
        {
            byte[] buffer = new byte[1];
            int num = (sample/0x100) + 0x80;
            if (num > 0xff)
            {
                num = 0xff;
            }
            else if (num < 0)
            {
                num = 0;
            }
            buffer[0] = (byte) num;
            return buffer;
        }

        public static byte[] SampleTo8Bit(float sample)
        {
            byte[] buffer = new byte[1];
            int num = ((int) (sample*128f)) + 0x80;
            if (num > 0xff)
            {
                num = 0xff;
            }
            else if (num < 0)
            {
                num = 0;
            }
            buffer[0] = (byte) num;
            return buffer;
        }

        public static byte SampleTo8Bit(byte[] sample)
        {
            int length = sample.Length;
            if (length < 1)
            {
                return 0;
            }
            return sample[length - 1];
        }

        public static byte[] SampleTo8Bit(byte sample)
        {
            return new byte[] {sample};
        }

        private static short ScanSampleLevel(short[] buffer, int pos, int chans)
        {
            short num = 0;
            short num2 = 0;
            for (int i = 0; i < chans; i++)
            {
                try
                {
                    if ((pos + i) < buffer.Length)
                    {
                        num2 = Math.Abs(buffer[pos + i]);
                    }
                    else
                    {
                        num2 = 0;
                    }
                }
                catch
                {
                    num2 = 0x7fff;
                }
                if (num2 > num)
                {
                    num = num2;
                }
            }
            return num;
        }

        public static float Seconds2BPM(double seconds)
        {
            if (seconds != 0.0)
            {
                return (float) (60.0/seconds);
            }
            return -1f;
        }

        public static float Semitone2Samplerate(float origfreq, int semitones)
        {
            return (origfreq*((float) Math.Pow(2.0, (double) (((float) semitones)/12f))));
        }
    }
}