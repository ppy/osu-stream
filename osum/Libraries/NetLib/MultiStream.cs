namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.IO;

    public class MultiStream : Stream
    {
        private long position;
        private ArrayList streamList = new ArrayList();

        public void AddStream(Stream stream)
        {
            this.streamList.Add(stream);
        }

        public override void Close()
        {
            foreach (Stream stream in this.streamList)
            {
                stream.Close();
            }
            base.Close();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = 0L;
            int num2 = 0;
            int num3 = offset;
            foreach (Stream stream in this.streamList)
            {
                if (this.position < (num + stream.Length))
                {
                    stream.Position = this.position - num;
                    int num4 = stream.Read(buffer, num3, count);
                    num2 += num4;
                    num3 += num4;
                    this.position += num4;
                    if (num4 >= count)
                    {
                        return num2;
                    }
                    count -= num4;
                }
                num += stream.Length;
            }
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long length = this.Length;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.position = offset;
                    break;

                case SeekOrigin.Current:
                    this.position += offset;
                    break;

                case SeekOrigin.End:
                    this.position = length - offset;
                    break;
            }
            if (this.position > length)
            {
                this.position = length;
            }
            else if (this.position < 0L)
            {
                this.position = 0L;
            }
            return this.position;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        }

        public override bool CanRead
        {
            get
            {
                return true;
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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                long num = 0L;
                foreach (Stream stream in this.streamList)
                {
                    num += stream.Length;
                }
                return num;
            }
        }

        public override long Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
    }
}

