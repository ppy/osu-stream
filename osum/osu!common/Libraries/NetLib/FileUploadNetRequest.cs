using System;
using System.IO;
using System.Threading;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// POSTs a byte buffer to a url
    /// </summary>
    public class FileUploadNetRequest : NetRequest
    {
        private readonly byte[] m_buffer;
        private readonly string m_filename;
        private readonly string m_labelname;
        private Http http;
        public HttpRequest request = new HttpRequest();

        public FileUploadNetRequest(string _url, byte[] _buffer, string _filename, string _labelname)
            : base(_url)
        {
            m_buffer = _buffer;
            BufferSize = 8192; //default buffer size is 8kb
            m_filename = _filename;
            m_labelname = _labelname;
        }

        public int BufferSize { get; set; }

        public event RequestStartHandler onStart;
        public event RequestUpdateHandler onUpdate;
        public event RequestCompleteHandler onFinish;

        public override void Perform()
        {
            BlockingPerform();
        }

        public string BlockingPerform()
        {
            //inform subscribers that we have started
            if (onStart != null)
                onStart();

            http = new Http();
            http.Request = request;

            MemoryStream res = new MemoryStream();
            try
            {
                SubmitFileRequestItem i = request.Items.AddSubmitFile(m_filename, m_labelname);
                i.AddDataArray(m_buffer);
                http.SendProgress += http_SendProgress;
                http.Post(m_url, request, res);
                http.SendProgress -= http_SendProgress;
            }
            catch (ThreadAbortException)
            {
                http.Close();

                if (onFinish != null)
                    onFinish(null, new AbortedException());

                return null;
            }
            catch (Exception e)
            {
                http.Close();

                if (onFinish != null)
                    onFinish(null, e);

                return null;
            }

            http.Close();

            res.Position = 0;
            StreamReader sr = new StreamReader(res);

            string response = sr.ReadToEnd();

            if (onFinish != null)
                onFinish(response, null);

            return response;
        }

        private void http_SendProgress(object sender, SocketProgressEventArgs e)
        {
            if (onUpdate != null)
                onUpdate(this, e.BytesProceed, e.TotalBytes);
        }

        public override bool Valid()
        {
            return true;
        }

        public override void OnException(Exception e)
        {
            throw e;
        }

        public void Close()
        {
            if (http != null)
                http.Close();
        }

        #region Nested type: CreatePostHeader

        public delegate string CreatePostHeader();

        #endregion

        #region Nested type: RequestCompleteHandler

        public delegate void RequestCompleteHandler(string _result, Exception e);

        #endregion
    }
}