using System;
using System.Threading;
using osum;
using System.Collections.Generic;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Updated Netmanager class. Uses a thread pool to service
    /// request objects.
    /// </summary>
    public static class NetManager
    {
        static List<NetRequest> activeRequests = new List<NetRequest>();
        static Queue<NetRequest> requestQueue = new Queue<NetRequest>();

        const int MAX_CONCURRENT_REQUESTS = 3;

        public static bool ReportCompleted(NetRequest request)
        {
            lock (requestQueue)
            {
                activeRequests.Remove(request);
                while (requestQueue.Count > 0)
                    AddRequest(null);
            }

            return true;
        }

        /// <summary>
        /// Adds a request to the application threadpool
        /// </summary>
        /// <param name="request">A NetRequest object that we want performed</param>
        /// <returns>true if the request can be added</returns>
        public static bool AddRequest(NetRequest request)
        {
            lock (requestQueue)
            {
                if (activeRequests.Count > MAX_CONCURRENT_REQUESTS)
                {
                    if (request != null)
                        requestQueue.Enqueue(request);
                    return true;
                }

                if (request == null)
                {
                    if (requestQueue.Count > 0)
                        request = requestQueue.Dequeue();
                    else
                        return false;
                }

                activeRequests.Add(request);
            }

#if iOS
            if (request.AbortRequested) return false;
            request.Perform();
#else
            ThreadPool.QueueUserWorkItem(work => {
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