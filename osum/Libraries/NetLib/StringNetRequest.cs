using System;
using System.Reflection;
using System.Text;
using System.Net;
using System.Threading;
using osum;

namespace osu_common.Libraries.NetLib
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
        }

        public new event RequestCompleteHandler onFinish;

        public override void processFinishedRequest()
        {
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

        public new delegate void RequestCompleteHandler(string _result, Exception e);
    }
}