namespace osu_common.Libraries.NetLib
{
    using System;
    using System.IO;
    using System.Text;

    public class ChunkedStream : Stream
    {
        private long chunkSize;
        private string chunkSizeStr;
        private long chunkWritten;
        private Stream destination;
        private bool isCompleted;
        private bool isReadChunk;
        private long totalWritten;

        public ChunkedStream(Stream destination)
        {
            this.destination = destination;
            this.chunkSizeStr = string.Empty;
            this.isReadChunk = false;
            this.chunkSize = 0L;
            this.chunkWritten = 0L;
            this.isCompleted = false;
            this.totalWritten = 0L;
        }

        public override void Flush()
        {
            this.destination.Flush();
        }

        private string GetChunkSizeStr(byte[] buffer, int offset, int count)
        {
            int num = 0;
            for (int i = offset; i < (offset + count); i++)
            {
                byte num3 = buffer[i];
                if ((((num3 < 0x30) || (num3 > 0x39)) && ((num3 < 0x61) || (num3 > 0x66))) && ((num3 < 0x41) || (num3 > 70)))
                {
                    break;
                }
                num++;
            }
            return Encoding.ASCII.GetString(buffer, offset, num);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        private bool ReadChunkData(byte[] buffer, ref int offset, int count)
        {
            bool flag = false;
            if (this.chunkSize > 0L)
            {
                int num = count - offset;
                if (num > ((int) (this.chunkSize - this.chunkWritten)))
                {
                    num = (int) (this.chunkSize - this.chunkWritten);
                }
                this.destination.Write(buffer, offset, num);
                offset += num;
                this.chunkWritten += num;
            }
            if (this.chunkWritten == this.chunkSize)
            {
                while (offset < count)
                {
                    if (buffer[offset] == 10)
                    {
                        offset++;
                        flag = true;
                        if (this.chunkSize == 0L)
                        {
                            this.isCompleted = true;
                        }
                        return flag;
                    }
                    offset++;
                }
            }
            return flag;
        }

        private bool ReadChunkSize(byte[] buffer, ref int offset, int count)
        {
            int num = offset;
            while (offset < count)
            {
                if (buffer[offset] == 10)
                {
                    this.chunkSizeStr = this.chunkSizeStr + this.GetChunkSizeStr(buffer, num, (offset - num) - 1);
                    if (!StringUtils.IsEmpty(this.chunkSizeStr))
                    {
                        this.chunkSizeStr = "0x" + this.chunkSizeStr;
                    }
                    this.chunkSize = StringUtils.StrToIntDef(this.chunkSizeStr, 0x10, 0);
                    this.chunkSizeStr = string.Empty;
                    this.chunkWritten = 0L;
                    offset++;
                    return true;
                }
                offset++;
            }
            this.chunkSizeStr = this.chunkSizeStr + this.GetChunkSizeStr(buffer, num, offset - num);
            return false;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long num = 0L;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    num = offset;
                    break;

                case SeekOrigin.Current:
                    num = this.totalWritten + offset;
                    break;

                case SeekOrigin.End:
                    num = this.totalWritten - offset;
                    break;
            }
            if (num != this.totalWritten)
            {
                throw new StreamError("Invalid Stream operation");
            }
            return num;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.totalWritten += count;
            do
            {
                if (!this.isReadChunk)
                {
                    this.isReadChunk = this.ReadChunkSize(buffer, ref offset, count);
                }
                if (this.isReadChunk)
                {
                    this.isReadChunk = !this.ReadChunkData(buffer, ref offset, count);
                }
            }
            while ((offset < count) & !this.IsCompleted);
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.isCompleted;
            }
        }

        public override long Length
        {
            get
            {
                return this.totalWritten;
            }
        }

        public override long Position
        {
            get
            {
                return this.Seek(0L, SeekOrigin.Current);
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}

