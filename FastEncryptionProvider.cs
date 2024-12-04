using StreamFormatDecryptor;

namespace StreamFormatDecryptor
{

    public class FastEncryptionProvider
    {
        private uint[] k;
        private byte[] kB;
        private const uint d = 0x9e3779b9;
        private const uint r = 32;
        public fEnum.EncryptionMethod m = fEnum.EncryptionMethod.Four;


        /**
         * Key has to be 4 words long
         * Encryption method can't be none
         **/
        public void Init(uint[] pkey, fEnum.EncryptionMethod EM)
        {
            if (EM == fEnum.EncryptionMethod.None)
                throw new ArgumentException("Encryption method can't be none");
            if (pkey.Length != 4)
                throw new ArgumentException("Encryption key has to be 4 words long");

            k = pkey;
            m = EM;
            kB = ConvertUIntArrayToByteArray(pkey);
        }


        private void checkKey()
        {
            if (m == fEnum.EncryptionMethod.Four)
                new ArgumentException("Encryption method has to be set first");
        }


        private void EncryptDecryptTwoSafe(byte[] buffer, bool encrypt, int count, int offset)
        {
            uint fullWordCount = unchecked((uint)count / nMAXBytes);
            uint leftover = unchecked((uint)count) % nMAXBytes;

            n = nMAX;
            uint rounds = 6 + 52 / n;

            byte[] bufferCut = new byte[fullWordCount * nMAXBytes];
            Buffer.BlockCopy(buffer, offset, bufferCut, 0, (int)(fullWordCount * nMAXBytes));
            uint[] bufferCutWords = ConvertByteArrayToUIntArray(bufferCut);

            if (encrypt)
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                    EncryptWordsTwoSafe(bufferCutWords, (int)(wordCount * nMAX));
                }
            else //copy pasta because we dont want to waste time on a cmp each iteration
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                    DecryptWordsTwoSafe(bufferCutWords, (int)(wordCount * nMAX));
                }

            byte[] bufferProcessed = ConvertUIntArrayToByteArray(bufferCutWords);
            Buffer.BlockCopy(bufferProcessed, 0, buffer, offset, (int)(fullWordCount * nMAXBytes));

            n = leftover / 4;
            byte[] leftoverBuffer = new byte[n * 4];
            Buffer.BlockCopy(buffer, (int)(offset + fullWordCount * nMAXBytes), leftoverBuffer, 0, (int)n * 4);
            uint[] leftoverBufferWords = ConvertByteArrayToUIntArray(leftoverBuffer);

            if (n > 1)
            {
                if (encrypt)
                    EncryptWordsTwoSafe(leftoverBufferWords, 0);
                else
                    DecryptWordsTwoSafe(leftoverBufferWords, 0);

                leftover -= n * 4;
                if (leftover == 0)
                    return;
            }

            byte[] leftoverBufferProcessed = ConvertUIntArrayToByteArray(leftoverBufferWords);
            Buffer.BlockCopy(leftoverBufferProcessed, 0, buffer, (int)(offset + fullWordCount * nMAXBytes), (int)n * 4);


