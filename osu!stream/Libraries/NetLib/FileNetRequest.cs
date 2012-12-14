using System;
using System.IO;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Downloads a file from the internet to a specified location
    /// </summary>
    public class FileNetRequest : DataNetRequest
    {
        private string path;

        public FileNetRequest(string path, string url, string method = "GET", string postData = null) : base(url, method, postData)
        {
            this.path = path;
        }

        public override void processFinishedRequest()
        {
            if (data != null && error == null && data.Length > 0)
                File.WriteAllBytes(path, data);
            base.processFinishedRequest();
        }

    }
}