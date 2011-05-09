namespace osu_common.Libraries.NetLib
{
    using System;

    public class HttpResponseEventArgs : EventArgs
    {
        private CookieList cookies;
        private string method;
        private string[] responseHeader;
        private Uri url;

        public HttpResponseEventArgs(string method, Uri url, string[] responseHeader, CookieList cookies)
        {
            this.method = method;
            this.url = url;
            this.responseHeader = responseHeader;
            this.cookies = cookies;
        }

        public CookieList Cookies
        {
            get
            {
                return this.cookies;
            }
        }

        public string Method
        {
            get
            {
                return this.method;
            }
        }

        public string[] ResponseHeader
        {
            get
            {
                return this.responseHeader;
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