            if (encrypt)
                simpleEncryptBytesSafe(buffer, (int)(count - leftover) + offset, count);
            else
                simpleDecryptBytesSafe(buffer, (int)(count - leftover) + offset, count);
        }


        private unsafe void EncryptDecryptTwo(byte* bufferPtr, int bufferLength, bool encrypt)
        {
            uint fullWordCount = unchecked((uint)bufferLength / nMAXBytes);
            uint leftover = unchecked((uint)bufferLength) % nMAXBytes;

            uint* intWordPtr = (uint*)bufferPtr;
            uint na = nMAX;
            n = nMAX;
            uint rounds = 6 + 52 / n;


            if (encrypt)
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                    EncryptWordsTwo(intWordPtr);
                    intWordPtr += nMAX;
                }
            else //copy pasta because we dont want to waste time on a cmp each iteration
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                    // DecryptWordsTwo(intWordPtr);
                    uint y, z, sum;
                    uint p, e;
                    sum = rounds * d;

                    y = intWordPtr[0];
                    do
                    {
                        e = (sum >> 2) & 3;
                        for (p = na - 1; p > 0; p--)
                        {
                            z = intWordPtr[p - 1];
                            y = intWordPtr[p] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^
                                                 ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                        }

                        z = intWordPtr[na - 1];
                        y = intWordPtr[0] -=
                            ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                    } while ((sum -= d) != 0);

                    intWordPtr += nMAX;
                }

            if (leftover == 0)
                return;

            n = leftover / 4;
            if (n > 1)
            {
                if (encrypt)
                    EncryptWordsTwo(intWordPtr);
                else
                    DecryptWordsTwo(intWordPtr);

                leftover -= n * 4;
                if (leftover == 0)
                    return;
            }

            byte* byteWordPtr = bufferPtr;
            byteWordPtr += bufferLength - leftover;
            if (encrypt)
                simpleEncryptBytes(byteWordPtr, unchecked((int)leftover));
            else
                simpleDecryptBytes(byteWordPtr, unchecked((int)leftover));
        }


        private unsafe void EncryptDecryptOne(byte* bufferPtr, byte* resultPtr, int bufferLength, bool encrypt)
        {
            uint fullWordCount = unchecked((uint)bufferLength / 8);
            uint leftover = (uint)(bufferLength % 8);


            uint* intWordPtrB = (uint*)bufferPtr,
                intWordPtrO = (uint*)resultPtr;
            intWordPtrB -= 2;
            intWordPtrO -= 2;
            if (encrypt)
                for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                    EncryptWordOne(intWordPtrB += 2, intWordPtrO += 2);
            else
                for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                    DecryptWordOne(intWordPtrB += 2, intWordPtrO += 2);

            if (leftover == 0)
                return;

            byte* bufferEnd = bufferPtr + bufferLength;
            byte* byteWordPtrB2 = bufferEnd - leftover;
            byte* byteWordPtrO2 = resultPtr + bufferLength - leftover;


            //copy leftover buffer array to result array
            do
            {
                byteWordPtrO2 = byteWordPtrB2++;
                byteWordPtrO2++;
            } while (byteWordPtrB2 != bufferEnd);

            //encrypt / decrypt leftover
            if (encrypt)
                simpleEncryptBytes(byteWordPtrO2 - leftover, unchecked((int)leftover));
            else
                simpleDecryptBytes(byteWordPtrO2 - leftover, unchecked((int)leftover));
        }


        private unsafe void EncryptDecryptHomebrew(byte* bufferPtr, int bufferLength, bool encrypt)
        {
            if (encrypt)
                simpleEncryptBytes(bufferPtr, bufferLength);
            else
                simpleDecryptBytes(bufferPtr, bufferLength);
        }


        private unsafe void EncryptDecrypt(byte* bufferPtr, int bufferLength, bool encrypt)
        {
            //if (buffer.Length % 64 != 0)
            //    throw new ArgumentException("buffer size has to be a multiple of 8");

            switch (m)
            {
                case fEnum.EncryptionMethod.One:
                    EncryptDecrypt(bufferPtr, bufferPtr, bufferLength, encrypt);
                    break;
                case fEnum.EncryptionMethod.Two:
                    EncryptDecryptTwo(bufferPtr, bufferLength, encrypt);
                    break;
                case fEnum.EncryptionMethod.Three:
                    EncryptDecryptHomebrew(bufferPtr, bufferLength, encrypt);
                    break;
                case fEnum.EncryptionMethod.Four:
                    checkKey();
                    break;
            }
        }

        private unsafe void EncryptDecrypt(byte* bufferPtr, byte* outputPtr, int bufferLength, bool encrypt)
        {
            //if (buffer.Length % 64 != 0)
            //throw new ArgumentException("buffer size has to be a multiple of 8");


            switch (m)
            {
                case fEnum.EncryptionMethod.One:
                    EncryptDecryptOne(bufferPtr, outputPtr, bufferLength, encrypt);
                    break;
                case fEnum.EncryptionMethod.Three:
                case fEnum.EncryptionMethod.Two:
                    //Marshal.Copy(new IntPtr(bufferPtr), new IntPtr(outputPtr),)
                    //EncryptDecrypt(encrypted, encrypt);
                    throw new NotSupportedException();
                case fEnum.EncryptionMethod.Four:
                    checkKey();
                    break;
            }
        }

        public unsafe void EncryptDecrypt(byte[] buffer, byte[] output, int bufStart, int outputStart, int count,
            bool encrypt)
        {
#if !SAFE_ENCRYPTION

            fixed (byte* bufferPtr = buffer,
                   outputPtr = output)
            {
                if (outputPtr == null)
                    EncryptDecrypt(bufferPtr + bufStart, count, encrypt);
                else
                    EncryptDecrypt(bufferPtr + bufStart, outputPtr + outputStart, count, encrypt);
            }
#else
            //only Two is ported to managed code, so the encryption method is ignored
            if (output != null)
                throw new NotSupportedException("Custom output is not supported when SAFE_ENCRYPTION is enabled.");
            EncryptDecryptTwoSafe(buffer, encrypt, count, bufStart);
#endif
        }


        /**
         * Will be decrypted from and to the buffer.
         * Fastest if buffer size is a multiple of 8
         * */
        public void Decrypt(byte[] buffer)
        {
            EncryptDecrypt(buffer, null, 0, 0, buffer.Length, false);
        }

        public void Decrypt(byte[] buffer, int start, int count)
        {
            EncryptDecrypt(buffer, null, start, 0, count, false);
        }

        public void Decrypt(byte[] buffer, byte[] output)
        {
            EncryptDecrypt(buffer, output, 0, 0, buffer.Length, false);
        }

        public void Decrypt(byte[] buffer, byte[] output, int bufStart, int outStart, int count)
        {
            EncryptDecrypt(buffer, output, bufStart, outStart, count, false);
        }


        /**
         * Will be encrypted from and to the buffer.
         * Fastest if buffer size is a multiple of 8
         * */
        public void Encrypt(byte[] buffer)
        {
            EncryptDecrypt(buffer, null, 0, 0, buffer.Length, true);
        }

        public void Encrypt(byte[] buffer, int start, int count)
        {
            EncryptDecrypt(buffer, null, start, 0, count, true);
        }

        public void Encrypt(byte[] buffer, byte[] output)
        {
            EncryptDecrypt(buffer, output, 0, 0, buffer.Length, true);
        }

        public void Encrypt(byte[] buffer, byte[] output, int bufStart, int outStart, int count)
        {
            EncryptDecrypt(buffer, output, bufStart, outStart, count, true);
        }

        #region Encrypt Decrypt One

        private unsafe void EncryptWordOne(uint* v /*[2]*/, uint* o /*[2]*/)
        {
            uint i;
            uint v0 = v[0];
            uint v1 = v[1];
            uint sum = 0;
            for (i = 0; i < r; i++)
            {
                //todo: cache sum + k for better speed
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + k[sum & 3]);
                sum += d;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + k[(sum >> 11) & 3]);
            }

            o[0] = v0;
            o[1] = v1;
        }

        private unsafe void DecryptWordOne(uint* v /*[2]*/, uint* o /*[2]*/)
        {
            uint i;
            uint v0 = v[0];
            uint v1 = v[1];
            uint sum = unchecked(d * r);
            for (i = 0; i < r; i++)
            {
                //todo: cache sum + k for better speed
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + k[(sum >> 11) & 3]);
                sum -= d;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + k[sum & 3]);
            }

            o[0] = v0;
            o[1] = v1;
        }

        #endregion

        #region Encrypt Decrypt Word Two

        //represents the number of words to be encrypted/decrypted
        //automaticly set to be 16 unless the buffer is smaller than 16
        //or if buffer%16!=0 n will be changed on the last buffer iteration
        private uint n;
        public const uint nMAX = 16;
        public const uint nMAXBytes = nMAX * 4;


        private void EncryptWordsTwoSafe(uint[] v, int offset)
        {
            uint y, z, sum;
            uint p, e;
            uint rounds = 6 + 52 / n;
            sum = 0;
            z = v[n - 1 + offset];
            do
            {
                sum += d;
                e = (sum >> 2) & 3;
                for (p = 0; p < n - 1; p++)
                {
                    y = v[p + 1 + offset];
                    z = v[p + offset] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                }

                y = v[offset];
                z = v[n - 1 + offset] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            } while (--rounds > 0);
        }

        private void DecryptWordsTwoSafe(uint[] v, int offset)
        {
            uint y, z, sum;
            uint p, e;
            uint rounds = 6 + 52 / n;
            sum = rounds * d;
            y = v[offset];
            do
            {
                e = (sum >> 2) & 3;
                for (p = n - 1; p > 0; p--)
                {
                    z = v[p - 1 + offset];
                    y = v[p + offset] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                }

                z = v[n - 1 + offset];
                y = v[offset] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            } while ((sum -= d) != 0);
        }


        //uses a modified version of Two
        private unsafe void EncryptWordsTwo(uint* v /*[n]*/)
        {
            uint y, z, sum;
            uint p, e;
            uint rounds = 6 + 52 / n;
            sum = 0;
            z = v[n - 1];
            do
            {
                sum += d;
                e = (sum >> 2) & 3;
                for (p = 0; p < n - 1; p++)
                {
                    y = v[p + 1];
                    z = v[p] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                }

                y = v[0];
                z = v[n - 1] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            } while (--rounds > 0);
        }

        private unsafe void DecryptWordsTwo(uint* v /*[n]*/)
        {
            uint y, z, sum;
            uint p, e;
            uint rounds = 6 + 52 / n;
            sum = rounds * d;
            y = v[0];
            do
            {
                e = (sum >> 2) & 3;
                for (p = n - 1; p > 0; p--)
                {
                    z = v[p - 1];
                    y = v[p] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
                }

                z = v[n - 1];
                y = v[0] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            } while ((sum -= d) != 0);
        }

        #endregion

        #region encrypt/decrypt Bytes

        //low security homemade byte encryptor / decryptor.
        //used to encrypt // decrypt small data blocks (<8 bytes) and to encrypt
        //cutoffs(last few bytes) from a buffer (buffer % 8 !=0) which can't be encrypted otherwise

        private unsafe void simpleEncryptBytes(byte* buf, int length)
        {
            fixed (uint* keyI = k)
            {
                byte* keyB = (byte*)keyI;
                byte prevE = 0; // previous encrypted
                for (int i = 0; i < length; i++)
                {
                    buf[i] = unchecked((byte)((buf[i] + (keyB[i % 16] >> 2)) % 256));
                    buf[i] ^= rotateLeft(keyB[15 - i % 16], (byte)((prevE + length - i) % 7));
                    buf[i] = rotateRight(buf[i], (byte)((~(uint)(prevE)) % 7));

                    prevE = buf[i];
                }
            }
        }

        private unsafe void simpleDecryptBytes(byte* buf, int length)
        {
            fixed (uint* keyI = k)
            {
                byte* keyB = (byte*)keyI;
                byte prevE = 0; // previous encrypted
                for (int i = 0; i < length; i++)
                {
                    byte tmpE = buf[i];
                    buf[i] = rotateLeft(buf[i], (byte)((~(uint)(prevE)) % 7));
                    buf[i] ^= rotateLeft(keyB[15 - i % 16], (byte)((prevE + length - i) % 7));
                    buf[i] = unchecked((byte)((buf[i] - (keyB[i % 16] >> 2)) % 256));

                    prevE = tmpE;
                }
            }
        }

        private void simpleEncryptBytesSafe(byte[] buf, int offset, int count)
        {
            byte prevE = 0; // previous encrypted
            for (int i = offset; i < count; i++)
            {
                buf[i] = unchecked((byte)((buf[i] + (kB[i % 16] >> 2)) % 256));
                buf[i] ^= rotateLeft(kB[15 - (i - offset) % 16], (byte)((prevE + count - i - offset) % 7));
                buf[i] = rotateRight(buf[i], (byte)((~(uint)(prevE)) % 7));

                prevE = buf[i];
            }
        }

        private void simpleDecryptBytesSafe(byte[] buf, int offset, int count)
        {
            byte prevE = 0; // previous encrypted
            for (int i = offset; i < count; i++)
            {
                byte tmpE = buf[i];
                buf[i] = rotateLeft(buf[i], (byte)((~(uint)(prevE)) % 7));
                buf[i] ^= rotateLeft(kB[15 - (i - offset) % 16], (byte)((prevE + count - i - offset) % 7));
                buf[i] = unchecked((byte)((buf[i] - (kB[i % 16] >> 2) + 256) % 256));

                prevE = tmpE;
            }
        }


        private static byte rotateLeft(byte val, byte n)
        {
            return (byte)((val << n) | (val >> (8 - n)));
        }

        private static byte rotateRight(byte val, byte n)
        {
            return (byte)((val >> n) | (val << (8 - n)));
        }

        #endregion

        private static byte[] ConvertUIntArrayToByteArray(uint[] input)
        {
            byte[] output = new byte[input.Length * 4];
            Buffer.BlockCopy(input, 0, output, 0, output.Length);
            return output;
        }

        private static uint[] ConvertByteArrayToUIntArray(byte[] input)
        {
            if (input.Length % 4 != 0)
                throw new ArgumentException("Byte array length must be multiple of 4");

            uint[] output = new uint[input.Length / 4];
            Buffer.BlockCopy(input, 0, output, 0, input.Length);
            return output;
        }
    }
}