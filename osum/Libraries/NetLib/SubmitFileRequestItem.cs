namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;

    public class SubmitFileRequestItem : HttpRequestItem
    {
        private string contentType;
        private const string DefaultContentType = "application/octet-stream";
        private const string DefaultFieldName = "FileName";
        private string fieldName;
        private string fileName;
        private byte[] bytes;

        public SubmitFileRequestItem()
        {
            this.fieldName = "FileName";
            this.fileName = string.Empty;
            this.contentType = "application/octet-stream";
        }

        public SubmitFileRequestItem(string fileName, string fieldName)
        {
            this.fieldName = fieldName;
            this.fileName = fileName;
            this.contentType = "application/octet-stream";
        }

        public SubmitFileRequestItem(string fileName, string fieldName, string contentType)
        {
            this.fieldName = fieldName;
            this.fileName = fileName;
            this.contentType = contentType;
        }

        public void AddDataArray(byte[] data)
        {
            bytes = data;
        }

        protected internal override void AddData(byte[] data, int index, int length)
        {
            if (base.Owner != null)
            {
                if (base.Owner.dataStream == null)
                {
                    GetDataStreamEventArgs e = new GetDataStreamEventArgs(this);
                    base.Owner.OnGetDataStream(e);
                    base.Owner.dataStream = e.Stream;
                }
                if (base.Owner.dataStream != null)
                {
                    base.Owner.dataStream.Write(data, index, length);
                }
            }
        }

        protected internal override void AfterAddData()
        {
            if ((base.Owner != null) && (base.Owner.dataStream != null))
            {
                base.Owner.dataStream.Seek(0L, SeekOrigin.Begin);
                base.Owner.OnDataAdded(new DataAddedEventArgs(this, base.Owner.dataStream));
            }
        }

        public override Stream GetData()
        {
            Stream stream2;
            if ((base.Owner == null) || !base.Owner.IsMultiPart())
            {
                return Stream.Null;
            }
            MultiStream stream = new MultiStream();
            try
            {
                stream.AddStream(new StringStream("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", new object[] { base.GetCanonicalizedValue(this.FieldName), base.GetCanonicalizedValue(Utils.ExtractFileName(this.FileName)), this.ContentType }));
                GetDataStreamEventArgs e = new GetDataStreamEventArgs(this);
                base.Owner.OnGetDataSourceStream(e);
                if (e.Stream == null)
                {
                    if (bytes == null)
                        stream.AddStream(new FileStream(this.FileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    else
                    stream.AddStream(new MemoryStream(bytes));
                }
                else
                {
                    stream.AddStream(e.Stream);
                }
                stream2 = stream;
            }
            catch (Exception exception)
            {
                stream.Close();
                throw exception;
            }
            return stream2;
        }

        protected internal override void ParseHeader(IList header, HeaderFieldList fieldList)
        {
            base.ParseHeader(header, fieldList);
            this.ContentType = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Type");
            string source = HeaderFieldList.GetHeaderFieldValue(header, fieldList, "Content-Disposition");
            this.FieldName = HeaderFieldList.GetHeaderFieldValueItem(source, "name=");
        }

        [DefaultValue("application/octet-stream")]
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
                    base.Update();
                }
            }
        }

        [DefaultValue("FileName")]
        public string FieldName
        {
            get
            {
                return this.fieldName;
            }
            set
            {
                if (this.fieldName != value)
                {
                    this.fieldName = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                if (this.fileName != value)
                {
                    this.fileName = value;
                    base.Update();
                }
            }
        }
    }
}

