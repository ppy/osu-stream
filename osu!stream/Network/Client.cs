using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using osu_common.Helpers;
using System.Net.Sockets;
using System.Threading;
using osu_common.Tencho.Requests;
using osu_common.Tencho.Objects;
using osum.Helpers;
using osu_common.Tencho;
using osum.Helpers.osu_common.Tencho.Objects;
using osum.GameModes;

namespace osum.Network
{
    partial class TenchoClient : GameComponent
    {
        private const int PING_TIMEOUT = 10000;
        private const int SEND_INTERVAL = 250;
        private int BUFFER_SIZE = 8192;

        private TcpClient client;
        private NetworkStream stream;
        private StreamWriter writer;
        private Thread thread;
        private bool AuthenticationSent;

        private Queue<Request> Requests = new Queue<Request>();

        private byte[] readByteArray = new byte[Request.HEADER_LEN];
        private int readBytes;

        event VoidDelegate OnConnect;
        event VoidDelegate OnDisconnect;

        private bool Connected
        {
            get { return client != null && client.Connected; }
        }

        public void Initialize()
        {
            thread = new Thread(Run);
            thread.Priority = ThreadPriority.Highest;
            thread.IsBackground = true;
            thread.Start();

            InitializeOverlay();
        }

        private void Run()
        {
            while (true)
            {
                try
                {
                    if (!Connected)
                        Connect();

                    if (!AuthenticationSent)
                    {
                        SendRequest(new Request(RequestType.Stream_Authenticate, new bString(GameBase.ClientId)));
                        AuthenticationSent = true;
                    }

                    if (Clock.Time - lastPingTime > PING_TIMEOUT)
                    {
                        Reset();
                        Thread.Sleep(500);
                        continue;
                    }

                    if (Connected && client != null)
                    {
                        try
                        {
                            if (!SendOutgoing())
                                continue;

                            while (Connected && stream != null && stream.DataAvailable)
                            {
                                lastPingTime = Clock.Time;

                                int newBytes = stream.Read(readByteArray, readBytes, readByteArray.Length - readBytes);

                                readBytes += newBytes;
                                ReceivedBytes += newBytes;

                                //Read header data
                                if (readBytes == readByteArray.Length && readingHeader)
                                {
                                    readType = (RequestType)BitConverter.ToUInt16(readByteArray, 0);
                                    bool compression = readByteArray[2] == 1;
                                    uint length = BitConverter.ToUInt32(readByteArray, 3);

#if PEPPY
                            Console.WriteLine("R" + length + ": " + readType + (compression ? " compressed" : ""));
#endif

                                    ResetReadArray(false);
                                    readByteArray = new byte[length];
                                }

                                //Read payload data
                                if (readBytes != readByteArray.Length) continue;

                                byte[] copy = new byte[readByteArray.Length];
                                for (int i = 0; i < readByteArray.Length; i++)
                                    copy[i] = readByteArray[i];

                                IncomingRequest(readType, new SerializationReader(new MemoryStream(copy)));

                                ResetReadArray(true); //reset to read the next packet's header.
                            }
                        }
                        catch (Exception e)
                        {
                            Reset();
                        }
                    }

                    Thread.Sleep(1);
                    sendWaitCurrent += 1;
                }
                catch { }
            }

        }

