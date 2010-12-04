using System;
using System.Drawing;
using System.Runtime.InteropServices;

[ComImport, Guid("0000000C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IStream
{
    int Read([In] IntPtr buf, [In] int len);
    int Write([In] IntPtr buf, [In] int len);
    [return: MarshalAs(UnmanagedType.I8)]
    long Seek([In, MarshalAs(UnmanagedType.I8)] long dlibMove, [In] int dwOrigin);
    void SetSize([In, MarshalAs(UnmanagedType.I8)] long libNewSize);
    [return: MarshalAs(UnmanagedType.I8)]
    long CopyTo([In, MarshalAs(UnmanagedType.Interface)] IStream pstm, [In, MarshalAs(UnmanagedType.I8)] long cb, [Out, MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);
    void Commit([In] int grfCommitFlags);
    void Revert();
    void LockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);
    void UnlockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, [In] int dwLockType);
    void Stat([In] IntPtr pStatstg, [In] int grfStatFlag);
    [return: MarshalAs(UnmanagedType.Interface)]
    IStream Clone();
}

