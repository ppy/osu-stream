namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Guid("15CC68E3-27CC-4ecd-B222-3F5D02D80BD5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
    internal interface IWMHeaderInfo3
    {
        uint GetAttributeCount([In] ushort wStreamNum, out ushort pcAttributes);
        uint GetAttributeByIndex([In] ushort wIndex, [In, Out] ref ushort pwStreamNum, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszName, [In, Out] ref ushort pcchNameLen, out WMT_ATTR_DATATYPE pType, IntPtr pValue, [In, Out] ref ushort pcbLength);
        uint GetAttributeByName([In, Out] ref ushort pwStreamNum, [Out, MarshalAs(UnmanagedType.LPWStr)] string pszName, out WMT_ATTR_DATATYPE pType, IntPtr pValue, [In, Out] ref ushort pcbLength);
        uint SetAttribute([In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In] WMT_ATTR_DATATYPE Type, IntPtr pValue, [In] ushort cbLength);
        uint GetMarkerCount(out ushort pcMarkers);
        uint GetMarker([In] ushort wIndex, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName, [In, Out] ref ushort pcchMarkerNameLen, out ulong pcnsMarkerTime);
        uint AddMarker([In, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName, [In] ulong cnsMarkerTime);
        uint RemoveMarker([In] ushort wIndex);
        uint GetScriptCount(out ushort pcScripts);
        uint GetScript([In] ushort wIndex, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszType, [In, Out] ref ushort pcchTypeLen, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand, [In, Out] ref ushort pcchCommandLen, out ulong pcnsScriptTime);
        uint AddScript([In, MarshalAs(UnmanagedType.LPWStr)] string pwszType, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand, [In] ulong cnsScriptTime);
        uint RemoveScript([In] ushort wIndex);
        uint GetCodecInfoCount(out uint pcCodecInfos);
        uint GetCodecInfo([In] uint wIndex, [In, Out] ref ushort pcchName, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszName, [In, Out] ref ushort pcchDescription, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription, out WMT_CODEC_INFO_TYPE pCodecType, [In, Out] ref ushort pcbCodecInfo, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo);
        uint GetAttributeCountEx([In] ushort wStreamNum, out ushort pcAttributes);
        uint GetAttributeIndices([In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName, [In] ref ushort pwLangIndex, [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwIndices, [In, Out] ref ushort pwCount);
        uint GetAttributeByIndexEx([In] ushort wStreamNum, [In] ushort wIndex, [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszName, [In, Out] ref ushort pwNameLen, out WMT_ATTR_DATATYPE pType, out ushort pwLangIndex, IntPtr pValue, [In, Out] ref uint pdwDataLength);
        uint ModifyAttribute([In] ushort wStreamNum, [In] ushort wIndex, [In] WMT_ATTR_DATATYPE Type, [In] ushort wLangIndex, IntPtr pValue, [In] uint dwLength);
        uint AddAttribute([In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName, out ushort pwIndex, [In] WMT_ATTR_DATATYPE Type, [In] ushort wLangIndex, IntPtr pValue, [In] uint dwLength);
        uint DeleteAttribute([In] ushort wStreamNum, [In] ushort wIndex);
        uint AddCodecInfo([In, MarshalAs(UnmanagedType.LPWStr)] string pszName, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription, [In] WMT_CODEC_INFO_TYPE codecType, [In] ushort cbCodecInfo, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo);
    }
}

