using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

public enum EncryptionMethod
{
    None,
    One,
    Two,
    Three,
    Four
}

public class FastEncryptionProvider
{
    private readonly uint[] k;
    private const uint d = 0x9e3779b9;
    private const uint r = 32;
    private readonly EncryptionMethod m;

    public FastEncryptionProvider(uint[] key, EncryptionMethod method = EncryptionMethod.One)
    {
        if (method == EncryptionMethod.None)
            throw new ArgumentException("Encryption method cannot be None");
        if (key.Length != 4)
            throw new ArgumentException("Key must be 4 words long");

        k = key;
        m = method;
    }

    public void Decrypt(byte[] buffer, int start, int count)
    {
        if (buffer == null || count == 0)
            return;

        // Process full blocks
        int fullBlockCount = count / 8;
        for (int i = 0; i < fullBlockCount; i++)
        {
            int offset = start + (i * 8);
            DecryptBlock(buffer, offset);
        }

        // Handle leftover bytes
        int leftoverStart = start + (fullBlockCount * 8);
        int leftoverLength = count - (fullBlockCount * 8);
        if (leftoverLength > 0)
        {
            DecryptLeftover(buffer, leftoverStart, leftoverLength);
        }
    }

    private void DecryptBlock(byte[] data, int offset)
    {
        uint v0 = BitConverter.ToUInt32(data, offset);
        uint v1 = BitConverter.ToUInt32(data, offset + 4);
        uint sum = unchecked(d * r);

        for (uint i = 0; i < r; i++)
        {
            uint e = (sum >> 2) & 3;
            v1 -= unchecked((((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + k[(1 & 3) ^ e]));
            sum -= d;
            v0 -= unchecked((((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + k[(0 & 3) ^ e]));
        }

        byte[] v0Bytes = BitConverter.GetBytes(v0);
        byte[] v1Bytes = BitConverter.GetBytes(v1);
        Buffer.BlockCopy(v0Bytes, 0, data, offset, 4);
        Buffer.BlockCopy(v1Bytes, 0, data, offset + 4, 4);
    }

    private void DecryptLeftover(byte[] data, int offset, int length)
    {
        uint sum = unchecked(d * r);
        for (int i = 0; i < length; i++)
        {
            uint e = (sum >> 2) & 3;
            uint p = (uint)i & 3;
            data[offset + i] ^= (byte)(k[p ^ e] >> ((i % 4) * 8));
            sum -= d;
        }
    }
}

public class Osz2Decryptor
{
    private readonly FastEncryptionProvider _encryptor;
    private readonly byte[] _iv;
    private readonly byte[] _bodyHash;
    private static readonly byte[] _knownPlain;

    static Osz2Decryptor()
    {
        _knownPlain = new byte[64];
        var rng = new FastRandom(1990);
        rng.NextBytes(_knownPlain);
    }

    public Osz2Decryptor(byte[] iv, byte[] bodyHash, uint[] key)
    {
        if (key == null || key.Length != 4)
            throw new ArgumentException("Key must be a 4-word array");
        if (iv == null || iv.Length != 16)
            throw new ArgumentException("IV must be a 16-byte array");
        if (bodyHash == null || bodyHash.Length != 16)
            throw new ArgumentException("Body hash must be a 16-byte array");

        _encryptor = new FastEncryptionProvider(key);
        _iv = iv;
        _bodyHash = bodyHash;
    }

    public byte[] DecryptData(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            throw new ArgumentException("Input data is null or empty");

        // Create a copy of the data to work with
        byte[] result = new byte[encryptedData.Length];
        Array.Copy(encryptedData, result, encryptedData.Length);

        // First decode the IV by XORing with body hash
        byte[] decodedIv = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            decodedIv[i] = (byte)(_iv[i] ^ _bodyHash[i]);
        }

        // Then XOR first block with decoded IV
        for (int i = 0; i < Math.Min(16, result.Length); i++)
        {
            result[i] ^= decodedIv[i];
        }

        // Decrypt the data
        _encryptor.Decrypt(result, 0, result.Length);

        // Validate first 64 bytes against known plain text
        if (result.Length >= 64)
        {
            bool matches = true;
            for (int i = 0; i < 64; i++)
            {
                if (result[i] != _knownPlain[i])
                {
                    matches = false;
                    break;
                }
            }
            Console.WriteLine($"Known plain text validation: {(matches ? "PASSED" : "FAILED")}");

            if (matches)
            {
                // Skip known plain text and return the rest
                byte[] actualData = new byte[result.Length - 64];
                Array.Copy(result, 64, actualData, 0, actualData.Length);
                return actualData;
            }
        }

        return result;
    }
}

public class FastRandom
{
    private const uint Y = 842502087, Z = 3579807591, W = 273326509;
    private uint x, y, z, w;

    public FastRandom(int seed)
    {
        x = (uint)seed;
        y = Y;
        z = Z;
        w = W;
    }

    public void NextBytes(byte[] buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
            buffer[i] = (byte)(w & 0xFF);
        }
    }
}

// Main entry point and helper functions
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet-script osz2_decrypt.cs <input_osz2_file>");
            Environment.Exit(1);
        }

