namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal class ID3v2Reader
    {
        private byte[] buffer;
        private byte DefaultMajorVersion = 3;
        private byte DefaultMinorVersion;
        private string frameId;
        private object frameValue;
        private int lastTagPos;
        private byte majorVersion;
        private byte minorVersion;
        private int offset;
        private Stream stream;

        public ID3v2Reader(IntPtr pID3v2)
        {
            if (Marshal.PtrToStringAnsi(pID3v2, 3) == "ID3")
            {
                offset += 3;
                majorVersion = Marshal.ReadByte(pID3v2, offset);
                offset++;
                minorVersion = Marshal.ReadByte(pID3v2, offset);
                offset++;
                byte num = Marshal.ReadByte(pID3v2, offset);
                offset++;
                int num2 = ReadSynchsafeInt32(pID3v2, offset);
                offset += 4;
                bool flag = (num & 0x40) > 0;
                int num3 = 0;
                if (flag)
                {
                    num3 = ReadSynchsafeInt32(pID3v2, offset);
                }
                buffer = new byte[num2 + 10];
                Marshal.Copy(pID3v2, buffer, 0, num2 + 10);
                stream = new MemoryStream(buffer);
                stream.Position = 10 + num3;
                int num4 = num2 + 10;
                lastTagPos = num4 - 10;
            }
            else
            {
                majorVersion = DefaultMajorVersion;
                minorVersion = DefaultMinorVersion;
                stream = null;
            }
            frameId = null;
            frameValue = null;
        }

        public void Close()
        {
            if (stream != null)
            {
                stream.Close();
            }
        }

        private Encoding GetFrameEncoding(byte frameEncoding)
        {
            switch (frameEncoding)
            {
                case 1:
                    return Encoding.Unicode;

                case 2:
                    return Encoding.BigEndianUnicode;

                case 3:
                    return Encoding.UTF8;
            }
            return Encoding.GetEncoding(0x4e4);
        }

        public string GetKey()
        {
            return frameId;
        }

        public object GetValue()
        {
            return frameValue;
        }

        public bool Read()
        {
            frameId = null;
            frameValue = null;
            if (stream == null)
            {
                return false;
            }
            if (stream.Position > lastTagPos)
            {
                return false;
            }
            frameId = ReadFrameId();
            int frameLength = ReadFrameLength();
            if (majorVersion > 2)
            {
                ReadFrameFlags();
            }
            if (frameLength == 0)
            {
                frameValue = string.Empty;
            }
            else
            {
                frameValue = ReadFrameValue(frameLength);
            }
            return true;
        }

        private short ReadFrameFlags()
        {
            int num = stream.ReadByte();
            int num2 = stream.ReadByte();
            return (short) ((num << 8) | num2);
        }

        private string ReadFrameId()
        {
            int count = 4;
            if (majorVersion == 2)
            {
                count = 3;
            }
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            return Encoding.ASCII.GetString(buffer, 0, count).TrimEnd(new char[1]);
        }

        private int ReadFrameLength()
        {
            if (majorVersion == 4)
            {
                return ReadSynchsafeInt32();
            }
            if (majorVersion == 3)
            {
                return ReadInt32();
            }
            if (majorVersion != 2)
            {
                throw new NotSupportedException("Unsupported ID3v2 version detected. Don't know how to deal with this version.");
            }
            return ReadInt24();
        }

        private object ReadFrameValue(int frameLength)
        {
            byte[] buffer = new byte[frameLength];
            stream.Read(buffer, 0, frameLength);
            if (((frameId == "COM") || (frameId == "COMM")) || (((frameId == "USER") || (frameId == "ULT")) || (frameId == "USLT")))
            {
                Encoding frameEncoding = GetFrameEncoding(buffer[0]);
                int index = 4;
                if ((buffer[0] == 1) && (frameLength > 6))
                {
                    if ((buffer[index + 1] == 0xfe) && (buffer[index + 2] == 0xff))
                    {
                        frameEncoding = Encoding.BigEndianUnicode;
                        index += 2;
                    }
                    else if ((buffer[index + 1] == 0xff) && (buffer[index + 2] == 0xfe))
                    {
                        frameEncoding = Encoding.Unicode;
                        index += 2;
                    }
                }
                string str = frameEncoding.GetString(buffer, index, frameLength - index).TrimEnd(new char[1]);
                string[] strArray = str.Split(new char[1]);
                if ((strArray != null) && (strArray.Length > 1))
                {
                    if (strArray[0].Trim().Length > 0)
                    {
                        str = strArray[0].Trim() + ":" + strArray[1].Trim();
                    }
                    else
                    {
                        str = strArray[1].Trim();
                    }
                }
                return str.Trim();
            }
            if (((frameId == "WXXX") || (frameId == "WXX")) || ((frameId == "TXXX") || (frameId == "TXX")))
            {
                Encoding bigEndianUnicode = GetFrameEncoding(buffer[0]);
                int num2 = 1;
                if ((buffer[0] == 1) && (frameLength > 6))
                {
                    if ((buffer[num2 + 1] == 0xfe) && (buffer[num2 + 2] == 0xff))
                    {
                        bigEndianUnicode = Encoding.BigEndianUnicode;
                        num2 += 2;
                    }
                    else if ((buffer[num2 + 1] == 0xff) && (buffer[num2 + 2] == 0xfe))
                    {
                        bigEndianUnicode = Encoding.Unicode;
                        num2 += 2;
                    }
                }
                string str2 = bigEndianUnicode.GetString(buffer, num2, frameLength - num2).TrimEnd(new char[1]);
                string[] strArray2 = str2.Split(new char[1]);
                if ((strArray2 != null) && (strArray2.Length > 1))
                {
                    if (strArray2[0].Trim().Length > 0)
                    {
                        str2 = strArray2[0].Trim() + ":" + strArray2[1].Trim();
                    }
                    else
                    {
                        str2 = strArray2[1].Trim();
                    }
                }
                return str2.Trim();
            }
            if (frameId[0] == 'T')
            {
                Encoding unicode = GetFrameEncoding(buffer[0]);
                int num3 = 1;
                if ((buffer[0] == 1) && (frameLength > 3))
                {
                    if ((buffer[1] == 0xfe) && (buffer[2] == 0xff))
                    {
                        unicode = Encoding.BigEndianUnicode;
                        num3 = 3;
                    }
                    else if ((buffer[1] == 0xff) && (buffer[2] == 0xfe))
                    {
                        unicode = Encoding.Unicode;
                        num3 = 3;
                    }
                }
                return unicode.GetString(buffer, num3, frameLength - num3).TrimEnd(new char[1]).Trim();
            }
            if (frameId[0] == 'W')
            {
                string str4 = Encoding.ASCII.GetString(buffer, 0, frameLength).TrimEnd(new char[1]).TrimEnd(new char[1]);
                string[] strArray3 = str4.Split(new char[1]);
                if ((strArray3 != null) && (strArray3.Length > 1))
                {
                    str4 = strArray3[0].Trim();
                }
                return str4.Trim();
            }
            if ((!(frameId == "UFI") && !(frameId == "LNK")) && (!(frameId == "UFID") && !(frameId == "LINK")))
            {
                return buffer;
            }
            string str5 = Encoding.ASCII.GetString(buffer, 0, frameLength).TrimEnd(new char[1]);
            string[] strArray4 = str5.Split(new char[1]);
            if ((strArray4 != null) && (strArray4.Length > 1))
            {
                if (strArray4[0].Trim().Length > 0)
                {
                    str5 = strArray4[0].Trim() + ":" + strArray4[1].Trim();
                }
                else
                {
                    str5 = strArray4[1].Trim();
                }
            }
            return str5.Trim();
        }

        private int ReadInt24()
        {
            byte[] buffer = new byte[3];
            stream.Read(buffer, 0, 3);
            return (((buffer[0] << 0x10) | (buffer[1] << 8)) | buffer[2]);
        }

        private int ReadInt24(IntPtr p, int offset)
        {
            byte[] buffer = new byte[] { Marshal.ReadByte(p, offset), Marshal.ReadByte(p, offset + 1), Marshal.ReadByte(p, offset + 2) };
            return (((buffer[0] << 0x10) | (buffer[1] << 8)) | buffer[2]);
        }

        private int ReadInt32()
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return ((((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3]);
        }

        private int ReadInt32(IntPtr p, int offset)
        {
            byte[] buffer = new byte[] { Marshal.ReadByte(p, offset), Marshal.ReadByte(p, offset + 1), Marshal.ReadByte(p, offset + 2), Marshal.ReadByte(p, offset + 3) };
            return ((((buffer[0] << 0x18) | (buffer[1] << 0x10)) | (buffer[2] << 8)) | buffer[3]);
        }

        private string ReadMagic(IntPtr p)
        {
            byte[] buffer = new byte[3];
            stream.Read(buffer, 0, 3);
            return Encoding.ASCII.GetString(buffer, 0, 3);
        }

        private int ReadSynchsafeInt32()
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            if ((((buffer[0] & 0x80) != 0) || ((buffer[1] & 0x80) != 0)) || (((buffer[2] & 0x80) != 0) || ((buffer[3] & 0x80) != 0)))
            {
                throw new FormatException("Found invalid syncsafe integer");
            }
            return ((((buffer[0] << 0x15) | (buffer[1] << 14)) | (buffer[2] << 7)) | buffer[3]);
        }

        private int ReadSynchsafeInt32(IntPtr p, int offset)
        {
            byte[] buffer = new byte[] { Marshal.ReadByte(p, offset), Marshal.ReadByte(p, offset + 1), Marshal.ReadByte(p, offset + 2), Marshal.ReadByte(p, offset + 3) };
            if ((((buffer[0] & 0x80) != 0) || ((buffer[1] & 0x80) != 0)) || (((buffer[2] & 0x80) != 0) || ((buffer[3] & 0x80) != 0)))
            {
                throw new FormatException("Found invalid syncsafe integer");
            }
            return ((((buffer[0] << 0x15) | (buffer[1] << 14)) | (buffer[2] << 7)) | buffer[3]);
        }

        private string ReadTextZero(byte[] frameValue, ref int offset)
        {
            StringBuilder builder = new StringBuilder();
            try
            {
                char ch;
                while ((ch = (char) frameValue[offset]) != '\0')
                {
                    builder.Append(ch);
                    offset++;
                }
            }
            catch
            {
            }
            return builder.ToString();
        }

        private string ReadTextZero(byte[] frameValue, ref int offset, Encoding encoding)
        {
            string str = string.Empty;
            try
            {
                if (frameValue[0] == 1)
                {
                    if ((frameValue[offset] == 0xfe) && (frameValue[offset + 1] == 0xff))
                    {
                        encoding = Encoding.BigEndianUnicode;
                    }
                    else if ((frameValue[offset] == 0xff) && (frameValue[offset + 1] == 0xfe))
                    {
                        encoding = Encoding.Unicode;
                    }
                }
                int num = 1;
                if ((frameValue[0] == 1) || (frameValue[0] == 2))
                {
                    num = 2;
                }
                int index = offset;
                while (true)
                {
                    while (num == 1)
                    {
                        if (frameValue[index] == 0)
                        {
                            goto Label_008A;
                        }
                        index++;
                    }
                    if (num == 2)
                    {
                        if ((frameValue[index] == 0) && (frameValue[index + 1] == 0))
                        {
                            index++;
                            break;
                        }
                        index++;
                    }
                }
            Label_008A:
                str = encoding.GetString(frameValue, offset, ((index - offset) + 1) - num);
                offset = index;
            }
            catch
            {
            }
            return str;
        }

        public byte MajorVersion
        {
            get
            {
                return majorVersion;
            }
        }

        public byte MinorVersion
        {
            get
            {
                return minorVersion;
            }
        }
    }
}

