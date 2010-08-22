using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Tags;

namespace Un4seen.Bass
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class Bass
    {
        public const int BASSVERSION = 0x204;
        public const int ERROR = -1;
        public const int FALSE = 0;
        public const int TRUE = 1;
        private static int _myModuleHandle = 0;
        private static string _myModuleName = "bass.dll";

        public static string SupportedMusicExtensions =
            "*.mod;*.mo3;*.s3m;*.xm;*.it;*.mtm;*.umx;*.mdz;*.s3z;*.itz;*.xmz";

        public static string SupportedStreamExtensions =
            "*.mp3;*.ogg;*.wav;*.mp2;*.mp1;*.aiff;*.m2a;*.mpa;*.m1a;*.mpg;*.mpeg;*.aif;*.mp3pro;*.bwf";

        public static string SupportedStreamName = "WAV/AIFF/MP3/MP2/MP1/OGG";

        static Bass()
        {
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern void BASS_Apply3D();

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern double BASS_ChannelBytes2Seconds(int handle, long pos);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern BASSFlag BASS_ChannelFlags(int handle, BASSFlag flags, BASSFlag mask);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelGet3DAttributes(int handle,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object mode,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object min,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object max,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object iangle,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object oangle,
                                                              [In, Out, MarshalAs(UnmanagedType.AsAny)] object outvol);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelGet3DAttributes(int handle, ref BASS3DMode mode, ref float min,
                                                              ref float max, ref int iangle, ref int oangle,
                                                              ref int outvol);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelGet3DPosition(int handle, [In, Out] BASS_3DVECTOR pos,
                                                            [In, Out] BASS_3DVECTOR orient, [In, Out] BASS_3DVECTOR vel);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelGetAttribute(int handle, BASSAttribute attrib, ref float value);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetData(int handle, [In, Out] byte[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetData(int handle, IntPtr buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetData(int handle, [In, Out] short[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetData(int handle, [In, Out] int[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetData(int handle, [In, Out] float[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetDevice(int handle);

        public static BASS_CHANNELINFO BASS_ChannelGetInfo(int handle)
        {
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            if (BASS_ChannelGetInfo(handle, info))
            {
                return info;
            }
            return null;
        }

        public static bool BASS_ChannelGetInfo(int handle, BASS_CHANNELINFO info)
        {
            bool flag = BASS_ChannelGetInfoInternal(handle, ref info._internal);
            if (flag)
            {
                info.chans = info._internal.chans;
                info.ctype = info._internal.ctype;
                info.flags = info._internal.flags;
                info.freq = info._internal.freq;
                info.origres = info._internal.origres;
                info.plugin = info._internal.plugin;
                info.sample = info._internal.sample;
                if ((info.flags & (BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE)) != BASSFlag.BASS_DEFAULT)
                {
                    info.filename = Marshal.PtrToStringUni(info._internal.filename);
                    return flag;
                }
                info.filename = Marshal.PtrToStringAnsi(info._internal.filename);
            }
            return flag;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_ChannelGetInfo", CharSet=CharSet.Auto)]
        private static extern bool BASS_ChannelGetInfoInternal(int handle, [In, Out] ref BASS_CHANNELINFO_INTERNAL info);

        public static long BASS_ChannelGetLength(int handle)
        {
            return BASS_ChannelGetLength(handle, BASSMode.BASS_POS_BYTES);
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern long BASS_ChannelGetLength(int handle, BASSMode mode);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelGetLevel(int handle);

        public static bool BASS_ChannelGetLevel(int handle, float[] level)
        {
            if (level.Length > 0)
            {
                Array.Clear(level, 0, level.Length);
            }
            else
            {
                return false;
            }
            int num = (int) BASS_ChannelSeconds2Bytes(handle, 0.02);
            if (num <= 0)
            {
                return false;
            }
            float[] buffer = new float[num/4];
            num = BASS_ChannelGetData(handle, buffer, num | 0x40000000)/4;
            int index = 0;
            for (int i = 0; i < num; i++)
            {
                float num3 = Math.Abs(buffer[i]);
                if (num3 > level[index])
                {
                    level[index] = num3;
                }
                index++;
                if (index >= level.Length)
                {
                    index = 0;
                }
            }
            return true;
        }

        public static string[] BASS_ChannelGetMidiTrackText(int handle, int track)
        {
            if (track >= 0)
            {
                return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, (BASSTag) (0x11000 + track)));
            }
            ArrayList list = new ArrayList();
            track = 0;
            while (true)
            {
                IntPtr pointer = BASS_ChannelGetTags(handle, (BASSTag) (0x11000 + track));
                if (!(pointer != IntPtr.Zero))
                {
                    break;
                }
                string[] c = Utils.IntPtrToArrayNullTermAnsi(pointer);
                if ((c != null) && (c.Length > 0))
                {
                    list.AddRange(c);
                }
                track++;
            }
            if (list.Count > 0)
            {
                return (string[]) list.ToArray(typeof (string));
            }
            return null;
        }

        public static string BASS_ChannelGetMusicInstrument(int handle, int instrument)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, (BASSTag) (0x10100 + instrument));
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        public static string BASS_ChannelGetMusicMessage(int handle)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_MUSIC_MESSAGE);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        public static string BASS_ChannelGetMusicName(int handle)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_MUSIC_NAME);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        public static string BASS_ChannelGetMusicSample(int handle, int sample)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, (BASSTag) (0x10300 + sample));
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        public static long BASS_ChannelGetPosition(int handle)
        {
            return BASS_ChannelGetPosition(handle, BASSMode.BASS_POS_BYTES);
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern long BASS_ChannelGetPosition(int handle, BASSMode mode);

        public static string BASS_ChannelGetTagLyrics3v2(int handle)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_LYRICS3);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr BASS_ChannelGetTags(int handle, BASSTag tags);

        public static string[] BASS_ChannelGetTagsAPE(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_APE);
        }

        public static string[] BASS_ChannelGetTagsArrayNullTermAnsi(int handle, BASSTag format)
        {
            return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, format));
        }

        public static string[] BASS_ChannelGetTagsArrayNullTermUtf8(int handle, BASSTag format)
        {
            return Utils.IntPtrToArrayNullTermUtf8(BASS_ChannelGetTags(handle, format));
        }

        public static string[] BASS_ChannelGetTagsHTTP(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_HTTP);
        }

        public static string[] BASS_ChannelGetTagsICY(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_ICY);
        }

        public static unsafe string[] BASS_ChannelGetTagsID3V1(int handle)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_ID3);
            if (!(ptr != IntPtr.Zero))
            {
                return null;
            }
            string[] strArray = new string[6];
            if (Marshal.PtrToStringAnsi(ptr, 3) != "TAG")
            {
                return null;
            }
            ptr = new IntPtr(ptr.ToInt64() + 3);
            strArray[0] = Marshal.PtrToStringAnsi(ptr).TrimEnd(new char[1]);
            ptr = new IntPtr(ptr.ToInt64() + 30);
            strArray[1] = Marshal.PtrToStringAnsi(ptr, 30).TrimEnd(new char[1]);
            ptr = new IntPtr(ptr.ToInt64() + 30);
            strArray[2] = Marshal.PtrToStringAnsi(ptr, 30).TrimEnd(new char[1]);
            ptr = new IntPtr(ptr.ToInt64() + 30);
            strArray[3] = Marshal.PtrToStringAnsi(ptr, 4).TrimEnd(new char[1]);
            ptr = new IntPtr(ptr.ToInt64() + 4);
            strArray[4] = Marshal.PtrToStringAnsi(ptr, 30).TrimEnd(new char[1]);
            ptr = new IntPtr(ptr.ToInt64() + 30);
            strArray[5] = Marshal.ReadByte(ptr).ToString();
            for (int i = 0; i < 6; i++)
            {
                int index = strArray[i].IndexOf('\0');
                if (index > 0)
                {
                    strArray[i] = strArray[i].Substring(0, index);
                }
            }
            return strArray;
        }

        public static string[] BASS_ChannelGetTagsID3V2(int handle)
        {
            IntPtr ptr = BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_ID3V2);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    ArrayList list = new ArrayList();
                    ID3v2Reader reader = new ID3v2Reader(ptr);
                    while (reader.Read())
                    {
                        string key = reader.GetKey();
                        object obj2 = reader.GetValue();
                        if ((key.Length > 0) && (obj2 is string))
                        {
                            list.Add(string.Format("{0}={1}", key, obj2));
                        }
                    }
                    reader.Close();
                    if (list.Count > 0)
                    {
                        return (string[]) list.ToArray(typeof (string));
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static string[] BASS_ChannelGetTagsMETA(int handle)
        {
            return Utils.IntPtrToArrayNullTermAnsi(BASS_ChannelGetTags(handle, BASSTag.BASS_TAG_META));
        }

        public static string[] BASS_ChannelGetTagsMP4(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_MP4);
        }

        public static string[] BASS_ChannelGetTagsOGG(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_OGG);
        }

        public static string[] BASS_ChannelGetTagsRIFF(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermAnsi(handle, BASSTag.BASS_TAG_RIFF_INFO);
        }

        public static string[] BASS_ChannelGetTagsWMA(int handle)
        {
            return BASS_ChannelGetTagsArrayNullTermUtf8(handle, BASSTag.BASS_TAG_WMA);
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern BASSActive BASS_ChannelIsActive(int handle);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelIsSliding(int handle, BASSAttribute attrib);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Channellock (int handle, [MarshalAs(UnmanagedType.Bool)] bool state);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelPause(int handle);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelPlay(int handle, [MarshalAs(UnmanagedType.Bool)] bool restart);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelRemoveDSP(int handle, int dsp);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelRemoveFX(int handle, int fx);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelRemoveLink(int handle, int chan);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelRemoveSync(int handle, int sync);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern long BASS_ChannelSeconds2Bytes(int handle, double pos);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSet3DAttributes(int handle, BASS3DMode mode, float min, float max,
                                                              int iangle, int oangle, int outvol);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSet3DPosition(int handle, [In] BASS_3DVECTOR pos,
                                                            [In] BASS_3DVECTOR orient, [In] BASS_3DVECTOR vel);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSetAttribute(int handle, BASSAttribute attrib, float value);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSetDevice(int handle, int device);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelSetDSP(int handle, DSPPROC proc, IntPtr user, int priority);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelSetFX(int handle, BASSFXType type, int priority);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSetLink(int handle, int chan);

        public static bool BASS_ChannelSetPosition(int handle, double seconds)
        {
            return BASS_ChannelSetPosition(handle, BASS_ChannelSeconds2Bytes(handle, seconds), BASSMode.BASS_POS_BYTES);
        }

        public static bool BASS_ChannelSetPosition(int handle, long pos)
        {
            return BASS_ChannelSetPosition(handle, pos, BASSMode.BASS_POS_BYTES);
        }

        public static bool BASS_ChannelSetPosition(int handle, int order, int row)
        {
            return BASS_ChannelSetPosition(handle, (long) Utils.MakeLong(order, row), BASSMode.BASS_POS_MUSIC_ORDERS);
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSetPosition(int handle, long pos, BASSMode mode);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_ChannelSetSync(int handle, BASSSync type, long param, SYNCPROC proc, IntPtr user);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelSlideAttribute(int handle, BASSAttribute attrib, float value, int time);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelStop(int handle);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_ChannelUpdate(int handle, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern BASSError BASS_ErrorGetCode();

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Free();

        public static bool BASS_FXGetParameters(int handle, object par)
        {
            bool flag = BASS_FXGetParametersExt(handle, par);
            if (par is BASS_BFX_MIX)
            {
                ((BASS_BFX_MIX) par).Get();
            }
            return flag;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_FXGetParameters", CharSet=CharSet.Auto)]
        private static extern bool BASS_FXGetParametersExt(int handle,
                                                           [In, Out, MarshalAs(UnmanagedType.AsAny)] object par);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_FXReset(int handle);

        public static bool BASS_FXSetParameters(int handle, object par)
        {
            if (par is BASS_BFX_MIX)
            {
                ((BASS_BFX_MIX) par).Set();
            }
            return BASS_FXSetParametersExt(handle, par);
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_FXSetParameters", CharSet=CharSet.Auto)]
        private static extern bool BASS_FXSetParametersExt(int handle, [In, MarshalAs(UnmanagedType.AsAny)] object par);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Get3DFactors(ref float distf, ref float rollf, ref float doppf);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Get3DFactors([In, Out, MarshalAs(UnmanagedType.AsAny)] object distf,
                                                    [In, Out, MarshalAs(UnmanagedType.AsAny)] object rollf,
                                                    [In, Out, MarshalAs(UnmanagedType.AsAny)] object doppf);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Get3DPosition([In, Out] BASS_3DVECTOR pos, [In, Out] BASS_3DVECTOR vel,
                                                     [In, Out] BASS_3DVECTOR front, [In, Out] BASS_3DVECTOR top);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_GetConfig(BASSConfig option);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_GetConfig", CharSet=CharSet.Auto)]
        public static extern bool BASS_GetConfigBool(BASSConfig option);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr BASS_GetConfigPtr(BASSConfig option);

        public static string BASS_GetConfigString(BASSConfig option)
        {
            IntPtr ptr = BASS_GetConfigPtr(option);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern float BASS_GetCPU();

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_GetDevice();

        public static int BASS_GetDeviceCount()
        {
            BASS_DEVICEINFO info = new BASS_DEVICEINFO();
            int device = 0;
            while (BASS_GetDeviceInfo(device, info))
            {
                device++;
            }
            BASS_GetCPU();
            return device;
        }

        public static BASS_DEVICEINFO BASS_GetDeviceInfo(int device)
        {
            BASS_DEVICEINFO info = new BASS_DEVICEINFO();
            if (BASS_GetDeviceInfo(device, info))
            {
                return info;
            }
            return null;
        }

        public static bool BASS_GetDeviceInfo(int device, BASS_DEVICEINFO info)
        {
            bool flag = BASS_GetDeviceInfoInternal(device, ref info._internal);
            if (flag)
            {
                info.name = Marshal.PtrToStringAnsi(info._internal.name);
                info.driver = Marshal.PtrToStringAnsi(info._internal.driver);
                info.flags = info._internal.flags;
            }
            return flag;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_GetDeviceInfo", CharSet=CharSet.Auto)]
        private static extern bool BASS_GetDeviceInfoInternal([In] int device,
                                                              [In, Out] ref BASS_DEVICEINFO_INTERNAL info);

        public static BASS_DEVICEINFO[] BASS_GetDeviceInfos()
        {
            BASS_DEVICEINFO bass_deviceinfo;
            ArrayList list = new ArrayList();
            for (int i = 0; (bass_deviceinfo = BASS_GetDeviceInfo(i)) != null; i++)
            {
                list.Add(bass_deviceinfo);
            }
            BASS_GetCPU();
            return (BASS_DEVICEINFO[]) list.ToArray(typeof (BASS_DEVICEINFO));
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr BASS_GetDSoundObject(int handle);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr BASS_GetDSoundObject(BASSDirectSound dsobject);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_GetEAXParameters([In, Out, MarshalAs(UnmanagedType.AsAny)] object env,
                                                        [In, Out, MarshalAs(UnmanagedType.AsAny)] object vol,
                                                        [In, Out, MarshalAs(UnmanagedType.AsAny)] object decay,
                                                        [In, Out, MarshalAs(UnmanagedType.AsAny)] object damp);

        [return : MarshalAs(UnmanagedType.Bool)]
        public static BASS_INFO BASS_GetInfo()
        {
            BASS_INFO info = new BASS_INFO();
            if (BASS_GetInfo(info))
            {
                return info;
            }
            return null;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_GetInfo([In, Out] BASS_INFO info);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_GetVersion();

        public static Version BASS_GetVersion(int fieldcount)
        {
            if (fieldcount < 1)
            {
                fieldcount = 1;
            }
            if (fieldcount > 4)
            {
                fieldcount = 4;
            }
            int num = BASS_GetVersion();
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

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern float BASS_GetVolume();

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Init(int device, int freq, BASSInit flags, IntPtr win, Guid clsid);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Init(int device, int freq, BASSInit flags, IntPtr win,
                                            [In, MarshalAs(UnmanagedType.AsAny)] object clsid);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_MusicFree(int handle);

        public static int BASS_MusicLoad(IntPtr memory, long offset, int length, BASSFlag flags, int freq)
        {
            return BASS_MusicLoadMemory(true, memory, offset, length, flags, freq);
        }

        public static int BASS_MusicLoad(string file, long offset, int length, BASSFlag flags, int freq)
        {
            flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
            return BASS_MusicLoadUnicode(false, file, offset, length, flags, freq);
        }

        public static int BASS_MusicLoad(byte[] memory, long offset, int length, BASSFlag flags, int freq)
        {
            return BASS_MusicLoadMemory(true, memory, offset, length, flags, freq);
        }

        [DllImport("bass.dll", EntryPoint="BASS_MusicLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_MusicLoadMemory([MarshalAs(UnmanagedType.Bool)] bool mem, IntPtr memory,
                                                       long offset, int length, BASSFlag flags, int freq);

        [DllImport("bass.dll", EntryPoint="BASS_MusicLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_MusicLoadMemory([MarshalAs(UnmanagedType.Bool)] bool mem, byte[] memory,
                                                       long offset, int length, BASSFlag flags, int freq);

        [DllImport("bass.dll", EntryPoint="BASS_MusicLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_MusicLoadUnicode([MarshalAs(UnmanagedType.Bool)] bool mem,
                                                        [In, MarshalAs(UnmanagedType.LPWStr)] string file, long offset,
                                                        int length, BASSFlag flags, int freq);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Pause();

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_PluginFree(int handle);

        public static BASS_PLUGININFO BASS_PluginGetInfo(int handle)
        {
            if (handle != 0)
            {
                IntPtr ptr = BASS_PluginGetInfoPtr(handle);
                if (ptr != IntPtr.Zero)
                {
                    BASS_PLUGININFO _plugininfo =
                        (BASS_PLUGININFO) Marshal.PtrToStructure(ptr, typeof (BASS_PLUGININFO));
                    return new BASS_PLUGININFO(_plugininfo.version, _plugininfo.formatc, ptr);
                }
                return null;
            }
            return
                new BASS_PLUGININFO(BASS_GetVersion(),
                                    new BASS_PLUGINFORM[]
                                        {
                                            new BASS_PLUGINFORM("WAVE Audio", "*.wav",
                                                                BASSChannelType.BASS_CTYPE_STREAM_WAV),
                                            new BASS_PLUGINFORM("Ogg Vorbis", "*.ogg",
                                                                BASSChannelType.BASS_CTYPE_STREAM_OGG),
                                            new BASS_PLUGINFORM("MPEG layer 1", "*.mp1;*.m1a",
                                                                BASSChannelType.BASS_CTYPE_STREAM_MP1),
                                            new BASS_PLUGINFORM("MPEG layer 2", "*.mp2;*.m2a;*.mpa",
                                                                BASSChannelType.BASS_CTYPE_STREAM_MP2),
                                            new BASS_PLUGINFORM("MPEG layer 3", "*.mp3;*.mpg;*.mpeg;*.mp3pro",
                                                                BASSChannelType.BASS_CTYPE_STREAM_MP3),
                                            new BASS_PLUGINFORM("Audio IFF", "*.aif;*.aiff",
                                                                BASSChannelType.BASS_CTYPE_STREAM_AIFF),
                                            new BASS_PLUGINFORM("Broadcast Wave", "*.bwf",
                                                                BASSChannelType.BASS_CTYPE_STREAM_WAV)
                                        });
        }

        [DllImport("bass.dll", EntryPoint="BASS_PluginGetInfo", CharSet=CharSet.Auto)]
        private static extern IntPtr BASS_PluginGetInfoPtr(int handle);

        public static int BASS_PluginLoad(string file)
        {
            return BASS_PluginLoadUnicode(file, BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE);
        }

        public static Hashtable BASS_PluginLoadDirectory(string dir)
        {
            Hashtable hashtable = new Hashtable();
            string[] files = Directory.GetFiles(dir, "bass*.dll");
            if (files != null)
            {
                foreach (string str in files)
                {
                    int key = BASS_PluginLoad(str);
                    if (key > 0)
                    {
                        hashtable.Add(key, str);
                    }
                }
            }
            BASS_GetCPU();
            if (hashtable.Count > 0)
            {
                return hashtable;
            }
            return null;
        }

        [DllImport("bass.dll", EntryPoint="BASS_PluginLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_PluginLoadUnicode([In, MarshalAs(UnmanagedType.LPWStr)] string file,
                                                         BASSFlag flags);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_RecordFree();

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_RecordGetDevice();

        public static int BASS_RecordGetDeviceCount()
        {
            BASS_DEVICEINFO info = new BASS_DEVICEINFO();
            int device = 0;
            while (BASS_RecordGetDeviceInfo(device, info))
            {
                device++;
            }
            BASS_GetCPU();
            return device;
        }

        public static BASS_DEVICEINFO BASS_RecordGetDeviceInfo(int device)
        {
            BASS_DEVICEINFO info = new BASS_DEVICEINFO();
            if (BASS_RecordGetDeviceInfo(device, info))
            {
                return info;
            }
            return null;
        }

        public static bool BASS_RecordGetDeviceInfo(int device, BASS_DEVICEINFO info)
        {
            bool flag = BASS_RecordGetDeviceInfoInternal(device, ref info._internal);
            if (flag)
            {
                info.name = Marshal.PtrToStringAnsi(info._internal.name);
                info.driver = Marshal.PtrToStringAnsi(info._internal.driver);
                info.flags = info._internal.flags;
            }
            return flag;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", EntryPoint="BASS_RecordGetDeviceInfo", CharSet=CharSet.Auto)]
        private static extern bool BASS_RecordGetDeviceInfoInternal([In] int device,
                                                                    [In, Out] ref BASS_DEVICEINFO_INTERNAL info);

        public static BASS_DEVICEINFO[] BASS_RecordGetDeviceInfos()
        {
            BASS_DEVICEINFO bass_deviceinfo;
            ArrayList list = new ArrayList();
            for (int i = 0; (bass_deviceinfo = BASS_RecordGetDeviceInfo(i)) != null; i++)
            {
                list.Add(bass_deviceinfo);
            }
            BASS_GetCPU();
            return (BASS_DEVICEINFO[]) list.ToArray(typeof (BASS_DEVICEINFO));
        }

        public static BASS_RECORDINFO BASS_RecordGetInfo()
        {
            BASS_RECORDINFO info = new BASS_RECORDINFO();
            if (BASS_RecordGetInfo(info))
            {
                return info;
            }
            return null;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_RecordGetInfo([In, Out] BASS_RECORDINFO info);

        public static BASSInput BASS_RecordGetInput(int input)
        {
            int num = BASS_RecordGetInputPtr(input, IntPtr.Zero);
            if (num != -1)
            {
                return (((BASSInput) num) & ((BASSInput) 0xff0000));
            }
            return BASSInput.BASS_INPUT_NONE;
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_RecordGetInput(int input, ref float volume);

        public static string BASS_RecordGetInputName(int input)
        {
            IntPtr ptr = BASS_RecordGetInputNamePtr(input);
            if (ptr != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

        [DllImport("bass.dll", EntryPoint="BASS_RecordGetInputName", CharSet=CharSet.Auto)]
        private static extern IntPtr BASS_RecordGetInputNamePtr(int input);

        public static string[] BASS_RecordGetInputNames()
        {
            string str;
            ArrayList list = new ArrayList();
            for (int i = 0; (str = BASS_RecordGetInputName(i)) != null; i++)
            {
                list.Add(str);
            }
            BASS_GetCPU();
            return (string[]) list.ToArray(typeof (string));
        }

        [DllImport("bass.dll", EntryPoint="BASS_RecordGetInput", CharSet=CharSet.Auto)]
        private static extern int BASS_RecordGetInputPtr(int input, IntPtr vol);

        public static BASSInputType BASS_RecordGetInputType(int input)
        {
            int num = BASS_RecordGetInputPtr(input, IntPtr.Zero);
            if (num != -1)
            {
                return (((BASSInputType) num) & ((BASSInputType) (-16777216)));
            }
            return ~BASSInputType.BASS_INPUT_TYPE_UNDEF;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_RecordInit(int device);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_RecordSetDevice(int device);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_RecordSetInput(int input, BASSInput setting, float volume);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_RecordStart(int freq, int chans, BASSFlag flags, RECORDPROC proc, IntPtr user);

        public static int BASS_RecordStart(int freq, int chans, BASSFlag flags, int period, RECORDPROC proc, IntPtr user)
        {
            return BASS_RecordStart(freq, chans, (BASSFlag) Utils.MakeLong((int) flags, period), proc, user);
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_SampleCreate(int length, int freq, int chans, int max, BASSFlag flags);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleFree(int handle);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_SampleGetChannel(int handle, [MarshalAs(UnmanagedType.Bool)] bool onlynew);

        public static int BASS_SampleGetChannelCount(int handle)
        {
            return BASS_SampleGetChannels(handle, null);
        }

        public static int[] BASS_SampleGetChannels(int handle)
        {
            int[] channels = new int[BASS_SampleGetInfo(handle).max];
            int length = BASS_SampleGetChannels(handle, channels);
            if (length >= 0)
            {
                int[] destinationArray = new int[length];
                Array.Copy(channels, destinationArray, length);
                return destinationArray;
            }
            return null;
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_SampleGetChannels(int handle, int[] channels);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetData(int handle, byte[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetData(int handle, short[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetData(int handle, int[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetData(int handle, float[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetData(int handle, IntPtr buffer);

        public static BASS_SAMPLE BASS_SampleGetInfo(int handle)
        {
            BASS_SAMPLE info = new BASS_SAMPLE();
            if (BASS_SampleGetInfo(handle, info))
            {
                return info;
            }
            return null;
        }

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleGetInfo(int handle, [In, Out] BASS_SAMPLE info);

        public static int BASS_SampleLoad(byte[] memory, long offset, int length, int max, BASSFlag flags)
        {
            return BASS_SampleLoadMemory(true, memory, offset, length, max, flags);
        }

        public static int BASS_SampleLoad(IntPtr memory, long offset, int length, int max, BASSFlag flags)
        {
            return BASS_SampleLoadMemory(true, memory, offset, length, max, flags);
        }

        public static int BASS_SampleLoad(string file, long offset, int length, int max, BASSFlag flags)
        {
            flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
            return BASS_SampleLoadUnicode(false, file, offset, length, max, flags);
        }

        [DllImport("bass.dll", EntryPoint="BASS_SampleLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_SampleLoadMemory([MarshalAs(UnmanagedType.Bool)] bool mem, byte[] memory,
                                                        long offset, int length, int max, BASSFlag flags);

        [DllImport("bass.dll", EntryPoint="BASS_SampleLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_SampleLoadMemory([MarshalAs(UnmanagedType.Bool)] bool mem, IntPtr memory,
                                                        long offset, int length, int max, BASSFlag flags);

        [DllImport("bass.dll", EntryPoint="BASS_SampleLoad", CharSet=CharSet.Auto)]
        private static extern int BASS_SampleLoadUnicode([MarshalAs(UnmanagedType.Bool)] bool mem,
                                                         [In, MarshalAs(UnmanagedType.LPWStr)] string file, long offset,
                                                         int length, int max, BASSFlag flags);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetData(int handle, byte[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetData(int handle, short[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetData(int handle, int[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetData(int handle, float[] buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetData(int handle, IntPtr buffer);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleSetInfo(int handle, [In] BASS_SAMPLE info);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SampleStop(int handle);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Set3DFactors(float distf, float rollf, float doppf);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Set3DPosition([In] BASS_3DVECTOR pos, [In] BASS_3DVECTOR vel,
                                                     [In] BASS_3DVECTOR front, [In] BASS_3DVECTOR top);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SetConfig(BASSConfig option, [In, MarshalAs(UnmanagedType.Bool)] bool newvalue);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SetConfig(BASSConfig option, int newvalue);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SetConfigPtr(BASSConfig option, IntPtr newvalue);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SetDevice(int device);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_SetVolume(float volume);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Start();

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Stop();

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamCreate(int freq, int chans, BASSFlag flags, STREAMPROC proc, IntPtr user);

        public static int BASS_StreamCreateDummy(int freq, int chans, BASSFlag flags, IntPtr user)
        {
            return BASS_StreamCreatePtr(freq, chans, flags, IntPtr.Zero, user);
        }

        public static int BASS_StreamCreateFile(IntPtr memory, long offset, long length, BASSFlag flags)
        {
            return BASS_StreamCreateFileMemory(true, memory, offset, length, flags);
        }

        public static int BASS_StreamCreateFile(string file, long offset, long length, BASSFlag flags)
        {
            flags |= BASSFlag.BASS_DEFAULT | BASSFlag.BASS_UNICODE;
            return BASS_StreamCreateFileUnicode(false, file, offset, length, flags);
        }

        [DllImport("bass.dll", EntryPoint="BASS_StreamCreateFile", CharSet=CharSet.Auto)]
        private static extern int BASS_StreamCreateFileMemory([MarshalAs(UnmanagedType.Bool)] bool mem, IntPtr memory,
                                                              long offset, long length, BASSFlag flags);

        [DllImport("bass.dll", EntryPoint="BASS_StreamCreateFile", CharSet=CharSet.Auto)]
        private static extern int BASS_StreamCreateFileUnicode([MarshalAs(UnmanagedType.Bool)] bool mem,
                                                               [In, MarshalAs(UnmanagedType.LPWStr)] string file,
                                                               long offset, long length, BASSFlag flags);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamCreateFileUser(BASSStreamSystem system, BASSFlag flags, BASS_FILEPROCS procs,
                                                           IntPtr user);

        [DllImport("bass.dll", EntryPoint="BASS_StreamCreate", CharSet=CharSet.Auto)]
        private static extern int BASS_StreamCreatePtr(int freq, int chans, BASSFlag flags, IntPtr procPtr, IntPtr user);

        public static int BASS_StreamCreatePush(int freq, int chans, BASSFlag flags, IntPtr user)
        {
            return BASS_StreamCreatePtr(freq, chans, flags, new IntPtr(-1), user);
        }

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamCreateURL([In, MarshalAs(UnmanagedType.LPStr)] string url, int offset,
                                                      BASSFlag flags, DOWNLOADPROC proc, IntPtr user);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_StreamFree(int handle);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern long BASS_StreamGetFilePosition(int handle, BASSStreamFilePosition mode);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutData(int handle, byte[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutData(int handle, short[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutData(int handle, float[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutData(int handle, IntPtr buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutData(int handle, int[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutFileData(int handle, byte[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutFileData(int handle, short[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutFileData(int handle, IntPtr buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutFileData(int handle, int[] buffer, int length);

        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern int BASS_StreamPutFileData(int handle, float[] buffer, int length);

        [return : MarshalAs(UnmanagedType.Bool)]
        [DllImport("bass.dll", CharSet=CharSet.Auto)]
        public static extern bool BASS_Update(int length);

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