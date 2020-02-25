using System;

namespace osum.Helpers
{
    public static class UnixTimestamp
    {
        private static DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        internal static DateTime Parse(int timestamp)
        {
            return origin.AddSeconds(timestamp);
        }

        internal static int FromDateTime(DateTime time)
        {
            return (int)time.Subtract(origin).TotalSeconds;
        }
    }
}