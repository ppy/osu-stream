namespace Un4seen.Bass.Misc
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    public sealed class HiPerfTimer
    {
        private long freq;
        private long startTime = 0L;
        private long stopTime = 0L;

        public HiPerfTimer()
        {
            if (!QueryPerformanceFrequency(out freq))
            {
                throw new Win32Exception();
            }
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);
        public void Start()
        {
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        public double Duration
        {
            get
            {
                return (((double) (stopTime - startTime)) / ((double) freq));
            }
        }
    }
}

