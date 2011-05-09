namespace osu_common.Libraries.NetLib
{
    using System;

    public class StreamError : Exception
    {
        public StreamError(string message) : base(message)
        {
        }

        public StreamError(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

