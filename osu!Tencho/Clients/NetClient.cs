using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using osu_Tencho.Helpers;
using System.Text;
using System.Collections.Generic;
using osu_common.Tencho.Requests;
using osu_common.Helpers;
using osu_common.Tencho.Objects;
using osu_common;
using System.Threading;
using osu_common.Tencho;

namespace osu_Tencho.Clients
{
    internal abstract class NetClient : Client
    {
        internal TcpClient client;

        public const int MAX_BUFFER_SIZE = 8192;

        private string awayMessage;
        internal override string AwayMessage
        {
            get { return awayMessage; }
            set
            {
                if (value == null)
                    awaySenders = null;
                else
                    awaySenders = new List<string>();
                awayMessage = value;
            }
        }

        public override bool IsAway { get { return AwayMessage != null; } }

        /// <summary>
        /// A list used primarily to keep track of other clients the away message has been sent to.
        /// This is so we can ensure it is only sent once.
        /// Using a string here to avoid keeping references to clients hanging around. Might be better to use WeakReferences...
        /// </summary>
        protected List<string> awaySenders;

        internal override bool RequireAwayMessage(Client target)
        {
            if (AwayMessage == null) return false;

            if (awaySenders.Contains(target.IrcFullName)) return false;
            awaySenders.Add(target.IrcFullName);

            return true;
        }

        internal bool isBanned;
        internal bool isPinging;
        internal long SilencedUntil;
        internal long lastReceiveTime;

        internal NetworkStream stream;

        protected readonly object SendQueueLock = new object();
        protected List<Request> sendList = new List<Request>();

        protected RollingTime timePublic;
        protected RollingTime timePrivate;

        public static int TotalClientCount;

        internal NetClient(TcpClient c, RequestTarget target)
        {
            Interlocked.Increment(ref TotalClientCount);

            RequestTargetType = target;

            lastReceiveTime = Tencho.CurrentTime;

            try
            {
                client = c;
                client.NoDelay = true;

                client.SendBufferSize = Tencho.Config.GetValue<int>("ClientBufferSize", MAX_BUFFER_SIZE);

                client.LingerState.Enabled = true;
                client.LingerState.LingerTime = 2000;

                Address = client.Client.RemoteEndPoint.ToString().Split(':')[0];

                GetLocation();

                stream = client.GetStream();

                timePublic = new RollingTime(5, 4000);
                timePrivate = new RollingTime(10, 5000);

                receiveBuffer = Tencho.Buffers.Pop();
                sendBuffer = Tencho.Buffers.Pop();
            }
            catch (Exception e)
            {
                Bacon.WriteLine("Error initialising client: " + e);
            }
        }

        ~NetClient()
        {
            Interlocked.Decrement(ref TotalClientCount);

            if (!isKilled && Tencho.AllowMultiplayer)
            {
                Console.WriteLine("FUCK SHIT FUCK GET THE FUCK OUT OF HERE!");
                kill("FINALIZER");
            }
        }

        internal bool CheckSameLineSpam(string message)
        {
            bool result = Tencho.CurrentTime - lastChatLineTime < 3000 && message == lastChatLine;

            //update last line.
            if (!result) lastChatLine = message;

            lastChatLineTime = Tencho.CurrentTime;

            return result;
        }

        internal bool IsAuthenticated;
        protected bool CompleteAuthentication()
        {
            if (UserManager.RegisterClient(this, true))
                IsAuthenticated = true;
            return IsAuthenticated;
        }

        public override string Username
        {
            get
            {
                return base.Username;
            }
            set
            {
                for (int i = value.Length - 1; i >= 0; i--)
                    if (value[i] > 0xa1 || value[i] == '!')
                        throw new Exception("Disallowed Username");

                if (value.Contains("TenchoBot"))
                    throw new Exception("tried to become tenchobot");

                base.Username = value;
            }
        }

        /// <summary>
        /// I/O handling once the connection is in an authenticated state.
        /// </summary>
        internal virtual bool CheckClient(TenchoWorker bw)
        {
            if (isKilled || client == null || !client.Connected)
            {
                Kill("lost connection");
                return false;
            }

            if (SilencedUntil > 0 && SilencedUntil < Tencho.CurrentTime)
                SilencedUntil = 0;

            if (!killPending) ReceiveIncoming();
            if (!killPending) SendOutgoing(bw.SerializationWriter);

            if (killPending)
                kill(killPendingReason);

            if (Tencho.CurrentTime - lastReceiveTime > (RequestTargetType == RequestTarget.Irc ? Tencho.PING_INTERVAL_IRC : Tencho.PING_INTERVAL_OSU))
                Ping();

            CheckForTimeout();

            return !isKilled;
        }

