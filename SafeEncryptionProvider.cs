using StreamFormatDecryptor;

namespace StreamFormatDecryptor;

public class SafeEncryptionProvider
{
    private uint[] k;
    private byte[] kB;
    private const uint d = 0x9e3779b9;
    private const uint r = 32;
    public fEnum.EncryptionMethod m = fEnum.EncryptionMethod.Four;

    #region Local array converter (Byte <-> Unsigned Integer)

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

    #endregion

    // Key has to be 4 words long and can't be null
    public void Init(uint[] pkey, fEnum.EncryptionMethod em)
    {
        if (em == fEnum.EncryptionMethod.None)
            throw new ArgumentException("Encryption method can't be none");
        if (pkey.Length != 4)
            throw new ArgumentException("Encryption key has to be 4 words long");

        k = pkey;
        m = em;
        kB = ConvertUIntArrayToByteArray(pkey);
    }

    private void CheckKey()
    {
        if (m == fEnum.EncryptionMethod.Four)
            throw new ArgumentException("Encryption method has to be set first");
    }

    #region Simple Bytes Encrypt/Decrypt

    private void SimpleEncryptBytesSafe(byte[] buf, int offset, int count)
    {
        byte prevE = 0; // previous encrypted
        for (int i = offset; i < count; i++)
        {
            buf[i] = unchecked((byte)((buf[i] + (kB[i % 16] >> 2)) % 256));
            buf[i] ^= RotateLeft(kB[15 - (i - offset) % 16], (byte)((prevE + count - i - offset) % 7));
            buf[i] = RotateRight(buf[i], (byte)((~(uint)(prevE)) % 7));

            prevE = buf[i];
        }
    }

    private void SimpleDecryptBytesSafe(byte[] buf, int offset, int count)
    {
        byte prevE = 0; // previous encrypted
        for (int i = offset; i < count; i++)
        {
            byte tmpE = buf[i];
            buf[i] = RotateLeft(buf[i], (byte)((~(uint)(prevE)) % 7));
            buf[i] ^= RotateLeft(kB[15 - (i - offset) % 16], (byte)((prevE + count - i - offset) % 7));
            buf[i] = unchecked((byte)((buf[i] - (kB[i % 16] >> 2) + 256) % 256));

            prevE = tmpE;
        }
    }

    #endregion

    #region Rotation

    private static byte RotateLeft(byte val, byte n)
    {
        return (byte)((val << n) | (val >> (8 - n)));
    }

    private static byte RotateRight(byte val, byte n)
    {
        return (byte)((val >> n) | (val << (8 - n)));
    }

    #endregion

    #region Encryption ONE

    private void EncryptDecryptOneSafe(byte[] bufferArr, byte[] resultArr, int bufferLen, bool isEncrypted)
    {
        uint fullWordCount = unchecked((uint)bufferLen / 8);
        uint leftover = (uint)(bufferLen % 8); //remaining of fullWordCount

        uint[] intWordArrB = ConvertByteArrayToUIntArray(bufferArr),
            intWordArrO = ConvertByteArrayToUIntArray(resultArr);

        intWordArrB -= 2;
        intWordArrO -= 2;

        if (isEncrypted)
            for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                EncryptWordOne(intWordArrB += 2, intWordArrO += 2);
        else
            for (int wordCount = 0; wordCount < fullWordCount; wordCount++)
                DecryptWordOne(intWordArrB += 2, intWordArrO += 2);

        if (leftover == 0) return; // no leftover for me? get lost :c

        byte[] bufferEnd = bufferArr + bufferLen;
        byte[] byteWordArrB2 = bufferEnd - leftover;
        byte[] byteWordArrO2 = resultArr + bufferLen - leftover;

        // copy leftoverBuffer[] -> result[]
        do
        {
            byteWordArrO2 = byteWordArrB2;
            byteWordArrO2++;
        } while (byteWordArrB2 != bufferEnd);

        // deal with leftover
        if (isEncrypted)
            SimpleEncryptBytesSafe(byteWordArrO2 - leftover, 0, unchecked((int)leftover));
        else
            SimpleDecryptBytesSafe(byteWordArrO2 - leftover, 0, unchecked((int)leftover));
    }

    #region Sub-Functions (Encrypt/Decrypt)

    private void EncryptWordOne(uint[] v /*[2]*/, uint[] o /*[2]*/)
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

    private void DecryptWordOne(uint[] v /*[2]*/, uint[] o /*[2]*/)
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

    #endregion

    #region Encryption TWO

    private uint _n;
    public const uint NMax = 16;
    public const uint NMaxBytes = NMax * 4;

