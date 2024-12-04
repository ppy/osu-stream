using System.Diagnostics.CodeAnalysis;

namespace StreamFormatDecryptor
{
    public class fMetadata
    {

        public string? ArtistName { set; get; }
        public string? BeatmapSetID { set; get; }
        public string? Mapper { set; get; }
        public string? SongTitle { set; get; }

        public Dictionary<fEnum.MapMetaType, string> MetaRead;

        /// <summary>
        /// Fetch metadata from a file stream from a .osz2/.osf2 file.
        ///
        /// This method reads the header of the file stream, then reads the
        /// number of metadata items, and then reads each metadata item in a for-loop iteration.
        /// </summary>
        /// <param name="stream">File stream containing the .osz/.osk file.</param>
        /// <returns>Metadata as a tuple of strings: (SongTitle, ArtistName, Mapper, BeatmapSetID)</returns>
        public string[]? Fetcher(FileStream stream)
        {
            if (stream == null || !stream.CanRead)
                throw new ArgumentException("Invalid or unreadable stream provided");

            try
            {
                // Incase this mf forgot to seek to 0
                stream.Position = 0;
                
                using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true);

                ReadHeader(stream);

                // Read metadata count using 7-bit encoded int for better compatibility
                var metadataCount = Read7BitEncodedInt(reader);
                if (metadataCount < 0 || metadataCount > 1000) // Sanity check
                    throw new InvalidDataException($"Invalid metadata count: {metadataCount}");

                MetaRead = ReadMetadata(reader, metadataCount);
                ExtractMetadataValues();

                return new[] { SongTitle ?? "", ArtistName ?? "", Mapper ?? "", BeatmapSetID ?? "" };
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException("Unexpected end of file while reading metadata");
            }
            catch (Exception ex) when (ex is not InvalidDataException)
            {
                throw new InvalidDataException($"Error reading metadata: {ex.Message}", ex);
            }
        }

        public byte[][] ReadHeader(FileStream stream)
        {
            const int HeaderSize = 68; // 3 (magic) + 1 (version) + 16 (iv) + 16 (hashMeta) + 16 (hashInfo) + 16 (hashBody)
            var header = new byte[HeaderSize];
            
            if (stream.Length < HeaderSize)
            {
                throw new InvalidDataException($"File is too small to contain a valid header. File size: {stream.Length} bytes, required: {HeaderSize} bytes.");
            }
            
            // Reset position
            stream.Position = 0;
            
            Console.WriteLine($"Stream length: {stream.Length}");
            Console.WriteLine($"Stream position before read: {stream.Position}");
            
            int bytesRead = stream.Read(header, 0, HeaderSize);

            Console.WriteLine($"Bytes read: {bytesRead}");
            Console.WriteLine($"Stream position after read: {stream.Position}");
            
            if (bytesRead != HeaderSize)
                throw new InvalidDataException($"Invalid header size. Expected {HeaderSize} bytes, but read {bytesRead} bytes.");
            
            // Check magic number (EC 48 4F)
            if (header[0] != 0xEC || header[1] != 0x48 || header[2] != 0x4F)
                throw new InvalidDataException("Invalid magic number in file header");

            Console.WriteLine("Valid .osz2/.osf2 header found");

            var version = header[3];
            Console.WriteLine($"Version byte: {version}");
            if (version > 1) // Accept both version 0 and 1
                throw new InvalidDataException($"Unsupported version: {version}");

            // Extract IV and hashes
            byte[] iv = new byte[16];
            byte[] hashMeta = new byte[16];
            byte[] hashInfo = new byte[16];
            byte[] hashBody = new byte[16];

            Buffer.BlockCopy(header, 4, iv, 0, 16);
            Buffer.BlockCopy(header, 20, hashMeta, 0, 16);
            Buffer.BlockCopy(header, 36, hashInfo, 0, 16);
            Buffer.BlockCopy(header, 52, hashBody, 0, 16);

            Console.WriteLine($"IV: {BitConverter.ToString(iv)}");
            Console.WriteLine($"Hash Meta: {BitConverter.ToString(hashMeta)}");
            Console.WriteLine($"Hash Info: {BitConverter.ToString(hashInfo)}");
            Console.WriteLine($"Hash Body: {BitConverter.ToString(hashBody)}");

            // Store these values if needed for decryption later
             //this._iv = iv;
             //this._hashMeta = hashMeta;
             //this._hashInfo = hashInfo;
             //this._hashBody = hashBody;
             return new[] { iv, hashMeta, hashInfo, hashBody };
        }

        private static int Read7BitEncodedInt(BinaryReader reader)
        {
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift == 35)
                    throw new InvalidDataException("Invalid 7-bit encoded int");
                b = reader.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            }
            while ((b & 0x80) != 0);
            return count;
        }

        private static Dictionary<fEnum.MapMetaType, string> ReadMetadata(BinaryReader reader, int metadataCount)
        {
            var metaRead = new Dictionary<fEnum.MapMetaType, string>(metadataCount);

            int count = 0;
            
            for (var i = 0; i < metadataCount; i++)
            {
                var key = (fEnum.MapMetaType)reader.ReadInt16();
                var value = reader.ReadString();

                metaRead[key] = value;
                //count = Read7BitEncodedInt(reader); //this breaks the metadata fetcher?
            }
            
            Console.WriteLine($"Found {count} metadata entries");
            
            return metaRead;
        }

        private void ExtractMetadataValues()
        {
            // Extract metadata values
            if (MetaRead.TryGetValue(fEnum.MapMetaType.Title, out var songTitle))
                SongTitle = songTitle;
            else
                return;

            if (MetaRead.TryGetValue(fEnum.MapMetaType.Artist, out var artistName))
                ArtistName = artistName;
            else
                return;

            if (MetaRead.TryGetValue(fEnum.MapMetaType.Creator, out var mapper))
                Mapper = mapper;
            else
                return;

            if (MetaRead.TryGetValue(fEnum.MapMetaType.BeatmapSetID, out var beatmapSetId))
                BeatmapSetID = beatmapSetId;
            else
                return;
        }
    }
}