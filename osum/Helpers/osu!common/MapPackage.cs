using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using osu_common.Bancho;
using osu_common.Helpers;
using System.Threading;

namespace osu_common.Libraries.Osz2
{
    public class MapPackage : IDisposable
    {

#if OSUM
        private const string EXT_MAP = "osc";
#else
        private const string EXT_MAP = "osu";
#endif
        private const string EXT_PACKAGE = "osz2";

        private const int F_OFFSET_METADATA = 68;
        private const byte VERSION_EXPORT = 0;
        private static readonly MD5CryptoServiceProvider fHasher = new MD5CryptoServiceProvider();

#if DIST
        //free key is changed for dist builds
        private byte[] KEY = new byte[]
                                        {
                                            185, 24, 168, 9, 27, 145, 191, 243, 17, 75, 95, 163, 168, 123, 198, 56,
                                            105, 29, 68, 74, 214, 62, 2, 142, 146, 28, 90, 167, 243, 85, 17, 96
                                        };
#else
        private byte[] KEY = new byte[]
                                        {
                                            216, 98, 163, 48, 2, 109, 118, 89, 244, 247, 37, 194, 235, 70, 174, 52,
                                            13, 106, 97, 84, 242, 62, 186, 48, 25, 66, 72, 85, 242, 22, 15, 92
                                        };
#endif

        private static readonly byte[] knownPlain = new byte[64];

        private readonly string fFilename;
        private readonly Dictionary<string, FileInfo> fFiles;
        private readonly Dictionary<string, byte[]> fFilesToAdd;
        private long fFilesAddedBytes = 0;
        private readonly byte[] fIV;

        public byte[] hash_meta { get; private set; }
        public byte[] hash_info { get; private set; }
        public byte[] hash_body { get; private set; }

        //private readonly List<string> fMapFiles;
        private readonly SortedDictionary<string, int> fMapIDsFiles;
        private readonly Dictionary<string, DateTime> fFilesToAddDateCreated;
        private readonly Dictionary<string, DateTime> fFilesToAddDateModified;
        private readonly Dictionary<string, bool> fFilesToAddDateEncrypted; 

        private readonly List<MapStream> fMapStreamsOpen;
        private readonly Dictionary<MapMetaType, string> fMetadata;

        /// <summary>
        /// Keep a handle on the .osz2 file itself to prevent writes while the file is open
        /// </summary>
        private FileStream fHandle;

        private bool fClosed;
        private bool fSavable = true;
        private bool fFiledataChanged;
        private bool fMetadataChanged;
        private bool fNotOnDisk;
        private int fOffsetData;
        private int fOffsetFileinfo;

        public int DataOffset
        {
            get { return fOffsetData; }
        }

        public bool NoVideoVersion {get; private set;}
       

        static MapPackage ()
        {
            new FastRandom(1990).NextBytes(knownPlain);
        }

        ~MapPackage()
        {
            Dispose(false);
        }

        private long brOffset = 0; //used to store binaryReader position used to do postprocessing later. 

        

        /// <summary>
        ///
        /// Open a map package file. Integrity checks will be performed. If the file does not exist, an exception will be thrown.
        /// </summary>
        public MapPackage(string filename) : this(filename, null, false, false)
        {
        }

        /// <summary>
        /// Open a map package file. Integrity checks will be performed.
        /// <param name="filename">The path to the package file.</param>
        /// <param name="createIfNotFound">This has no effect if the specified file exists. If the file does not exist and this is true, the file will be created when Save() is called. Otherwise, an exception will be thrown.</param>
        /// </summary>
        public MapPackage(string filename, bool createIfNotFound, bool metadataOnly = false) : this(filename, null, createIfNotFound, metadataOnly)
        {
        }

        /// <summary>
        /// Note: use the factory method instead
        /// Open a map package file. Integrity checks will be performed.
        /// <param name="filename">The path to the package file.</param>
        /// <param name="createIfNotFound">This has no effect if the specified file exists. If the file does not exist and this is true, the file will be created when Save() is called. Otherwise, an exception will be thrown.</param>
        /// </summary>
        public MapPackage(string filename, byte[] key, bool createIfNotFound, bool metadataOnly)
        {
            fNotOnDisk = !File.Exists(filename);
            if (fNotOnDisk && !createIfNotFound)
            {
                throw new IOException("File does not exist.");
            }

            fFilename = filename;
            fMetadata = new Dictionary<MapMetaType, string>();
            fFiles = new Dictionary<string, FileInfo>(StringComparer.CurrentCultureIgnoreCase);
            //fMapFiles = new List<string>();
            fMapIDsFiles = new SortedDictionary<string, int>();
            fFilesToAddDateCreated = new Dictionary<string, DateTime>();
            fFilesToAddDateModified = new Dictionary<string, DateTime>();
            fFilesToAddDateEncrypted = new Dictionary<string, bool>();
            fMapStreamsOpen = new List<MapStream>();
            fFilesToAdd = new Dictionary<string, byte[]>(StringComparer.CurrentCultureIgnoreCase);

            fMetadataChanged = false;
            fFiledataChanged = false;
            fClosed = false;

            if (fNotOnDisk)
            {
                // set with default values
                fOffsetData = 0;
                fOffsetFileinfo = 0;
                using (Aes aes = new AesManaged())
                {
                    aes.GenerateIV();
                    fIV = new byte[aes.IV.Length];
                    Array.Copy(aes.IV, fIV, fIV.Length);
                    aes.Clear();
                }
                return;
            }

            // read data and perform integrity checks
            using (BinaryReader br = new BinaryReader(File.OpenRead(filename)))
            {
                // check magic number
                byte[] magic = br.ReadBytes(3);
                if (magic.Length < 3 || magic[0] != 0xec || magic[1] != 'H' || magic[2] != 'O')
                    throw new IOException("Invalid file.");

                // version
                int version = br.ReadByte();

                // xor'd iv - 'decoded' once file data is read
                fIV = br.ReadBytes(16);

                // read hashes
                hash_meta = br.ReadBytes(16);
                hash_info = br.ReadBytes(16);
                hash_body = br.ReadBytes(16);

                // metadata
                using (MemoryStream memstream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(memstream))
                {
                    int count = br.ReadInt32();
                    writer.Write(count);

                    for (int i = 0; i < count; i++)
                    {
                        short dKey = br.ReadInt16();
                        string dValue = br.ReadString();

                        // check the value is clean and add to dictionary
                        if (Enum.IsDefined(typeof (MapMetaType), (int) dKey))
                            fMetadata.Add((MapMetaType) dKey, dValue);

                        writer.Write(dKey);
                        writer.Write(dValue);
                    }
                    writer.Flush();

                    // check hash
                    byte[] hash = GetOszHash(memstream.ToArray(), count*3, 0xa7);
                    if (!GeneralHelper.CompareByteSequence(hash, hash_meta))
                        throw new IOException("File failed integrity check.");


                    writer.Close();
                }

                //get beatmap(difficulty) data
                int mapCount = br.ReadInt32();
                for (int i = 0; i < mapCount; i++)
                    fMapIDsFiles.Add(br.ReadString(), br.ReadInt32());
                

                //get key
                if (key != null)
                {
                    KEY = key;
                }
                else
                {
                    //no key is given, we'll generate it from the metadata.
#if DIST
                    string seed = (char)0x08 + fMetadata[MapMetaType.Title] + "4390gn8931i" + fMetadata[MapMetaType.Artist];
#else
                    string seed = fMetadata[MapMetaType.Creator] + "yhxyfjo5" + fMetadata[MapMetaType.BeatmapSetID];
#endif
                    KEY = GetMD5Hash(Encoding.ASCII.GetBytes(seed));
                }

                // lock the file from writes
                fHandle = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
              
                //all metadata is now loaded.
                if (metadataOnly)
                    //save brOffset for if postProcessing gets called later
                    brOffset = br.BaseStream.Position; 
                else
                    doPostProcessing(br);
            }
        }

