using System.IO;

namespace osum.Helpers
{
    public class FastEncryptorStream : Stream
    {

        public static bool instanceActive { get; private set; }
        public Stream internalStream{ get; }
        public bool isClosed { get; private set; }
        private readonly FastEncryptionProvider FastEncryptionProvider = new FastEncryptionProvider();
        /// <summary>
        /// Wraps around the fastEncryptionProvider and the given stream and rebounds data.
        /// </summary>
        /// <param name="InternalStream"></param>
        /// <param name="EM">It's recommended to use XXTEA, it's the most secure and fastest method.</param>
        /// <param name="Key">Key has to be 4 words long</param>
        /// <exception cref="T:System.AccessViolationException">No access is granted when an instance of this class is already active.</exception>
        public FastEncryptorStream (Stream InternalStream, EncryptionMethod EM, uint[] Key)
        {
            internalStream = InternalStream;
            FastEncryptionProvider.Init(Key,EM);
        }

        /// <summary>
        /// Wraps around the fastEncryptionProvider and the given stream and rebounds data.
        /// </summary>
        /// <param name="InternalStream"></param>
        /// <param name="EM">It's recommended to use XXTEA, it's the most secure and fastest method.</param>
        /// <param name="Key">Key has to be 16 bytes big</param>
        /// <exception cref="T:System.AccessViolationException">No access is granted when an instance of this class is already active.</exception>
        public FastEncryptorStream(Stream InternalStream, EncryptionMethod EM, byte[] Key)
        {
            uint[] uKey = new uint[4];
            unsafe
            {
                fixed (byte* keyPtr = Key)
                fixed (uint* uKeyPtr = uKey)
                {
                    uint* keyPtrWord = (uint*) keyPtr;
                    uKeyPtr[0] = keyPtrWord[0];
                    uKeyPtr[1] = keyPtrWord[1];
                    uKeyPtr[2] = keyPtrWord[2];
                    uKeyPtr[3] = keyPtrWord[3];
                }
            }
            internalStream = InternalStream;
            FastEncryptionProvider.Init(uKey, EM);
        }

        ~FastEncryptorStream ()
        {
            Close();
        }


        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><filterpriority>2</filterpriority>
        public override void Flush()
        {
            internalStream.Flush();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. 
        ///                 </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. 
        ///                 </param><exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return internalStream.Seek(offset, origin);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. 
        ///                 </param><exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            internalStream.SetLength(value);
        }

        /// <summary>
        /// Decrypts a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. 
        ///                 </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. 
        ///                 </param><param name="count">The maximum number of bytes to be read from the current stream. 
        ///                 </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. 
        ///                 </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
        ///                 </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
        ///                 </exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {

            int sizeRead = internalStream.Read(buffer, offset, count);
            FastEncryptionProvider.Decrypt(buffer,offset, count);
            return sizeRead;

        }

        /// <summary>
        /// Encrypts a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. 
        ///                 </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. 
        ///                 </param><param name="count">The number of bytes to be written to the current stream. 
        ///                 </param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. 
        ///                 </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. 
        ///                 </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. 
        ///                 </exception><exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support writing. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] bufferCopy = new byte[buffer.Length];
            buffer.CopyTo(bufferCopy, 0);
            FastEncryptionProvider.Encrypt(bufferCopy, offset, count);
            internalStream.Write(bufferCopy, offset, count);
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead => internalStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek => internalStream.CanSeek;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite => internalStream.CanWrite;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Length => internalStream.Length;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. 
        ///                 </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. 
        ///                 </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. 
        ///                 </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get => internalStream.Position;
            set => internalStream.Position = value;
        }


        /// <summary>
        /// Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Close()
        {
            if (!isClosed)
            {
                //internalStream.Close();
                instanceActive = false;
                isClosed = true;
            }
        }


    }
}
