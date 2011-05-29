using System;
using System.Runtime.InteropServices;
using MonoTouch;
using MonoTouch.UIKit; // Only needed if you use constant instead of hardcoded library name

namespace osum.Support.iPhone
{
    public enum HardwareVersion
    {
        iPhone,
        iPhone3G,
        iPhone3GS,
        iPhone4,
        iPod1G,
        iPod2G,
        iPod3G,
        iPod4G,
        iPad,
        iPhoneSimulator,
        iPhone4Simulator,
        iPadSimulator,
        Unknown
    }

    public class HardwareDetection // As it works on any Darwin
    {
        public const string HardwareProperty = "hw.machine"; // Change to "hw.model" for getting the model in Mac OS X and not just the CPU model


        // Changing the constant to "/usr/lib/libSystem.dylib" makes the P/Invoke work for Mac OS X also (tested), but returns only running arch (that's the thing it's getting in the simulator)
        // For getting the Macintosh computer model property must be "hw.model" instead (and works on ppc, ppc64, i386 and x86_64 Mac OS X)
        [DllImport(MonoTouch.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        public static HardwareVersion Version {
            get
            {
                Console.WriteLine("system library is {0}.", MonoTouch.Constants.SystemLibrary);
                var pLen = Marshal.AllocHGlobal(sizeof(int));
                sysctlbyname(HardwareProperty, IntPtr.Zero, pLen, IntPtr.Zero, 0);

                var length = Marshal.ReadInt32(pLen);

                if (length == 0)
                {
                    Marshal.FreeHGlobal(pLen);

                    return HardwareVersion.Unknown;
                }

                var pStr = Marshal.AllocHGlobal(length);
                sysctlbyname(HardwareProperty, pStr, pLen, IntPtr.Zero, 0);

                var hardwareStr = Marshal.PtrToStringAnsi(pStr);
                var ret = HardwareVersion.Unknown;

                if (hardwareStr == "iPhone1,1")
                    ret = HardwareVersion.iPhone;
                else if (hardwareStr == "iPhone1,2")
                    ret = HardwareVersion.iPhone3G;
                else if (hardwareStr == "iPhone2,1")
                    ret = HardwareVersion.iPhone3GS;
                else if (hardwareStr == "iPhone3,1")
                    ret = HardwareVersion.iPhone4;
                else if (hardwareStr == "iPad1,1")
                    ret = HardwareVersion.iPad;
                else if (hardwareStr == "iPod1,1")
                    ret = HardwareVersion.iPod1G;
                else if (hardwareStr == "iPod2,1")
                    ret = HardwareVersion.iPod2G;
                else if (hardwareStr == "iPod3,1")
                    ret = HardwareVersion.iPod3G;
                else if (hardwareStr == "iPod4,1")
                    ret = HardwareVersion.iPod3G;
                else if (hardwareStr == "i386" || hardwareStr == "x86_64")
                {
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                        ret = UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale == 960 || UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale == 960 ? HardwareVersion.iPhone4Simulator : HardwareVersion.iPhoneSimulator;
                    else
                        ret = HardwareVersion.iPadSimulator;
                }

                Marshal.FreeHGlobal(pLen);
                Marshal.FreeHGlobal(pStr);

                return ret;
            }
        }
    }
}