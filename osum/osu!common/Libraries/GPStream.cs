namespace System.Drawing.Internal
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public class GPStream : IStream
    {
        protected Stream dataStream;
        private long virtualPosition = -1L;

        public GPStream(Stream stream)
        {
            if (!stream.CanSeek)
            {
                int num;
                byte[] sourceArray = new byte[0x100];
                int offset = 0;
                do
                {
                    if (sourceArray.Length < (offset + 0x100))
                    {
                        byte[] destinationArray = new byte[sourceArray.Length * 2];
                        Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                        sourceArray = destinationArray;
                    }
                    num = stream.Read(sourceArray, offset, 0x100);
                    offset += num;
                }
                while (num != 0);
                this.dataStream = new MemoryStream(sourceArray);
            }
            else
            {
                this.dataStream = stream;
            }
        }

        private void ActualizeVirtualPosition()
        {
            if (this.virtualPosition != -1L)
            {
                if (this.virtualPosition > this.dataStream.Length)
                {
                    this.dataStream.SetLength(this.virtualPosition);
                }
                this.dataStream.Position = this.virtualPosition;
                this.virtualPosition = -1L;
            }
        }

        public virtual IStream Clone()
        {
            NotImplemented();
            return null;
        }

        public virtual void Commit(int grfCommitFlags)
        {
            this.dataStream.Flush();
            this.ActualizeVirtualPosition();
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode), UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        public virtual long CopyTo(IStream pstm, long cb, long[] pcbRead)
        {
            int num = 0x1000;
            IntPtr buf = Marshal.AllocHGlobal(num);
            if (buf == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }
            long num2 = 0L;
            try
            {
                while (num2 < cb)
                {
                    int length = num;
                    if ((num2 + length) > cb)
                    {
                        length = (int) (cb - num2);
                    }
                    int len = this.Read(buf, length);
                    if (len == 0)
                    {
                        goto Label_006C;
                    }
                    if (pstm.Write(buf, len) != len)
                    {
                        throw EFail("Wrote an incorrect number of bytes");
                    }
                    num2 += len;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        Label_006C:
            if ((pcbRead != null) && (pcbRead.Length > 0))
            {
                pcbRead[0] = num2;
            }
            return num2;
        }

        protected static ExternalException EFail(string msg)
        {
            throw new ExternalException(msg, -2147467259);
        }

        public virtual Stream GetDataStream()
        {
            return this.dataStream;
        }

        public virtual void LockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        protected static void NotImplemented()
        {
        }

        public virtual int Read(IntPtr buf, int length)
        {
            byte[] buffer = new byte[length];
            int num = this.Read(buffer, length);
            Marshal.Copy(buffer, 0, buf, length);
            return num;
        }

        public virtual int Read(byte[] buffer, int length)
        {
            this.ActualizeVirtualPosition();
            return this.dataStream.Read(buffer, 0, length);
        }

        public virtual void Revert()
        {
            NotImplemented();
        }

        public virtual long Seek(long offset, int origin)
        {
            long virtualPosition = this.virtualPosition;
            if (this.virtualPosition == -1L)
            {
                virtualPosition = this.dataStream.Position;
            }
            long length = this.dataStream.Length;
            switch (origin)
            {
                case 0:
                    if (offset > length)
                    {
                        this.virtualPosition = offset;
                        break;
                    }
                    this.dataStream.Position = offset;
                    this.virtualPosition = -1L;
                    break;

                case 1:
                    if ((offset + virtualPosition) > length)
                    {
                        this.virtualPosition = offset + virtualPosition;
                        break;
                    }
                    this.dataStream.Position = virtualPosition + offset;
                    this.virtualPosition = -1L;
                    break;

                case 2:
                    if (offset > 0L)
                    {
                        this.virtualPosition = length + offset;
                        break;
                    }
                    this.dataStream.Position = length + offset;
                    this.virtualPosition = -1L;
                    break;
            }
            if (this.virtualPosition != -1L)
            {
                return this.virtualPosition;
            }
            return this.dataStream.Position;
        }

        public virtual void SetSize(long value)
        {
            this.dataStream.SetLength(value);
        }

        public void Stat(IntPtr pstatstg, int grfStatFlag)
        {
            STATSTG structure = new STATSTG();
            structure.cbSize = this.dataStream.Length;
            Marshal.StructureToPtr(structure, pstatstg, true);
        }

        public virtual void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        public virtual int Write(IntPtr buf, int length)
        {
            byte[] destination = new byte[length];
            Marshal.Copy(buf, destination, 0, length);
            return this.Write(destination, length);
        }

        public virtual int Write(byte[] buffer, int length)
        {
            this.ActualizeVirtualPosition();
            this.dataStream.Write(buffer, 0, length);
            return length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class STATSTG
        {
            public IntPtr pwcsName = IntPtr.Zero;
            public int type;
            [MarshalAs(UnmanagedType.I8)]
            public long cbSize;
            [MarshalAs(UnmanagedType.I8)]
            public long mtime;
            [MarshalAs(UnmanagedType.I8)]
            public long ctime;
            [MarshalAs(UnmanagedType.I8)]
            public long atime;
            [MarshalAs(UnmanagedType.I4)]
            public int grfMode;
            [MarshalAs(UnmanagedType.I4)]
            public int grfLocksSupported;
            public int clsid_data1;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data2;
            [MarshalAs(UnmanagedType.I2)]
            public short clsid_data3;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b0;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b1;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b2;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b3;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b4;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b5;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b6;
            [MarshalAs(UnmanagedType.U1)]
            public byte clsid_b7;
            [MarshalAs(UnmanagedType.I4)]
            public int grfStateBits;
            [MarshalAs(UnmanagedType.I4)]
            public int reserved;
        }
    }
}

