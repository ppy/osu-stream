namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;

    public class DataAddedEventArgs : EventArgs
    {
        private HttpRequestItem item;
        private System.IO.Stream stream;

        public DataAddedEventArgs(HttpRequestItem item, System.IO.Stream stream)
        {
            this.item = item;
            this.stream = stream;
        }

        public HttpRequestItem Item
        {
            get
            {
                return this.item;
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                return this.stream;
            }
        }
    }
}

