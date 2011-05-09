using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace osu_common.Libraries.NetLib
{
    public abstract class Connection : IDisposable
    {
        public const int WSAEWOULDBLOCK = 0x2733;
        internal bool active_;
        private int batchSize = 0x2000;
        internal long bytesProceed;
        private long bytesToProceed = -1L;
        private bool isAborted;
        private NetworkStream networkStream;
        public SocketProgressEventHandler Progress;
        public EventHandler Ready;
        private Socket socket;
        internal long totalBytes;
        internal long totalBytesProceed;

        public bool Active
        {
            get { return active_; }
        }

        public int BatchSize
        {
            get { return batchSize; }
            set { batchSize = value; }
        }

        public int BitsPerSec { get; set; }

        public long BytesProceed
        {
            get { return totalBytesProceed; }
        }

        public long BytesToProceed
        {
            get { return bytesToProceed; }
            set { bytesToProceed = value; }
        }

        public bool IsAborted
        {
            get { return isAborted; }
        }

        public NetworkStream NetworkStream
        {
            get
            {
                if (networkStream == null)
                {
                    throw new SocketError("NetworkStream is required", -1);
                }
                return networkStream;
            }
            set
            {
                if (networkStream != value)
                {
                    if ((networkStream != null) && (value != null))
                    {
                        networkStream.CopyTo(value);
                    }
                    networkStream = null;
                    networkStream = value;
                    if (networkStream != null)
                    {
                        networkStream.connection = this;
                    }
                }
            }
        }

        public Socket Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Abort()
        {
            isAborted = true;
        }

        public void Close(bool notifyPeer)
        {
            try
            {
                if (Active)
                {
                    CloseSession(notifyPeer);
                }
            }
            finally
            {
                ShutdownSocket();
            }
        }

        public void CloseSession(bool notifyPeer)
        {
            NetworkStream.Close(notifyPeer);
            if (notifyPeer)
            {
                DispatchNextAction();
            }
        }

        public abstract void DispatchNextAction();

        protected virtual void Dispose(bool disposing)
        {
            Close(false);
        }

        ~Connection()
        {
            try
            {
                Dispose(false);
            }
            catch { }
        }

        public static string GetHostIP(string host)
        {
            IPAddress iPAddress = GetIPAddress(host);
            if (iPAddress != null)
            {
                return iPAddress.ToString();
            }
            return host;
        }

        public static IPAddress GetIPAddress(string ip)
        {
            IPAddress address2;
            try
            {
                if (StringUtils.IsEmpty(ip))
                {
                    throw new SocketError("Invalid host address", -1);
                }
                if (IsIP(ip))
                {
                    return IPAddress.Parse(ip);
                }
                foreach (IPAddress address in Dns.GetHostEntry(ip).AddressList)
                {
                    if (address.AddressFamily.Equals(AddressFamily.InterNetwork))
                    {
                        return address;
                    }
                }
                address2 = null;
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
            return address2;
        }

        public void InitProgress(long bytesProceed, long totalBytes)
        {
            totalBytesProceed = bytesProceed;
            this.bytesProceed = 0L;
            this.totalBytes = totalBytes;
        }

        private static bool IsIP(string ip)
        {
            if (StringUtils.IsEmpty(ip))
            {
                return false;
            }
            string[] stringArray = StringUtils.GetStringArray(ip, '.');
            if (stringArray.Length != 4)
            {
                return false;
            }
            foreach (string str in stringArray)
            {
                if (!Regex.IsMatch(str, @"^\d+$"))
                {
                    return false;
                }
            }
            return true;
        }

        protected internal bool IsProceedLimit()
        {
            return ((bytesToProceed > -1L) && (bytesToProceed <= bytesProceed));
        }

        protected bool NeedStop()
        {
            if (!IsAborted)
            {
                return IsProceedLimit();
            }
            return true;
        }

        protected internal abstract void NetworkStreamAccept();
        protected internal abstract bool NetworkStreamConnect(IPAddress ip, int port);
        protected internal abstract void NetworkStreamListen(int port);
        protected internal abstract bool NetworkStreamRead(Stream data);
        protected internal abstract bool NetworkStreamWrite(Stream data);

        protected internal virtual void OnProgress(SocketProgressEventArgs e)
        {
            if (Progress != null)
            {
                Progress(this, e);
            }
        }

        protected internal virtual void OnReady(EventArgs e)
        {
            if (Ready != null)
            {
                Ready(this, e);
            }
        }

        public abstract void ReadData(Stream data);

        private void ShutdownSocket()
        {
            lock (this)
                {
                    active_ = false;
                    if (socket != null)
                    {
                        try
                        {
                            if (socket.Connected)
                            {
                                socket.Shutdown(SocketShutdown.Both);
                            }
                            socket.Close();
                        }
                        catch
                        { }
                    }
                    socket = null;
                }
            
        }

        public abstract void WriteData(Stream data);
    }
}