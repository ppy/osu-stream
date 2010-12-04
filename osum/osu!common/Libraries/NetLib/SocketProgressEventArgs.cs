namespace osu_common.Libraries.NetLib
{
    using System;

    public class SocketProgressEventArgs : EventArgs
    {
        private long bytesProceed;
        private long totalBytes;

        public SocketProgressEventArgs(long bytesProceed, long totalBytes)
        {
            this.bytesProceed = bytesProceed;
            this.totalBytes = totalBytes;
        }

        public long BytesProceed
        {
            get
            {
                return this.bytesProceed;
            }
        }

        public long TotalBytes
        {
            get
            {
                return this.totalBytes;
            }
        }
    }
}

