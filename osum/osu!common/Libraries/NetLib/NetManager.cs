using System;
using System.Threading;

namespace osu_common.Libraries.NetLib
{
    /// <summary>
    /// Updated Netmanager class. Uses a thread pool to service
    /// request objects.
    /// </summary>
    public static class NetManager
    {
        static long lastRequestTick;
        static int chainedFastRequests;
        
        private static readonly object RequestLock = new object();

        const int DELAY_BETWEEN_REQUESTS = 150;

        /// <summary>
        /// Adds a request to the application threadpool
        /// </summary>
        /// <param name="request">A NetRequest object that we want performed</param>
        /// <returns>true if the request can be added</returns>
        public static bool AddRequest(NetRequest request)
        {
            if (request==null)
                throw new ArgumentNullException();

            bool requireDelay = false;
            int delayLength;

            lock (RequestLock)
            {
                long nowTick = DateTime.Now.Ticks;
                if (nowTick - lastRequestTick < TimeSpan.TicksPerMillisecond * DELAY_BETWEEN_REQUESTS)
                {
                    requireDelay = true;
                    chainedFastRequests++;
                }
                else
                {
                    chainedFastRequests = 0;
                }

                delayLength = chainedFastRequests * DELAY_BETWEEN_REQUESTS;
                lastRequestTick = nowTick;
            }

            ParameterizedThreadStart pts = delegate
                                               {
                                                   try
                                                   {
                                                       if (requireDelay)
                                                       {
                                                           request.IsQueued = true;
                                                           Thread.Sleep(delayLength);
                                                           request.IsQueued = false;
                                                       }

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
                                               };


            Thread t = new Thread(pts);
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();

            request.thread = t;

            return true;
        }
    }
}