        #region Receiving Logic
        private Buffer sendBuffer;
        private Buffer receiveBuffer;

        int totalBytesRead = 0;

        /// <summary>
        /// Process the input buffer.
        /// </summary>
        /// <returns>Whether any messages were handled.</returns>
        protected virtual void ReceiveIncoming()
        {
            if (stream == null || killPending) return;

            if (!isReceiving)
            {
                //we either have completed a previous async operation, or just started a new one and completed synchoronously.
                while (totalBytesRead > 0)
                {
                    long oldPos = receiveBuffer.Stream.Position;

                    int consumedBytes = HandleIncoming(receiveBuffer, totalBytesRead);

                    if (consumedBytes < 0)
                    {
                        Bacon.WriteSystem("fatal: consumed returned " + consumedBytes);
                        Kill("buffer");
                        return;
                    }

                    if (consumedBytes == 0)
                    {
                        receiveBuffer.Stream.Seek(oldPos, SeekOrigin.Begin);
                        break; //the read wasn't useful.
                    }

                    //force the stream to the new end position.
                    //we need to use the previously stored position just in-case HandleIncoming() read too short/far.
                    receiveBuffer.Stream.Seek(oldPos + consumedBytes, SeekOrigin.Begin);

                    //we just successfully read some data, so let's reduce the amount remaining.
                    totalBytesRead -= consumedBytes;
                }

                if (totalBytesRead > 0)
                {
                    //we still have unprocessed bytes.
                    //this copy operation relies on the stream being in the right place (left off from above).
                    //we should only need to do this when we are near filling our buffer, but to make sure things actually work we will do it every time for now.
                    int streamOffset = receiveBuffer.StreamOffset;
                    for (int i = 0; i < totalBytesRead; i++)
                        receiveBuffer.bBlock[receiveBuffer.bOffset + i] = receiveBuffer.bBlock[streamOffset + i];
                }

                receiveBuffer.Stream.Seek(0, SeekOrigin.Begin);

                receiveAsync();
            }
        }

