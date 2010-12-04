using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using STATSTG=System.Runtime.InteropServices.ComTypes.STATSTG;

namespace osu_common.Libraries
{
    /// <summary>
    /// COM IStream wrapper for a MemoryStream.
    /// Thanks to Willy Denoyette for the if(System.IntPr != IntPtr.Zero) test for a NULL parameter via COM
    /// CLR will make the class implement the IDispatch COM interface
    /// so COM objects can make calls to IMemoryStream methods
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class IMemoryStream : MemoryStream, IStream
    {
        public IMemoryStream(byte[] data)
            : base(data)
        {

        }
        
        // convenience method for writing Strings to the stream

        // Implementation of the IStream interface

        #region IStream Members

        public void Clone(out IStream ppstm)
        {
            ppstm = null;
        }

        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            long bytesRead = Read(pv, 0, cb);
            if (pcbRead != IntPtr.Zero) Marshal.WriteInt64(pcbRead, bytesRead);
        }

        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            Write(pv, 0, cb);
            if (pcbWritten != IntPtr.Zero) Marshal.WriteInt64(pcbWritten, cb);
        }

        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            long pos = base.Seek(dlibMove, (SeekOrigin) dwOrigin);
            if (plibNewPosition != IntPtr.Zero) Marshal.WriteInt64(plibNewPosition, pos);
        }

        public void SetSize(long libNewSize)
        {
        }

        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
        }

        public void Commit(int grfCommitFlags)
        {
        }

        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        public void Revert()
        {
        }

        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
        }

        public void Stat(out STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new STATSTG();
        }

        #endregion

        public void Write(string s)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] pv = encoding.GetBytes(s);
            Write(pv, 0, pv.GetLength(0));
        }
    }
}