        /// <summary>
        /// Loads additional info besides metadata like FileList and aditional Fileinfo.
        /// Note: calling this when HasPostProcessed is set to true will cause an exception.
        /// </summary>
        public void DoPostProcessing()
        {
            if (HasPostProcessed)
                throw new Exception("Already has been postprocessed");

            using (BinaryReader br = new BinaryReader(File.OpenRead(fFilename)))
            {
                br.BaseStream.Seek(brOffset, SeekOrigin.Begin);
                doPostProcessing(br);
            }

        }

        /// <summary>
        /// Loads additional info besides metadata like FileList and aditional Fileinfo.
        /// </summary>
        /// <param name="br">The binaryReader used to open metadata or a new binaryReader if called from outside of this class.</param>
        private void doPostProcessing(BinaryReader br)
        {
            HasPostProcessed = true;


#if !NO_ENCRYPTION
            //check whether we have the correct key by comparing to a known plain.
            using (FastEncryptorStream decryptor = new FastEncryptorStream(br.BaseStream, EncryptionMethod.One, KEY))
            {
                byte[] decryptedPlain = new byte[64];
                decryptor.Read(decryptedPlain, 0, 64);
                if (!GeneralHelper.CompareByteSequence(decryptedPlain, knownPlain))
                    throw new Exception("Invalid key");

            }
#endif


            //read data and perform integrity checks

            // fileinfo - set offset
            fOffsetFileinfo = (int)br.BaseStream.Position;

            // read and 'decode' length
            int length = br.ReadInt32();
            for (int i = 0; i < 16; i += 2)
                length -= hash_info[i] | (hash_info[i + 1] << 17);

            // read data into memory - decode later
            byte[] fileinfo = br.ReadBytes(length);

            // set global offset to file data
            fOffsetData = (int)br.BaseStream.Position;

            // check body hash + 'decode' iv
            {
                ////byte[] buffer = br.ReadBytes((int)br.BaseStream.Length - fOffsetData);
                //byte[] hash = GetBodyHash(br.BaseStream, (int) ((br.BaseStream.Length - fOffsetData) / 2), 0x9f);
                //for (int i = 0; i < 16; i++)
                //{
                //    if (hash[i] != hash_body[i])
                //        throw new IOException("File failed integrity check.");
                //}

                // 'decode' iv
                for (int i = 0; i < fIV.Length; i++)
                    fIV[i] ^= hash_body[i % 16];
            }

            // decrypt fileinfo
            using (Aes aes = new AesManaged())
            {
                // set up decrypter
                aes.IV = fIV;
                aes.Key = KEY; //TODO: key etc etc

                using (MemoryStream memstream = new MemoryStream(fileinfo))
                
#if STRONG_ENCRYPTION
                using (CryptoStream cstream = new CryptoStream(memstream, aes.CreateDecryptor(), CryptoStreamMode.Read))
#elif NO_ENCRYPTION
                using (Stream cstream = memstream)
#else
                using (Stream cstream = new FastEncryptorStream(memstream, EncryptionMethod.Two, KEY))
#endif
                using (BinaryReader reader = new BinaryReader(cstream))
                {
                    // read the encrypted count
                    int count = reader.ReadInt32();

                    // check hash
                    byte[] hash = GetOszHash(fileinfo, count * 4, 0xd1);
                    if (!GeneralHelper.CompareByteSequence(hash, hash_info))
                        throw new IOException("File failed integrity check.");

                    // add files and offsets to dictionary and list
                    int offset_cur = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.ReadString();
                        byte[] fileHash = reader.ReadBytes(16);
                        DateTime fileDateCreated = DateTime.FromBinary(reader.ReadInt64());
                        DateTime fileDateModified = DateTime.FromBinary(reader.ReadInt64());
                        
                        // get next offset in order to calculate length of file
                        int offset_next;
                        if (i + 1 < count)
                            offset_next = reader.ReadInt32();
                        else
                            offset_next = (int)br.BaseStream.Length - fOffsetData;

                        //if (IsMapFile(name))
                        //    fMapIDsFiles.Add(name);

                        int fileLength = offset_next - offset_cur;

                        //if we're dealing with a video file we check if the video data is correct
                        //if not we pretend it doesn't exist.
                        if (IsVideoFile(name))
                        {
                            bool invalid = false;
                            long oldPos = br.BaseStream.Position;
                            long newPos = fOffsetData + offset_cur + fileLength / 2 - 512 + 4;
                            
                            
                            if (newPos >= br.BaseStream.Length || fileLength < 1024)
                                invalid = true;
                            else
                            {
                                byte[] footData = new byte[1024];
                               
                                byte[] videoData = new byte[fileLength - 4];
                                //decrypt data then check videhash
                                using (MapStream ms = new MapStream(Filename, fOffsetData + offset_cur, fileLength - 4, fIV, KEY))
                                //using (FastEncryptorStream ms = new FastEncryptorStream(br.BaseStream, EncryptionMethod.XXTEA, KEY))
                                {
                                    int halfLength = (fileLength - 4) / 2;
                                    //(data.LongLength / 2) - ((data.LongLength / 2) % 16) - 512 + 16
                                    //ms.Position = fOffsetData + offset_cur + halfLength - (halfLength % 16) - 512 + 16 + 4;
                                    ////ms.Position = fOffsetData + offset_cur + 4;
                                    //br.BaseStream.Position = newPos;
                                    //br.BaseStream.Read(footData, 0, 1024);
                                    //br.BaseStream.Position = oldPos;
                                    //ms.Read(footData, 0, 1024);
                                    ms.Seek(halfLength - (halfLength % 16) - 512 + 16, SeekOrigin.Begin);
                                    ms.Read(footData, 0, 1024);
                                    br.BaseStream.Position = oldPos;

                                    //Array.Copy(videoData, halfLength - (halfLength % 16) - 512 + 16, footData, 0, 1024);

                                }
                                string videoHash = BitConverter.ToString(fHasher.ComputeHash(footData)).Replace("-", "");
                                invalid = !videoHash.Equals(fMetadata[MapMetaType.VideoHash]);
                            }
                            if (invalid)
                            {
                                //hash is invalid so we skip adding the file
                                //we also mark the file as unsavable as it's missing data
                                fSavable = false;
                                NoVideoVersion = true;
                                offset_cur = offset_next;
                                continue;
                            }

                        }

                        fFiles.Add(name, new FileInfo(name, offset_cur, fileLength, fileHash, fileDateCreated, fileDateModified));

                        offset_cur = offset_next;
                    }

                    reader.Close();
                }

                aes.Clear();
            }

