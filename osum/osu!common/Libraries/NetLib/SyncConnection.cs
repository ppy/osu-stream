using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace osu_common.Libraries.NetLib
{
    public abstract class SyncConnection : Connection
    {
        private bool isFirstReadPass;
        private bool needClose;
        private int timeOut = 0x1388;
        public bool IsReadUntilClose { get; set; }

        public int TimeOut
        {
            get { return timeOut; }
            set { timeOut = value; }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            AsyncAcceptStatusObject asyncState = (AsyncAcceptStatusObject) ar.AsyncState;
            try
            {
                asyncState.AcceptedSocket = asyncState.socket.EndAccept(ar);
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
            }
            finally
            {
                asyncState.waitEvent.Set();
            }
        }

        private void CheckAsyncException(AsyncStatusObject obj)
        {
            if (obj.exception != null)
            {
                throw obj.exception;
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            AsyncStatusObject asyncState = (AsyncStatusObject) ar.AsyncState;
            try
            {
                asyncState.socket.EndConnect(ar);
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
            }
            finally
            {
                asyncState.waitEvent.Set();
            }
        }

        protected void CreateSocket(SocketType socketType, ProtocolType protocolType)
        {
            try
            {
                base.Socket = new Socket(AddressFamily.InterNetwork, socketType, protocolType);
                base.Socket.Blocking = true;
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
        }

        public override void DispatchNextAction()
        {
            switch (base.NetworkStream.NextAction)
            {
                case NetworkStreamAction.Read:
                    ReadData(null);
                    return;

                case NetworkStreamAction.Write:
                    WriteData(null);
                    return;
            }
        }

        private void InternalReadData(Stream data)
        {
            needClose = false;
            if (base.NetworkStream.HasReadData)
            {
                base.NetworkStream.HasReadData = false;
                base.NetworkStream.Read(data);
                if (!IsReadUntilClose || !base.Active)
                {
                    if (!base.Active)
                    {
                        base.NetworkStream.ClearNextAction();
                    }
                    return;
                }
            }
            isFirstReadPass = true;
            base.NetworkStream.Read(data);
            if (needClose)
            {
                base.Close(false);
            }
            else
            {
                isFirstReadPass = false;
                if (data != null)
                {
                    while ((base.NetworkStream.Read(data) && !needClose) && !base.NeedStop())
                    {
                    }
                    if (needClose)
                    {
                        base.Close(false);
                    }
                }
            }
        }

        private void InternalWriteData(Stream data)
        {
            if (!base.NetworkStream.Write(data))
            {
                do
                {
                    if (base.NetworkStream.Write(data))
                    {
                        return;
                    }
                } while (!base.NeedStop());
            }
        }

        protected internal override void NetworkStreamAccept()
        {
            try
            {
                AsyncAcceptStatusObject state = new AsyncAcceptStatusObject(base.Socket);
                base.Socket.BeginAccept(new AsyncCallback(AcceptCallback), state);
                WaitForEvent(state.waitEvent, TimeOut);
                CheckAsyncException(state);
                base.Socket = state.AcceptedSocket;
                base.Socket.Blocking = true;
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
        }

        protected internal override bool NetworkStreamConnect(IPAddress ip, int port)
        {
            bool flag;
            try
            {
                if (port <= 0)
                {
                    throw new SocketError("Invalid port number", -1);
                }
                IPEndPoint remoteEP = new IPEndPoint(ip, port);
                /*AsyncStatusObject state = new AsyncStatusObject(base.Socket);
                base.Socket.BeginConnect(remoteEP, ConnectCallback, state);
                WaitForEvent(state.waitEvent, TimeOut);
                CheckAsyncException(state);*/
                Socket.Connect(remoteEP);

                flag = true;
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
            return flag;
        }

        protected internal override void NetworkStreamListen(int port)
        {
            try
            {
                if (port < 0)
                {
                    throw new SocketError("Invalid port number", -1);
                }
                IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
                Socket.Bind(localEP);
                Socket.Listen(1);
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
        }

        protected internal override bool NetworkStreamRead(Stream data)
        {
            int num2;
            int batchSize = base.NetworkStream.GetBatchSize();
            if (batchSize <= 0)
            {
                return true;
            }
            byte[] buf = new byte[batchSize];
            if (isFirstReadPass || IsReadUntilClose)
            {
                base.Socket.Blocking = true;
                num2 = ReadBlockAsync(buf, batchSize);
                if (num2 == 0)
                {
                    needClose = true;
                }
            }
            else
            {
                try
                {
                    if (base.Socket == null)
                    {
                        throw new SocketError("Connection is closed", 0x2736);
                    }
                    base.Socket.Blocking = false;

                    if (Socket.Available > 0)
                    {
                        num2 = base.Socket.Receive(buf, 0, batchSize, SocketFlags.None);
                        if (num2 == 0)
                            needClose = true;
                    }
                    else
                        num2 = 0;
                }
                catch (SocketException exception)
                {
                    
                    if (exception.NativeErrorCode != 0x2733)
                    {
                        throw new SocketError(exception.Message, exception.ErrorCode, exception);
                    }
                    num2 = 0;
                }
            }
            if (num2 > 0)
            {
                data.Write(buf, 0, num2);
                base.NetworkStream.UpdateProgress(num2);
                return true;
            }
            return false;
        }

        protected internal override bool NetworkStreamWrite(Stream data)
        {
            int batchSize = base.NetworkStream.GetBatchSize();
            if (batchSize > 0)
            {
                byte[] buffer = new byte[batchSize];
                long position = data.Position;
                do
                {
                    batchSize = base.NetworkStream.GetBatchSize();
                    if (batchSize > (data.Length - position))
                    {
                        batchSize = (int) (data.Length - position);
                    }
                    data.Read(buffer, 0, batchSize);
                    int bytesProceed = WriteBlock (buffer, batchSize);
                    position += bytesProceed;
                    if (bytesProceed < batchSize)
                    {
                        data.Seek((data.Position - batchSize) + bytesProceed, SeekOrigin.Begin);
                    }
                    base.NetworkStream.UpdateProgress(bytesProceed);
                } while ((position < data.Length) && !base.NeedStop());
            }
            return true;
        }

        private int ReadBlockAsync(byte[] buf, int bufLen)
        {
            int processedBytes;
            if (base.Socket == null)
            {
                throw new SocketError("Connection is closed", 0x2736);
            }
            try
            {
                
                processedBytes = base.Socket.Receive(buf, 0, bufLen, SocketFlags.None);
                /*AsyncStatusObject state = new AsyncStatusObject(base.Socket);
                base.Socket.BeginReceive(buf, 0, bufLen, SocketFlags.None, new AsyncCallback(ReadCallback), state);
                WaitForEvent(state.waitEvent, TimeOut);
                CheckAsyncException(state);
                processedBytes = state.processedBytes;*/
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
            return processedBytes;
        }

        private void ReadCallback(IAsyncResult ar)
        {
            AsyncStatusObject asyncState = (AsyncStatusObject) ar.AsyncState;
            try
            {
                asyncState.processedBytes = asyncState.socket.EndReceive(ar);
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
            }
            finally
            {
                asyncState.waitEvent.Set();
            }
        }

        public override void ReadData(Stream data)
        {
            InternalReadData(data);
            Label_0007:
            switch (base.NetworkStream.NextAction)
            {
                case NetworkStreamAction.Read:
                    InternalReadData(null);
                    goto Label_0007;

                case NetworkStreamAction.Write:
                    WriteData(null);
                    goto Label_0007;
            }
        }

        public string ReadString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                base.InitProgress(0L, 0L);
                ReadData(stream);
                stream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return Encoding.ASCII.GetString(buffer);
            }
        }

        private void WaitForEvent(WaitHandle obj, int timeout)
        {
            while (!base.IsAborted)
            {
                if (!obj.WaitOne(timeout, false))
                {
                    throw new SocketError("Timeout error occured", -1);
                }
                break;
            }
        }

        private int WriteBlock (byte[] buf, int toWrite)
        {
            int processedBytes;
            if (base.Socket == null)
            {
                throw new SocketError("Connection is closed", 0x2736);
            }
            try
            {
                /*AsyncStatusObject state = new AsyncStatusObject(base.Socket);
                base.Socket.BeginSend(buf, 0, toWrite, SocketFlags.None, new AsyncCallback(WriteCallback), state);
                WaitForEvent(state.waitEvent, TimeOut);
                CheckAsyncException(state);
                processedBytes = state.processedBytes;*/
                processedBytes = Socket.Send(buf, 0, toWrite, SocketFlags.None);
            }
            catch (SocketException exception)
            {
                throw new SocketError(exception.Message, exception.ErrorCode, exception);
            }
            return processedBytes;
        }

        private void WriteCallback(IAsyncResult ar)
        {
            AsyncStatusObject asyncState = (AsyncStatusObject) ar.AsyncState;
            try
            {
                asyncState.processedBytes = asyncState.socket.EndSend(ar);
            }
            catch (Exception exception)
            {
                asyncState.exception = exception;
            }
            finally
            {
                asyncState.waitEvent.Set();
            }
        }

        public override void WriteData(Stream data)
        {
            InternalWriteData(data);
            Label_0007:
            switch (base.NetworkStream.NextAction)
            {
                case NetworkStreamAction.Read:
                    ReadData(null);
                    goto Label_0007;

                case NetworkStreamAction.Write:
                    InternalWriteData(null);
                    goto Label_0007;
            }
        }

        public void WriteString(string theValue)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(theValue);
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0L, SeekOrigin.Begin);
                WriteData(stream);
            }
        }
    }
}