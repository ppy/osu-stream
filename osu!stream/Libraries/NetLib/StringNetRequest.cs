using System;
using System.Text;

namespace osum.Libraries.NetLib
{
    /// <summary>
    /// request string peforms a web request on a url and returns back the 
    /// result string.
    /// </summary>
    public class StringNetRequest : DataNetRequest
    {
        public StringNetRequest(string _url, string method = "GET", string postData = null)
            : base(_url, method, postData)
        {
#if DEBUG
            Console.WriteLine("URL: " + _url + "\nPOST: " + postData);
#endif
        }

        public new event RequestCompleteHandler onFinish;

        public override void processFinishedRequest()
        {
            NetManager.ReportCompleted(this);

            if (AbortRequested) return;
            
            GameBase.Scheduler.Add(delegate
            {
                if (onFinish != null)
                {
                    string output = null;
                    if (data != null && error == null)
                        output = Encoding.UTF8.GetString(data);
                    onFinish(output, error);
                }
            });
        }

        public delegate void RequestCompleteHandler(string _result, Exception e);
    }
}