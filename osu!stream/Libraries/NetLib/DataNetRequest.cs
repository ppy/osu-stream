using System;
using System.Net.Http;
using System.Text;
using System.Threading;

#if iOS
using Foundation;
using System.Runtime.InteropServices;
#endif

namespace osum.Libraries.NetLib
{
#if iOS
    public class NRDelegate : NSUrlConnectionDataDelegate
    {
        public byte[] result;
        public int written = 0;
        public bool finished;
        public Exception error;

        DataNetRequest nr;

        public NRDelegate(DataNetRequest nr) : base()
        {
            this.nr = nr;
        }

        public override void ReceivedResponse(NSUrlConnection connection, NSUrlResponse response)
        {
            long length = response.ExpectedContentLength;
            if (length != -1)
                result = new byte[length];
        }

        public override void ReceivedData(NSUrlConnection connection, NSData data)
        {
            if (result == null)
            {
                result = new byte[data.Length];
                Marshal.Copy(data.Bytes, result, 0, (int)data.Length);
            }
            else if (written + (int)data.Length > result.Length)
            {
                byte[] nb = new byte [result.Length + (int)data.Length];
                result.CopyTo(nb, 0);
                Marshal.Copy(data.Bytes, nb, result.Length, (int)data.Length);
                result = nb;
            }
            else
                Marshal.Copy(data.Bytes, result, written, (int)data.Length);

            written += (int)data.Length;

            if (nr.AbortRequested)
            {
                connection.Cancel();
                return;
            }

            nr.TriggerUpdate();
        }

        public override void FinishedLoading(NSUrlConnection connection)
        {
            finished = true;

#if !DIST
            if (error != null)
                Console.WriteLine("ERROR: " + error.ToString());
#endif

            nr.TriggerUpdate();

            nr.data = result;
            nr.error = error;

            nr.processFinishedRequest();
        }

        public override void FailedWithError(NSUrlConnection connection, NSError err)
        {
            if (err != null)
            {
                error = new Exception(err.ToString());
                nr.error = error;
            }

            finished = true;

            nr.processFinishedRequest();
        }
    }
#endif

    /// <summary>
    /// Downloads a file from the internet to a specified location
    /// </summary>
    public class DataNetRequest : NetRequest
    {
        private readonly string method;
        private readonly string postData;

        public DataNetRequest(string _url, string method = "GET", string postData = null)
            : base(_url)
        {
            this.method = method;
            this.postData = postData;
        }

        public event RequestStartHandler onStart;
        public event RequestUpdateHandler onUpdate;
        public event RequestCompleteHandler onFinish;

        public byte[] data;
        public Exception error;

#if iOS
        NRDelegate del;

        public void TriggerUpdate()
        {
            if (del.result == null) return;

            int len = del.result.Length;
            if (len == 0) return;

            if (onUpdate != null)
                onUpdate(this, del.written, len);
        }
#endif

        public override void Perform()
        {
            try
            {
                //inform subscribers that we have started
                onStart?.Invoke();

#if iOS
                del = new NRDelegate(this);

                NSMutableUrlRequest req = new NSMutableUrlRequest(new NSUrl(UrlEncode(m_url)), NSUrlRequestCachePolicy.ReloadIgnoringCacheData, 30);
                req.HttpMethod = method;
                if (method == "POST")
                {
                    NSMutableDictionary headers = (NSMutableDictionary)req.Headers.MutableCopy();
                    headers.SetValueForKey(new NSString("application/x-www-form-urlencoded"), new NSString("content-type"));

                    req.Headers = headers;

                    req.Body = NSData.FromString(postData);
                }
                NSUrlConnection conn = new NSUrlConnection(req, del, true);

#if !DIST
                if (error != null)
                    Console.WriteLine("requst finished with error " + error);
#endif

#else
                using (HttpClient hc = new HttpClient())
                using (var request = new HttpRequestMessage(postData != null ? HttpMethod.Post : HttpMethod.Get, m_url))
                {
                    if (postData != null)
                    {
                        request.Content = new StringContent(postData, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                    }

                    var response = hc.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                    response.EnsureSuccessStatusCode();

                    // If server returns Content-Length, support download progress reporting.
                    if(response.Content.Headers.ContentLength != null)
                    {
                        int bytesDownloaded = 0;
                        int length = (int)response.Content.Headers.ContentLength;
                        data = new byte[length];

                        using (var stream = response.Content.ReadAsStreamAsync().Result)
                        {
                            while (stream.CanRead)
                            {
                                bytesDownloaded += stream.Read(data, bytesDownloaded, Math.Min(80 * 1024, (int) length - bytesDownloaded));

                                if (onUpdate != null)
                                {
                                    GameBase.Scheduler.Add(delegate
                                    {
                                        onUpdate?.Invoke(this, bytesDownloaded, (int)length);
                                    });
                                }

                                if (bytesDownloaded == length)
                                    break;
                            }
                        }
                    } else
                    {
                        data = response.Content.ReadAsByteArrayAsync().Result;
                    }
                }

                processFinishedRequest();
#endif
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                error = e;
            }
        }

        private const string badChars = " \"%'\\";

        public static string UrlEncode(string s)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in s)
            {
                ushort u = c;
                if (u < 32 || badChars.IndexOf(c) >= 0)
                {
                    result.Append('%');
                    result.Append(u.ToString("X2"));
                }
                else result.Append(c);
            }

            return result.ToString();
        }

        public virtual void processFinishedRequest()
        {
            NetManager.ReportCompleted(this);

            if (AbortRequested) return;

            GameBase.Scheduler.Add(delegate
            {
                onFinish?.Invoke(data, error);
            });
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