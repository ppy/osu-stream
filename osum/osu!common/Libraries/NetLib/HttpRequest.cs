using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace osu_common.Libraries.NetLib
{
    public class HttpRequest : Component, ISupportInitialize
    {
        private const string FormDataContentType = "application/x-www-form-urlencoded";
        private const string MultiPartDataContentType = "multipart/form-data";
        private static readonly object changed = new object();
        private static readonly object dataAdded = new object();
        private static readonly object getDataSourceStream = new object();
        private static readonly object getDataStream = new object();
        private static readonly object getFormNumber = new object();
        private int batchSize;
        internal Stream dataStream;
        private HttpRequestHeader header;
        private string[] headerSource;
        private bool isParse;
        private bool isUnderUI;
        private HttpRequestItemList items;
        private string[] requestSource;
        private Timer timer;
        private int updateCount;

        public HttpRequest()
        {
            Init();
        }

        [DefaultValue(0x2000)]
        public int BatchSize
        {
            get { return batchSize; }
            set
            {
                if (batchSize != value)
                {
                    batchSize = value;
                    Update();
                }
            }
        }

        [Description("Gets the header fields for posting to the server"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public HttpRequestHeader Header
        {
            get { return header; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] HeaderSource
        {
            get
            {
                if (headerSource == null)
                {
                    isParse = true;
                    try
                    {
                        InitHeader();
                    }
                    finally
                    {
                        isParse = false;
                    }
                    var header = new StringCollectionEx();
                    Header.AssignHeader(header);
                    headerSource = header.ToArray();
                }
                return headerSource;
            }
            set
            {
                isParse = true;
                try
                {
                    Header.ParseHeader(value);
                }
                finally
                {
                    isParse = false;
                }
            }
        }

        [Description("Gets the indexed access to the request items"),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public HttpRequestItemList Items
        {
            get { return items; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string[] RequestSource
        {
            get
            {
                if (requestSource == null)
                {
                    var ex = new StringCollectionEx();
                    using (Stream stream = RequestStream)
                    {
                        ex.LoadFromStream(stream, string.Empty);
                    }
                    requestSource = ex.ToArray();
                }
                return requestSource;
            }
            set
            {
                if (value != null)
                {
                    RequestStream = new StringStream(StringUtils.GetStringsAsString(value));
                }
                else
                {
                    RequestStream = null;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Stream RequestStream
        {
            get
            {
                isParse = true;
                try
                {
                    InitBoundary();
                }
                finally
                {
                    isParse = false;
                }
                var stream = new MultiStream();
                try
                {
                    GetTotalRequestData(stream);
                }
                catch
                {
                    stream.Close();
                    throw;
                }
                return stream;
            }
            set
            {
                isParse = true;
                BeginInit();
                try
                {
                    Items.Clear();
                    if (value != null)
                    {
                        string str = ReadLine(value, 250);
                        value.Seek(0L, SeekOrigin.Begin);
                        if (str.IndexOf("--") == 0)
                        {
                            Header.Boundary = str.Remove(0, 2).Trim();
                            ParseMultiPartRequest(value);
                        }
                        else
                        {
                            Header.Boundary = string.Empty;
                            CreateSingleItem(value);
                        }
                    }
                }
                finally
                {
                    ClearDataStream();
                    EndInit();
                    isParse = false;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long TotalSize
        {
            get
            {
                string str = GenerateBoundary();
                long num = 0L;
                foreach (HttpRequestItem item in Items)
                {
                    num += GetItemSize(item);
                }
                if (IsMultiPart())
                {
                    return (num + ((("\r\n--" + str + "\r\n").Length*Items.Count) + ("--" + str + "--\r\n").Length));
                }
                if (IsForm())
                {
                    num += "&".Length*(Items.Count - 1);
                }
                return num;
            }
        }

        #region ISupportInitialize Members

        public void BeginInit()
        {
            updateCount++;
        }

        public void EndInit()
        {
            if (updateCount > 0)
            {
                updateCount--;
            }
            Update();
        }

        #endregion

        [Description("Occurs when a property value has been changed")]
        public event EventHandler Changed
        {
            add { base.Events.AddHandler(changed, value); }
            remove { base.Events.RemoveHandler(changed, value); }
        }

        public event DataAddedEventHandler DataAdded
        {
            add { base.Events.AddHandler(dataAdded, value); }
            remove { base.Events.RemoveHandler(dataAdded, value); }
        }

        public event GetDataStreamEventHandler GetDataSourceStream
        {
            add { base.Events.AddHandler(getDataSourceStream, value); }
            remove { base.Events.RemoveHandler(getDataSourceStream, value); }
        }

        [Description("Occurs when the request builder is about to get the stream data of the binary request item")]
        public event GetDataStreamEventHandler GetDataStream
        {
            add { base.Events.AddHandler(getDataStream, value); }
            remove { base.Events.RemoveHandler(getDataStream, value); }
        }

        public virtual void Clear()
        {
            BeginInit();
            try
            {
                items.Clear();
                Header.Clear();
            }
            finally
            {
                EndInit();
            }
        }

        private void ClearDataStream()
        {
            if (dataStream != null)
            {
                dataStream.Close();
                dataStream = null;
            }
        }

        protected virtual HttpRequestHeader CreateHeader()
        {
            return new HttpRequestHeader();
        }

        protected virtual HttpRequestItem CreateItem(IList header, HeaderFieldList fieldList)
        {
            string source = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Disposition");
            if (
                !HeaderFieldList.GetHeaderFieldValueItem(source, "").ToLower(CultureInfo.InvariantCulture).Equals(
                     "form-data"))
            {
                return Items.AddTextData("");
            }
            string headerFieldValueItem = HeaderFieldList.GetHeaderFieldValueItem(source, "filename=");
            if (!StringUtils.IsEmpty(headerFieldValueItem))
            {
                return Items.AddSubmitFile(headerFieldValueItem, "");
            }
            return Items.AddFormField(HeaderFieldList.GetHeaderFieldValueItem(source, "name="), string.Empty);
        }

        private HttpRequestItem CreateMultiPartItem(string header)
        {
            ClearDataStream();
            string[] stringArray = StringUtils.GetStringArray(header);
            var fieldList = new HeaderFieldList();
            HeaderFieldList.GetHeaderFieldList(0, stringArray, fieldList);
            HttpRequestItem item = CreateItem(stringArray, fieldList);
            item.ParseHeader(stringArray, fieldList);
            return item;
        }

        protected virtual void CreateSingleItem(Stream stream)
        {
            var count = (int) (stream.Length - stream.Position);
            var buffer = new byte[count];
            stream.Read(buffer, 0, count);
            string source = Encoding.ASCII.GetString(buffer);
            if (IsForm())
            {
                ParseFormFieldRequest(source);
            }
            else
            {
                Items.AddTextData(source).AfterAddData();
            }
        }

        protected internal string GenerateBoundary()
        {
            DateTime now = DateTime.Now;
            string str = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}",
                                       new object[]
                                           {
                                               now.Month.ToString("X2", CultureInfo.InvariantCulture),
                                               now.Day.ToString("X2", CultureInfo.InvariantCulture),
                                               now.Hour.ToString("X2", CultureInfo.InvariantCulture),
                                               now.Minute.ToString("X2", CultureInfo.InvariantCulture),
                                               now.Second.ToString("X2", CultureInfo.InvariantCulture),
                                               now.Millisecond.ToString("X2", CultureInfo.InvariantCulture)
                                           });
            return string.Format(CultureInfo.InvariantCulture, "---------------------------{0}",
                                 new object[] {str.Substring(0, 12)});
        }

        protected virtual string GetContentType()
        {
            bool flag = Items.Count > 0;
            foreach (HttpRequestItem item in Items)
            {
                if (item is SubmitFileRequestItem)
                {
                    return "multipart/form-data";
                }
                flag = flag && (item is FormFieldRequestItem);
            }
            if (!flag)
            {
                return string.Empty;
            }
            return "application/x-www-form-urlencoded";
        }

        private long GetItemSize(HttpRequestItem item)
        {
            using (Stream stream = item.GetData())
            {
                return stream.Length;
            }
        }

        private void GetTotalRequestData(MultiStream stream)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                string str = string.Empty;
                if (IsMultiPart())
                {
                    str = "--" + Header.Boundary + "\r\n";
                    if (i > 0)
                    {
                        str = "\r\n" + str;
                    }
                }
                else if ((i > 0) && IsForm())
                {
                    str = "&";
                }
                if (!StringUtils.IsEmpty(str))
                {
                    stream.AddStream(new StringStream(str));
                }
                stream.AddStream(Items[i].GetData());
            }
            if (IsMultiPart())
            {
                stream.AddStream(new StringStream("\r\n--" + Header.Boundary + "--\r\n"));
            }
        }

        private void HeaderChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void Init()
        {
            items = new HttpRequestItemList(this);
            header = new HttpRequestHeader();
            header.Changed += HeaderChanged;
            updateCount = 0;
            requestSource = null;
            headerSource = null;
            isParse = false;
            dataStream = null;
            batchSize = 0x2000;
            Assembly assembly =
                Assembly.Load("System.Windows.Forms, Version=" +
                              ((Environment.Version.Major > 1) ? "2.0.0.0" : "1.0.3300.0") +
                              ", Culture=neutral, PublicKeyToken=b77a5c561934e089");
            isUnderUI =
                (bool)
                assembly.GetType("System.Windows.Forms.SystemInformation").InvokeMember("UserInteractive",
                                                                                        BindingFlags.GetProperty |
                                                                                        BindingFlags.Public |
                                                                                        BindingFlags.Static, null, null,
                                                                                        null);
            if (!isUnderUI)
            {
                timer = new Timer(TimerCallback, null, 0x1b7740, 0x1b7740);
            }
        }

        private void InitBoundary()
        {
            if (IsMultiPart())
            {
                if (StringUtils.IsEmpty(Header.Boundary))
                {
                    Header.Boundary = GenerateBoundary();
                }
            }
            else
            {
                Header.Boundary = string.Empty;
            }
        }

        protected virtual void InitHeader()
        {
            InitBoundary();
            Header.ContentLength = TotalSize.ToString();
            if (Header.ContentLength.Equals("0"))
            {
                Header.ContentLength = "";
            }
        }

        protected internal bool IsForm()
        {
            return Header.ContentType.ToLower(CultureInfo.InvariantCulture).Equals("application/x-www-form-urlencoded");
        }

        protected internal bool IsMultiPart()
        {
            return (Header.ContentType.ToLower(CultureInfo.InvariantCulture).IndexOf("multipart/") > -1);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            var handler = (EventHandler) base.Events[changed];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal virtual void OnDataAdded(DataAddedEventArgs e)
        {
            var handler = (DataAddedEventHandler) base.Events[dataAdded];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal virtual void OnGetDataSourceStream(GetDataStreamEventArgs e)
        {
            var handler = (GetDataStreamEventHandler) base.Events[getDataSourceStream];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal virtual void OnGetDataStream(GetDataStreamEventArgs e)
        {
            var handler = (GetDataStreamEventHandler) base.Events[getDataStream];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ParseFormField(string fieldInfo)
        {
            int index = fieldInfo.IndexOf('=');
            string str = fieldInfo;
            string str2 = string.Empty;
            if (index > 0)
            {
                str = fieldInfo.Substring(0, index);
                str2 = fieldInfo.Substring(index + 1);
            }
            Items.AddFormField(str.Trim(), str2.Trim()).AfterAddData();
        }

        private void ParseFormFieldRequest(string source)
        {
            string[] strArray = source.Split(new[] {'&'});
            for (int i = 0; i < strArray.Length; i++)
            {
                ParseFormField(strArray[i]);
            }
        }

        private void ParseMultiPartRequest(Stream stream)
        {
            int num4;
            byte[] bytes = Encoding.ASCII.GetBytes("\r\n--" + Header.Boundary);
            var buffer2 = new byte[] {13, 10, 13, 10};
            int batchSize = BatchSize;
            if (batchSize < bytes.Length)
            {
                batchSize = bytes.Length;
            }
            if (batchSize > ((int) (stream.Length - stream.Position)))
            {
                batchSize = (int) (stream.Length - stream.Position);
            }
            string str = string.Empty;
            byte[] data = null;
            HttpRequestItem item = null;
            int index = 2;
            int num3 = 0;
            var buffer = new byte[batchSize];
            while ((num4 = stream.Read(buffer, 0, batchSize)) > 0)
            {
                int num5;
                int num6 = 0;
                for (int i = 0; i < num4; i++)
                {
                    if (buffer[i] == bytes[index])
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                        if (buffer[i] == bytes[index])
                        {
                            index++;
                        }
                        if ((data != null) && (item != null))
                        {
                            item.AddData(data, 0, data.Length);
                        }
                        data = null;
                    }
                    if (buffer[i] == buffer2[num3])
                    {
                        num3++;
                    }
                    else
                    {
                        num3 = 0;
                        if (buffer[i] == buffer2[num3])
                        {
                            num3++;
                        }
                    }
                    if (index >= bytes.Length)
                    {
                        num5 = ((i - num6) - index) + 1;
                        if (((item != null) && (num6 < num4)) && (num5 > 0))
                        {
                            item.AddData(buffer, num6, num5);
                            item.AfterAddData();
                        }
                        item = null;
                        str = string.Empty;
                        num6 = 0;
                        index = 0;
                    }
                    else if (item == null)
                    {
                        str = str + Encoding.ASCII.GetString(buffer, i, buffer.Length - i);
                        if (num3 >= buffer2.Length)
                        {
                            item = CreateMultiPartItem(str.Trim());
                            str = string.Empty;
                            num6 = i + 1;
                        }
                    }
                    if (num3 >= buffer2.Length)
                    {
                        num3 = 0;
                    }
                }
                num5 = (num4 - num6) - index;
                if (((item != null) && (num6 < num4)) && (num5 > 0))
                {
                    if (index > 0)
                    {
                        var array = new byte[data.Length + index];
                        data.CopyTo(array, 0);
                        Array.Copy(buffer, num4 - index, array, data.Length, index);
                        data = array;
                    }
                    item.AddData(buffer, num6, num5);
                }
            }
        }

        private string ReadLine(Stream stream, int maxBytes)
        {
            int num2;
            var builder = new StringBuilder();
            var numArray = new[] {13, 10};
            int index = 0;
            while ((maxBytes > 0) && ((num2 = stream.ReadByte()) > 0))
            {
                if (num2 == numArray[index])
                {
                    index++;
                }
                else
                {
                    index = 0;
                }
                if (index >= numArray.Length)
                {
                    break;
                }
                builder.Append(Convert.ToChar(num2, CultureInfo.InvariantCulture));
                maxBytes--;
            }
            return builder.ToString();
        }

        private void TimerCallback(object state)
        {
            timer.Dispose();
        }

        public void Update()
        {
            if (updateCount == 0)
            {
                if (!isParse)
                {
                    Header.Boundary = string.Empty;
                }
                headerSource = null;
                requestSource = null;
                OnChanged(EventArgs.Empty);
            }
        }

        protected internal void UpdateContentType()
        {
            if ((updateCount == 0) && !isParse)
            {
                Header.ContentType = GetContentType();
            }
        }
    }
}