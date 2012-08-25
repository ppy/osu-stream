using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osu_common.Helpers;

namespace osu_Tencho
{

    public class BufferStack : Stack<Buffer>
    {
        byte[] block;

        int SingleBufferSize;
        int TotalBuffers;

        public BufferStack(int singleBufferSize, int totalBuffers)
            : base(totalBuffers)
        {
            SingleBufferSize = singleBufferSize;
            TotalBuffers = totalBuffers;

            block = new byte[singleBufferSize * totalBuffers];

            for (int i = 0; i < totalBuffers; i++)
                Push(new Buffer(block, i * singleBufferSize, singleBufferSize));
        }

        public new Buffer Pop()
        {
            lock (this)
            {
                Buffer b = base.Pop();
                b.Reset();
                return b;
            }
        }

        public new void Push(Buffer buffer)
        {
            lock (this)
                base.Push(buffer);
        }

    }

    public class Buffer
    {
        public byte[] bBlock;
        public int bOffset;
        public int bSize;

        SerializationReader reader;
        public SerializationReader Reader
        {
            get
            {
                if (reader == null)
                    reader = new SerializationReader(Stream);
                return reader;
            }
        }

        public MemoryStream Stream;

        public Buffer(byte[] block, int offset, int size)
        {
            this.bBlock = block;
            this.bOffset = offset;
            this.bSize = size;

            Stream = new MemoryStream(block, offset, size, true);
        }

        /// <summary>
        /// The offset of the current buffer in the byte[] block *plus* how much has been read from the attached stream.
        /// </summary>
        public int StreamOffset { get { return bOffset + (int)Stream.Position; } }

        internal void Reset()
        {
            Stream.Position = 0;
        }
    }
}