        try
        {
            string inputFile = args[0];
            DecryptFile(inputFile);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Decryption failed: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }

    private static void DecryptFile(string inputFile, string outputFile = null)
    {
        using (var inputStream = File.OpenRead(inputFile))
        using (var reader = new BinaryReader(inputStream))
        {
            // Read file header (68 bytes)
            byte[] header = reader.ReadBytes(68);
            if (header.Length != 68)
                throw new InvalidDataException("Invalid header size");

            // Check magic number (EC 48 4F)
            if (header[0] != 0xEC || header[1] != 'H' || header[2] != 'O')
                throw new InvalidDataException("Invalid file header");

            // Get IV and hashes
            byte[] iv = new byte[16];
            Array.Copy(header, 4, iv, 0, 16);

            byte[] hashMeta = new byte[16];
            Array.Copy(header, 20, hashMeta, 0, 16);

            byte[] hashInfo = new byte[16];
            Array.Copy(header, 36, hashInfo, 0, 16);

            byte[] hashBody = new byte[16];
            Array.Copy(header, 52, hashBody, 0, 16);

            // Read metadata
            var metadata = new Dictionary<MapMetaType, string>();
            var rawMetadata = new Dictionary<int, string>();

            // Reset stream position to start of metadata
            inputStream.Position = 68;  // After header

            // Read metadata count
            int count = Read7BitEncodedInt(reader);
            Console.WriteLine($"Found {count} metadata entries");

            for (int i = 0; i < count; i++)
            {
                // Read key as Int16
                short metaKey = reader.ReadInt16();

                // Read string value
                int strLen = Read7BitEncodedInt(reader);
                string value = Encoding.UTF8.GetString(reader.ReadBytes(strLen));

                // Store in raw metadata
                rawMetadata[metaKey] = value;

                // Also store in typed metadata if it matches MapMetaType
                int metaKeyInt = metaKey;  // Cast to int
                if (Enum.IsDefined(typeof(MapMetaType), metaKeyInt))
                {
                    metadata[(MapMetaType)metaKeyInt] = value;
                }
            }

            // Generate key from metadata
            string creator = metadata.GetValueOrDefault(MapMetaType.Creator, "Unknown");
            string beatmapId = rawMetadata.GetValueOrDefault(10001, "0");  // Key 10001 seems to be beatmap ID
            var key = GenerateKey(creator, beatmapId);

            // Read remaining encrypted data
            long encryptedDataStart = inputStream.Position;
            byte[] encryptedData = new byte[inputStream.Length - encryptedDataStart];
            inputStream.Read(encryptedData, 0, encryptedData.Length);

            // Decrypt data
            var decryptor = new Osz2Decryptor(iv, hashBody, key);
            byte[] decryptedData = decryptor.DecryptData(encryptedData);

            // Write to output file
            outDir = Directory.CreateDirectory(File.GetFileNameWithoutExtension(outputFile));
            File.WriteAllBytes(outputFile, decryptedData);
            Console.WriteLine($"Decrypted file saved to: {outputFile}");;
        }
    }

    private static int Read7BitEncodedInt(BinaryReader reader)
    {
        int result = 0;
        int shift = 0;
        byte b;

        do
        {
            if (shift >= 35)
                throw new InvalidDataException("Invalid 7-bit encoded integer");

            b = reader.ReadByte();
            result |= (b & 0x7F) << shift;
            shift += 7;
        }
        while ((b & 0x80) != 0);

        return result;
    }

    private static uint[] GenerateKey(string creator, string beatmapId)
    {
        // Ensure creator and beatmapId are in the correct format
        creator = creator.Trim();
        beatmapId = beatmapId.Trim();

        string keySource = $"{creator}yhxyfjo5{beatmapId}";
        Console.WriteLine($"Using key source: {keySource}");

        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            uint[] key = new uint[4];

            // Convert to little endian
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 4; i++)
                {
                    key[i] = BitConverter.ToUInt32(hashBytes, i * 4);
                }
            }
            else
            {
                // If on a big-endian system, manually convert to little endian
                for (int i = 0; i < 4; i++)
                {
                    key[i] = (uint)(hashBytes[i * 4] | (hashBytes[i * 4 + 1] << 8) | (hashBytes[i * 4 + 2] << 16) | (hashBytes[i * 4 + 3] << 24));
                }
            }

            Console.WriteLine($"Using key: {string.Join(", ", key.Select(k => $"0x{k:X8}"))}");
            return key;
        }
    }
}

// Metadata type enum
public enum MapMetaType
{
    Title = 0,
    Artist = 1,
    Creator = 2,
    Version = 3,
    BeatmapSetID = 4
}
