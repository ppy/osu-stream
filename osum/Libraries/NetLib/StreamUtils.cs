namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;

    public sealed class StreamUtils
    {
        private StreamUtils()
        {
        }

        public static long Copy(Stream source, Stream destination, long count)
        {
            if (count == 0L)
            {
                source.Position = 0L;
                count = source.Length;
            }
            long num = count;
            byte[] buffer = new byte[0xf000];
            int length = (int) count;
            if (length > buffer.Length)
            {
                length = buffer.Length;
            }
            while (count != 0L)
            {
                int num3 = (int) count;
                if (count > length)
                {
                    num3 = length;
                }
                source.Read(buffer, 0, num3);
                destination.Write(buffer, 0, num3);
                count -= num3;
            }
            return num;
        }
    }
}

