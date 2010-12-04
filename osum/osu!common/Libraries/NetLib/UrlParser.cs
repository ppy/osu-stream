namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Globalization;

    internal class UrlParser
    {
        private string absolutePath;
        private string absoluteUri;
        private string host;
        private string password;
        private int port;
        private string scheme;
        private Uri url;
        private string userName;

        private void InternalParse(Uri url)
        {
            this.url = url;
            this.host = string.Empty;
            this.port = 80;
            this.absolutePath = string.Empty;
            this.absoluteUri = string.Empty;
            this.userName = string.Empty;
            this.password = string.Empty;
            this.scheme = string.Empty;
            if (this.url != null)
            {
                this.host = this.url.Host;
                this.port = this.url.Port;
                this.absolutePath = this.url.PathAndQuery;
                this.absoluteUri = this.url.AbsoluteUri;
                this.scheme = this.url.Scheme;
                int index = this.url.UserInfo.IndexOf(':');
                if (index > 0)
                {
                    this.userName = this.url.UserInfo.Substring(0, index);
                    this.password = this.url.UserInfo.Substring(index + 1);
                }
                else
                {
                    this.userName = this.url.UserInfo;
                }
            }
        }

        public void Parse(string url)
        {
            try
            {
                this.InternalParse(new Uri(url));
            }
            catch (UriFormatException)
            {
                if (url != "*")
                {
                    if (StringUtils.IsEmpty(url) || (url.ToLower(CultureInfo.InvariantCulture).IndexOf(Uri.UriSchemeHttp) >= 0))
                    {
                        throw;
                    }
                    this.InternalParse(new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + url));
                }
                else
                {
                    this.InternalParse(null);
                    this.host = "*";
                    this.port = 80;
                    this.absolutePath = "/";
                    this.absoluteUri = "http://*/";
                    this.userName = string.Empty;
                    this.password = string.Empty;
                    this.scheme = Uri.UriSchemeHttp;
                }
            }
        }

        public void Parse(Uri baseUri, string relativeUri)
        {
            this.InternalParse(new Uri(baseUri, relativeUri));
        }

        public string AbsolutePath
        {
            get
            {
                return this.absolutePath;
            }
        }

        public string AbsoluteUri
        {
            get
            {
                return this.absoluteUri;
            }
        }

        public string Host
        {
            get
            {
                return this.host;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
        }

        public string Scheme
        {
            get
            {
                return this.scheme;
            }
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }
        }
    }
}

