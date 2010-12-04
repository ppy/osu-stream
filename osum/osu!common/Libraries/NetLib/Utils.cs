namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;

    public sealed class Utils
    {
        private static int TwoDigitYearCenturyWindow = 50;

        private Utils()
        {
        }

        public static string ExtractFileName(string path)
        {
            string fileName = Path.GetFileName(path);
            if (fileName == null)
            {
                return string.Empty;
            }
            return fileName;
        }

        public static int GetCorrectY2k(int year)
        {
            if (year >= 100)
            {
                return year;
            }
            int num = year;
            if (TwoDigitYearCenturyWindow > 0)
            {
                if (num > TwoDigitYearCenturyWindow)
                {
                    return (num + (((DateTime.Today.Year / 100) - 1) * 100));
                }
                return (num + ((DateTime.Today.Year / 100) * 100));
            }
            return (num + ((DateTime.Today.Year / 100) * 100));
        }

        public static int IndexOfArray(byte[] theValue, byte[] buffer, int startPos)
        {
            return IndexOfArray(theValue, buffer, startPos, buffer.Length);
        }

        public static int IndexOfArray(byte[] theValue, byte[] buffer, int startPos, int length)
        {
            int index = 0;
            int num2 = theValue.Length;
            for (int i = startPos; i < length; i++)
            {
                if (buffer[i] == theValue[index])
                {
                    index++;
                }
                else
                {
                    index = 0;
                    continue;
                }
                if (index == num2)
                {
                    return ((i - num2) + 1);
                }
            }
            return -1;
        }
    }
}

