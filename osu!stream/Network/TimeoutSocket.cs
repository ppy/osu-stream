using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace osu_common.Helpers
{
    public class ClientHelper
    {
        private static bool IsConnectionSuccessful = false;
        private static Exception socketexception;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        public static TcpClient Connect(IPEndPoint remoteEndPoint, int timeoutMSec)
        {
            TimeoutObject.Reset();
            socketexception = null;

            string serverip = Convert.ToString(remoteEndPoint.Address);
            int serverport = remoteEndPoint.Port;
            TcpClient tcpclient = new TcpClient();

            tcpclient.BeginConnect(serverip, serverport,
                new AsyncCallback(CallBackMethod), tcpclient);

            if (TimeoutObject.WaitOne(timeoutMSec, false))
            {
                if (IsConnectionSuccessful)
                {
                    return tcpclient;
                }
                else
                {
                    throw socketexception;
                }
            }
            else
            {
                tcpclient.Close();
                throw new TimeoutException("TimeOut Exception");
            }
        }
        private static void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(asyncresult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
    }
}
