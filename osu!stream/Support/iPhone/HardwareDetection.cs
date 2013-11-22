using System;
using System.Runtime.InteropServices;
using MonoTouch;
using MonoTouch.UIKit; // Only needed if you use constant instead of hardcoded library name

namespace osum.Support.iPhone
{
    public enum HardwareVersion
    {
        Uncached = 0,
        iPhone,
        iPhone3G,
        iPhone3GS,
        iPhone4,
        iPod1G,
        iPod2G,
        iPod3G,
        iPod4G,
        iPad,
        iPad2,
        iPhoneSimulator,
        iPhone4Simulator,
        iPadSimulator,
        Unknown
    }

    public class HardwareDetection // As it works on any Darwin
    {
        public const string HardwareProperty = "hw.machine"; // Change to "hw.model" for getting the model in Mac OS X and not just the CPU model

        public static int BaseOSVersion {
            get {
                int ver = 0;
                if (!int.TryParse(MonoTouch.UIKit.UIDevice.CurrentDevice.SystemVersion.Split('.')[0], out ver))
                    return -1;
                return ver;
            }
        }

        public static bool RunningiOS5OrHigher {
            get { 
                return BaseOSVersion >= 5 || BaseOSVersion == 0;
            }
        }

        public static bool RunningiOS6OrHigher {
            get { 
                    return BaseOSVersion >= 6 || BaseOSVersion == 0;
            }
        }

        // Changing the constant to "/usr/lib/libSystem.dylib" makes the P/Invoke work for Mac OS X also (tested), but returns only running arch (that's the thing it's getting in the simulator)
        // For getting the Macintosh computer model property must be "hw.model" instead (and works on ppc, ppc64, i386 and x86_64 Mac OS X)
        [DllImport(MonoTouch.Constants.SystemLibrary)]
        static internal extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

        static HardwareVersion cachedVersion = HardwareVersion.Uncached;

        public static HardwareVersion Version {
            get
            {
                if (cachedVersion != HardwareVersion.Uncached)
                    return cachedVersion;

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

                switch (hardwareStr)
                {
                case "iPhone1,1":
                    ret = HardwareVersion.iPhone;
                    break;
                case "iPhone1,2":
                    ret = HardwareVersion.iPhone3G;
                    break;
                case "iPhone2,1":
                    ret = HardwareVersion.iPhone3GS;
                    break;
                case "iPhone3,1":
                    ret = HardwareVersion.iPhone4;
                    break;
                case "iPad1,1":
                    ret = HardwareVersion.iPad;
                    break;
                case "iPad2,1":
                case "iPad2,2":
                case "iPad2,3":
                    ret = HardwareVersion.iPad2;
                    break;
                case "iPod1,1":
                    ret = HardwareVersion.iPod1G;
                    break;
                case "iPod2,1":
                    ret = HardwareVersion.iPod2G;
                    break;
                case "iPod3,1":
                    ret = HardwareVersion.iPod3G;
                    break;
                case "iPod4,1":
                    ret = HardwareVersion.iPod3G;
                    break;
                case "i386":
                case "x86_64":
                    if (UIDevice.CurrentDevice.Model.Contains("iPhone"))
                        ret = UIScreen.MainScreen.Bounds.Height * UIScreen.MainScreen.Scale == 960 || UIScreen.MainScreen.Bounds.Width * UIScreen.MainScreen.Scale == 960 ? HardwareVersion.iPhone4Simulator : HardwareVersion.iPhoneSimulator;
                    else
                        ret = HardwareVersion.iPadSimulator;
                    break;
                }

                Marshal.FreeHGlobal(pLen);
                Marshal.FreeHGlobal(pStr);

                cachedVersion = ret;
                return ret;
            }
        }
    }
}