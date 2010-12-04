namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class HttpRequestHeader : HttpEntityHeader
    {
        private string accept;
        private string acceptCharSet;
        private string acceptEncoding;
        private string acceptLanguage;
        private string authorization;
        private string host;
        private string proxyAuthorization;
        private string range;
        private string referer;
        private string userAgent;

        public override void Clear()
        {
            base.BeginUpdate();
            try
            {
                base.Clear();
                this.Accept = "*/*";
                this.AcceptCharSet = string.Empty;
                this.AcceptEncoding = string.Empty;
                this.AcceptLanguage = string.Empty;
                this.Authorization = string.Empty;
                this.Host = string.Empty;
                this.ProxyAuthorization = string.Empty;
                this.Range = string.Empty;
                this.Referer = string.Empty;
                this.UserAgent = string.Empty;
            }
            finally
            {
                base.EndUpdate();
            }
        }

        protected override void InternalAssignHeader(StringCollection header)
        {
            HeaderFieldList.AddHeaderField(header, "Accept", this.Accept);
            HeaderFieldList.AddHeaderField(header, "Accept-Charset", this.AcceptCharSet);
            HeaderFieldList.AddHeaderField(header, "Accept-Encoding", this.AcceptEncoding);
            HeaderFieldList.AddHeaderField(header, "Accept-Language", this.AcceptLanguage);
            HeaderFieldList.AddHeaderField(header, "Range", this.Range);
            HeaderFieldList.AddHeaderField(header, "Referer", this.Referer);
            HeaderFieldList.AddHeaderField(header, "Host", this.Host);
            HeaderFieldList.AddHeaderField(header, "User-Agent", this.UserAgent);
            HeaderFieldList.AddHeaderField(header, "Authorization", this.Authorization);
            HeaderFieldList.AddHeaderField(header, "Proxy-Authorization", this.ProxyAuthorization);
            base.InternalAssignHeader(header);
        }

        protected override void InternalParseHeader(IList header, HeaderFieldList fieldList)
        {
            base.InternalParseHeader(header, fieldList);
            this.Accept = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Accept");
            this.AcceptCharSet = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Accept-Charset");
            this.AcceptEncoding = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Accept-Encoding");
            this.AcceptLanguage = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Accept-Language");
            this.Authorization = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Authorization");
            this.Host = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Host");
            this.ProxyAuthorization = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Proxy-Authorization");
            this.Range = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Range");
            this.Referer = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Referer");
            this.UserAgent = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "User-Agent");
        }

        protected override void RegisterFields()
        {
            base.RegisterFields();
            base.RegisterField("Accept");
            base.RegisterField("Accept-Charset");
            base.RegisterField("Accept-Encoding");
            base.RegisterField("Accept-Language");
            base.RegisterField("Authorization");
            base.RegisterField("Host");
            base.RegisterField("Proxy-Authorization");
            base.RegisterField("Range");
            base.RegisterField("Referer");
            base.RegisterField("User-Agent");
        }

        [DefaultValue("*/*"), Description("Gets or sets the acceptable media types")]
        public string Accept
        {
            get
            {
                return this.accept;
            }
            set
            {
                if (this.accept != value)
                {
                    this.accept = value;
                    base.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the acceptable character sets")]
        public string AcceptCharSet
        {
            get
            {
                return this.acceptCharSet;
            }
            set
            {
                if (this.acceptCharSet != value)
                {
                    this.acceptCharSet = value;
                    base.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the acceptable response encoding")]
        public string AcceptEncoding
        {
            get
            {
                return this.acceptEncoding;
            }
            set
            {
                if (this.acceptEncoding != value)
                {
                    this.acceptEncoding = value;
                    base.Update();
                }
            }
        }

        [Description("Gets or sets the preferred language"), DefaultValue("")]
        public string AcceptLanguage
        {
            get
            {
                return this.acceptLanguage;
            }
            set
            {
                if (this.acceptLanguage != value)
                {
                    this.acceptLanguage = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Authorization
        {
            get
            {
                return this.authorization;
            }
            set
            {
                if (this.authorization != value)
                {
                    this.authorization = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Host
        {
            get
            {
                return this.host;
            }
            set
            {
                if (this.host != value)
                {
                    this.host = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string ProxyAuthorization
        {
            get
            {
                return this.proxyAuthorization;
            }
            set
            {
                if (this.proxyAuthorization != value)
                {
                    this.proxyAuthorization = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Range
        {
            get
            {
                return this.range;
            }
            set
            {
                if (this.range != value)
                {
                    this.range = value;
                    base.Update();
                }
            }
        }

        [Description("Gets or sets the URL for a referring document"), DefaultValue("")]
        public string Referer
        {
            get
            {
                return this.referer;
            }
            set
            {
                if (this.referer != value)
                {
                    this.referer = value;
                    base.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the name of the program making a HTTP request")]
        public string UserAgent
        {
            get
            {
                return this.userAgent;
            }
            set
            {
                if (this.userAgent != value)
                {
                    this.userAgent = value;
                    base.Update();
                }
            }
        }
    }
}

