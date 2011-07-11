using System;
using System.IO;
using System.Threading;
using System.Net;
using osum;

#if iOS
using MonoTouch.Foundation;
using System.Runtime.InteropServices;
#endif

namespace osu_common.Libraries.NetLib
{
#if iOS
    public class NRDelegate : NSUrlConnectionDelegate
    {
        public byte[] result;
        public int written = 0;
        public bool finished;
        public Exception error;

        public override void ReceivedResponse (NSUrlConnection connection, NSUrlResponse response)
        {
            long length = response.ExpectedContentLength;
            if (length != -1)
                result = new byte[length];
        }

        public override void ReceivedData (NSUrlConnection connection, NSData data)
        {
            if (written + data.Length > result.Length)
            {
                byte[] nb = new byte [result.Length + data.Length];
                result.CopyTo (nb, 0);
                Marshal.Copy (data.Bytes, nb, result.Length, (int) data.Length);
                result = nb;
            }
            else
                Marshal.Copy(data.Bytes, result, written, (int)data.Length);
            written += (int)data.Length;
        }

        public override void FinishedLoading (NSUrlConnection connection)
        {
            finished = true;
        }

        public override void FailedWithError (NSUrlConnection connection, NSError err)
        {
            error = new Exception(err.ToString());
            finished = true;
        }
    }
#endif

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

        protected byte[] data;
        protected Exception error;

#if iOS
        NRDelegate del;
#endif

        public override void Perform()
        {
            try
            {
                //inform subscribers that we have started
                if (onStart != null)
                    onStart();

#if iOS
                using (NSAutoreleasePool pool = new NSAutoreleasePool())
                {
                    del = new NRDelegate();

                    NSUrlRequest req = new NSUrlRequest (new NSUrl(m_url.Replace(" ","%20")), NSUrlRequestCachePolicy.ReloadIgnoringCacheData, 15);
                    NSUrlConnection conn = new NSUrlConnection(req, del, false);
                    conn.Start();

                    int progress = 0;
                    while (!del.finished)
                    {
                        if (progress != del.written)
                        {
                            if (onUpdate != null)
                                onUpdate(this, del.written, del.result.Length);
                            progress = del.written;
                        }
                        NSRunLoop.Current.RunUntil(NSDate.FromTimeIntervalSinceNow(0.1));
                    }

                    data = del.result;
                    error = del.error;
                }
#else
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadDataCompleted += wc_DownloadDataCompleted;
                    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc.DownloadDataAsync(new Uri(m_url));

                    while (wc.IsBusy)
                        Thread.Sleep(500);
                }
#endif

                processFinishedRequest();
                
            }
            catch (ThreadAbortException)
            { }
        }

        protected virtual void processFinishedRequest()
        {
            GameBase.Scheduler.Add(delegate
            {
                if (onFinish != null)
                    onFinish(data, error);
            });
        }

        long totalBytesReceived = 0;

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            totalBytesReceived += e.BytesReceived;
            GameBase.Scheduler.Add(delegate
            {
                if (onUpdate != null)
                    onUpdate(this, totalBytesReceived, e.TotalBytesToReceive);
            });
        }

        void wc_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            data = e.Result;
            error = e.Error;
        }

        public override bool Valid()
        {
            return true;
        }

        public override void OnException(Exception e)
        {
#if !DIST
            Console.WriteLine("net error:" + e);
#endif
            processFinishedRequest();
        }

        #region Nested type: RequestCompleteHandler

        public delegate void RequestCompleteHandler(byte[] data, Exception e);

        #endregion
    }
}