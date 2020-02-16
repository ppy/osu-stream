using System;
using System.IO;

namespace osum.Helpers
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
        private Stream internalStream;
#else
        private readonly Stream internalStream;
        //private byte[] internalBuffer;
        private byte[] decryptedBuffer;
        private readonly byte[] skipBuffer = new byte[64];
        private readonly FastEncryptionProvider encryptor = new FastEncryptionProvider();
#endif
        private readonly int fOffset;
        private long fPosition;
        
        public MapStream(Stream str, int offset, int length, byte[] iv, byte[] key)
        {
            internalStream = str;

            byte[] data = new byte[4];
            internalStream.Seek(offset, SeekOrigin.Begin);
            internalStream.Read(data, 0, 4);
            internalStream.Position = fOffset = offset + 4;

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

            //fLength = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
            fLength = length - 4;
            fPosition = fOffset;
#else


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
            
            encryptor.Init(uKey, EncryptionMethod.Two);

            byte[] lengthB = { data[0], data[1], data[2], data[3] };
            encryptor.Decrypt(lengthB, 0, 4);
            fLength = lengthB[0] | (lengthB[1] << 8) | (lengthB[2] << 16) | (lengthB[3] << 24);
            fPosition = fOffset;

#if STREAM_DEBUG || SAFE_ENCRYPTION
            decryptedBuffer = new byte[fLength];
            internalStream.Read(decryptedBuffer, 0, fLength);
            //Array.Copy(internalBuffer, 4, decryptedBuffer, 0, fLength);
            encryptor.Decrypt(decryptedBuffer);
            Console.WriteLine("<<<<<<<<<<MAPSTREAM OPENED>>>>>>>>>>>>");
            internalStream.Position = fPosition;
#endif

#endif
        }

        public MapStream(string filename, int offset, int length, byte[] iv, byte[] key)
            : this(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read), offset, length, iv, key)
        {
        }

        ~MapStream()
        {
            Dispose(false);
        }


        public override bool CanRead => !IsDisposed;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => fLength;

        public bool IsDisposed { get; protected set; }

        public override long Position
        {
#if STRONG_ENCRYPTION
            get { return fPosition; }
#else
            get { return fPosition - fOffset; }
#endif
            set 
            {
                
#if !STRONG_ENCRYPTION
                internalStream.Seek(value, SeekOrigin.Begin);
#else
                fPosition = value + fOffset;
#endif
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
            {
                OnStreamClosed(this);
                OnStreamClosed = null;
            }

            base.Close();
        }

        public new void Dispose() //todo: check if we actually want to override here.
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset >= 0)
                        fPosition = Math.Min(offset, fLength) + fOffset;
                    break;

                case SeekOrigin.Current:
                    if ( Position + offset >= 0)
                        fPosition = Math.Min(fPosition + offset - fOffset, fLength) + fOffset;
                    break;

                case SeekOrigin.End:
                    if (fLength + offset >= 0)
                        fPosition = fLength + offset + fOffset;
                    break;
            }
#if !STRONG_ENCRYPTION
            internalStream.Seek(fPosition, SeekOrigin.Begin);
            return Position;
#else
            return fStream.Seek(fOffset + offset, origin) - fOffset;
#endif
            
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            if (disposing)
                internalStream.Dispose();

            base.Dispose(disposing);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // limit count
            if (Position + count > fLength)
                count = fLength - (int) Position;

            if (count == 0)
                return 0;
#if STRONG_ENCRYPTION
            int bytes = fStream.Read(buffer, offset, count);
#elif NO_ENCRYPTION
            int bytes = internalStream.Read(buffer, offset, count);
#elif SAFE_ENCRYPTION
            int bytes = count;
            Array.Copy(decryptedBuffer, Position, buffer, offset, count);
#else
            long rPosition = fPosition - fOffset;
            long  seekablePosition = rPosition & ~0x3FL;
            int skipped = (int) rPosition % 64;
            int bytes = count;
            int seekableBytes = count - (64 - skipped);

            int endLeftover = 0, seekableEnd = 0;
            if (seekableBytes > 0)
            {
                seekableEnd = ((int)rPosition + count) & ~0x3F;
                endLeftover = ((int)rPosition + count) % 64;
                seekableBytes = seekableEnd - (64 - skipped + (int)rPosition);

                if (seekableBytes > 0)
                {
                    //Array.Copy(internalBuffer, fPosition, buffer, offset, count);
                    internalStream.Position = fPosition;
                    internalStream.Read(buffer, offset, count);
                    encryptor.Decrypt(buffer, 64 - skipped + offset, seekableBytes);
                }
            }
            int firstBytes = Math.Min(64,  fLength - (int) seekablePosition);
            //Array.Copy(internalBuffer, seekablePosition + 4, skipBuffer, 0, firstBytes);
            internalStream.Position = seekablePosition + fOffset;
            internalStream.Read(skipBuffer, 0, firstBytes);
            encryptor.Decrypt(skipBuffer, 0, firstBytes);
            Array.Copy(skipBuffer, skipped, buffer, offset, Math.Min(64 - skipped, count));
            if (endLeftover > 0)
            {
                int lastBytes = Math.Min(64, fLength - seekableEnd);
                //Array.Copy(internalBuffer, seekableEnd + 4, skipBuffer, 0, lastBytes);
                internalStream.Position = seekableEnd + fOffset;
                internalStream.Read(skipBuffer, 0, lastBytes);
                encryptor.Decrypt(skipBuffer, 0, lastBytes);
                Array.Copy(skipBuffer, 0, buffer, count - endLeftover + offset, endLeftover);

            }
            internalStream.Position = fPosition;
            //Array.Copy(internalBuffer, fPosition, buffer, offset, count);

#if STREAM_DEBUG
            for (int i = 0; i < count; i++)
            {
                byte byteA = buffer[i + offset];
                byte byteB = decryptedBuffer[i + Position];
                if (byteA != byteB)
                {
                    byte[] bufferA = new byte[count];
                    byte[] bufferB = new byte[count];
                    Array.Copy(buffer, i + offset, bufferA, 0, count - i);
                    Array.Copy(decryptedBuffer, i + Position, bufferB, 0, count - i);
                    System.Diagnostics.Debugger.Break();
                }
            }
#endif
#endif
            Seek(bytes, SeekOrigin.Current);
            return bytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }


        
    }
}