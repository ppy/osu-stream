namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class TcpClientConnection : SyncConnection
    {
        public void Open(IPAddress ip, int port)
        {
            bool isReadUntilClose = base.IsReadUntilClose;
            base.IsReadUntilClose = false;
            base.CreateSocket(SocketType.Stream, ProtocolType.Tcp);
            base.NetworkStream.Connect(ip, port);
            if (base.IsAborted)
            {
                base.Close(false);
            }
            else
            {
                base.active_ = true;
                this.DispatchNextAction();
                base.NetworkStream.StreamReady();
            }
            base.IsReadUntilClose = isReadUntilClose;
        }

        public void OpenSession()
        {
            bool isReadUntilClose = base.IsReadUntilClose;
            base.IsReadUntilClose = false;
            base.NetworkStream.OpenClientSession();
            if (base.IsAborted)
            {
                base.Close(false);
            }
            else
            {
                this.DispatchNextAction();
                base.NetworkStream.StreamReady();
            }
            base.IsReadUntilClose = isReadUntilClose;
        }

        public IPAddress IP
        {
            get
            {
                return base.NetworkStream.IP;
            }
        }

        public int Port
        {
            get
            {
                return base.NetworkStream.Port;
            }
        }
    }
}

