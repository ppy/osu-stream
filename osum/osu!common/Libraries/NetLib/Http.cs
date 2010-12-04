using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace osu_common.Libraries.NetLib
{
    public class Http : TcpClient
    {
        public const int DefaultHttpPort = 80;
        public const int DefaultHttpProxyPort = 0x1f90;
        public const string DefaultInternetAgent = "Mozilla/4.0 (compatible; Clever Internet Suite 6.1)";
        private static readonly string[] cacheFields = new[] {"Pragma", "Cache-Control"};
        private static readonly string[] connFields = new[] {"Connection", "Proxy-Connection"};
        private static readonly string[] httpVersions = new[] {"HTTP/1.0", "HTTP/1.1"};
        private static readonly object receiveProgress = new object();
        private static readonly object redirecting = new object();
        private static readonly object requestSent = new object();
        private static readonly object responseReceived = new object();
        private static readonly object sendProgress = new object();
        private readonly CookieList cookies;
        private readonly HttpProxySettings proxySettings;
        private readonly HttpResponseHeader responseHeader;
        private readonly UrlParser url;
        private bool allowCaching;
        private bool allowCompression;
        private bool allowCookies;
        private bool allowRedirects;
        private AuthenticationType authenticationType;
        private HttpVersion httpVersion;
        private bool keepConnection;
        private int maxAuthRetries;
        private int maxRedirects;
        private string method;
        private int oldProxyPort;
        private string oldProxyServer;
        private string password;
        private bool progressHandled;
        private HttpRequest request;
        private HttpVersion responseVersion;
        private int statusCode;
        private string userAgent;
        private string userName;

        public Http()
        {
            responseHeader = new HttpResponseHeader();
            cookies = new CookieList();
            proxySettings = new HttpProxySettings();
            url = new UrlParser();
            httpVersion = HttpVersion.Http1_1;
            authenticationType = AuthenticationType.AutoDetect;
            userAgent = "Mozilla/4.0 (compatible; Clever Internet Suite 6.1)";
            keepConnection = true;
            allowCaching = true;
            allowRedirects = true;
            allowCookies = true;
            allowCompression = true;
            maxRedirects = 15;
            maxAuthRetries = 4;
            userName = string.Empty;
            password = string.Empty;
        }

        public bool AllowCaching
        {
            get { return allowCaching; }
            set
            {
                if (allowCaching != value)
                {
                    allowCaching = value;
                    OnChanged(new PropertyChangedEventArgs("AllowCaching"));
                }
            }
        }

        public bool AllowCompression
        {
            get { return allowCompression; }
            set
            {
                if (allowCompression != value)
                {
                    allowCompression = value;
                    OnChanged(new PropertyChangedEventArgs("AllowCompression"));
                }
            }
        }

        public bool AllowCookies
        {
            get { return allowCookies; }
            set
            {
                if (allowCookies != value)
                {
                    allowCookies = value;
                    OnChanged(new PropertyChangedEventArgs("AllowCookies"));
                }
            }
        }

        public bool AllowRedirects
        {
            get { return allowRedirects; }
            set
            {
                if (allowRedirects != value)
                {
                    allowRedirects = value;
                    OnChanged(new PropertyChangedEventArgs("AllowRedirects"));
                }
            }
        }

        public AuthenticationType AuthenticationType
        {
            get { return authenticationType; }
            set
            {
                if (authenticationType != value)
                {
                    authenticationType = value;
                    OnChanged(new PropertyChangedEventArgs("AuthenticationType"));
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CookieList Cookies
        {
            get { return cookies; }
        }

        public HttpVersion HttpVersion
        {
            get { return httpVersion; }
            set
            {
                if (httpVersion != value)
                {
                    httpVersion = value;
                    OnChanged(new PropertyChangedEventArgs("HttpVersion"));
                }
            }
        }

        public bool KeepConnection
        {
            get { return keepConnection; }
            set
            {
                if (keepConnection != value)
                {
                    keepConnection = value;
                    OnChanged(new PropertyChangedEventArgs("KeepConnection"));
                }
            }
        }

        public int MaxAuthRetries
        {
            get { return maxAuthRetries; }
            set
            {
                if (maxAuthRetries != value)
                {
                    maxAuthRetries = value;
                    OnChanged(new PropertyChangedEventArgs("MaxAuthRetries"));
                }
            }
        }

        public int MaxRedirects
        {
            get { return maxRedirects; }
            set
            {
                if (maxRedirects != value)
                {
                    maxRedirects = value;
                    OnChanged(new PropertyChangedEventArgs("MaxRedirects"));
                }
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (password != value)
                {
                    password = value;
                    OnChanged(new PropertyChangedEventArgs("Password"));
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public HttpProxySettings ProxySettings
        {
            get { return proxySettings; }
        }

        public HttpRequest Request
        {
            get { return request; }
            set
            {
                request = value;
                OnChanged(new PropertyChangedEventArgs("Request"));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpResponseHeader ResponseHeader
        {
            get { return responseHeader; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpVersion ResponseVersion
        {
            get { return responseVersion; }
        }

        [Browsable(false)]
        public int StatusCode
        {
            get { return statusCode; }
        }

        internal UrlParser Url
        {
            get { return url; }
        }

        public string UserAgent
        {
            get { return userAgent; }
            set
            {
                if (userAgent != value)
                {
                    userAgent = value;
                    OnChanged(new PropertyChangedEventArgs("UserAgent"));
                }
            }
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnChanged(new PropertyChangedEventArgs("UserName"));
                }
            }
        }

        public event SocketProgressEventHandler ReceiveProgress
        {
            add { base.Events.AddHandler(receiveProgress, value); }
            remove { base.Events.RemoveHandler(receiveProgress, value); }
        }

        public event HttpRedirectEventHandler Redirecting
        {
            add { base.Events.AddHandler(redirecting, value); }
            remove { base.Events.RemoveHandler(redirecting, value); }
        }

        public event HttpRequestEventHandler RequestSent
        {
            add { base.Events.AddHandler(requestSent, value); }
            remove { base.Events.RemoveHandler(requestSent, value); }
        }

        public event HttpResponseEventHandler ResponseReceived
        {
            add { base.Events.AddHandler(responseReceived, value); }
            remove { base.Events.RemoveHandler(responseReceived, value); }
        }

        public event SocketProgressEventHandler SendProgress
        {
            add { base.Events.AddHandler(sendProgress, value); }
            remove { base.Events.RemoveHandler(sendProgress, value); }
        }

        private void ConnectProxy()
        {
            bool allowCompression = AllowCompression;
            bool allowCookies = AllowCookies;
            HttpVersion httpVersion = HttpVersion;
            string method = this.method;
            try
            {
                HttpVersion = HttpVersion.Http1_0;
                KeepConnection = true;
                AllowCompression = false;
                AllowCaching = false;
                AllowCookies = false;
                InternalSendRequest("CONNECT", null, null, null);
            }
            finally
            {
                this.method = method;
                HttpVersion = httpVersion;
                AllowCookies = allowCookies;
                AllowCompression = allowCompression;
            }
        }

        public virtual void Delete(string url)
        {
            HttpRequest request = null;
            HttpRequest request2 = Request;
            try
            {
                if (request2 == null)
                {
                    request = new HttpRequest();
                    request2 = request;
                }
                SendRequest("DELETE", url, request2, null);
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                }
            }
        }

        private void DoDataReceiveProgress(object sender, SocketProgressEventArgs e)
        {
            OnReceiveProgress(e);
        }

        private void DoDataSendProgress(object sender, SocketProgressEventArgs e)
        {
            OnSendProgress(e);
        }

        private HttpVersion ExtractResponseVersion(byte[] buffer, int startPos)
        {
            int count = Utils.IndexOfArray(new byte[] {0x20}, buffer, startPos);
            if (count > -1)
            {
                string str = Encoding.ASCII.GetString(buffer, 0, count).Trim().ToUpper(CultureInfo.InvariantCulture);
                foreach (HttpVersion version in Enum.GetValues(typeof (HttpVersion)))
                {
                    if (httpVersions[(int) version] == str)
                    {
                        return version;
                    }
                }
            }
            return HttpVersion;
        }

        private int ExtractStatusCode(byte[] buffer, int startPos)
        {
            int num = Utils.IndexOfArray(new byte[] {0x20}, buffer, startPos);
            if (num > -1)
            {
                return StringUtils.StrToIntDef(Encoding.ASCII.GetString(buffer, num + 1, 3), 0);
            }
            return 0;
        }

        public string[] Get(string url)
        {
            Stream destination = new MemoryStream();
            Get(url, destination);
            destination.Position = 0L;
            var ex = new StringCollectionEx();
            ex.LoadFromStream(destination, ResponseHeader.CharSet);
            return ex.ToArray();
        }

        public void Get(string url, Stream destination)
        {
            HttpRequest _request = Request;
            try
            {
                if (_request == null)
                    _request = new HttpRequest();
                
                SendRequest("GET", url, _request, destination);
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                }
            }
        }

        protected override int GetDefaultPort()
        {
            return 80;
        }

        private bool GetKeepAlive()
        {
            bool flag;
            if ((HttpVersion == HttpVersion.Http1_1) && (ResponseVersion == HttpVersion.Http1_1))
            {
                if (IsUseProxy())
                {
                    flag =
                        string.Compare("close", ResponseHeader.ProxyConnection, true, CultureInfo.InvariantCulture) != 0;
                }
                else
                {
                    flag = string.Compare("close", ResponseHeader.Connection, true, CultureInfo.InvariantCulture) != 0;
                }
            }
            else if (IsUseProxy())
            {
                flag =
                    string.Compare("Keep-Alive", ResponseHeader.ProxyConnection, true, CultureInfo.InvariantCulture) !=
                    0;
            }
            else
            {
                flag = string.Compare("Keep-Alive", ResponseHeader.Connection, true, CultureInfo.InvariantCulture) != 0;
            }
            return (flag && KeepConnection);
        }

        private string GetPassword()
        {
            if (!StringUtils.IsEmpty(Password))
            {
                return Password;
            }
            return Url.Password;
        }

        private string GetPortIfNeed(int port)
        {
            if ((port != 80) && (port != 0x1bb))
            {
                return string.Format(":{0}", port);
            }
            return string.Empty;
        }

        protected virtual string GetRedirectMethod(int statusCode, string method)
        {
            if ((statusCode != 0x12e) && (statusCode != 0x12f))
            {
                return method;
            }
            return "GET";
        }

        private string GetResourcePath()
        {
            string absoluteUri = Url.AbsoluteUri;
            int length = absoluteUri.LastIndexOf('/');
            absoluteUri = absoluteUri.Substring(0, length);
            if ((absoluteUri.Length > 0) && (absoluteUri[absoluteUri.Length - 1] != '/'))
            {
                absoluteUri = absoluteUri + '/';
            }
            return absoluteUri;
        }

        private long GetResponseLength()
        {
            if ((string.Compare("CONNECT", method, true, CultureInfo.InvariantCulture) == 0) && (StatusCode == 200))
            {
                return 0L;
            }
            if ((string.Compare("HEAD", method, true, CultureInfo.InvariantCulture) == 0) || (StatusCode == 0x130))
            {
                return 0L;
            }
            if (((HttpVersion == HttpVersion.Http1_0) || (ResponseVersion == HttpVersion.Http1_0)) &&
                StringUtils.IsEmpty(ResponseHeader.ContentLength))
            {
                return -1L;
            }
            if (StringUtils.IsEmpty(ResponseHeader.ContentLength) && ((StatusCode & 200) == 200))
            {
                return -1L;
            }
            return StringUtils.StrToInt64Def(ResponseHeader.ContentLength, 0L);
        }

        private HttpTunnelStatus GetTunnelStatus()
        {
            if (!IsUseProxy() || (string.Compare("https", Url.Scheme, true, CultureInfo.InvariantCulture) != 0))
            {
                return HttpTunnelStatus.None;
            }
            if (string.Compare("CONNECT", method, true, CultureInfo.InvariantCulture) == 0)
            {
                return HttpTunnelStatus.Connect;
            }
            return HttpTunnelStatus.Tunnel;
        }

        private string GetUserName()
        {
            if (!StringUtils.IsEmpty(UserName))
            {
                return UserName;
            }
            return Url.UserName;
        }

        public void Head(string url)
        {
            HttpRequest request = null;
            HttpRequest request2 = Request;
            try
            {
                if (request2 == null)
                {
                    request = new HttpRequest();
                    request2 = request;
                }
                SendRequest("HEAD", url, request2, null);
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                }
            }
        }

        private void InitConnection(HttpTunnelStatus tunnelStatus)
        {
            if (IsUseProxy())
            {
                if ((oldProxyServer != ProxySettings.Server) || (oldProxyPort != ProxySettings.Port))
                {
                    base.Close();
                }
                oldProxyServer = ProxySettings.Server;
                oldProxyPort = ProxySettings.Port;
            }
            if (!StringUtils.IsEmpty(Url.Host) && (Url.Host != "*"))
            {
                if ((base.Server != Url.Host) || (base.Port != Url.Port))
                {
                    base.Close();
                }
                base.Server = Url.Host;
                base.Port = Url.Port;
            }
            base.Open();
        }

        private void InitProgress(long bytesProceed, long totalBytes)
        {
            progressHandled = false;
            base.Connection.InitProgress(bytesProceed, totalBytes);
        }

        private string[] InternalSendRequest(string method, string[] requestHeader, Stream requestBody,
                                             Stream responseBody)
        {
            string[] strArray2;
            using (var stream = new MemoryStream())
            {
                var ex = new StringCollectionEx();
                if (requestHeader != null)
                {
                    ex.AddRange(requestHeader);
                }
                var responseHeader = new StringCollectionEx();
                this.method = method;
                if (requestBody == null)
                {
                    requestBody = Stream.Null;
                }
                if (responseBody == null)
                {
                    responseBody = Stream.Null;
                }
                string[] responseText = null;
                long position = requestBody.Position;
                bool flag = false;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                long extraSize = 0L;
                HttpTunnelStatus none = HttpTunnelStatus.None;
                try
                {
                    while (true)
                    {
                        none = GetTunnelStatus();
                        WriteRequestHeader(none, ex, requestBody);
                        requestBody.Position = position;
                        WriteRequestData(requestBody);
                        OnRequestSent(new HttpRequestEventArgs(this.method, Url.Url, ex.ToArray()));
                        ReadResponseHeader(responseHeader, stream);
                        extraSize = stream.Length - stream.Position;
                        flag = !GetKeepAlive();
                        if ((((StatusCode/100) == 3) && (StatusCode != 0x130)) && (StatusCode != 0x131))
                        {
                            num2++;
                            responseText = ReadResponseText(responseHeader, extraSize, stream);
                            if ((MaxRedirects > 0) && (num2 > MaxRedirects))
                            {
                                RaiseHttpError(StatusCode, responseHeader, responseText);
                            }
                            string str = this.method;
                            if (!AllowRedirects || !Redirect(ex, responseText, ref str))
                            {
                                RaiseHttpError(StatusCode, responseHeader, responseText);
                            }
                            if (string.Compare(str, "GET", true, CultureInfo.InvariantCulture) == 0)
                            {
                                requestBody = Stream.Null;
                            }
                            this.method = str;
                            if (IsUseProxy())
                            {
                                flag = true;
                            }
                        }
                        else if (StatusCode == 0x191)
                        {
                            num3++;
                            responseText = ReadResponseText(responseHeader, extraSize, stream);
                            if ((MaxAuthRetries > 0) && (num3 > MaxAuthRetries))
                            {
                                RaiseHttpError(StatusCode, responseHeader, responseText);
                            }
                        }
                        else if (StatusCode == 0x197)
                        {
                            num4++;
                            responseText = ReadResponseText(responseHeader, extraSize, stream);
                            if ((MaxAuthRetries > 0) && (num4 > MaxAuthRetries))
                            {
                                RaiseHttpError(StatusCode, responseHeader, responseText);
                            }
                        }
                        else if (StatusCode >= 400)
                        {
                            responseText = ReadResponseText(responseHeader, extraSize, stream);
                            RaiseHttpError(StatusCode, responseHeader, responseText);
                        }
                        else
                        {
                            ReadResponseBody(responseHeader, extraSize, stream, responseBody);
                            goto Label_032C;
                        }
                        if (flag)
                        {
                            base.Close();
                        }
                    }
                }
                finally
                {
                    if (flag && (none != HttpTunnelStatus.Connect))
                    {
                        base.Close();
                    }
                }
                Label_032C:
                strArray2 = responseHeader.ToArray();
            }
            return strArray2;
        }

        private bool IsUseProxy()
        {
            return !StringUtils.IsEmpty(ProxySettings.Server);
        }

        protected virtual void OnReceiveProgress(SocketProgressEventArgs e)
        {
            progressHandled = true;
            var handler = (SocketProgressEventHandler) base.Events[receiveProgress];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRedirecting(HttpRedirectEventArgs e)
        {
            var handler = (HttpRedirectEventHandler) base.Events[redirecting];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRequestSent(HttpRequestEventArgs e)
        {
            var handler = (HttpRequestEventHandler) base.Events[requestSent];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnResponseReceived(HttpResponseEventArgs e)
        {
            var handler = (HttpResponseEventHandler) base.Events[responseReceived];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSendProgress(SocketProgressEventArgs e)
        {
            progressHandled = true;
            var handler = (SocketProgressEventHandler) base.Events[sendProgress];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OpenConnection(string server, int port)
        {
            if (IsUseProxy())
            {
                base.OpenConnection(ProxySettings.Server, ProxySettings.Port);
            }
            else
            {
                base.OpenConnection(server, port);
            }
        }

        public string[] Post(string url)
        {
            Stream responseBody = new MemoryStream();
            Post(url, responseBody);
            responseBody.Position = 0L;
            var ex = new StringCollectionEx();
            ex.LoadFromStream(responseBody, ResponseHeader.CharSet);
            return ex.ToArray();
        }

        public void Post(string url, Stream responseBody)
        {
            Post(url, Request, responseBody);
        }

        public void Post(string url, HttpRequest request, Stream responseBody)
        {
            HttpRequest request2 = null;
            HttpRequest request3 = request;
            try
            {
                if (request3 == null)
                {
                    request2 = new HttpRequest();
                    request3 = request2;
                }
                SendRequest("POST", url, request3, responseBody);
            }
            finally
            {
                if (request2 != null)
                {
                    request2.Dispose();
                }
            }
        }

        private void PrepareRequestHeader(HttpTunnelStatus tunnelStatus, StringCollection requestHeader,
                                          Stream requestBody)
        {
            string str2;
            RemoveRequestLine(requestHeader);
            RemoveHeaderTrailer(requestHeader);
            string absolutePath = Url.AbsolutePath;
            if (IsUseProxy())
            {
                switch (tunnelStatus)
                {
                    case HttpTunnelStatus.None:
                        absolutePath = Url.AbsoluteUri;
                        break;

                    case HttpTunnelStatus.Connect:
                        absolutePath = string.Format("{0}:{1}", Url.Host, Url.Port);
                        break;
                }
            }
            if (StringUtils.IsEmpty(absolutePath))
            {
                absolutePath = "/";
            }
            requestHeader.Insert(0, string.Format("{0} {1} {2}", method, absolutePath, httpVersions[(int) HttpVersion]));
            var fieldList = new HeaderFieldList();
            HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
            HeaderFieldList.RemoveHeaderField(requestHeader, fieldList, "Host");
            fieldList.Clear();
            HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
            if (AllowCompression &&
                StringUtils.IsEmpty(HeaderFieldList.GetHeaderFieldValue(requestHeader, fieldList, "Accept-Encoding")))
            {
                HeaderFieldList.AddHeaderField(requestHeader, "Accept-Encoding", "gzip, deflate");
            }
            if (!StringUtils.IsEmpty(Url.Host))
            {
                str2 = Url.Host + GetPortIfNeed(Url.Port);
            }
            else
            {
                str2 = base.Server + GetPortIfNeed(base.Port);
            }
            HeaderFieldList.AddHeaderField(requestHeader, "Host", str2);
            if (StringUtils.IsEmpty(HeaderFieldList.GetHeaderFieldValue(requestHeader, fieldList, "User-Agent")))
            {
                HeaderFieldList.AddHeaderField(requestHeader, "User-Agent", UserAgent);
            }
            string val = HeaderFieldList.GetHeaderFieldValue(requestHeader, fieldList, "Content-Length");
            if (requestBody.Length == 0L)
            {
                HeaderFieldList.RemoveHeaderField(requestHeader, fieldList, "Content-Length");
                fieldList.Clear();
                HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
                HeaderFieldList.RemoveHeaderField(requestHeader, fieldList, "Content-Type");
                fieldList.Clear();
                HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
            }
            else if (StringUtils.IsEmpty(val) &&
                     ((tunnelStatus == HttpTunnelStatus.Connect) || (requestBody.Length > 0L)))
            {
                HeaderFieldList.AddHeaderField(requestHeader, "Content-Length", requestBody.Length.ToString());
            }
            if (!AllowCaching &&
                StringUtils.IsEmpty(HeaderFieldList.GetHeaderFieldValue(requestHeader, fieldList,
                                                                        cacheFields[(int) HttpVersion])))
            {
                HeaderFieldList.AddHeaderField(requestHeader, cacheFields[(int) HttpVersion], "no-cache");
            }
            if (KeepConnection &&
                StringUtils.IsEmpty(HeaderFieldList.GetHeaderFieldValue(requestHeader, fieldList,
                                                                        connFields[IsUseProxy() ? 1 : 0])))
            {
                HeaderFieldList.AddHeaderField(requestHeader, connFields[IsUseProxy() ? 1 : 0], "Keep-Alive");
            }
            if (AllowCookies)
            {
                Cookies.SetRequestCookies(requestHeader);
            }
            requestHeader.Add("");
        }

        public string[] Put(string url, Stream source)
        {
            Stream responseBody = new MemoryStream();
            Put(url, source, responseBody);
            responseBody.Position = 0L;
            var ex = new StringCollectionEx();
            ex.LoadFromStream(responseBody, ResponseHeader.CharSet);
            return ex.ToArray();
        }

        public void Put(string url, Stream source, Stream responseBody)
        {
            HttpRequest request = null;
            HttpRequest request2 = Request;
            try
            {
                if (request2 == null)
                {
                    request = new HttpRequest();
                    request.Header.Accept = string.Empty;
                    request2 = request;
                }
                SendRequest("PUT", url, request2.HeaderSource, source, responseBody);
            }
            finally
            {
                if (request != null)
                {
                    request.Dispose();
                }
            }
        }

        private void RaiseHttpError(int statusCode, StringCollection responseHeader, string[] responseText)
        {
            string message = string.Empty;
            if (responseHeader.Count > 0)
            {
                message = responseHeader[0];
            }
            throw new HttpError(message, statusCode, responseText);
        }

        private void ReadCookies(StringCollectionEx responseHeader)
        {
            var cookies = new CookieList();
            if (AllowCookies)
            {
                cookies.GetResponseCookies(responseHeader);
            }
            OnResponseReceived(new HttpResponseEventArgs(method, Url.Url, responseHeader.ToArray(), cookies));
        }

        private void ReadResponseBody(StringCollectionEx responseHeader, long extraSize, Stream extraData,
                                      Stream responseBody)
        {
            Stream stream = null;
            TcpClientConnection connection = base.Connection;
            connection.Progress =
                (SocketProgressEventHandler)
                Delegate.Combine(connection.Progress, new SocketProgressEventHandler(DoDataReceiveProgress));
            Stream destination = new MemoryStream();
            try
            {
                if (string.Compare("chunked", ResponseHeader.TransferEncoding, true, CultureInfo.InvariantCulture) == 0)
                {
                    using (var stream3 = new ChunkedStream(destination))
                    {
                        if (extraSize > 0L)
                        {
                            StreamUtils.Copy(extraData, stream3, extraSize);
                        }
                        InitProgress(extraSize, -1L);
                        if (base.Active)
                        {
                            while (!stream3.IsCompleted)
                            {
                                base.Connection.ReadData(stream3);
                            }
                        }
                        if (!progressHandled)
                        {
                            OnReceiveProgress(new SocketProgressEventArgs(base.Connection.BytesProceed, -1L));
                        }
                        goto Label_01AC;
                    }
                }
                if (extraSize > 0L)
                {
                    StreamUtils.Copy(extraData, destination, extraSize);
                }
                long responseLength = GetResponseLength();
                long totalBytes = responseLength;
                InitProgress(extraSize, totalBytes);
                if (responseLength < 0L)
                {
                    if (base.Active)
                    {
                        base.Connection.IsReadUntilClose = true;
                        base.Connection.ReadData(destination);
                    }
                }
                else
                {
                    long bytesProceed = base.Connection.BytesProceed;
                    responseLength -= extraSize;
                    if (base.Active)
                    {
                        while ((base.Connection.BytesProceed - bytesProceed) < responseLength)
                        {
                            base.Connection.ReadData(destination);
                        }
                    }
                }
                if (!progressHandled)
                {
                    OnReceiveProgress(new SocketProgressEventArgs(totalBytes, totalBytes));
                }
            }
            finally
            {
                TcpClientConnection connection2 = base.Connection;
                connection2.Progress =
                    (SocketProgressEventHandler)
                    Delegate.Remove(connection2.Progress, new SocketProgressEventHandler(DoDataReceiveProgress));
                if (stream != null)
                {
                    stream.Close();
                }
                stream = null;
            }
            Label_01AC:
            ReadCookies(responseHeader);

            destination.Position = 0;

            if (ResponseHeader.ContentEncoding == "gzip")
            {
                GZipStream s = new GZipStream(destination, CompressionMode.Decompress);

                int offset = 0;
                byte[] buffer = new byte[512];
                while (true)
                {
                    int bytesRead = s.Read(buffer, 0, 512);
                    if (bytesRead == 0)
                        break;
                    offset += bytesRead;
                    responseBody.Write(buffer,0,bytesRead);
                }

                s.Close();
            }
            else
            {
                StreamUtils.Copy(destination, responseBody, destination.Length);
            }
        }

        private void ReadResponseHeader(StringCollectionEx responseHeader, MemoryStream rawData)
        {
            rawData.SetLength(0L);
            rawData.Position = 0L;
            int startPos = 0;
            statusCode = 0;
            int num2 = 0;
            responseVersion = HttpVersion;
            base.Connection.IsReadUntilClose = false;

            do
            {
                base.Connection.ReadData(rawData);
                while (true)
                {
                    num2 = Utils.IndexOfArray(Encoding.ASCII.GetBytes("\r\n\r\n"), rawData.GetBuffer(), startPos,
                                              (int) rawData.Length);
                    if (num2 <= 0)
                    {
                        break;
                    }
                    statusCode = ExtractStatusCode(rawData.GetBuffer(), startPos);
                    responseVersion = ExtractResponseVersion(rawData.GetBuffer(), startPos);
                    if (statusCode != 100)
                    {
                        break;
                    }
                    startPos = num2 + "\r\n\r\n".Length;
                }
            } while ((statusCode == 0) || (statusCode == 100));
            rawData.Position = startPos;
            responseHeader.Clear();
            responseHeader.LoadFromStream(rawData, ((num2 + "\r\n\r\n".Length) - startPos), false, string.Empty);
            ResponseHeader.ParseHeader(responseHeader);
        }

        private string[] ReadResponseText(StringCollectionEx responseHeader, long extraSize, Stream extraData)
        {
            var responseBody = new MemoryStream();
            ReadResponseBody(responseHeader, extraSize, extraData, responseBody);
            responseBody.Position = 0L;
            return StringUtils.GetStringArray(Translator.GetString(responseBody.GetBuffer(), ResponseHeader.CharSet));
        }

        private bool Redirect(StringCollectionEx requestHeader, string[] responseText, ref string method)
        {
            string location = ResponseHeader.Location;
            var e = new HttpRedirectEventArgs(requestHeader.ToArray(), statusCode, ResponseHeader, responseText, method,
                                              false, false);
            string resourcePath = GetResourcePath();
            OnRedirecting(e);
            method = e.Method;
            if (e.Handled || StringUtils.IsEmpty(location))
            {
                return e.CanRedirect;
            }
            url.Parse(Url.Url, location);
            if (StringUtils.IsEmpty(Url.AbsoluteUri))
            {
                return false;
            }
            method = GetRedirectMethod(StatusCode, method);
            if (GetResourcePath().IndexOf(resourcePath) != 0)
            {
                var fieldList = new HeaderFieldList();
                HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
                HeaderFieldList.RemoveHeaderField(requestHeader, fieldList, "Authorization");
            }
            return true;
        }

        private void RemoveHeaderTrailer(StringCollection header)
        {
            while ((header.Count > 0) && StringUtils.IsEmpty(header[header.Count - 1]))
            {
                header.RemoveAt(header.Count - 1);
            }
        }

        private void RemoveRequestLine(StringCollection header)
        {
            if (header.Count > 0)
            {
                int index = header[0].IndexOf(' ');
                int num2 = header[0].IndexOf(':');
                if ((index >= 0) && ((num2 < 0) || (index < num2)))
                {
                    header.RemoveAt(0);
                }
            }
        }

        public string[] SendRequest(string method, string url, HttpRequest request, Stream responseBody)
        {
            string[] headerSource = request.HeaderSource;
            using (Stream stream = request.RequestStream)
            {
                return SendRequest(method, url, headerSource, stream, responseBody);
            }
        }

        public string[] SendRequest(string method, string url, string[] requestHeader, Stream requestBody,
                                    Stream responseBody)
        {
            Url.Parse(url);
            return InternalSendRequest(method, requestHeader, requestBody, responseBody);
        }

        private void WriteRequestData(Stream requestBody)
        {
            if (requestBody.Length > 0L)
            {
                InitProgress(requestBody.Position, requestBody.Length);
                TcpClientConnection connection = base.Connection;
                connection.Progress =
                    (SocketProgressEventHandler)
                    Delegate.Combine(connection.Progress, new SocketProgressEventHandler(DoDataSendProgress));
                try
                {
                    base.Connection.WriteData(requestBody);
                    if (!progressHandled)
                    {
                        OnSendProgress(new SocketProgressEventArgs(requestBody.Length, requestBody.Length));
                    }
                }
                finally
                {
                    TcpClientConnection connection2 = base.Connection;
                    connection2.Progress =
                        (SocketProgressEventHandler)
                        Delegate.Remove(connection2.Progress, new SocketProgressEventHandler(DoDataSendProgress));
                }
            }
        }

        private void WriteRequestHeader(HttpTunnelStatus tunnelStatus, StringCollection requestHeader,
                                        Stream requestBody)
        {
            bool active = base.Active;
            InitConnection(tunnelStatus);
            PrepareRequestHeader(tunnelStatus, requestHeader, requestBody);
            try
            {
                base.Connection.WriteString(requestHeader.ToString());
            }
            catch (SocketError error)
            {
                if (!active || (error.ErrorCode != 0x2745))
                {
                    throw;
                }
                base.Close();
                base.Open();
                base.Connection.WriteString(requestHeader.ToString());
            }
        }

        #region Nested type: HttpTunnelStatus

        protected enum HttpTunnelStatus
        {
            None,
            Connect,
            Tunnel
        }

        #endregion
    }
}