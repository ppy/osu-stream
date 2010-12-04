namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class HttpEntityHeader
    {
        private string boundary;
        private string cacheControl;
        private string charSet;
        private string connection;
        private string contentEncoding;
        private string contentLanguage;
        private string contentLength;
        private string contentType;
        private string contentVersion;
        private string date;
        private string expires;
        private string[] extraFields;
        private StringCollectionEx knownFields = new StringCollectionEx();
        private string lastModified;
        private string proxyConnection;
        private string transferEncoding;
        private int updateCount = 0;

        [Browsable(false)]
        public event EventHandler Changed;

        public HttpEntityHeader()
        {
            this.Clear();
            this.RegisterFields();
        }

        public static string AddHttpFieldItem(string fieldValue, string itemName, string itemValue)
        {
            if (!StringUtils.IsEmpty(itemValue))
            {
                return string.Format("{0}; {1}={2}", fieldValue, itemName, itemValue);
            }
            return fieldValue;
        }

        protected virtual void AssignContentType(StringCollection header)
        {
            string contentType = this.ContentType;
            if (!StringUtils.IsEmpty(contentType))
            {
                contentType = AddHttpFieldItem(AddHttpFieldItem(contentType, "boundary", this.Boundary), "charset", this.CharSet);
                HeaderFieldList.AddHeaderField(header, "Content-Type", contentType);
            }
        }

        public void AssignHeader(StringCollection header)
        {
            this.InternalAssignHeader(header);
        }

        public void BeginUpdate()
        {
            this.updateCount++;
        }

        public virtual void Clear()
        {
            this.BeginUpdate();
            try
            {
                this.Boundary = string.Empty;
                this.CharSet = string.Empty;
                this.CacheControl = string.Empty;
                this.Connection = string.Empty;
                this.ContentEncoding = string.Empty;
                this.ContentLanguage = string.Empty;
                this.ContentLength = string.Empty;
                this.ContentType = string.Empty;
                this.ContentVersion = string.Empty;
                this.Date = string.Empty;
                this.Expires = string.Empty;
                this.LastModified = string.Empty;
                this.ProxyConnection = string.Empty;
                this.TransferEncoding = string.Empty;
                this.ExtraFields = null;
            }
            finally
            {
                this.EndUpdate();
            }
        }

        public void EndUpdate()
        {
            if (this.updateCount > 0)
            {
                this.updateCount--;
                this.Update();
            }
        }

        protected virtual void InternalAssignHeader(StringCollection header)
        {
            HeaderFieldList.AddHeaderField(header, "Content-Encoding", this.ContentEncoding);
            HeaderFieldList.AddHeaderField(header, "Content-Language", this.ContentLanguage);
            HeaderFieldList.AddHeaderField(header, "Content-Length", this.ContentLength);
            HeaderFieldList.AddHeaderField(header, "Content-Version", this.ContentVersion);
            this.AssignContentType(header);
            HeaderFieldList.AddHeaderField(header, "Date", this.Date);
            HeaderFieldList.AddHeaderField(header, "Expires", this.Expires);
            HeaderFieldList.AddHeaderField(header, "LastModified", this.LastModified);
            HeaderFieldList.AddHeaderField(header, "Transfer-Encoding", this.TransferEncoding);
            HeaderFieldList.AddHeaderField(header, "Cache-Control", this.CacheControl);
            HeaderFieldList.AddHeaderField(header, "Connection", this.Connection);
            HeaderFieldList.AddHeaderField(header, "Proxy-Connection", this.ProxyConnection);
            if (this.ExtraFields != null)
            {
                header.AddRange(this.ExtraFields);
            }
        }

        protected virtual void InternalParseHeader(IList header, HeaderFieldList fieldList)
        {
            this.CacheControl = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Cache-Control");
            this.Connection = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Connection");
            this.ContentEncoding = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Encoding");
            this.ContentLanguage = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Language");
            this.ContentLength = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Length");
            this.ContentVersion = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Version");
            this.Date = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Date");
            this.Expires = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Expires");
            this.LastModified = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Last-Modified");
            this.ProxyConnection = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Proxy-Connection");
            this.TransferEncoding = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Transfer-Encoding");
            this.ParseContentType(header, fieldList);
        }

        protected virtual void OnChanged()
        {
            if (this.Changed != null)
            {
                this.Changed(this, EventArgs.Empty);
            }
        }

        protected virtual void ParseContentType(IList header, HeaderFieldList fieldList)
        {
            string source = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Type");
            this.ContentType = HeaderFieldList.GetHeaderFieldValueItem(source, string.Empty);
            this.Boundary = HeaderFieldList.GetHeaderFieldValueItem(source, "boundary=");
            this.CharSet = HeaderFieldList.GetHeaderFieldValueItem(source, "charset=");
        }

        public void ParseHeader(IList header)
        {
            this.BeginUpdate();
            try
            {
                this.Clear();
                HeaderFieldList fieldList = new HeaderFieldList();
                HeaderFieldList.GetHeaderFieldList(0, header, fieldList);
                this.InternalParseHeader(header, fieldList);
                StringCollectionEx ex = new StringCollectionEx();
                foreach (HeaderField field in fieldList)
                {
                    if (!this.knownFields.Contains(field.Name, true))
                    {
                        ex.Add(field.Name + ": " + HeaderFieldList.GetHeaderFieldValue(header, field));
                    }
                }
                if (ex.Count > 0)
                {
                    this.ExtraFields = ex.ToArray();
                }
            }
            finally
            {
                this.EndUpdate();
            }
        }

        protected void RegisterField(string fieldName)
        {
            if (!this.knownFields.Contains(fieldName, true))
            {
                this.knownFields.Add(fieldName);
            }
        }

        protected virtual void RegisterFields()
        {
            this.RegisterField("Cache-Control");
            this.RegisterField("Connection");
            this.RegisterField("Content-Encoding");
            this.RegisterField("Content-Language");
            this.RegisterField("Content-Length");
            this.RegisterField("Content-Type");
            this.RegisterField("Content-Version");
            this.RegisterField("Date");
            this.RegisterField("Expires");
            this.RegisterField("Last-Modified");
            this.RegisterField("Proxy-Connection");
            this.RegisterField("Transfer-Encoding");
        }

        public void Update()
        {
            if (this.updateCount == 0)
            {
                this.OnChanged();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Boundary
        {
            get
            {
                return this.boundary;
            }
            set
            {
                if (this.boundary != value)
                {
                    this.boundary = value;
                    this.Update();
                    this.boundary = value;
                }
            }
        }

        [DefaultValue("")]
        public string CacheControl
        {
            get
            {
                return this.cacheControl;
            }
            set
            {
                if (this.cacheControl != value)
                {
                    this.cacheControl = value;
                    this.Update();
                }
            }
        }

        [DefaultValue("")]
        public string CharSet
        {
            get
            {
                return this.charSet;
            }
            set
            {
                if (this.charSet != value)
                {
                    this.charSet = value;
                    this.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the connection type")]
        public string Connection
        {
            get
            {
                return this.connection;
            }
            set
            {
                if (this.connection != value)
                {
                    this.connection = value;
                    this.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the encoding for the server response")]
        public string ContentEncoding
        {
            get
            {
                return this.contentEncoding;
            }
            set
            {
                if (this.contentEncoding != value)
                {
                    this.contentEncoding = value;
                    this.Update();
                }
            }
        }

        [Description("Gets or sets the language for the server response"), DefaultValue("")]
        public string ContentLanguage
        {
            get
            {
                return this.contentLanguage;
            }
            set
            {
                if (this.contentLanguage != value)
                {
                    this.contentLanguage = value;
                    this.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the length of the server response")]
        public string ContentLength
        {
            get
            {
                return this.contentLength;
            }
            set
            {
                if (this.contentLength != value)
                {
                    this.contentLength = value;
                    this.Update();
                }
            }
        }

        [DefaultValue(""), Description("Gets or sets the content type for the server response")]
        public string ContentType
        {
            get
            {
                return this.contentType;
            }
            set
            {
                if (this.contentType != value)
                {
                    this.contentType = value;
                    this.Update();
                }
            }
        }

        [Description("Gets or sets the content version of the server response"), DefaultValue("")]
        public string ContentVersion
        {
            get
            {
                return this.contentVersion;
            }
            set
            {
                if (this.contentVersion != value)
                {
                    this.contentVersion = value;
                    this.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Date
        {
            get
            {
                return this.date;
            }
            set
            {
                if (this.date != value)
                {
                    this.date = value;
                    this.Update();
                }
            }
        }

        [DefaultValue("")]
        public string Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                if (this.expires != value)
                {
                    this.expires = value;
                    this.Update();
                }
            }
        }

        [Description("Gets or sets the optional headers for the request or response"), DefaultValue((string) null)]
        public string[] ExtraFields
        {
            get
            {
                return this.extraFields;
            }
            set
            {
                this.extraFields = value;
                this.Update();
            }
        }

        [DefaultValue("")]
        public string LastModified
        {
            get
            {
                return this.lastModified;
            }
            set
            {
                if (this.lastModified != value)
                {
                    this.lastModified = value;
                    this.Update();
                }
            }
        }

        [DefaultValue("")]
        public string ProxyConnection
        {
            get
            {
                return this.proxyConnection;
            }
            set
            {
                if (this.proxyConnection != value)
                {
                    this.proxyConnection = value;
                    this.Update();
                }
            }
        }

        [DefaultValue("")]
        public string TransferEncoding
        {
            get
            {
                return this.transferEncoding;
            }
            set
            {
                if (this.transferEncoding != value)
                {
                    this.transferEncoding = value;
                    this.Update();
                }
            }
        }
    }
}

