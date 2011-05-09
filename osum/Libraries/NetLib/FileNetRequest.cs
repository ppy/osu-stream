using System;
using System.IO;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Downloads a file from the internet to a specified location
    /// </summary>
    public class FileNetRequest : NetRequest
    {
        public readonly string m_filename;
        private Http h;

        public FileNetRequest(string _filename, string _url)
            : base(_url)
        {
            m_filename = _filename;
        }

        public event RequestStartHandler onStart;
        public event RequestUpdateHandler onUpdate;
        public event RequestCompleteHandler onFinish;

        FileStream fileStream;


        public override void Perform()
        {
            //inform subscribers that we have started
            if (onStart != null)
                onStart();

            string dir = Path.GetDirectoryName(m_filename);
            if (dir != string.Empty && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (fileStream = new FileStream(m_filename, FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
            {
                try
                {
                    h = new Http();
                    h.ReceiveProgress += http_ReceiveProgress;
                    h.Get(m_url, fileStream);
                    h.Close();
                }
                catch
                {
                    fileStream.Close();
                    File.Delete(m_filename);
                    return;
                }
            }

            //inform subscribers that we have finished
            if (onFinish != null)
                onFinish(m_filename, null);
        }

        private void http_ReceiveProgress(object sender, SocketProgressEventArgs e)
        {
            if (onUpdate != null) onUpdate(this, e.BytesProceed, e.TotalBytes);
        }

        public override void Abort()
        {
            try
            {
                h.Close();
                if (fileStream != null)
                    fileStream.Close();
                File.Delete(m_filename);
            }
            catch { }

            //inform subscribers that we have finished
            if (onFinish != null)
                onFinish(m_filename, new Exception("aborted"));
        }

        public override bool Valid()
        {
            //check that the filename given corrosponds to a valid
            //path and that the file does not exist
            if (File.Exists(m_filename))
            {
                Console.WriteLine(m_filename + " already exists");
                return false;
            }

            return true;
        }

        public override void OnException(Exception e)
        {
            Console.Write("exception! - url " + m_url + " filename " + m_filename);
            if (onFinish != null)
                onFinish(null, e);
        }

        #region Nested type: RequestCompleteHandler

        public delegate void RequestCompleteHandler(string _fileLocation, Exception e);

        #endregion
    }
}