            fHandle.Seek(0, SeekOrigin.Begin);

            
        }

        #region Data processing methods

        private void EncryptData(Dictionary<string, byte[]> files, ICryptoTransform encryptor)
        {
            // we are going to modify the collection, so we can't foreach it directly
            string[] keys = new string[files.Keys.Count];
            files.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                using (MemoryStream memstream = new MemoryStream())
#if STRONG_ENCRYPTION
                using (CryptoStream cstream = new CryptoStream(memstream, encryptor, CryptoStreamMode.Write))
#elif NO_ENCRYPTION
                using (Stream cstream = memstream)
#else
                using (Stream cstream = new FastEncryptorStream(memstream,EncryptionMethod.Two,KEY))
#endif
                using (BinaryWriter writer = new BinaryWriter(cstream))
                {
                    // include original length
                    writer.Write(files[key].Length);
                    if (!fFilesToAddDateEncrypted[key])
                        writer.Write(files[key]);
                    else
                        memstream.Write(files[key], 0, files[key].Length);
#if STRONG_ENCRYPTION
                    cstream.FlushFinalBlock();
#else
                    cstream.Flush();
#endif
                    files[key] = memstream.ToArray();

                    writer.Close();
                }
            }
        }

        private static byte[] BytifyData(SortedDictionary<string, byte[]> data)
        {
            using (MemoryStream memstream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memstream))
            {
                foreach (byte[] stuff in data.Values)
                {
                    writer.Write(stuff);
                }

                writer.Flush();

                byte[] buffer = memstream.ToArray();

                writer.Close();

                return buffer;
            }
        }

        private byte[] EncryptFileinfo(SortedDictionary<string, byte[]> files, Dictionary<string, byte[]> filesHashes, 
            Dictionary<string, DateTime> filesTimeCreated, Dictionary<string, DateTime> filesTimeModified, ICryptoTransform encryptor)
        {
            using (MemoryStream memstream = new MemoryStream())
#if STRONG_ENCRYPTION
            using (CryptoStream cstream = new CryptoStream(memstream, encryptor, CryptoStreamMode.Write))
#elif NO_ENCRYPTION
            using (Stream cstream = memstream)
#else
            using (FastEncryptorStream cstream = new FastEncryptorStream(memstream,EncryptionMethod.Two,KEY))
#endif
            using (BinaryWriter writer = new BinaryWriter(cstream))
            {
                // count is encrypted as well
                writer.Write(files.Count);

                int offset = 0;
                foreach (KeyValuePair<string, byte[]> pair in files)
                {
                    writer.Write(offset);
                    writer.Write(pair.Key);
                   
                    //temporarily decrypt it so we can hash
#if NO_ENCRYPTION
                    using (Stream decryptor = new MemoryStream(pair.Value, false))
#else
                    using (FastEncryptorStream decryptor = new FastEncryptorStream(new MemoryStream(pair.Value, false),EncryptionMethod.Two,KEY))
#endif
                    {
                        byte[] decrypted = new byte[pair.Value.Length];
                        decryptor.Read(decrypted,0,pair.Value.Length);
                        byte[] hash = fHasher.ComputeHash(decrypted, 4, pair.Value.Length - 4);
                        writer.Write(hash);
                        writer.Write(filesTimeCreated[pair.Key].ToBinary());
                        writer.Write(filesTimeModified[pair.Key].ToBinary());
                        filesHashes[pair.Key] = hash;
                    }
                    offset += pair.Value.Length;
                }
#if STRONG_ENCRYPTION
                cstream.FlushFinalBlock();
#else
                cstream.Flush();
#endif
                writer.Flush();

                byte[] data = memstream.ToArray();

                cstream.Close();
                writer.Close();

                return data;
            }
        }

        private static byte[] BytifyMetadata(Dictionary<MapMetaType, string> data)
        {
            using (MemoryStream memstream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memstream))
            {
                writer.Write(data.Count);
                foreach (KeyValuePair<MapMetaType, string> pair in data)
                {
                    writer.Write((ushort) pair.Key);
                    //todo: do we want to use empty strings or something else here?
                    writer.Write(pair.Value ?? string.Empty);
                }

                writer.Close();

                return memstream.ToArray();
            }
        }
        
        //todo: give stream instead so we don't copy memory unnecesseraly
        private static byte[] GetOszHash(byte[] buffer, int pos, byte swap)
        {
            try
            {
                buffer[pos] ^= swap;
                byte[] hash = fHasher.ComputeHash(buffer);
                buffer[pos] ^= swap;

                for (int i = 0; i < 8; i++)
                {
                    byte a = hash[i];
                    hash[i] = hash[i + 8];
                    hash[i + 8] = a;
                }

                hash[5] ^= 0x2d;

                //return fHasher.ComputeHash(hash);
                return hash;
            }
            catch (Exception)
            {
                throw new IOException("File failed integrity check.");
            }
        }

        private byte[] GetBodyHash(Stream data, int pos, byte swap)
        {
            byte[] toBeHashed = null;
            long strPos = data.Position;
            long bytesLeft = data.Length - strPos;

            string VideoOffsetS = GetMetadata(MapMetaType.VideoDataOffset);
            if (!string.IsNullOrEmpty(VideoOffsetS))
            {
                long VideoOffset = Convert.ToInt64(VideoOffsetS);
                long VideoLength = Convert.ToInt64(GetMetadata(MapMetaType.VideoDataLength));
                //ignore video data while hashing
                toBeHashed = new byte[bytesLeft - VideoLength];
                data.Read(toBeHashed, 0, (int) (VideoOffset));
                long newPos = data.Position + VideoLength;
                if (newPos >= data.Length)
                {
                    data.Position = newPos;
                    data.Read(toBeHashed, (int)(data.Position - strPos - VideoLength), (int)(data.Length - data.Position));
                }
                pos %= (int)(bytesLeft - VideoLength);
            }
            else
            {
                toBeHashed = new byte[bytesLeft];
                data.Read(toBeHashed, 0, toBeHashed.Length);
            }
            return GetOszHash(toBeHashed, pos, swap);

        }

        public static byte[] GetMD5Hash(byte[] buffer)
        {
            return fHasher.ComputeHash(buffer);
        }

        public static bool IsMapFile(string name)
        {
            return Path.GetExtension(name).ToLower() == "." + EXT_MAP;
        }

        public static bool IsMusicFile(string name)
        {
            string ext = Path.GetExtension(name).ToLower();
            return (ext == ".mp3" || ext == ".ogg" || ext == ".m4a");

        }

        public static bool IsStoryboardFile(string name)
        {
            string ext = Path.GetExtension(name).ToLower();
            return (ext == ".osq" || ext == ".osb");
        }

        public static bool IsPackageFile(string name)
        {
            return Path.GetExtension(name).ToLower() == "." + EXT_PACKAGE;
        }
        public static bool IsVideoFile(string name)
        {
            string ext = Path.GetExtension(name).ToLower();
            return (ext == ".avi" || ext == ".flv" || ext == ".mpg");
        }


        private static string EnsureDirectorySeparatorChar(string dir)
        {
            if (dir[dir.Length - 1] != Path.DirectorySeparatorChar)
                return dir + Path.DirectorySeparatorChar;
            return dir;
        }

        public static MapMetaType GetMetaType(string meta)
        {
            MapMetaType value;
            try
            {
                value = (MapMetaType) Enum.Parse(typeof(MapMetaType), meta, true);
            }
            catch(Exception)
            {
                return MapMetaType.Unknown;
            }
            return value;
        }

        #endregion

        /// <summary>
        /// Checks whether aditional postprocessing has been done after generating metadata.
        /// If false anything but metadata will be unavailable.
        /// </summary>
        public bool HasPostProcessed { get; private set; }
        /// <summary>
        /// Get the absolute path to this file.
        /// </summary>
        public string Filename
        {
            get { return fFilename; }
        }

        /// <summary>
        /// Get a string array containing the filenames of the map files.
        /// </summary>
        public string[] MapFiles
        {
            get
            {
                CheckClosed();
                string[] fMapFiles = new string[fMapIDsFiles.Count];
                fMapIDsFiles.Keys.CopyTo(fMapFiles,0);
                return fMapFiles;
            }
        }

        public string GetMapByID(int id)
        {
            foreach(KeyValuePair<string,int> mapIDPair in fMapIDsFiles)
                if (id == mapIDPair.Value)
                    return mapIDPair.Key;

            return string.Empty;
        }
        public int GetIDByMap(string map)
        {
            //we don't use tryGetValue as int is not nullable
            return fMapIDsFiles.ContainsKey(map)? fMapIDsFiles[map] : -2;
        }

        public void SetMapID(string map, int id)
        {
            if (!fMapIDsFiles.ContainsKey(map))
                throw new Exception("Map does not exist in this mappackage");
#if !DEBUG
            if (fMapIDsFiles.ContainsValue(id) && id != -1 && GetIDByMap(map)!=id)
                throw new Exception("An other map already has this ID set");
#endif

            fMapIDsFiles[map] = id;
        }

        /// <summary>
        /// Check to see if the specified file is within this map package.
        /// </summary>
        public bool FileExists(string filename)
        {
            return fFiles.ContainsKey(filename) || fFilesToAdd.ContainsKey(filename);
        }

        /// <summary>
        /// Get the MD5 hash for each file.
        /// </summary>
        public Dictionary<string, byte[]> GetHashes()
        {
            // force a save
            Save();

            Dictionary<string, byte[]> list = new Dictionary<string, byte[]>();
            foreach (KeyValuePair<string, FileInfo> pair in fFiles)
                list[pair.Key] = pair.Value.Hash;

            return list;


            /*using (FileStream fs = File.OpenRead(fFilename))
            {
                // read files to memory
                fs.Seek(fOffsetData, SeekOrigin.Begin);
                byte[] file = new byte[fs.Length - fOffsetData];
                fs.Read(file, 0, file.Length);

                // get hash for each file
                foreach (KeyValuePair<string, FileInfo> pair in fFiles)
                {
                    byte[] buffer = new byte[pair.Value.Length];
                    Array.Copy(file, pair.Value.Offset, buffer, 0, buffer.Length);
                    list.Add(pair.Key, GetMD5Hash(buffer));
                }
            }*/

            return list;
        }


        public byte[] GetMapHash(string map)
        {
            byte[] buffer;

            using (Stream mapFile = GetFile(map))
            {
                buffer = new byte[mapFile.Length];
                mapFile.Read(buffer, 0, (int)mapFile.Length);
            }

            return GetMD5Hash(buffer);
            
        }
        //only works for saved files.
        public byte[] GetCachedFileHash(string filename)
        {
            FileInfo fileInfo;
            if (!fFiles.TryGetValue(filename, out fileInfo))
                return null;

            return fileInfo.Hash;
        }

        /// <summary>
        /// Get the value of the specified metadata type.
        /// </summary>
        /// <returns>A string representing the specified metadata type, or null if it does not exist.</returns>
        public string GetMetadata(MapMetaType type)
        {
            CheckClosed();

            string o;
            fMetadata.TryGetValue(type, out o);
            return o;
        }


        /// <summary>
        /// Get a Stream to the specified file within this map package.
        /// </summary>
        /// <returns>A Stream that can be used to access the file, or null if the file does not exist.</returns>
        public Stream GetFile(string filename)
        {
            return GetFile(filename, false);
        }

        /// <summary>
        /// Get a Stream to the specified file within this map package.
        /// </summary>
        /// <returns>A Stream that can be used to access the file, or null if the file does not exist.</returns>
        public Stream GetFile(string filename, bool raw)
        {
            CheckClosed();

            Stream stream = null;

            if (fFiles.ContainsKey(filename))
            {
                if (!raw)
                {
                    MapStream ms = new MapStream(fFilename, fOffsetData + fFiles[filename].Offset, fFiles[filename].Length, fIV, KEY);
                    fMapStreamsOpen.Add(ms);
                    ms.OnStreamClosed += MapStream_OnStreamClosed;
                    stream = ms;
                }
                else
                {
                    byte[] file = new byte[fFiles[filename].Length - 4];
                    FileStream fs =  File.Open(fFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    fs.Seek(fOffsetData + fFiles[filename].Offset + 4, SeekOrigin.Begin);
                    fs.Read(file, 0, file.Length);
                    stream = new MemoryStream(file, false);
                }

                
            }
            else if (fFilesToAdd.ContainsKey(filename))
            {
                stream = new MemoryStream(fFilesToAdd[filename], false);
            }

            return stream;
        }


        private void MapStream_OnStreamClosed(MapStream ms)
        {
            if (fClosed)
                return;

            if (fMapStreamsOpen.Contains(ms))
                fMapStreamsOpen.Remove(ms);
        }

        /// <summary>
        /// Add a new metadata item. If the type already exists, it will be replaced.
        /// </summary>
        public void AddMetadata(MapMetaType type, string data)
        {
            CheckClosed();
            fMetadata[type] = data;
            fMetadataChanged = true;
            switch (type)
            {
                case MapMetaType.Artist:
                case MapMetaType.Title:
                    string artist;
                    string title;

                    fMetadata.TryGetValue(MapMetaType.Artist, out artist);
                    fMetadata.TryGetValue(MapMetaType.Title, out title);
                    if (artist == null || title == null)
                        return;
#if DIST
                    string seed = (char)0x08 + fMetadata[MapMetaType.Title] + "4390gn8931i" + fMetadata[MapMetaType.Artist];
#else
                    string seed = fMetadata[MapMetaType.Creator] + "yhxyfjo5" + fMetadata[MapMetaType.BeatmapSetID];
#endif
                    KEY = GetMD5Hash(Encoding.ASCII.GetBytes(seed));

                    break;
            }
        }

        /// <summary>
        /// Remove the specified metadata item.
        /// </summary>
        public void RemoveMetadata(MapMetaType type)
        {
            CheckClosed();

            if (fMetadata.ContainsKey(type))
            {
                fMetadata.Remove(type);
                fMetadataChanged = true;
            }
        }

        /// <summary>
        /// Add all the files in a directory to this map package. If there are name collsions, the old files will be overwritten.
        /// Map packages within the directory will be ignored.
        /// </summary>
        /// <param name="path">The path to this directory on disk.</param>
        /// <param name="recursive">If set to true, this method will search directories within this directory for files to add. The directory structure for these files will be identical to that on disk.</param>
        public void AddDirectory(string path, bool recursive)
        {
            CheckClosed();

            path = EnsureDirectorySeparatorChar(path);

            string[] filenames;
            if (!recursive)
            {
                filenames = Directory.GetFiles(path);
            }
            else
            {
                List<string> files = new List<string>();
                Stack<string> dirs = new Stack<string>();
                dirs.Push(path);

                while (dirs.Count > 0)
                {
                    string dir = dirs.Pop();

                    foreach (string p in Directory.GetFiles(dir))
                        files.Add(p);

                    foreach (string d in Directory.GetDirectories(dir))
                        dirs.Push(d);
                }

                filenames = files.ToArray();
            }

            foreach (string p in filenames)
            {
                if (IsPackageFile(p))
                    continue;

                AddFile(p.Replace(path, ""), p, File.GetCreationTimeUtc(p), File.GetLastWriteTimeUtc(p));
            }

            fFiledataChanged = true;
        }

        /// <summary>
        /// Add a file to this map package. If there is a name collision, the old file will be overwritten.
        /// </summary>
        /// <param name="filename">The internal path used to reference this file.</param>
        /// <param name="path">The path to this file on disk.</param>
        public void AddFile(string filename, string path, DateTime creationTime, DateTime modifiedTime)
        {
            CheckClosed();

            if (IsPackageFile(filename))
                throw new IOException("Cannot add other map packages to a map package.");

            byte[] data;

            using (FileStream fs = File.OpenRead(path))
            {
                data = new byte[fs.Length];
                fs.Read(data, 0, (int) fs.Length);
                fs.Close();
            }

            AddFile(filename, data, creationTime, modifiedTime);

            fFiledataChanged = true;
        }

        /// <summary>
        /// Add a file to this map package. If there is a name collision, the old file will be overwritten.
        /// </summary>
        /// <param name="filename">The internal path used to reference this file.</param>
        /// <param name="data">The contents of the file in a byte array.</param>
        public void AddFile(string filename, byte[] data, DateTime creationTime, DateTime modifiedTime)
        {
            AddFile(filename, data, creationTime, modifiedTime, false);
        }

        /// <summary>
        /// Add a file to this map package. If there is a name collision, the old file will be overwritten.
        /// </summary>
        /// <param name="filename">The internal path used to reference this file.</param>
        /// <param name="data">The contents of the file in a byte array.</param>
        public void AddFile(string filename, byte[] data, DateTime creationTime, DateTime modifiedTime, bool alreadyEncrypted)
        {
            CheckClosed();

            if (IsPackageFile(filename))
                throw new IOException("Cannot add other map packages to a map package.");


            if (IsMapFile(filename) && !fMapIDsFiles.ContainsKey(filename))
                fMapIDsFiles.Add(filename,-1);

            if (IsVideoFile(filename))
            {
                if (data.Length < 1024)
                    throw new IOException("Video needs to be atleast 1024 bytes big");
                long offset = fFilesAddedBytes;
                foreach (KeyValuePair<string, FileInfo> pair in fFiles)
                    offset += pair.Value.Length;
                byte[] footData = new byte[1024];
                Array.Copy(data, (data.LongLength / 2) - ((data.LongLength / 2) % 16) - 512 + 16, footData, 0, 1024);
                //Array.Copy(data, 0, footData, 0, 1024);
                //byte[] videoHash = fHasher.ComputeHash(data, (int) (data.LongLength/2 - 512), 1024);
                byte[] videoHash = fHasher.ComputeHash(footData);
                AddMetadata(MapMetaType.VideoDataOffset, Convert.ToString(offset));
                AddMetadata(MapMetaType.VideoDataLength, Convert.ToString(data.Length));
                AddMetadata(MapMetaType.VideoHash, BitConverter.ToString(videoHash).Replace("-", ""));
                fMetadataChanged = true;

            }

            fFilesToAdd[filename] = data;
            fFilesToAddDateCreated[filename] = creationTime;
            fFilesToAddDateModified[filename] = modifiedTime;
            fFilesToAddDateEncrypted[filename] = alreadyEncrypted;
            fFilesAddedBytes += data.LongLength;

            if (fFiles.ContainsKey(filename))
                fFiles.Remove(filename);



            fFiledataChanged = true;
        }

        /// <summary>
        /// Remove a file from this map package.
        /// </summary>
        public void RemoveFile(string filename)
        {
            CheckClosed();
            //todo: check special video remove case
            bool removed = false;

            if (fFiles.ContainsKey(filename))
            {
                fFiles.Remove(filename);
                removed = true;
            }

            if (fFilesToAdd.ContainsKey(filename))
            {
                fFilesToAdd.Remove(filename);
                fFilesToAddDateCreated.Remove(filename);
                fFilesToAddDateModified.Remove(filename);
                fFilesToAddDateEncrypted.Remove(filename);
                removed = true;
            }

            if (removed)
            {
                fFiledataChanged = true;
                if (fMapIDsFiles.ContainsKey(filename))
                    fMapIDsFiles.Remove(filename);
            }
        }

        /// <summary>
        /// Modify the specified osz2 to match the given file table. Headers WILL be broken.
        /// Existing files will be moved/padded/truncated to the correct length. Space for on-existent files will be padded with zeros.
        /// </summary>
        /// <param name="path">Path to the osz2 that needs to be broken.</param>
        /// <param name="filetable">A list containing the details of the new file table.</param>
        /// <param name="dataoffset">Offset to the file data in the new file (fOffsetData).</param>
        /// <param name="filesize">The filesize of the target mappackage.</param>
        public static void Pad(string path, List<FileInfo> filetable, int dataOffset, long filesize)
        {
            // make sure smallest offset is first
            filetable.Sort((a, b) => a.Offset - b.Offset);


            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                // don't need to write headers
                // hashes will need updating which means the first block will always need a transfer
                bw.Write(new byte[dataOffset]);

                using (MapPackage p = new MapPackage(path))
                using (FileStream f = File.OpenRead(path))
                {

                    int oldDataOffset = p.DataOffset;
                    foreach (FileInfo fi in filetable)
                    {
                        // copy existing data (if it exists) from the old file
                        if (p.fFiles.ContainsKey(fi.Filename))
                        {
                            FileInfo old = p.fFiles[fi.Filename];

                            // write nullblocks
                            long skipSize = fi.Offset - bw.BaseStream.Position;
                            if (skipSize > 0)
                                bw.Write(new byte[skipSize]);


                            // due to encryption, if file lengths don't match, they probably need full update anyway
                            // so in order to speed up the process only copy if lengths are identical
                            //if (old.Length == fi.Length)
                            //{
                                byte[] data = new byte[fi.Length];
                                bw.Seek(fi.Offset + dataOffset, SeekOrigin.Begin);
                                f.Seek(old.Offset + oldDataOffset, SeekOrigin.Begin);
                                f.Read(data, 0, data.Length);
                                bw.Write(data);
                            //}
                        }
                    }
                }

                // overwrite existing file with memorystream contents
                bw.Seek(0, SeekOrigin.Begin);
                using (FileStream f = File.Open(path, FileMode.Create))
                {
                    const int SIZE = 8*1024;
                    byte[] buffer = new byte[SIZE];
                    Stream s = bw.BaseStream;
                    int count;
                    while ((count = s.Read(buffer, 0, SIZE)) > 0)
                    {
                        f.Write(buffer, 0, count);
                    }
                    //we want to have the same filesize as the new mappackage
                    //so we seek to the end of the file and write a byte.
                    if (f.Position != filesize)
                    {
                        //f.Seek(filesize-1, SeekOrigin.Begin);
                        //f.Write(new byte[]{137},0,1 );
                    }
                }
            }
        }

