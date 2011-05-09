using System;
using System.IO;
using System.Threading;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Downloads a file from the internet to a specified location
    /// </summary>
    public class DataNetRequest : NetRequest
    {
        public DataNetRequest(string _url)
            : base(_url)
        {
        }

        public event RequestStartHandler onStart;
        public event RequestUpdateHandler onUpdate;
        public event RequestCompleteHandler onFinish;


        public override void Perform()
        {
            try
            {
                //inform subscribers that we have started
                if (onStart != null)
                    onStart();


                using (Stream dataStream = new MemoryStream())
                {
                    using (Http h = new Http())
                        h.Get(m_url, dataStream);
                   
                    dataStream.Position = 0;
                    byte[] output = new byte[dataStream.Length];
                    dataStream.Read(output, 0, (int)dataStream.Length);
                    
                    //inform subscribers that we have finished
                    if (onFinish != null)
                        onFinish(output, null);
                }
            }
            catch (ThreadAbortException)
            { }
        }

        public override bool Valid()
        {
            return true;
        }

        public override void OnException(Exception e)
        {
            Console.Write("exception! - url " + m_url);
            onFinish(null, e);
        }

        #region Nested type: RequestCompleteHandler

        public delegate void RequestCompleteHandler(byte[] data, Exception e);

        #endregion
    }
}