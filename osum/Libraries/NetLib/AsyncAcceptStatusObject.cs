using System.Net.Sockets;

namespace osu_common.Libraries.NetLib
{
    internal class AsyncAcceptStatusObject : AsyncStatusObject
    {
        public Socket AcceptedSocket;

        public AsyncAcceptStatusObject(Socket sock) : base(sock)
        {
        }
    }
}