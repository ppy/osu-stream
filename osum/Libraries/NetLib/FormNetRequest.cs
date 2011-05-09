using System;
using System.IO;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// POSTs a byte buffer to a url
    /// </summary>
    public class FormNetRequest : NetRequest
    {
        private readonly byte[] m_buffer;
        private readonly string m_filename;
        private readonly string m_labelname;
        private Http http;

        public FormNetRequest(string _url)
            : base(_url)
        {
            BufferSize = 8192; //default buffer size is 8kb
            
            http = new Http();
            req = new HttpRequest();
            http.Request = req;
        }

        public void AddField(string key, string val)
        {
            req.Items.AddFormField(key,val);
        }

        public int BufferSize { get; set; }

        public event RequestStartHandler onStart;
        public event RequestUpdateHandler onUpdate;
        public event RequestCompleteHandler onFinish;
        private HttpRequest req;

        public override void Perform()
        {
            BlockingPerform();
        }

        public string BlockingPerform()
        {
            //inform subscribers that we have started
            if (onStart != null)
                onStart();

            MemoryStream res = new MemoryStream();
            try
            {

                http.SendProgress += http_SendProgress;
                http.Post(m_url, req, res);
                http.SendProgress -= http_SendProgress;
            }
            catch
            {
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
            throw new NotImplementedException();
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