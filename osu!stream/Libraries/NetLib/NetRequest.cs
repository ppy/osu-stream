using System;
using System.Text;
using System.Threading;

namespace osum.Libraries.NetLib
{
    public class AbortedException : Exception
    {
        public AbortedException()
            : base("Request has been aborted")
        {

        }

    }

    /// <summary>
    /// base type for all net requests. Pass this object to a Netmanager
    /// which calls Perform in its thread pool.
    /// </summary>
    public abstract class NetRequest
    {
        public string m_url;
        internal Thread thread;
        internal bool AbortRequested;

        public NetRequest(string _url)
        {
            m_url = _url;
        }

        /// <summary>
        /// Abstract method, this is called by the NetManager's thread pool
        /// </summary>
        public abstract void Perform();

        public virtual void Abort()
        {
            AbortRequested = true;
            NetManager.ReportCompleted(this);
        }

        /// <summary>
        /// Abstract method, this is called before the request is added to the
        /// thread pool.
        /// <returns>returns true if the request validates</returns>
        /// </summary>
        public abstract bool Valid();


        /// <summary>
        /// Called if there was an exception performing the request
        /// </summary>
        /// <param name="e">the exception that was thrown</param>
        public abstract void OnException(Exception e);

        #region Nested type: RequestStartHandler

        public delegate void RequestStartHandler();

        #endregion

        #region Nested type: RequestUpdateHandler

        public delegate void RequestUpdateHandler(object sender, long current, long total);

        #endregion

        public static string UrlEncode(string s)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte i in Encoding.UTF8.GetBytes(s))
            {
                if ((i >= 'A' && i <= 'Z') ||
                                (i >= 'a' && i <= 'z') ||
                                (i >= '0' && i <= '9') ||
                                i == '-' || i == '_')
                {
                    sb.Append((char)i);
                }
                else if (i == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%');
                    sb.Append(i.ToString("X2"));
                }
            }

            return sb.ToString();
        }
    }
}