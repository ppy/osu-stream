namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;

    public class GetDataStreamEventArgs : EventArgs
    {
        private HttpRequestItem item;
        private System.IO.Stream stream;

        public GetDataStreamEventArgs(HttpRequestItem item)
        {
            this.item = item;
            this.stream = null;
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
            set
            {
                this.stream = value;
            }
        }
    }
}

