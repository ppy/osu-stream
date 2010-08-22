namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public sealed class BassTags
    {
        private BassTags()
        {
        }

        public static TAG_INFO BASS_TAG_GetFromFile(string file)
        {
            return BASS_TAG_GetFromFile(file, true, true);
        }

        public static bool BASS_TAG_GetFromFile(int stream, TAG_INFO tags)
        {
            if ((stream == 0) || (tags == null))
            {
                return false;
            }
            bool flag = false;
            BASS_CHANNELINFO bass_channelinfo = new BASS_CHANNELINFO();
            if (!Un4seen.Bass.Bass.BASS_ChannelGetInfo(stream, bass_channelinfo))
            {
                return flag;
            }
            tags.channelinfo = bass_channelinfo;
            BASSTag tagType = BASSTag.BASS_TAG_UNKNOWN;
            IntPtr p = BASS_TAG_GetIntPtr(stream, bass_channelinfo, out tagType);
            if (p != IntPtr.Zero)
            {
                switch (tagType)
                {
                    case BASSTag.BASS_TAG_MUSIC_NAME:
                        tags.title = Un4seen.Bass.Bass.BASS_ChannelGetMusicName(stream);
                        tags.artist = Un4seen.Bass.Bass.BASS_ChannelGetMusicMessage(stream);
                        flag = true;
                        goto Label_0229;

                    case BASSTag.BASS_TAG_MIDI_TRACK:
                    {
                        int num = 0;
                        while (true)
                        {
                            IntPtr data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, (BASSTag) (0x11000 + num));
                            if (!(data != IntPtr.Zero))
                            {
                                if (!flag && (tags.NativeTags.Length > 0))
                                {
                                    flag = true;
                                    if (tags.NativeTags.Length > 0)
                                    {
                                        tags.title = tags.NativeTags[0].Trim();
                                    }
                                    if (tags.NativeTags.Length > 1)
                                    {
                                        tags.artist = tags.NativeTags[1].Trim();
                                    }
                                }
                                goto Label_0229;
                            }
                            flag |= tags.UpdateFromMETA(data, false);
                            num++;
                        }
                    }
                    case BASSTag.BASS_TAG_ID3:
                        flag = ReadID3v1(p, tags);
                        goto Label_0229;

                    case BASSTag.BASS_TAG_ID3V2:
                        flag = ReadID3v2(p, tags);
                        goto Label_0229;

                    case BASSTag.BASS_TAG_OGG:
                        flag = tags.UpdateFromMETA(p, true);
                        goto Label_0229;

                    case BASSTag.BASS_TAG_HTTP:
                    case BASSTag.BASS_TAG_ICY:
                    case BASSTag.BASS_TAG_META:
                        goto Label_0229;

                    case BASSTag.BASS_TAG_APE:
                        flag = tags.UpdateFromMETA(p, true);
                        goto Label_0229;

                    case BASSTag.BASS_TAG_MP4:
                        flag = tags.UpdateFromMETA(p, true);
                        goto Label_0229;

                    case BASSTag.BASS_TAG_RIFF_INFO:
                        flag = tags.UpdateFromMETA(p, false);
                        goto Label_0229;
                }
            }
        Label_0229:
            tags.duration = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(stream, Un4seen.Bass.Bass.BASS_ChannelGetLength(stream));
            if (tags.bitrate == 0)
            {
                long num2 = Un4seen.Bass.Bass.BASS_StreamGetFilePosition(stream, BASSStreamFilePosition.BASS_FILEPOS_END);
                tags.bitrate = (int) ((((double) num2) / (125.0 * tags.duration)) + 0.5);
            }
            return flag;
        }

        public static TAG_INFO BASS_TAG_GetFromFile(string file, bool setDefaultTitle, bool prescan)
        {
            TAG_INFO tags = new TAG_INFO(file, setDefaultTitle);
            int stream = Un4seen.Bass.Bass.BASS_StreamCreateFile(file, 0L, 0L, BASSFlag.BASS_MUSIC_DECODE | (prescan ? BASSFlag.BASS_SAMPLE_OVER_POS : BASSFlag.BASS_DEFAULT));
            if (stream != 0)
            {
                BASS_TAG_GetFromFile(stream, tags);
                Un4seen.Bass.Bass.BASS_StreamFree(stream);
                return tags;
            }
            return null;
        }

        public static bool BASS_TAG_GetFromURL(int stream, TAG_INFO tags)
        {
            if ((stream == 0) || (tags == null))
            {
                return false;
            }
            bool flag = false;
            BASS_CHANNELINFO info = new BASS_CHANNELINFO();
            if (Un4seen.Bass.Bass.BASS_ChannelGetInfo(stream, info))
            {
                tags.channelinfo = info;
            }
            IntPtr data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ICY);
            if (data == IntPtr.Zero)
            {
                data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_HTTP);
            }
            if (data != IntPtr.Zero)
            {
                flag = tags.UpdateFromMETA(data, false);
            }
            data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_META);
            if (data != IntPtr.Zero)
            {
                flag = tags.UpdateFromMETA(data, false);
            }
            else
            {
                data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                if (data == IntPtr.Zero)
                {
                    data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                }
                if (data == IntPtr.Zero)
                {
                    data = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_WMA);
                }
                if (data != IntPtr.Zero)
                {
                    flag = tags.UpdateFromMETA(data, true);
                }
            }
            tags.duration = Un4seen.Bass.Bass.BASS_ChannelBytes2Seconds(stream, Un4seen.Bass.Bass.BASS_ChannelGetLength(stream));
            return flag;
        }

        private static IntPtr BASS_TAG_GetIntPtr(int stream, BASS_CHANNELINFO info, out BASSTag tagType)
        {
            IntPtr zero = IntPtr.Zero;
            tagType = BASSTag.BASS_TAG_UNKNOWN;
            if ((stream == 0) || (info == null))
            {
                return zero;
            }
            BASSChannelType ctype = info.ctype;
            if ((ctype & BASSChannelType.BASS_CTYPE_STREAM_WAV) > BASSChannelType.BASS_CTYPE_UNKNOWN)
            {
                ctype = BASSChannelType.BASS_CTYPE_STREAM_WAV;
            }
            switch (ctype)
            {
                case BASSChannelType.BASS_CTYPE_STREAM_WMA:
                case BASSChannelType.BASS_CTYPE_STREAM_WMA_MP3:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_WMA);
                    tagType = BASSTag.BASS_TAG_WMA;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_WINAMP:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                    if (!(zero == IntPtr.Zero))
                    {
                        tagType = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                    if (!(zero == IntPtr.Zero))
                    {
                        tagType = BASSTag.BASS_TAG_APE;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3);
                        if (zero != IntPtr.Zero)
                        {
                            tagType = BASSTag.BASS_TAG_ID3;
                        }
                        return zero;
                    }
                    tagType = BASSTag.BASS_TAG_OGG;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_OGG:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                    if (!(zero == IntPtr.Zero))
                    {
                        tagType = BASSTag.BASS_TAG_OGG;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                    tagType = BASSTag.BASS_TAG_APE;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_MP1:
                case BASSChannelType.BASS_CTYPE_STREAM_MP2:
                case BASSChannelType.BASS_CTYPE_STREAM_MP3:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                    if (!(zero == IntPtr.Zero))
                    {
                        tagType = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3);
                    tagType = BASSTag.BASS_TAG_ID3;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_AIFF:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV_PCM:
                case BASSChannelType.BASS_CTYPE_STREAM_WAV_FLOAT:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_INFO);
                    if (zero != IntPtr.Zero)
                    {
                        tagType = BASSTag.BASS_TAG_RIFF_INFO;
                    }
                    return zero;

                case BASSChannelType.BASS_CTYPE_MUSIC_MO3:
                case BASSChannelType.BASS_CTYPE_MUSIC_MOD:
                case BASSChannelType.BASS_CTYPE_MUSIC_MTM:
                case BASSChannelType.BASS_CTYPE_MUSIC_S3M:
                case BASSChannelType.BASS_CTYPE_MUSIC_XM:
                case BASSChannelType.BASS_CTYPE_MUSIC_IT:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_MUSIC_NAME);
                    if (zero != IntPtr.Zero)
                    {
                        tagType = BASSTag.BASS_TAG_MUSIC_NAME;
                    }
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_WV:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_H:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_L:
                case BASSChannelType.BASS_CTYPE_STREAM_WV_LH:
                case BASSChannelType.BASS_CTYPE_STREAM_OFR:
                case BASSChannelType.BASS_CTYPE_STREAM_APE:
                case BASSChannelType.BASS_CTYPE_STREAM_FLAC:
                case BASSChannelType.BASS_CTYPE_STREAM_SPX:
                case BASSChannelType.BASS_CTYPE_STREAM_MPC:
                case BASSChannelType.BASS_CTYPE_STREAM_TTA:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                            if (zero == IntPtr.Zero)
                            {
                                zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3);
                                if (zero != IntPtr.Zero)
                                {
                                    tagType = BASSTag.BASS_TAG_ID3;
                                }
                                return zero;
                            }
                            tagType = BASSTag.BASS_TAG_ID3V2;
                            return zero;
                        }
                        tagType = BASSTag.BASS_TAG_OGG;
                        return zero;
                    }
                    tagType = BASSTag.BASS_TAG_APE;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_MIDI:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_MIDI_TRACK);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_RIFF_INFO);
                        tagType = BASSTag.BASS_TAG_RIFF_INFO;
                        return zero;
                    }
                    tagType = BASSTag.BASS_TAG_MIDI_TRACK;
                    return zero;

                case BASSChannelType.BASS_CTYPE_STREAM_AAC:
                case BASSChannelType.BASS_CTYPE_STREAM_MP4:
                case BASSChannelType.BASS_CTYPE_STREAM_ALAC:
                    zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_MP4);
                    if (zero == IntPtr.Zero)
                    {
                        zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_ID3V2);
                        if (zero == IntPtr.Zero)
                        {
                            zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_APE);
                            if (zero == IntPtr.Zero)
                            {
                                zero = Un4seen.Bass.Bass.BASS_ChannelGetTags(stream, BASSTag.BASS_TAG_OGG);
                                if (zero != IntPtr.Zero)
                                {
                                    tagType = BASSTag.BASS_TAG_OGG;
                                }
                                return zero;
                            }
                            tagType = BASSTag.BASS_TAG_APE;
                            return zero;
                        }
                        tagType = BASSTag.BASS_TAG_ID3V2;
                        return zero;
                    }
                    tagType = BASSTag.BASS_TAG_MP4;
                    return zero;
            }
            return IntPtr.Zero;
        }

        private unsafe static bool ReadID3v1(IntPtr p, TAG_INFO tags)
        {
            if ((p == IntPtr.Zero) || (tags == null))
            {
                return false;
            }
            if (Marshal.PtrToStringAnsi(p, 3) != "TAG")
            {
                return false;
            }

            IntPtr p1 = new IntPtr(p.ToInt64() + 3);
            tags.title = Marshal.PtrToStringAnsi(p, 30).TrimEnd(new char[1]);
            int index = tags.title.IndexOf('\0');
            if (index > 0)
            {
                tags.title = tags.title.Substring(0, index).Trim();
            }
            else if (tags.title.Length >= 30)
            {
                tags.title = tags.title.Substring(0, 30).Trim();
            }
            p = new IntPtr(p.ToInt64() + 30);
            tags.artist = Marshal.PtrToStringAnsi(p, 30).TrimEnd(new char[1]);
            index = tags.artist.IndexOf('\0');
            if (index > 0)
            {
                tags.artist = tags.artist.Substring(0, index).Trim();
            }
            else if (tags.artist.Length >= 30)
            {
                tags.artist = tags.artist.Substring(0, 30).Trim();
            }
            p = new IntPtr(p.ToInt64() + 30);
            tags.album = Marshal.PtrToStringAnsi(p, 30).TrimEnd(new char[1]);
            index = tags.album.IndexOf('\0');
            if (index > 0)
            {
                tags.album = tags.album.Substring(0, index).Trim();
            }
            else if (tags.album.Length >= 30)
            {
                tags.album = tags.album.Substring(0, 30).Trim();
            }
            p = new IntPtr(p.ToInt64() + 30);
            tags.year = Marshal.PtrToStringAnsi(p, 4).TrimEnd(new char[1]);
            index = tags.year.IndexOf('\0');
            if (index > 0)
            {
                tags.year = tags.year.Substring(0, index).Trim();
            }
            else if (tags.year.Length >= 4)
            {
                tags.year = tags.year.Substring(0, 4).Trim();
            }
            p = new IntPtr(p.ToInt64() + 4);
            tags.comment = Marshal.PtrToStringAnsi(p, 30).TrimEnd(new char[1]);
            index = tags.comment.IndexOf('\0');
            if (index > 0)
            {
                tags.comment = tags.comment.Substring(0, index).Trim();
            }
            else if (tags.comment.Length >= 30)
            {
                tags.comment = tags.comment.Substring(0, 30).Trim();
            }
            p = new IntPtr(p.ToInt64() + 30);
            int num2 = Marshal.ReadByte(p);
            try
            {
                tags.genre = Enum.GetName(typeof(ID3v1Genre), num2);
            }
            catch
            {
                tags.genre = ID3v1Genre.Unknown.ToString();
            }
            return true;
        }

        private static bool ReadID3v2(IntPtr p, TAG_INFO tags)
        {
            if ((p == IntPtr.Zero) || (tags == null))
            {
                return false;
            }
            try
            {
                tags.ResetTags();
                ID3v2Reader reader = new ID3v2Reader(p);
                while (reader.Read())
                {
                    string key = reader.GetKey();
                    object obj2 = reader.GetValue();
                    if ((key.Length > 0) && (obj2 is string))
                    {
                        tags.EvalTagEntry(string.Format("{0}={1}", key, obj2));
                    }
                }
                reader.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

