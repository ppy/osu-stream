using System;
using System.Threading;
using osum;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Updated Netmanager class. Uses a thread pool to service
    /// request objects.
    /// </summary>
    public static class NetManager
    {
        /// <summary>
        /// Adds a request to the application threadpool
        /// </summary>
        /// <param name="request">A NetRequest object that we want performed</param>
        /// <returns>true if the request can be added</returns>
        public static bool AddRequest(NetRequest request)
        {
#if iOS
            if (request.AbortRequested) return false;
            request.Perform();
#else
            Console.WriteLine("added new request");
            request.thread = GameBase.Instance.RunInBackground(delegate {
                try
                {
                    if (request.AbortRequested) return;
                     request.Perform();
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    request.OnException(ex);
                }
            });
#endif

            return true;
        }
    }
}