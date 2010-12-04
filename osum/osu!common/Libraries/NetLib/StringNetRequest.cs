using System;
using System.Reflection;
using System.Text;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// request string peforms a web request on a url and returns back the 
    /// result string.
    /// </summary>
    public class StringNetRequest : NetRequest
    {
        public StringNetRequest(string _url)
            : base(_url)
        {
        }

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
            {
                onStart();
            }

            Http h = new Http();
            string[] result = h.Get(m_url);
            h.Close();


            StringBuilder sb = new StringBuilder();
            int l = result.Length;
            for (int i = 0; i < l; i++)
                sb.Append((i > 0 ? "\n" : "") + result[i]);

            string str = sb.ToString();
            //inform subscribers that we have finished
            if (onFinish != null)
            {
                try
                {
                    onFinish(str,null);
                }
                catch { }
            }

            return str;
        }


        public override bool Valid()
        {
            //see if we have any subscribers to onFinish
            if (onFinish.GetInvocationList().Length == 0)
            {
                Console.WriteLine("no subscribers to this StringNetRequest complete event");
                return false;
            }

            //all string requests are valid, as long as the address formats
            //to a correct URI
            //TODO: regex checking of m_url
            return true;
        }

        public override void OnException(Exception e)
        {
            Console.Write("exception! - url " + m_url);
            if (onFinish != null)
            {
                try
                {
                    onFinish(null, e);
                }
                catch { }
            }
        }

        #region Nested type: RequestCompleteHandler

        public delegate void RequestCompleteHandler(string _result, Exception e);

        #endregion
    }
}