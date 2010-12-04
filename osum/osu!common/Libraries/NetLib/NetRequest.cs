using System;
using System.Threading;

namespace osu_common.Libraries.NetLib
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
        protected string m_url;
        internal Thread thread;
        internal bool AbortRequested;
        internal bool IsQueued;

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
            if (IsQueued)
            {
                AbortRequested = true;
                return;
            }
            
            if (thread != null && thread.IsAlive) thread.Abort();
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
    }
}