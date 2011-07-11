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

        public FileNetRequest(string path, string url) : base(url)
        {
            this.path = path;
        }

        protected override void processFinishedRequest()
        {
            File.WriteAllBytes(path, data);
            base.processFinishedRequest();
        }

    }
}