#if !OSUM
        public static bool UpdateFromPatch(string inputFilename, string patchFilename, string outputFilename)
        {
            try
            {
                BSPatcher bsp = new BSPatcher();
                bsp.Patch(inputFilename, outputFilename, patchFilename, Compression.GZip);
            }
            catch
            {
                return false;
            }
            return true;

        }

        public void getVideoOffset (out int offset, out int length)
        {
            CheckClosed();
            offset = -1;
            length = -1;
            List<FileInfo> files = GetFileInfo();
            foreach (FileInfo fi in files)
            {
                if (IsVideoFile(fi.Filename))
                {
                    offset = fi.Offset + DataOffset;
                    length = fi.Length;
                    return;
                }
            }
        }
#endif

        public byte[] getRawHeader()
        {
            CheckClosed();
            byte[] header = new byte[fOffsetData];
            //read uptil fOffsetData
            fHandle.Read(header, 0, fOffsetData);
            fHandle.Position = 0;
            return header;
        }

#if !OSUM
        /// <summary>
        /// Returns a list of FileInfo representing the files in this package. For use with padding.
        /// </summary>
        public List<FileInfo> GetFileInfo()
        {
    
            //Save();

            List<FileInfo> ret = new List<FileInfo>();
            foreach (FileInfo fi in fFiles.Values)
                ret.Add(fi);

            return ret;
        }