        private void IncomingRequest(RequestType reqType, SerializationReader sr)
        {
            GameBase.Scheduler.Add(delegate
            {
                try
                {
                    switch (reqType)
                    {
                        case RequestType.Tencho_Authenticated:
                            if (OnConnect != null) OnConnect();
                            break;
                        case RequestType.Tencho_Ping:
                            SendRequest(RequestType.Osu_Pong, null);
                            break;
                        case RequestType.Tencho_MatchFound:
                            {
                                ClientMatch m = new ClientMatch(sr);
                                if (m != null)
                                {
                                    GameBase.Match = m;
                                    Console.WriteLine("We have a match!");
                                }
                            }
                            break;
                        case RequestType.Tencho_MatchStateChange:
                        case RequestType.Tencho_MatchPlayerDataChange:
                            {
                                ClientMatch m = GameBase.Match;
                                if (m != null)
                                    m.Update(reqType, new ClientMatch(sr));
                                else
                                    GameBase.LeaveMatch();
                                lastStateChange = Clock.Time;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    File.WriteAllText("network-error.txt", e.ToString());
                }
            }, true);
        }

        internal void Connect()
        {
            try
            {
                IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 16384);
                UdpClient udp = new UdpClient(16384);

                string returnData = null;
                do
                {
                    byte[] receiveBytes = udp.Receive(ref recvEndPoint);
                    returnData = Encoding.ASCII.GetString(receiveBytes);
                } while (returnData == null || !returnData.StartsWith("Tencho"));

                udp.Close();

                Reset();

                IPEndPoint endpoint = new IPEndPoint(recvEndPoint.Address, 16384);
                //IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("192.168.2.3"), 16384);

                client = ClientHelper.Connect(endpoint, 10000);

                client.LingerState.Enabled = true;
                client.LingerState.LingerTime = 2000;

                client.ReceiveTimeout = 32000;
                client.SendTimeout = 32000;
                client.NoDelay = true;

                stream = client.GetStream();
                writer = new StreamWriter(stream);
            }
            catch (Exception)
            {
            }
        }

        private void Reset()
        {
            GameBase.LeaveMatch();

            if (client != null && client.Connected)
            {
                if (OnDisconnect != null)
                    OnDisconnect();

                client.Close();
                client = null;
            }

            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }

            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }

            GameBase.Match = null;

            AuthenticationSent = false;
            lastPingTime = Clock.Time;
            ResetReadArray();

            Requests.Clear();
        }

        private void ResetReadArray(bool isHeader = true)
        {
            if (isHeader)
                readByteArray = new byte[Request.HEADER_LEN];
            readingHeader = isHeader;
            readBytes = 0;
        }

        internal void SendRequest(RequestType resType, bSerializable obj)
        {
            if (!Connected)
                return;

            Request r = new Request(resType, obj);
            SendRequest(r);
        }

        internal void SendRequest(Request request)
        {
            Requests.Enqueue(request);
        }


        object queueLock = new object();
        int send_seq;
        private int SentBytes;
        private SerializationWriter sw = new SerializationWriter(new MemoryStream());
        private int lastPingTime;
        private RequestType readType;
        private int ReceivedBytes;
        private bool readingHeader;
        private int sendWaitCurrent;
        private int lastStateChange;


        /// <summary>
        /// Sends any requests waiting in the outgoing queue.
        /// </summary>
        /// <returns>false if the connection is no longer valid</returns>
        private bool SendOutgoing()
        {
            List<Request> thisRunQueue = new List<Request>();

            //Send any waiting requests.
            if (Requests.Count > 0 && stream.CanWrite)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {

#if DEBUG
                    Console.WriteLine("Sending queue requests (" + Requests.Count + ")");
#endif

                    send_seq = (send_seq + 1) % ushort.MaxValue;

                    while (Requests.Count > 0)
                    {
                        try
                        {
                            Request r;

                            lock (queueLock)
                                r = Requests.Dequeue();

                            if (r != null)
                            {
                                SentBytes += r.Send(memoryStream, sw);
                                thisRunQueue.Add(r);
                            }
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    if (memoryStream.Length > 0 && stream != null)
                    {
                        memoryStream.Position = 0;

                        byte[] buff = new byte[BUFFER_SIZE];

                        int read;
                        do
                        {
                            read = memoryStream.Read(buff, 0, buff.Length);
                            if (read > 0) stream.Write(buff, 0, read);
                        } while (read > 0);

                        lastPingTime = Clock.Time;
                    }
                }
            }

            return true;
        }
    }
}
