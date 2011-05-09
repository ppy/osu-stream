namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    internal sealed class HttpRequestUtils
    {
        public static string DownloadUrl(string url)
        {
            WebResponse response = WebRequest.Create(url).GetResponse();
            string str = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                {
                    return reader.ReadToEnd();
                }
            }
            finally
            {
                response.Close();
            }
        }
    }
}

