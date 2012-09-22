using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_Tencho.Clients;
using System.Net.Sockets;
using osu_common.Tencho.Requests;
using osu_common.Helpers;
using osu_common.Tencho.Objects;
using osu_Tencho.Multiplayer;
using osum.Helpers.osu_common.Tencho.Objects;

namespace osu_Tencho.Clients
{
    internal class ClientStream : NetClient
    {
        private ServerMatch CurrentMatch;

        internal ClientStream(TcpClient client)
            : base(client, RequestTarget.Stream)
        {
            this.client = client;
        }

        internal override void InitializeClient()
        {
            base.InitializeClient();
        }

        protected override int HandleIncoming(osu_Tencho.Buffer buffer, int size)
        {
            //not yet enough header data.
            if (size < Request.HEADER_LEN) return 0;

            RequestType readType = (RequestType)buffer.Reader.ReadInt16();
            bool compression = buffer.Reader.ReadBoolean();
            int bodyLength = buffer.Reader.ReadInt32();

            //not yet enough body data.
            if (size - Request.HEADER_LEN < bodyLength) return 0;

            IncomingRequest(readType, buffer.Reader);

            if (!IsAuthenticated && Username != null)
            {
                if (CompleteAuthentication())
                    SendRequest(RequestType.Tencho_Authenticated, null);
                else
                    Kill("auth failure");
            }

            return Request.HEADER_LEN + bodyLength;
        }

        private void IncomingRequest(RequestType reqType, SerializationReader sr)
        {
#if DEBUG
            Console.WriteLine("RECV {0} {1}", username, reqType);
#endif
            switch (reqType)
            {
                case RequestType.Stream_Authenticate:
                    Username = new bString(sr).text;
                    break;
                case RequestType.Stream_RequestMatch:
                    ServerMatch m = Lobby.Matchmake(this);
                    SendRequest(RequestType.Tencho_MatchFound, m);
                    CurrentMatch = m;
                    break;
                case RequestType.Stream_CancelMatch:
                    if (CurrentMatch != null)
                        CurrentMatch.Leave(this);
                    CurrentMatch = null;
                    break;
                case RequestType.Stream_RequestStateChange:
                    if (CurrentMatch == null) break;
                    CurrentMatch.RequestState(this, (MatchState)new bInt(sr).number);
                    break;
                case RequestType.Stream_RequestSong:
                    if (CurrentMatch == null) break;
                    CurrentMatch.ChangeSong(this, new bBeatmap(sr));
                    break;
                case RequestType.Osu_Pong:
                    Pong();
                    break;
                case RequestType.Stream_InputUpdate:
                    if (CurrentMatch == null) break;
                    CurrentMatch.UpdatePlayer(this, new bPlayerData(sr));
                    break;
            }
        }

        protected override void kill(string reason)
        {
            if (CurrentMatch != null)
            {
                CurrentMatch.Leave(this);
                CurrentMatch = null;
            }

            base.kill(reason);
        }

        protected override void SendPing()
        {
            SendRequest(RequestType.Tencho_Ping, null);
        }

        internal override void HandleChangeUsername(Client other, string newUsername)
        {
            throw new NotImplementedException();
        }
    }
}
