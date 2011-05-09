namespace osu_common.Libraries.NetLib
{
    using System;

    public class HttpRedirectEventArgs : EventArgs
    {
        private bool canRedirect;
        private bool handled;
        private string method;
        private string[] requestHeader;
        private HttpResponseHeader responseHeader;
        private string[] responseText;
        private int statusCode;

        public HttpRedirectEventArgs(string[] requestHeader, int statusCode, HttpResponseHeader responseHeader, string[] responseText, string method, bool canRedirect, bool handled)
        {
            this.requestHeader = requestHeader;
            this.statusCode = statusCode;
            this.responseHeader = responseHeader;
            this.responseText = responseText;
            this.method = method;
            this.canRedirect = canRedirect;
            this.handled = handled;
        }

        public bool CanRedirect
        {
            get
            {
                return this.canRedirect;
            }
            set
            {
                this.canRedirect = value;
            }
        }

        public bool Handled
        {
            get
            {
                return this.handled;
            }
            set
            {
                this.handled = value;
            }
        }

        public string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        public string[] RequestHeader
        {
            get
            {
                return this.requestHeader;
            }
        }

        public HttpResponseHeader ResponseHeader
        {
            get
            {
                return this.responseHeader;
            }
        }

        public string[] ResponseText
        {
            get
            {
                return this.responseText;
            }
        }

        public int StatusCode
        {
            get
            {
                return this.statusCode;
            }
        }
    }
}

