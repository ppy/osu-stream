namespace osu_common.Libraries.NetLib
{
    using System;

    public class HttpRequestEventArgs : EventArgs
    {
        private string method;
        private string[] requestHeader;
        private Uri url;

        public HttpRequestEventArgs(string method, Uri url, string[] requestHeader)
        {
            this.method = method;
            this.url = url;
            this.requestHeader = requestHeader;
        }

        public string Method
        {
            get
            {
                return this.method;
            }
        }

        public string[] RequestHeader
        {
            get
            {
                return this.requestHeader;
            }
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
        }
    }
}

