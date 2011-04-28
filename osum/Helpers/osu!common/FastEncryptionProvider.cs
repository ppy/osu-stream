using System;

namespace osu_common.Helpers
{
    public enum EncryptionMethod
    {
        XTEA,
        XXTEA,
        Homebrew,
        None
    }


    //made static as an optimalization. (static method calls are faster)
    public class FastEncryptionProvider
    {

        private uint[] key;
        private const UInt32 DELTA = 0x9e3779b9;
        private const UInt32 ROUNDS = 32;
        public EncryptionMethod CurrentMethod = EncryptionMethod.None;


        /**
         * Key has to be 4 words long
         * Encryption method can't be none
         **/
        public void SetKey (uint[] pkey, EncryptionMethod EM)
        {
            if (EM == EncryptionMethod.None)
                throw new ArgumentException("Encryption method can't be none");
            if (pkey.Length !=4)
                throw new ArgumentException("Encryption key has to be 4 words long");

            key = pkey;
            CurrentMethod = EM;
        }


        private void checkKey()
        {
            if (CurrentMethod == EncryptionMethod.None)
                new ArgumentException("Encryption method has to be set first");
        }

        private unsafe void EncryptDecryptXXTEA(byte* bufferPtr, int bufferLength, bool encrypt)
        {
            uint fullWordCount = unchecked((uint)bufferLength / nMAXBytes);
            uint leftover = unchecked((uint)bufferLength) % nMAXBytes;

            uint* intWordPtr = (uint*) bufferPtr;
            uint na = nMAX;
            n = nMAX;
            UInt32 rounds = 6 + 52 / n;
            
            
            if (encrypt)
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                    EncryptWordsXXTEA(intWordPtr);
                    intWordPtr+=nMAX;
                }
            else //copy pasta because we dont want to waste time on a cmp each iteration
                for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
                {
                   // DecryptWordsXXTEA(intWordPtr);
                    UInt32 y, z, sum;
                    UInt32 p, e;
                    sum = rounds * DELTA;
                    
                    y = intWordPtr[0];
                    do
                    {
                        e = (sum >> 2) & 3;
                        for (p = na - 1; p > 0; p--)
                        {
                            z = intWordPtr[p - 1];
                            y = intWordPtr[p] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z));
                        }
                        z = intWordPtr[na - 1];
                        y = intWordPtr[0] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (key[(p & 3) ^ e] ^ z));
                    }
                    while ((sum -= DELTA) != 0);
                    intWordPtr+=nMAX;
                }

            if (leftover == 0)
                return;

            n = leftover / 4;
            if (n > 1)
            {
                if (encrypt)
                    EncryptWordsXXTEA(intWordPtr);
                else
                    DecryptWordsXXTEA(intWordPtr);

                leftover -= n*4;
                if (leftover == 0)
                    return;
            }

            byte* byteWordPtr = bufferPtr;
            byteWordPtr += bufferLength - leftover;
            if (encrypt)
                simpleEncryptBytes(byteWordPtr, unchecked((int)leftover));
            else
                simpleDecryptBytes(byteWordPtr, unchecked((int)leftover));

            /*uint byteLeftover = leftover % 4;
            if (byteLeftover != 0)
            {
                uint shiftSize = 4 - byteLeftover;
                byte* bufferBytePtr = (byte*)intWordPtr + leftover;
                uint smallWord;
                if (encrypt)
                    for (uint i = 0; i < byteLeftover; i++)
                    {
                        smallWord = (uint*) *(bufferBytePtr + i);
                        EncryptWordsXXTEA(smallWord);
                        (*(bufferBytePtr + i)) = smallWord;
                    }
                else //copy pasta because we dont want to waste speed on a cmp each iteration
                    for (uint i = 0; i < byteLeftover; i++)
                    {
                        smallWord = (uint*)*(bufferBytePtr + i);
                        DecryptWordsXXTEA(smallWord);
                        (*(bufferBytePtr + i)) = smallWord;
                    }

            }*/
            
        }


        private unsafe void EncryptDecryptXTEA(byte* bufferPtr, byte* resultPtr,int bufferLength, bool encrypt)
        {
            uint fullWordCount = unchecked((uint)bufferLength / 8);
            uint leftover = (uint)(bufferLength % 8);


            uint* intWordPtrB = (uint*)bufferPtr,
                  intWordPtrO = (uint*)resultPtr;
            intWordPtrB -= 2;
            intWordPtrO -= 2;
            if (encrypt)
                for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                    EncryptWordXTEA(intWordPtrB+=2, intWordPtrO+=2);
            else 
                for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                    DecryptWordXTEA(intWordPtrB += 2, intWordPtrO+=2);
            
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
                simpleEncryptBytes(byteWordPtrO2 - leftover, unchecked ((int) leftover));
            else
                simpleDecryptBytes(byteWordPtrO2 - leftover, unchecked ((int) leftover));


            

        }


        private unsafe void EncryptDecryptHomebrew(byte* bufferPtr, int bufferLength, bool encrypt)
        {

            if (encrypt)
                simpleEncryptBytes(bufferPtr, bufferLength);
            else
                simpleDecryptBytes(bufferPtr, bufferLength);
        }      


        private unsafe void EncryptDecrypt(byte* bufferPtr,int bufferLength, bool encrypt)
        {
            //if (buffer.Length % 8 != 0)
            //    throw new ArgumentException("buffer size has to be a multiple of 8");

            switch (CurrentMethod)
            {
                case EncryptionMethod.XTEA:
                    EncryptDecrypt(bufferPtr, bufferPtr,bufferLength, encrypt);
                    break;
                case EncryptionMethod.XXTEA:
                    EncryptDecryptXXTEA(bufferPtr,bufferLength, encrypt);
                    break;
                case EncryptionMethod.Homebrew:
                    EncryptDecryptHomebrew(bufferPtr, bufferLength, encrypt);
                    break;
                case EncryptionMethod.None:
                    checkKey();
                    break;
            }

        }

        private unsafe void EncryptDecrypt(byte* bufferPtr, byte* outputPtr, int bufferLength, bool encrypt)
        {
            //if (buffer.Length % 8 != 0)
            //throw new ArgumentException("buffer size has to be a multiple of 8");


            switch (CurrentMethod)
            {
                case EncryptionMethod.XTEA:
                    EncryptDecryptXTEA(bufferPtr, outputPtr, bufferLength, encrypt);
                    break;
                case EncryptionMethod.Homebrew:
                case EncryptionMethod.XXTEA:
                    //Marshal.Copy(new IntPtr(bufferPtr), new IntPtr(outputPtr),)
                    //EncryptDecrypt(encrypted, encrypt);
                    throw new NotSupportedException();
                    break;
                case EncryptionMethod.None:
                    checkKey();
                    break;
            }
        }

        public unsafe void EncryptDecrypt(byte[] buffer, byte[] output, int bufStart, int outputStart, int count, bool encrypt)
        {
            fixed (byte* bufferPtr = buffer,
                outputPtr = output)
            {
                if (outputPtr == null)
                    EncryptDecrypt(bufferPtr + bufStart, count, encrypt);
                else
                    EncryptDecrypt(bufferPtr + bufStart, outputPtr+outputStart, count, encrypt);
            }
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

        #region Encrypt Decrypt XTEA
        unsafe void EncryptWordXTEA( UInt32* v/*[2]*/, UInt32* o/*[2]*/ ) 
        {
            UInt32 i;
            UInt32 v0=v[0];  UInt32 v1=v[1]; 
            UInt32 sum=0;
            for (i=0; i < ROUNDS; i++) 
            {
                //todo: cache sum + k for better speed
                v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
                sum += DELTA;
                v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum>>11) & 3]);
            }
            o[0]=v0; o[1]=v1;
        }
         
        unsafe void DecryptWordXTEA(UInt32* v/*[2]*/, UInt32* o/*[2]*/) 
        {
            UInt32 i;
            UInt32 v0=v[0]; UInt32 v1=v[1];  
            UInt32 sum=unchecked(DELTA*ROUNDS);
            for (i=0; i < ROUNDS; i++) 
            {
                //todo: cache sum + k for better speed
                v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum>>11) & 3]);
                sum -= DELTA;
                v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
            }
            o[0]=v0; o[1]=v1;
        }

        #endregion
        #region Encrypt Decrypt Word XXTEA
        //represents the number of words to be encrypted/decrypted
        //automaticly set to be 16 unless the buffer is smaller than 16
        //or if buffer%16!=0 n will be changed on the last buffer iteration
        private uint n;
        public const uint nMAX = 16;
        public const uint nMAXBytes = nMAX * 4;
        
        //uses a modified version of XXTEA
        unsafe private void EncryptWordsXXTEA(uint* v/*[n]*/) 
        {
            UInt32 y, z, sum;
            UInt32 p, e;
            UInt32 rounds = 6 + 52/n;
            sum = 0;
            z = v[n-1];
            do 
            {
                sum += DELTA;
                e = (sum >> 2) & 3;
                for (p = 0; p < n - 1; p++)
                {
                    y = v[p + 1]; 
                    z = v[p] += ((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (key[(p&3)^e] ^ z));
                }
                y = v[0];
                z = v[n-1] += ((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (key[(p&3)^e] ^ z));
            }   
            while (--rounds>0);
        }

        unsafe private void DecryptWordsXXTEA(uint* v/*[n]*/) 
        {
            UInt32 y, z, sum;
            UInt32 p, e;
            UInt32 rounds = 6 + 52/n;
            sum = rounds*DELTA;
            y = v[0];
            do 
            {
                e = (sum >> 2) & 3;
                for (p=n-1; p>0; p--)
                {
                    z = v[p-1];
                    y = v[p] -= ((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (key[(p&3)^e] ^ z));
                }
                z = v[n-1];
                y = v[0] -= ((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (key[(p&3)^e] ^ z));
            } 
            while ((sum -= DELTA) != 0);

        }
        #endregion 

        #region encrypt/decrypt Bytes
        //low security homemade byte encryptor / decryptor.
        //used to encrypt // decrypt small data blocks (<8 bytes) and to encrypt
        //cutoffs(last few bytes) from a buffer (buffer % 8 !=0) which can't be encrypted otherwise

        private unsafe void simpleEncryptBytes(byte* buf, int length)
        {
            fixed (uint* keyI = key )
            {
                byte* keyB = (byte*) keyI;
                byte prevE = 0; // previous encrypted
                for (int i = 0; i < length; i++)
                {
                    buf[i] = unchecked ((byte) ((buf[i] + (keyB[i%16] >> 2))%256));
                    buf[i] ^= rotateLeft(keyB[16 - i%16], (byte)((prevE + length - i) % 7));
                    buf[i] = rotateRight(buf[i], (byte)((~(uint)(prevE)) % 7));

                    prevE = buf[i];
                }
            }
        }

        private unsafe void simpleDecryptBytes(byte* buf, int length)
        {

            fixed (uint* keyI = key)
            {
                byte* keyB = (byte*) keyI;
                byte prevE = 0; // previous encrypted
                for (int i = 0; i < length; i++)
                {
                    byte tmpE = buf[i];
                    buf[i] = rotateLeft(buf[i], (byte)((~(uint)(prevE)) % 7));
                    buf[i]^= rotateLeft(keyB[16 - i%16], (byte) ((prevE + length - i)%7));
                    buf[i] = unchecked((byte) ((buf[i] - (keyB[i%16] >> 2))%256));

                    prevE = tmpE;
                }
            }
        }

        private static byte rotateLeft(byte val, byte n)
        {
            return (byte) ((val << n) | (val >> (8 - n)));
        }

        private static byte rotateRight(byte val, byte n)
        {
            return (byte)((val >> n) | (val << (8 - n)));
        }
        #endregion

    }
}


