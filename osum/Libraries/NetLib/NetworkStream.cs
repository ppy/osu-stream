using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace osu_common.Libraries.NetLib
{
    public class NetworkStream
    {
        internal Connection connection;
        private IPAddress ip;
        private int listenPort;
        private NetworkStreamAction nextAction;
        private string peerIP;
        private string peerName;
        private int port;

        public Connection Connection
        {
            get { return connection; }
        }

        public bool HasReadData { get; set; }

        public IPAddress IP
        {
            get { return ip; }
        }

        public int ListenPort
        {
            get { return listenPort; }
        }

        public NetworkStreamAction NextAction
        {
            get { return nextAction; }
        }

        public string PeerIP
        {
            get { return peerIP; }
        }

        public string PeerName
        {
            get { return peerName; }
        }

        public int Port
        {
            get { return port; }
        }

        public virtual void Accept()
        {
            ClearNextAction();
            Connection.NetworkStreamAccept();
            try
            {
                IPHostEntry hostByAddress = Dns.GetHostEntry(((IPEndPoint) Connection.Socket.RemoteEndPoint).Address);
                if (hostByAddress.AddressList.Length > 0)
                {
                    peerIP = hostByAddress.AddressList[0].ToString();
                    peerName = hostByAddress.HostName;
                    if (StringUtils.IsEmpty(peerName))
                    {
                        peerName = peerIP;
                    }
                    peerName = peerName.Trim();
                }
            }
            catch (SocketException)
            {
                peerIP = string.Empty;
                peerName = string.Empty;
            }
        }

        public void ClearNextAction()
        {
            nextAction = NetworkStreamAction.None;
        }

        public virtual void Close(bool notifyPeer)
        {
            ClearNextAction();
        }

        public virtual bool Connect(IPAddress ip, int port)
        {
            ClearNextAction();
            this.ip = ip;
            this.port = port;
            return Connection.NetworkStreamConnect(ip, port);
        }

        public virtual void CopyTo(NetworkStream destination)
        {
            destination.listenPort = ListenPort;
            destination.peerName = PeerName;
            destination.peerIP = PeerIP;
            destination.ip = IP;
            destination.port = Port;
        }

        public virtual int GetBatchSize()
        {
            if (Connection.BatchSize < 1)
            {
                throw new SocketError("Invalid Batch Size", -1);
            }
            if ((Connection.BytesToProceed > -1L) &&
                ((Connection.BytesToProceed - Connection.bytesProceed) < Connection.BatchSize))
            {
                return (int) (Connection.BytesToProceed - Connection.bytesProceed);
            }
            return Connection.BatchSize;
        }

        public virtual void Listen(int port)
        {
            ClearNextAction();
            ip = null;
            this.port = port;
            Connection.NetworkStreamListen(port);
            listenPort = ((IPEndPoint) Connection.Socket.LocalEndPoint).Port;
        }

        public virtual void OpenClientSession()
        {
        }

        public virtual void OpenServerSession()
        {
        }

        public virtual bool Read(Stream data)
        {
            ClearNextAction();
            return Connection.NetworkStreamRead(data);
        }

        public void SetNextAction(NetworkStreamAction action)
        {
            if (nextAction == NetworkStreamAction.None)
            {
                nextAction = action;
            }
        }

        protected internal virtual void StreamReady()
        {
            Connection.OnReady(new EventArgs());
        }

        protected internal virtual void UpdateProgress(int bytesProceed)
        {
            Connection connection = Connection;
            connection.bytesProceed += bytesProceed;
            Connection connection2 = Connection;
            connection2.totalBytesProceed += bytesProceed;
            Connection.OnProgress(new SocketProgressEventArgs(Connection.totalBytesProceed, Connection.totalBytes));
        }

        public virtual bool Write(Stream data)
        {
            ClearNextAction();
            return ((data.Length == 0L) || Connection.NetworkStreamWrite(data));
        }
    }
}