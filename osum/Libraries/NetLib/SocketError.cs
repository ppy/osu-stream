namespace osu_common.Libraries.NetLib
{
    using System;

    public class SocketError : Exception
    {
        public const string BatchSizeInvalid = "Invalid Batch Size";
        private int errorCode;
        public const string InvalidAddress = "Invalid host address";
        public const string InvalidPort = "Invalid port number";
        public const string NoNetworkStream = "NetworkStream is required";
        public const string SocketIsInvalid = "Connection is closed";
        public const int SocketIsInvalidCode = 0x2736;
        public const string TimeoutOccured = "Timeout error occured";

        public SocketError(string message, int errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        public SocketError(string message, int errorCode, Exception exception) : base(message, exception)
        {
            this.errorCode = errorCode;
        }

        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }
}

