namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Un4seen.Bass;

    [SuppressUnmanagedCodeSecurity]
    public sealed class TAG_INFO
    {
        private int _commentCounter;
        private int _multiCounter;
        public string album;
        public string albumartist;
        public string artist;
        public int bitrate;
        public string bpm;
        public BASS_CHANNELINFO channelinfo;
        public string comment;
        public string composer;
        public string copyright;
        public double duration;
        public string encodedby;
        public string filename;
        public string genre;
        private ArrayList nativetags;
        private ArrayList pictures;
        public string publisher;
        public string title;
        public string track;
        public string year;

        public TAG_INFO()
        {
            title = string.Empty;
            artist = string.Empty;
            album = string.Empty;
            albumartist = string.Empty;
            year = string.Empty;
            comment = string.Empty;
            genre = string.Empty;
            track = string.Empty;
            copyright = string.Empty;
            encodedby = string.Empty;
            composer = string.Empty;
            publisher = string.Empty;
            bpm = string.Empty;
            filename = string.Empty;
            pictures = new ArrayList();
            nativetags = new ArrayList();
            channelinfo = new BASS_CHANNELINFO();
        }

        public TAG_INFO(string FileName)
        {
            title = string.Empty;
            artist = string.Empty;
            album = string.Empty;
            albumartist = string.Empty;
            year = string.Empty;
            comment = string.Empty;
            genre = string.Empty;
            track = string.Empty;
            copyright = string.Empty;
            encodedby = string.Empty;
            composer = string.Empty;
            publisher = string.Empty;
            bpm = string.Empty;
            filename = string.Empty;
            pictures = new ArrayList();
            nativetags = new ArrayList();
            channelinfo = new BASS_CHANNELINFO();
            filename = FileName;
            title = Path.GetFileNameWithoutExtension(FileName);
        }

        public TAG_INFO(string FileName, bool setDefaultTitle)
        {
            title = string.Empty;
            artist = string.Empty;
            album = string.Empty;
            albumartist = string.Empty;
            year = string.Empty;
            comment = string.Empty;
            genre = string.Empty;
            track = string.Empty;
            copyright = string.Empty;
            encodedby = string.Empty;
            composer = string.Empty;
            publisher = string.Empty;
            bpm = string.Empty;
            filename = string.Empty;
            pictures = new ArrayList();
            nativetags = new ArrayList();
            channelinfo = new BASS_CHANNELINFO();
            filename = FileName;
            if (setDefaultTitle)
            {
                title = Path.GetFileNameWithoutExtension(FileName);
            }
        }

        internal bool EvalTagEntry(string tagEntry)
        {
            string[] strArray2;
            int num;
            string str3;
            bool flag2;
            if (tagEntry == null)
            {
                return false;
            }
            bool flag = false;
            string s = string.Empty;
            string[] strArray = tagEntry.Trim().Split(new char[] {'=', ':'}, 2);
            if (strArray.Length != 2)
            {
                if (!nativetags.Contains(tagEntry) && (tagEntry != string.Empty))
                {
                    nativetags.Add(tagEntry);
                }
                return flag;
            }
            if (NativeTag(strArray[0].Trim()) != null)
            {
                _multiCounter++;
                strArray[0] = strArray[0].Trim() + _multiCounter.ToString();
            }
            try
            {
                nativetags.Add(strArray[0].Trim() + "=" + strArray[1].Trim());
            }
            catch
            {
            }
            switch (strArray[0].ToLower().Trim())
            {
                case "iart":
                case "tpe1":
                case "tp1":
                case "artist":
                case "author":
                case "wm/author":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        artist = s;
                        flag = true;
                    }
                    return flag;

                case "isbj":
                case "tpe2":
                case "tp2":
                case "albumartist":
                case "wm/albumartist":
                case "remixer":
                case "orchestra":
                case "ensemble":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        albumartist = s;
                        flag = true;
                    }
                    return flag;

                case "trck":
                case "trk":
                case "tracknumber":
                case "tracknum":
                case "track":
                case "wm/tracknumber":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        track = s;
                        flag = true;
                    }
                    return flag;

                case "icop":
                case "tcop":
                case "tcr":
                case "copyright":
                case "wm/provider":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        copyright = s;
                        flag = true;
                    }
                    return flag;

                case "isrf":
                case "itch":
                case "tool":
                case "tenc":
                case "ten":
                case "wm/encodedby":
                case "encodedby":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        encodedby = s;
                        flag = true;
                    }
                    return flag;

                case "inam":
                case "tit2":
                case "tt2":
                case "title":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        title = s;
                        flag = true;
                    }
                    return flag;

                case "isrc":
                case "tpub":
                case "tpb":
                case "publisher":
                case "wm/publisher":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        publisher = s;
                        flag = true;
                    }
                    return flag;

                case "ieng":
                case "tcom":
                case "tcm":
                case "composer":
                case "wm/composer":
                case "writer":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        composer = s;
                        flag = true;
                    }
                    return flag;

                case "icmt":
                case "comm":
                case "com":
                case "comment":
                case "description":
                    s = strArray[1].Trim();
                    if ((s != string.Empty) && (_commentCounter == 0))
                    {
                        comment = s;
                        flag = true;
                        _commentCounter++;
                    }
                    return flag;

                case "iprd":
                case "talb":
                case "tal":
                case "album":
                case "wm/albumtitle":
                case "icy-name":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        album = s;
                        flag = true;
                    }
                    return flag;

                case "icrd":
                case "tyer":
                case "tdrl":
                case "tye":
                case "tda":
                case "year":
                case "date":
                case "wm/year":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        year = s;
                        flag = true;
                    }
                    return flag;

                case "ignr":
                case "tcon":
                case "tco":
                    s = strArray[1].Trim();
                    if (!(s != string.Empty))
                    {
                        return flag;
                    }
                    strArray2 = s.Split(new char[1]);
                    if ((strArray2 == null) || (strArray2.Length <= 0))
                    {
                        genre = s;
                        goto Label_0A02;
                    }
                    num = 0;
                    goto Label_09DC;

                case "streamtitle":
                    {
                        s = strArray[1].Trim(new char[] {'\'', '"'});
                        if (!(s != string.Empty))
                        {
                            return flag;
                        }
                        int index = s.IndexOf(" - ");
                        if ((index <= 0) || ((index + 3) >= s.Length))
                        {
                            title = s;
                        }
                        else
                        {
                            artist = s.Substring(0, index).Trim();
                            title = s.Substring(index + 3).Trim();
                        }
                        return true;
                    }
                case "streamurl":
                    s = strArray[1].Trim(new char[] {'\'', '"'});
                    if (s != string.Empty)
                    {
                        comment = s;
                        flag = true;
                    }
                    return flag;

                case "genre":
                case "wm/genre":
                case "icy-genre":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        genre = s;
                        flag = true;
                    }
                    return flag;

                case "icy-url":
                    s = strArray[1].Trim();
                    if (s != string.Empty)
                    {
                        comment = s;
                        flag = true;
                    }
                    return flag;

                case "icy-br":
                    s = strArray[1].Trim();
                    if (!(s != string.Empty))
                    {
                        return flag;
                    }
                    try
                    {
                        bitrate = int.Parse(s);
                        return true;
                    }
                    catch
                    {
                        return flag;
                    }
                    goto Label_0B4C;

                case "currentbitrate":
                    goto Label_0B4C;

                case "bpm":
                case "tbp":
                case "tbpm":
                case "wm/beatsperminute":
                case "beatsperminute":
                case "tempo":
                    goto Label_0B82;

                default:
                    return flag;
            }
            Label_099F:
            if (flag2)
            {
                try
                {
                    strArray2[num] = Enum.GetName(typeof (ID3v1Genre), int.Parse(str3));
                    if (strArray2[num] == null)
                    {
                        strArray2[num] = str3;
                    }
                }
                catch
                {
                }
            }
            Label_09D6:
            num++;
            Label_09DC:
            if (num < strArray2.Length)
            {
                str3 = strArray2[num].Trim();
                switch (str3)
                {
                    case "RX":
                    case "(RX)":
                        strArray2[num] = "Remix";
                        goto Label_09D6;

                    case "CR":
                    case "(CR)":
                        strArray2[num] = "Cover";
                        goto Label_09D6;
                }
                if ((str3.IndexOf('(') < str3.LastIndexOf(')')) && (str3.Length > 2))
                {
                    int num2 = str3.IndexOf('(');
                    int num3 = str3.LastIndexOf(')');
                    s = str3.Substring(num2 + 1, (num3 - num2) - 1);
                    try
                    {
                        strArray2[num] = Enum.GetName(typeof (ID3v1Genre), int.Parse(s));
                        if (strArray2[num] == null)
                        {
                            strArray2[num] = str3;
                        }
                        goto Label_09D6;
                    }
                    catch
                    {
                        strArray2[num] = str3;
                        goto Label_09D6;
                    }
                }
                strArray2[num] = str3;
                if (str3.Length >= 4)
                {
                    goto Label_09D6;
                }
                flag2 = true;
                foreach (char ch in str3)
                {
                    if (!char.IsNumber(ch))
                    {
                        flag2 = false;
                        break;
                    }
                }
                goto Label_099F;
            }
            genre = string.Join(", ", strArray2);
            Label_0A02:
            return true;
            Label_0B4C:
            s = strArray[1].Trim();
            if (!(s != string.Empty))
            {
                return flag;
            }
            try
            {
                bitrate = int.Parse(s)/0x3e8;
                return flag;
            }
            catch
            {
                return flag;
            }
            Label_0B82:
            s = strArray[1].Trim();
            if (!(s != string.Empty))
            {
                return flag;
            }
            if (s.ToUpper().EndsWith("BPM"))
            {
                s = s.Substring(0, s.Length - 3).Trim().Trim(new char[] {'0'});
            }
            bpm = s;
            return true;
        }

        public string NativeTag(string tagname)
        {
            if (tagname != null)
            {
                try
                {
                    foreach (string str2 in nativetags)
                    {
                        if (str2.StartsWith(tagname))
                        {
                            string[] strArray = str2.Split(new char[] {'=', ':'}, 2);
                            if (strArray.Length == 2)
                            {
                                return strArray[1].Trim();
                            }
                        }
                    }
                    return null;
                }
                catch
                {
                }
            }
            return null;
        }

        internal void ResetTags()
        {
            _multiCounter = 0;
            _commentCounter = 0;
            nativetags.Clear();
        }

        public override string ToString()
        {
            string artist = this.artist;
            if (artist == string.Empty)
            {
                artist = albumartist;
            }
            if ((artist == string.Empty) && (title != string.Empty))
            {
                return title;
            }
            if ((artist != string.Empty) && (title == string.Empty))
            {
                return artist;
            }
            if ((artist != string.Empty) && (title != string.Empty))
            {
                return string.Format("{0} - {1}", artist, title);
            }
            return Path.GetFileNameWithoutExtension(filename);
        }

        public unsafe bool UpdateFromMETA(IntPtr data, bool utf8)
        {
            if (data == IntPtr.Zero)
            {
                return false;
            }
            bool flag = false;
            ResetTags();
            string str = null;
            bool flag2 = true;
            int num = 0;
            IntPtr ptr = data;
            UTF8Encoding encoding = new UTF8Encoding();
            while (flag2)
            {
                if (utf8)
                {
                    ptr = new IntPtr(ptr.ToInt64() + num);
                    str = Marshal.PtrToStringAnsi(ptr);
                    byte[] destination = new byte[str.Length];
                    Marshal.Copy(ptr, destination, 0, str.Length);
                    num = str.Length + 1;
                    str = encoding.GetString(destination);
                }
                else
                {
                    str = Marshal.PtrToStringAnsi(new IntPtr(ptr.ToInt64() + num));
                    num += str.Length + 1;
                }
                if (str.Length != 0)
                {
                    string[] strArray = str.Split(new char[] {';'});
                    if (strArray.Length > 0)
                    {
                        foreach (string str2 in strArray)
                        {
                            flag |= EvalTagEntry(str2.Trim());
                        }
                    }
                    if (str.StartsWith("StreamTitle"))
                    {
                        flag2 = false;
                    }
                }
                else
                {
                    flag2 = false;
                }
            }
            return flag;
        }

        public string[] NativeTags
        {
            get { return (string[]) nativetags.ToArray(typeof (string)); }
        }

        public int PictureCount
        {
            get { return pictures.Count; }
        }
    }
}