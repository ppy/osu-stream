namespace osu_common.Libraries.NetLib
{
    using System;

    public class HttpError : SocketError
    {
        private string[] responseText;

        public HttpError(string message, int errorCode, string[] responseText) : base(message, errorCode)
        {
            this.responseText = responseText;
        }

        public HttpError(string message, int errorCode, string[] responseText, Exception innerException) : base(message, errorCode, innerException)
        {
            this.responseText = responseText;
        }

        public string[] ResponseText
        {
            get
            {
                return this.responseText;
            }
        }
    }
}