#endif

        /// <summary>
        /// Save the current map package to disk.
        /// </summary>
        public bool Save()
        {
            CheckClosed();
            if (!fSavable)
                throw new Exception("Cannot save a beatmap if it's missing videodata");

            // no changes - no need to do disk operations
            if (!fFiledataChanged && !fMetadataChanged)
                return false;

            // all streams to this file must be closed
            if (fMapStreamsOpen.Count != 0)
                throw new IOException("Cannot save while streams are open.");

            if (fFiles.Count + fFilesToAdd.Count == 0)
                throw new IOException("Cannot save an empty package.");

            // release lock on file
            if (fHandle != null)
                fHandle.Close();

            // read current file to memory since we will be overwriting

            byte[] file = null;
            if (fNotOnDisk)
            {
                // force write of everything
                fFiledataChanged = true;
                fMetadataChanged = true;
            }
            else
            {
                file = File.ReadAllBytes(fFilename);
            }

            using (Aes aes = new AesManaged())
            using (BinaryWriter bw = new BinaryWriter(new MemoryStream())) //we write file to memory first
            {

                // set up encryptor
                aes.IV = fIV;
                aes.Key = KEY; //TODO: key stuff

                // header
                bw.Write(new byte[] {0xEC, (byte) 'H', (byte) 'O'});
                bw.Write(VERSION_EXPORT);


                // get encrypted file data
                SortedDictionary<string, byte[]> files = new SortedDictionary<string, byte[]>(new FileComparer());
                Dictionary<string, byte[]> filesHashes = new Dictionary<string, byte[]>();
                Dictionary<string, DateTime> filesDateCreated = new Dictionary<string, DateTime>();
                Dictionary<string, DateTime> filesDateModified = new Dictionary<string, DateTime>();
                

                byte[] file_data;
                if (fFiledataChanged)
                {
                    // create a new Dictionary<string, byte[]> containing all current files
                    foreach (KeyValuePair<string, FileInfo> pair in fFiles)
                    {
                        byte[] data = new byte[pair.Value.Length];
                        Array.Copy(file, fOffsetData + pair.Value.Offset, data, 0, data.Length);
                        files.Add(pair.Key, data);
                        filesDateCreated[pair.Key] = pair.Value.CreationTime;
                        filesDateModified[pair.Key] = pair.Value.ModifiedTime;
                    }

                    EncryptData(fFilesToAdd, aes.CreateEncryptor());
                    foreach (KeyValuePair<string, byte[]> pair in fFilesToAdd)
                    {
                        files.Add(pair.Key, pair.Value );
                        filesDateCreated[pair.Key] = fFilesToAddDateCreated[pair.Key];
                        filesDateModified[pair.Key] = fFilesToAddDateModified[pair.Key];
                    }

                    file_data = BytifyData(files);
                }
                else
                {
                    // read directly from file, skipping hash in order to xor iv
                    file_data = new byte[file.Length - fOffsetData];
                    Array.Copy(file, fOffsetData, file_data, 0, file_data.Length);
                }

                // write iv
                byte[] iv = new byte[fIV.Length];
                for (int i = 0; i < iv.Length; i++)
                    iv[i] = (byte) (fIV[i] ^ file_data[i]);
                bw.Write(iv);

                // hashes inserted here when writing to file

                // process metadata
                byte[] meta_data;
                if (fMetadataChanged || fFiledataChanged)
                {
                    meta_data = BytifyMetadata(fMetadata);
                    bw.Write(meta_data);
                    bw.Write(fMapIDsFiles.Count);
                    foreach (KeyValuePair<string, int> mapID in fMapIDsFiles)
                    {
                        bw.Write(mapID.Key);
                        bw.Write(mapID.Value);
                    }
#if !NO_ENCRYPTION
                    using (FastEncryptorStream encryptor = new FastEncryptorStream(bw.BaseStream, 
                        EncryptionMethod.One, KEY))
                    {
                        encryptor.Write(knownPlain, 0, 64);
                    }
#endif
                }
                else
                {
                    // read directly from file
                    meta_data = new byte[fOffsetFileinfo - F_OFFSET_METADATA];
                    Array.Copy(file, F_OFFSET_METADATA, meta_data, 0, meta_data.Length);

                    bw.Write(meta_data);
                }

                // update offset to fileinfo
                int tOffsetFileinfo = (int) bw.BaseStream.Position;

                // get fileinfo
                byte[] info_data;
                if (fFiledataChanged)
                {
                    info_data = EncryptFileinfo(files, filesHashes, filesDateCreated, filesDateModified, aes.CreateEncryptor());

                    // info hash must be calculated here in order to obfuscate length
                    hash_info = GetOszHash(info_data, files.Count * 4, 0xd1);

                    // hide length of data
                    int l = info_data.Length;
                    for (int i = 0; i < 16; i += 2)
                        l += hash_info[i] | (hash_info[i + 1] << 17);

                    // write to file
                    bw.Write(l);
                    bw.Write(info_data);
                }
                else
                {
                    info_data = new byte[fOffsetData - fOffsetFileinfo];
                    Array.Copy(file, fOffsetFileinfo, info_data, 0, info_data.Length);

                    bw.Write(info_data);
                }

                // update offset to data
                int tOffsetData = (int) bw.BaseStream.Position;

                // write file data
                bw.Write(file_data);
                
                // calculate remaining hashes
                hash_meta = GetOszHash(meta_data, fMetadata.Count * 3, 0xa7);
                hash_body = GetBodyHash(new MemoryStream(file_data, false), file_data.Length / 2, 0x9f);
                
                //TODO: maybe an async write while getting the hash to speed things up
                using (FileStream fs = File.Open(fFilename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    const int blockSize = 1024*8;
                    byte[] buffer = new byte[blockSize];
                    int count;

                    bw.Seek(0, SeekOrigin.Begin);
                    Stream ms = bw.BaseStream;

                    // write the first 20 bytes of header
                    ms.Read(buffer, 0, 20);
                    fs.Write(buffer, 0, 20);

                    // then do hashes
                    fs.Write(hash_meta, 0, 16);
                    fs.Write(hash_info, 0, 16);
                    fs.Write(hash_body, 0, 16);

                    // write the rest of the file
                    while ((count = ms.Read(buffer, 0, blockSize)) > 0)
                    {
                        fs.Write(buffer, 0, count);
                    }
                }

                bw.Close();
                aes.Clear();

                // update internal file structure and status
                if (fFiledataChanged)
                {
                    fFiles.Clear();

                    int offset = 0;
                    foreach (KeyValuePair<string, byte[]> pair in files)
                    {
                        fFiles.Add(pair.Key, new FileInfo(pair.Key, offset, pair.Value.Length, 
                            filesHashes[pair.Key], filesDateCreated[pair.Key], filesDateModified[pair.Key]));
                        offset += pair.Value.Length;
                    }
                }

                fOffsetFileinfo = tOffsetFileinfo; // bad choice of variable names, but w/e
                fOffsetData = tOffsetData;

                fNotOnDisk = false;
                fMetadataChanged = false;
                fFiledataChanged = false;
                fFilesToAddDateCreated.Clear();
                fFilesToAddDateModified.Clear();
                fFilesToAdd.Clear();
            }

            // set lock on file against writes
            fHandle = File.Open(fFilename, FileMode.Open, FileAccess.Read, FileShare.Read);

            return true;
        }

        /// <summary>
        /// Close this package. Accessing this object after it has closed will cause exceptions to be thrown.
        /// </summary>
        public void Close()
        {
            //If the object is already closed, just return gracefully.
            if (fClosed) return;
            
            if (fMapStreamsOpen != null)
                fMapStreamsOpen.ForEach(s => s.Close());
            
            //if(fSavable)
            //    Save();

            if (fIV != null)
                Array.Clear(fIV, 0, fIV.Length);
            //Array.Clear(KEY, 0, KEY.Length); //TODO: key stuff

            if (fFiles != null) fFiles.Clear();
            if (fFilesToAdd != null) fFilesToAdd.Clear();
            if (fFilesToAddDateCreated != null) fFilesToAddDateCreated.Clear();
            if (fFilesToAddDateModified != null) fFilesToAddDateModified.Clear();
            if (fFilesToAddDateEncrypted != null) fFilesToAddDateEncrypted.Clear();
            if (fMetadata != null) fMetadata.Clear();

            if (fHandle != null)
            {
                fHandle.Close();
                fHandle.Dispose();
            }

            fClosed = true;
        }

        object packageLock = new object();
 
        public bool AcquireLock(int timeOut, bool releaseFileLock)
        {
           
            //is safe from deadlocks as the same thread can enter the same object multiple times
            try
            {
                lock (packageLock)
                {
                    if (!Monitor.TryEnter(packageLock, timeOut))
                        return false;
                }
            }
            catch
            {
                return false;
            }

            if (releaseFileLock && fHandle!=null)
            {
                fHandle.Close();
                fHandle = null;
            }
     
            return true;

        }

        public void Unlock()
        {
            if(fHandle == null)
                fHandle = File.Open(fFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            lock(packageLock)
            {
                Monitor.Exit(packageLock);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            //Osz2Factory.CloseMapPackage(this);
            Close();
        }

        private void CheckClosed()
        {
            if (fClosed)
                throw new Exception("Object is closed.");
            /*if (!HasPostProcessed)
                throw new Exception("Object hasn't postprocessed");*/
        }


        public class FileComparer : IComparer<string>
        {

            #region IComparer<string> Members

            public int Compare(string x, string y)
            {
                //ms std implementation sorts items with themself
                if (Object.ReferenceEquals(x, y))
                    return 0;

                if (MapPackage.IsVideoFile(x))
                    return 1;

                if (MapPackage.IsVideoFile(y))
                    return -1;

                return x.CompareTo(y);
            }

            #endregion
        }

    }



    public struct FileInfo : bSerializable
    {
        public string Filename { get; private set;}
        public int Length { get; private set; }
        public int Offset { get; private set; }
        public byte[/*16*/] Hash  { get; private set; }
        public DateTime CreationTime { get; private set; }
        public DateTime ModifiedTime { get; private set; }


        public FileInfo(string filename, int offset, int length, byte[] hash, DateTime creationTime, DateTime modifiedTime) : this()
        {
            Filename = filename;
            Offset = offset;
            Length = length;
            Hash = hash;
            CreationTime = creationTime;
            ModifiedTime = modifiedTime;


        }

        public void ReadFromStream(SerializationReader sr)
        {
            Filename = sr.ReadString();
            Length = sr.ReadInt32();
            Offset = sr.ReadInt32();
            Hash = sr.ReadByteArray();
            CreationTime = (DateTime)sr.ReadObject();
            ModifiedTime = (DateTime)sr.ReadObject();
            

        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(Filename);
            sw.Write(Length);
            sw.Write(Offset);
            sw.WriteByteArray(Hash);
            sw.WriteObject(CreationTime);
            sw.WriteObject(ModifiedTime);
        }
    }

    public enum MapMetaType
    {
        Title,
        Artist,
        Creator,
        Version,
        Source,
        Tags,
        VideoDataOffset,
        VideoDataLength,
        VideoHash,
        BeatmapSetID,
        Genre,
        Language,
        TitleUnicode,
        ArtistUnicode,
        ArtistUrl,
        Unknown = 9999,
        DifficultyRating,
        PreviewPoint,
        ArtistFullName,
    }
}