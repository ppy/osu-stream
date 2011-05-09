namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Net.Sockets;
    using System.Threading;

    internal class AsyncStatusObject
    {
        public Exception exception;
        public int processedBytes;
        public Socket socket;
        public AutoResetEvent waitEvent = new AutoResetEvent(false);

        public AsyncStatusObject(Socket sock)
        {
            this.socket = sock;
        }
    }
}