        bool isReceiving;
        /// <summary>
        /// Start a new async read.
        /// </summary>
        /// <returns>True if the read completed synchronously</returns>
        private bool receiveAsync()
        {
            //ensure we only start a new receive if we aren't already.
            if (isReceiving) return false;
            isReceiving = true;

            try
            {
                if (receiveBuffer.Stream.Position == 0 && totalBytesRead >= MAX_BUFFER_SIZE)
                {
                    //we've already read a full buffer and haven't been able to process it.
                    //this is a hard error.
                    Bacon.WriteSystem("fatal: stream greater than buffer (" + totalBytesRead + ")");
                    string header = "";
                    for (int i = 0; i < Request.HEADER_LEN; i++)
                        header += receiveBuffer.bBlock[receiveBuffer.bOffset + i] + " ";
                    Bacon.WriteSystem(header);
                    Kill("overflow");
                    return false;
                }

                //start reading based on the current position of the reader. we could be halfway through an existing packet
                stream.BeginRead(receiveBuffer.bBlock, receiveBuffer.bOffset + totalBytesRead, 1, ar =>
                {
                    try
                    {
                        if (stream != null && client.Connected)
                        {
                            int bytesRead = stream.EndRead(ar);

                            if (bytesRead == 0)
                            {
                                Kill("connection dropped");
                                return;
                            }

                            if (bytesRead != 1)
                            {
                                Bacon.WriteSystem("Async read didn't read 1 byte... it read " + bytesRead);
                                Kill("buffer");
                            }
                            //we are always reading a single byte.
                            totalBytesRead += bytesRead;

                            int available = client.Client.Available;

                            if (available > 0)
                            {
                                int maxReadSize = MAX_BUFFER_SIZE - totalBytesRead;

                                if (maxReadSize > 0)
                                {
                                    //perform a blocking read to get the rest of the data that is available.
                                    totalBytesRead += stream.Read(receiveBuffer.bBlock, receiveBuffer.bOffset + totalBytesRead, Math.Min(maxReadSize, available));
                                }
                            }

                            isReceiving = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Kill("read timeout");
                        return;
                    }
                }, null);

                return false;
            }
            catch (Exception e)
            {
                Kill("receive error");
            }

            return false;
        }

        protected abstract int HandleIncoming(Buffer buffer, int length);
        #endregion

        #region Sending Logic

        protected void SendOutgoing()
        {
            //todo: optimise this.
            using (MemoryStream ms = new MemoryStream())
            using (SerializationWriter sw = new SerializationWriter(ms))
                SendOutgoing(sw);
        }

        long pendingSendStartTime = -1;
        IAsyncResult pendingSendResult;
        protected void SendOutgoing(SerializationWriter sw)
        {
            IAsyncResult result = pendingSendResult;

            if (result != null && !result.IsCompleted)
            {
                if (pendingSendStartTime > 0 && (Tencho.CurrentTime - pendingSendStartTime)
                    > (RequestTargetType == RequestTarget.Osu ? Tencho.PING_TIMEOUT_OSU : Tencho.PING_TIMEOUT_IRC))
                    Kill("send timeout");
                return;
            }

            try
            {
                int count = sendList.Count;

                if (count == 0) return;

                List<Request> thisRunSendList;

                lock (SendQueueLock)
                {
                    thisRunSendList = new List<Request>(sendList);
                    sendList.Clear();
                    if (sendList.Capacity > 512) sendList.TrimExcess();
                }

                int sendLength = 0;

                MemoryStream swStream = (MemoryStream)sw.BaseStream;

                int processCount = 0;
                foreach (Request r in thisRunSendList)
                {
#if DEBUG
                    Console.WriteLine("SEND {0} {1}", username, r.type);
#endif
                    int thisSize = r.Send(RequestTargetType, sw);

                    if (sendLength + thisSize > MAX_BUFFER_SIZE)
                        break; //we have too much to send in a single async send; need to return some to the queue and hold off.

                    swStream.Seek(0, SeekOrigin.Begin);

                    swStream.Read(sendBuffer.bBlock, sendBuffer.bOffset + sendLength, thisSize);
                    sendLength += thisSize;

                    processCount++;
                }

                if (processCount != thisRunSendList.Count)
                {
                    //return unsent items to the queue.
                    lock (SendQueueLock)
                        sendList.InsertRange(0, thisRunSendList.GetRange(processCount, thisRunSendList.Count - processCount));
                }

                if (sendLength == 0 || stream == null)
                    return;

                pendingSendStartTime = Tencho.CurrentTime;

                pendingSendResult = stream.BeginWrite(sendBuffer.bBlock, sendBuffer.bOffset, sendLength, ar =>
                {
                    try
                    {
                        if (stream != null)
                        {
                            stream.EndWrite(ar); //this blocks momentarily.
                            pendingSendResult = null;
                            pendingSendStartTime = -1;
                        }
                        else
                            Kill("send error");
                    }
                    catch
                    {
                        Kill("send error");
                    }
                }, null);
            }
            catch
            {
                Kill("send error");
            }
        }
        #endregion

        internal void SendRequest(RequestType resType, bSerializable obj)
        {
            Request r = new Request(resType, obj);
            SendRequest(r);
        }

        internal override void SendRequest(Request request)
        {
            if (request == null || isKilled) return;
            lock (SendQueueLock)
                sendList.Add(request);
        }

        protected void GetLocation()
        {
            try
            {
                Location = Tencho.GeoIpLookup.getLocation(Address);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sends a ping request to the underlying client.
        /// </summary>
        internal void Ping()
        {
            if (!isPinging)
            {
                isPinging = true;
                SendPing();
            }
        }

        /// <summary>
        /// Protected method to send a ping packet.
        /// </summary>
        protected abstract void SendPing();

        internal void Pong()
        {
            lastReceiveTime = Tencho.CurrentTime;
            isPinging = false;
        }

        internal bool CheckForTimeout()
        {
            bool didTimeout = isPinging && Tencho.CurrentTime - lastReceiveTime > Tencho.PING_TIMEOUT_OSU;

            if (didTimeout)
                Kill("ping timeout " + ((Tencho.CurrentTime - lastReceiveTime) / 1000 + "s"));
            return didTimeout;
        }

        protected override void kill(string reason)
        {
            lock (InternalLock)
                if (isKilled) return;

#if DEBUG
            WriteConsole("[-] " + reason);
#endif

            base.kill(reason);

            SendOutgoing();

            try
            {
                if (client != null)
                    client.Close();
            }
            catch (Exception)
            {
            }

            Tencho.Buffers.Push(receiveBuffer);
            Tencho.Buffers.Push(sendBuffer);

            receiveBuffer = null;
            sendBuffer = null;

            client = null;
            stream = null;
            pendingSendResult = null;
            sendList = null;
        }
    }
}