    private void EncryptDecryptTwoSafe(byte[] buffer, bool encrypt, int count, int offset)
    {
        uint fullWordCount = unchecked((uint)count / NMaxBytes);
        uint leftover = unchecked((uint)count) % NMaxBytes;

        _n = NMax;
        uint rounds = 6 + 52 / _n;

        byte[] bufferCut = new byte[fullWordCount * NMaxBytes];
        Buffer.BlockCopy(buffer, offset, bufferCut, 0, (int)(fullWordCount * NMaxBytes));
        uint[] bufferCutWords = ConvertByteArrayToUIntArray(bufferCut);

        if (encrypt)
            for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
            {
                EncryptWordsTwoSafe(bufferCutWords, (int)(wordCount * NMax));
            }
        else //copy pasta because we dont want to waste time on a cmp each iteration
            for (uint wordCount = 0; wordCount < fullWordCount; wordCount++)
            {
                DecryptWordsTwoSafe(bufferCutWords, (int)(wordCount * NMax));
            }

        byte[] bufferProcessed = ConvertUIntArrayToByteArray(bufferCutWords);
        Buffer.BlockCopy(bufferProcessed, 0, buffer, offset, (int)(fullWordCount * NMaxBytes));

        _n = leftover / 4;
        byte[] leftoverBuffer = new byte[_n * 4];
        Buffer.BlockCopy(buffer, (int)(offset + fullWordCount * NMaxBytes), leftoverBuffer, 0, (int)_n * 4);
        uint[] leftoverBufferWords = ConvertByteArrayToUIntArray(leftoverBuffer);

        if (_n > 1)
        {
            if (encrypt)
                EncryptWordsTwoSafe(leftoverBufferWords, 0);
            else
                DecryptWordsTwoSafe(leftoverBufferWords, 0);

            leftover -= _n * 4;
            if (leftover == 0)
                return;
        }

        byte[] leftoverBufferProcessed = ConvertUIntArrayToByteArray(leftoverBufferWords);
        Buffer.BlockCopy(leftoverBufferProcessed, 0, buffer, (int)(offset + fullWordCount * NMaxBytes), (int)_n * 4);


        if (encrypt)
            SimpleEncryptBytesSafe(buffer, (int)(count - leftover) + offset, count);
        else
            SimpleDecryptBytesSafe(buffer, (int)(count - leftover) + offset, count);
    }

    #region Sub-Functions (Encrypt/Decrypt)

    private void EncryptWordsTwoSafe(uint[] v, int offset)
    {
        uint y, z, sum;
        uint p, e;
        uint rounds = 6 + 52 / _n;
        sum = 0;
        z = v[_n - 1 + offset];
        do
        {
            sum += d;
            e = (sum >> 2) & 3;
            for (p = 0; p < _n - 1; p++)
            {
                y = v[p + 1 + offset];
                z = v[p + offset] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            }

            y = v[offset];
            z = v[_n - 1 + offset] += ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
        } while (--rounds > 0);
    }

    private void DecryptWordsTwoSafe(uint[] v, int offset)
    {
        uint y, z, sum;
        uint p, e;
        uint rounds = 6 + 52 / _n;
        sum = rounds * d;
        y = v[offset];
        do
        {
            e = (sum >> 2) & 3;
            for (p = _n - 1; p > 0; p--)
            {
                z = v[p - 1 + offset];
                y = v[p + offset] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
            }

            z = v[_n - 1 + offset];
            y = v[offset] -= ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
        } while ((sum -= d) != 0);
    }

    #endregion

    #endregion
    
    #region Encryption HOMEBREW
    private void EncryptDecryptHomebrew(byte[] bufferArr, int bufferLen, bool isEncrypted)
    {
        if (isEncrypted)
            SimpleEncryptBytesSafe(bufferArr,0, bufferLen);
        else
            SimpleDecryptBytesSafe(bufferArr,0, bufferLen); // Unsafe counterparts uses 0 anyways
    }
    #endregion

    #region Encrypt/Decrypt Main
    
    private void EncryptDecryptSafe(byte[] bufferArr, int bufferLen, bool isEncrypted)
    {
        switch (m)
        {
            case fEnum.EncryptionMethod.One:
                EncryptDecryptSafe(bufferArr, bufferArr, bufferLen, isEncrypted);
                break;
            case fEnum.EncryptionMethod.Two:
                EncryptDecryptTwoSafe(bufferArr, bufferLen, isEncrypted);
                break;
            case fEnum.EncryptionMethod.Three:
                EncryptDecryptHomebrew(bufferArr, bufferLen, isEncrypted);
                break;
            case fEnum.EncryptionMethod.Four:
                CheckKey();
                break;

        }
    }

    private void EncryptDecryptSafe(byte[] bufferArr, byte[] outputArr, int bufferLength, bool encrypt)
    {
        switch (m)
        {
            case fEnum.EncryptionMethod.One:
                EncryptDecryptOneSafe(bufferArr, outputArr, bufferLength, encrypt);
                break;
            
            case fEnum.EncryptionMethod.Two:
                throw new NotSupportedException(); //nuh uh
            
            case fEnum.EncryptionMethod.Three: return;
            
            case fEnum.EncryptionMethod.Four:
                CheckKey();
                break;
        }
    }

    public  void EncryptDecrypt(byte[] buffer, byte[] output, int bufStart, int outputStart, int count,
        bool encrypt)
    {
            //only Two is ported to managed code, so the encryption method is ignored
            if (output != null)
                throw new NotSupportedException("Custom output is not supported when SAFE_ENCRYPTION is enabled.");
            EncryptDecryptTwoSafe(buffer, encrypt, count, bufStart);
    }

    #endregion
    
    #region Encrypt/Decrypt Methods

    #region Decrypt

    /**
    * Will be decrypted from and to the buffer.
    * Fastest if buffer size is a multiple of 8
    **/
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
    
    #endregion

    #region Encrypt

    /**
    * Will be encrypted from and to the buffer.
    * Fastest if buffer size is a multiple of 8
    **/
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


    #endregion
    
    #endregion
}