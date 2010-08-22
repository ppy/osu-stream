namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Guid("96406BD9-2B2B-11d3-B36B-00C04F6108FF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface IWMMetadataEditor
    {
        uint Open([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename);
        uint Close();
        uint Flush();
    }
}

