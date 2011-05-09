namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Text;

    public sealed class Translator
    {
        private Translator()
        {
        }

        public static byte[] GetBytes(string text, string charSet)
        {
            if (!StringUtils.IsEmpty(charSet))
            {
                try
                {
                    return Encoding.GetEncoding(charSet).GetBytes(text);
                }
                catch (NotSupportedException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            return Encoding.ASCII.GetBytes(text);
        }

        public static string GetString(byte[] bytes, string charSet)
        {
            if (!StringUtils.IsEmpty(charSet))
            {
                try
                {
                    return Encoding.GetEncoding(charSet).GetString(bytes);
                }
                catch
                {
                }
            }
            return Encoding.ASCII.GetString(bytes);
        }
    }
}

