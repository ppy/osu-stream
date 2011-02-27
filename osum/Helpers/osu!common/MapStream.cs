using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using osu_common.Helpers;

namespace osu_common.Libraries.Osz2
{
    public class MapStream : Stream
    {
        #region Delegates

        public delegate void MapStreamDelegate(MapStream ms);

        #endregion



        private readonly int fLength;
#if STRONG_ENCRYPTION
        private readonly Aes fAes;
        private readonly CryptoStream fStream;
#elif NO_ENCRYPTION
        private byte[] internalBuffer;
#else
        private byte[] internalBuffer;
        private FastEncryptionProvider encryptor = new FastEncryptionProvider();
#endif
        //private int fOffset;
        private long fPosition;

        public MapStream(string filename, int offset, int length, byte[] iv, byte[] key)
        {
            FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] data = new byte[length];
            file.Seek(offset, SeekOrigin.Begin);
            file.Read(data, 0, length);
            file.Close();

#if STRONG_ENCRYPTION
            // create decryptor
            fAes = new AesManaged();
            fAes.Key = key;
            fAes.IV = iv;

            // create stream
            fStream = new CryptoStream(new MemoryStream(data), fAes.CreateDecryptor(), CryptoStreamMode.Read);

            // read length as an int
            fLength = fStream.ReadByte() | (fStream.ReadByte() << 8) | (fStream.ReadByte() << 16) | (fStream.ReadByte() << 24);
            fPosition = 0;
#elif NO_ENCRYPTION
            internalBuffer = data;

            using (Stream fStream = new MemoryStream(data))
                fLength = fStream.ReadByte() | (fStream.ReadByte() << 8) | (fStream.ReadByte() << 16) | (fStream.ReadByte() << 24);
            fPosition = 4;
#else

            internalBuffer = data;

            uint[] uKey = new uint[4];
            unsafe
            {
                fixed (byte* keyPtr = key)
                fixed (uint* uKeyPtr = uKey)
                {
                    uint* keyPtrWord = (uint*) keyPtr;
                    uKeyPtr[0] = keyPtrWord[0];
                    uKeyPtr[1] = keyPtrWord[1];
                    uKeyPtr[2] = keyPtrWord[2];
                    uKeyPtr[3] = keyPtrWord[3];
                }
            }
            
            encryptor.SetKey(uKey, EncryptionMethod.XXTEA);

            byte[] lengthB = new byte[] { data[0], data[1], data[2], data[3] };
            encryptor.Decrypt(lengthB, 0, 4);
            fLength = lengthB[0] | (lengthB[1] << 8) | (lengthB[2] << 16) | (lengthB[3] << 24);
            fPosition = 4;
#endif

#if DEBUG
            Console.WriteLine("<<<<<<<<<<MAPSTREAM OPENED>>>>>>>>>>>>");
#endif
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
#if STRONG_ENCRYPTION
            get { return fPosition; }
#else
            get { return fPosition - 4; }
#endif
            set
            {
                if (value % 4 > 0)
                    throw new Exception("fPosition may only be a multiple of 4 bytes");
                fPosition = value + 4;
            }

            /*
            get { return fStream.Position - fOffset; }
            set { Seek(value, SeekOrigin.Begin); }
            */
        }

        public event MapStreamDelegate OnStreamClosed;

        public override void Close()
        {
#if STRONG_ENCRYPTION
            // clean up decryptor
            fAes.Clear();

            // close streams
            fStream.Close();
#endif
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
                count = fLength - (int)Position;
#if STRONG_ENCRYPTION
            int bytes = fStream.Read(buffer, offset, count);
#elif NO_ENCRYPTION
            int bytes = count;
            Array.Copy(internalBuffer, fPosition, buffer, offset, count);
#else
            int bytes = count;
            Array.Copy(internalBuffer,fPosition,buffer,offset,count);
            encryptor.Decrypt(buffer, offset, count);
#endif
            fPosition += bytes;
            return bytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}