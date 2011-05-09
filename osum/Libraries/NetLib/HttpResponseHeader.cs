namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;

    public class HttpResponseHeader : HttpEntityHeader
    {
        private string acceptRanges;
        private string age;
        private string allow;
        private string[] authenticate;
        private string contentRange;
        private string eTag;
        private string location;
        private string[] proxyAuthenticate;
        private string retryAfter;
        private string server;

        public override void Clear()
        {
            base.BeginUpdate();
            try
            {
                base.Clear();
                this.AcceptRanges = string.Empty;
                this.Age = string.Empty;
                this.Allow = string.Empty;
                this.Authenticate = null;
                this.ContentRange = string.Empty;
                this.ETag = string.Empty;
                this.Location = string.Empty;
                this.ProxyAuthenticate = null;
                this.RetryAfter = string.Empty;
                this.Server = string.Empty;
            }
            finally
            {
                base.EndUpdate();
            }
        }

        private string[] GetAuthChallenge(IList header, HeaderFieldList fieldList, string fieldName)
        {
            ArrayList list = new ArrayList();
            foreach (HeaderField field in fieldList)
            {
                if (string.Compare(fieldName, field.Name, true, CultureInfo.InvariantCulture) == 0)
                {
                    list.Add(HeaderFieldList.GetHeaderFieldValue(header, field));
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        protected override void InternalAssignHeader(StringCollection header)
        {
            HeaderFieldList.AddHeaderField(header, "Allow", this.Allow);
            HeaderFieldList.AddHeaderField(header, "Accept-Ranges", this.AcceptRanges);
            HeaderFieldList.AddHeaderField(header, "Age", this.Age);
            HeaderFieldList.AddHeaderField(header, "Content-Range", this.ContentRange);
            HeaderFieldList.AddHeaderField(header, "Content-Encoding", this.ContentEncoding);
            HeaderFieldList.AddHeaderField(header, "ETag", this.ETag);
            HeaderFieldList.AddHeaderField(header, "Location", this.Location);
            HeaderFieldList.AddHeaderField(header, "Retry-After", this.RetryAfter);
            HeaderFieldList.AddHeaderField(header, "Server", this.Server);
            this.SetAuthChallenge(header, "WWW-Authenticate", this.Authenticate);
            this.SetAuthChallenge(header, "Proxy-Authenticate", this.ProxyAuthenticate);
            base.InternalAssignHeader(header);
        }

        protected override void InternalParseHeader(IList header, HeaderFieldList fieldList)
        {
            base.InternalParseHeader(header, fieldList);
            this.AcceptRanges = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Accept-Ranges");
            this.Age = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Age");
            this.Allow = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Allow");
            this.ContentRange = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Range");
            this.ContentEncoding = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Encoding");
            this.ETag = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "ETag");
            this.Location = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Location");
            this.RetryAfter = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Retry-After");
            this.Server = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Server");
            this.Authenticate = this.GetAuthChallenge(header, fieldList, "WWW-Authenticate");
            this.ProxyAuthenticate = this.GetAuthChallenge(header, fieldList, "Proxy-Authenticate");
        }

        protected override void RegisterFields()
        {
            base.RegisterFields();
            base.RegisterField("Accept-Ranges");
            base.RegisterField("Age");
            base.RegisterField("Allow");
            base.RegisterField("WWW-Authenticate");
            base.RegisterField("Content-Range");
            base.RegisterField("Content-Encoding");
            base.RegisterField("ETag");
            base.RegisterField("Location");
            base.RegisterField("Proxy-Authenticate");
            base.RegisterField("Retry-After");
            base.RegisterField("Server");
            base.RegisterField("Transfer-Encoding");
        }

        private void SetAuthChallenge(StringCollection header, string fieldName, IEnumerable authChallenge)
        {
            if (authChallenge != null)
            {
                foreach (string str in authChallenge)
                {
                    HeaderFieldList.AddHeaderField(header, fieldName, str);
                }
            }
        }

        [DefaultValue("")]
        public string AcceptRanges
        {
            get
            {
                return this.acceptRanges;
            }
            set
            {
                if (this.acceptRanges != value)
                {
                    this.acceptRanges = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Age
        {
            get
            {
                return this.age;
            }
            set
            {
                if (this.age != value)
                {
                    this.age = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Allow
        {
            get
            {
                return this.allow;
            }
            set
            {
                if (this.allow != value)
                {
                    this.allow = value;
                    base.Update();
                }
            }
        }

        [DefaultValue((string) null)]
        public string[] Authenticate
        {
            get
            {
                return this.authenticate;
            }
            set
            {
                this.authenticate = value;
                base.Update();
            }
        }

        [DefaultValue("")]
        public string ContentRange
        {
            get
            {
                return this.contentRange;
            }
            set
            {
                if (this.contentRange != value)
                {
                    this.contentRange = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string ETag
        {
            get
            {
                return this.eTag;
            }
            set
            {
                if (this.eTag != value)
                {
                    this.eTag = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                if (this.location != value)
                {
                    this.location = value;
                    base.Update();
                }
            }
        }

        [DefaultValue((string) null)]
        public string[] ProxyAuthenticate
        {
            get
            {
                return this.proxyAuthenticate;
            }
            set
            {
                this.proxyAuthenticate = value;
                base.Update();
            }
        }

        [DefaultValue("")]
        public string RetryAfter
        {
            get
            {
                return this.retryAfter;
            }
            set
            {
                if (this.retryAfter != value)
                {
                    this.retryAfter = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Server
        {
            get
            {
                return this.server;
            }
            set
            {
                if (this.server != value)
                {
                    this.server = value;
                    base.Update();
                }
            }
        }
    }
}

