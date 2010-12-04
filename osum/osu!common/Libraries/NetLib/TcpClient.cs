using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace osu_common.Libraries.NetLib
{
    public abstract class TcpClient : Component
    {
        public const string NotConnectedError = "The connection is not active";
        private static readonly object changed = new object();
        private static readonly object closed = new object();
        private static readonly object opened = new object();
        private TcpClientConnection connection;
        private bool inProgress;
        private int port;
        private string server;

        protected TcpClient()
        {
            string str = (Environment.Version.Major > 1) ? "2.0.0.0" : "1.0.3300.0";

            connection = new TcpClientConnection();
            BatchSize = 0x2000;
            TimeOut = 0xea60;
            BitsPerSec = 0;
            port = GetDefaultPort();
            server = string.Empty;
        }

        [Browsable(false)]
        public bool Active
        {
            get { return ((Connection != null) && Connection.Active); }
        }

        [DefaultValue(0x2000)]
        public int BatchSize
        {
            get { return Connection.BatchSize; }
            set
            {
                if (BatchSize != value)
                {
                    Connection.BatchSize = value;
                    OnChanged(new PropertyChangedEventArgs("BatchSize"));
                }
            }
        }

        [DefaultValue(0)]
        public int BitsPerSec
        {
            get { return Connection.BitsPerSec; }
            set
            {
                if (BitsPerSec != value)
                {
                    Connection.BitsPerSec = value;
                    OnChanged(new PropertyChangedEventArgs("BitsPerSec"));
                }
            }
        }

        [Browsable(false)]
        public TcpClientConnection Connection
        {
            get { return connection; }
        }

        protected bool InProgress
        {
            get { return inProgress; }
            set { inProgress = value; }
        }

        public int Port
        {
            get { return port; }
            set
            {
                if (port != value)
                {
                    port = value;
                    OnChanged(new PropertyChangedEventArgs("Port"));
                }
            }
        }

        [DefaultValue("")]
        public string Server
        {
            get { return server; }
            set
            {
                if (server != value)
                {
                    server = value;
                    OnChanged(new PropertyChangedEventArgs("Server"));
                }
            }
        }

        [DefaultValue(0xea60)]
        public int TimeOut
        {
            get { return Connection.TimeOut; }
            set
            {
                if (TimeOut != value)
                {
                    Connection.TimeOut = value;
                    OnChanged(new PropertyChangedEventArgs("TimeOut"));
                }
            }
        }

        public event PropertyChangedEventHandler Changed
        {
            add { base.Events.AddHandler(changed, value); }
            remove { base.Events.RemoveHandler(changed, value); }
        }

        public event EventHandler Closed
        {
            add { base.Events.AddHandler(closed, value); }
            remove { base.Events.RemoveHandler(closed, value); }
        }


        public event EventHandler Opened
        {
            add { base.Events.AddHandler(opened, value); }
            remove { base.Events.RemoveHandler(opened, value); }
        }

        protected void CheckConnected()
        {
            if (!Active)
            {
                throw new SocketError("The connection is not active", -1);
            }
        }

        public void Close()
        {
            bool active = Active;
            InternalClose();
            if (active)
            {
                OnClosed(EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (connection != null)
                {
                    Close();
                    connection.Dispose();
                    connection = null;
                }
                base.Dispose(disposing);
            }
            catch { }
        }

        protected virtual int GetDefaultPort()
        {
            return 0;
        }

        protected virtual void InternalClose()
        {
            if (Connection != null)
            {
                Connection.Close(true);
            }
        }

        protected virtual void InternalOpen()
        {
            if (BatchSize < 1)
            {
                throw new SocketError("Invalid Batch Size", -1);
            }
            OpenConnection(Server, Port);
        }

        protected virtual void OnChanged(PropertyChangedEventArgs e)
        {
            var handler = (PropertyChangedEventHandler) base.Events[changed];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnClosed(EventArgs e)
        {
            var handler = (EventHandler) base.Events[closed];
            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnOpened(EventArgs e)
        {
            var handler = (EventHandler) base.Events[opened];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Open()
        {
            if (!Active)
            {
                try
                {
                    InternalOpen();
                    OnOpened(EventArgs.Empty);
                }
                catch
                {
                    inProgress = true;
                    try
                    {
                        Close();
                    }
                    catch (SocketException)
                    {
                    }
                    catch (SocketError)
                    {
                    }
                    inProgress = false;
                    throw;
                }
            }
        }

        protected virtual void OpenConnection(string server, int port)
        {
            Connection.NetworkStream = new NetworkStream();
            Connection.Open(NetLib.Connection.GetIPAddress(server), port);
        }
    }
}