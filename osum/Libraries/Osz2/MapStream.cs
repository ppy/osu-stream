using System;
using System.IO;
using System.Security.Cryptography;

namespace osu_common.Libraries.Osz2
{
    public class MapStream : Stream
    {
        #region Delegates

        public delegate void MapStreamDelegate(MapStream ms);

        #endregion

        private readonly Aes fAes;

        private readonly int fLength;
        private readonly CryptoStream fStream;
        //private int fOffset;
        private int fPosition;

        public MapStream(string filename, int offset, int length, byte[] iv, byte[] key)
        {
            FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] data = new byte[length];
            file.Seek(offset, SeekOrigin.Begin);
            file.Read(data, 0, length);
            file.Close();

            // create decryptor
            fAes = new AesManaged();
            fAes.Key = key;
            fAes.IV = iv;

            // create stream
            fStream = new CryptoStream(new MemoryStream(data), fAes.CreateDecryptor(), CryptoStreamMode.Read);

            // read length as an int
            fLength = fStream.ReadByte() | (fStream.ReadByte() << 8) | (fStream.ReadByte() << 16) | (fStream.ReadByte() << 24);

            //fOffset = offset + 4;
            fPosition = 0;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return fLength; }
        }

        public override long Position
        {
            get { return fPosition; }
            set { throw new NotSupportedException(); } /*
            get { return fStream.Position - fOffset; }
            set { Seek(value, SeekOrigin.Begin); }
            */
        }

        public event MapStreamDelegate OnStreamClosed;

        public override void Close()
        {
            // clean up decryptor
            fAes.Clear();

            // close streams
            fStream.Close();

            // fire events
            if (OnStreamClosed != null)
                OnStreamClosed(this);

            base.Close();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
            /*
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset > fLength || offset < 0)
                        throw new ArgumentOutOfRangeException("offset");
                    break;

                case SeekOrigin.Current:
                    if (Position + offset > fLength || Position + offset < 0)
                        throw new ArgumentOutOfRangeException("offset");
                    break;

                case SeekOrigin.End:
                    if (offset > 0 || fLength + offset < 0)
                        throw new ArgumentOutOfRangeException("offset");
                    break;
            }

            return fStream.Seek(fOffset + offset, origin) - fOffset;
            */
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // limit count
            if (Position + count > fLength)
                count = fLength - (int) Position;

            int bytes = fStream.Read(buffer, offset, count);
            fPosition += bytes;
            return bytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}