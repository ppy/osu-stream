using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace osu_common.Libraries.Osz2
{
    public class MapPackage : IDisposable
    {
        private const string EXT_MAP = "osu";
        private const string EXT_PACKAGE = "osz2";
        private const int F_OFFSET_METADATA = 68;
        private const byte VERSION_EXPORT = 0;
        private static readonly MD5CryptoServiceProvider fHasher = new MD5CryptoServiceProvider();
        private static readonly byte[] KEY = new byte[] {2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6, 2, 4, 8, 6};

        private readonly string fFilename;
        private readonly Dictionary<string, FileInfo> fFiles;
        private readonly Dictionary<string, byte[]> fFilesToAdd;
        private readonly byte[] fIV;

        public byte[] hash_meta { get; private set; }
        public byte[] hash_info { get; private set; }
        public byte[] hash_body { get; private set; }

        private readonly List<string> fMapFiles;

        private readonly List<MapStream> fMapStreamsOpen;
        private readonly Dictionary<MapMetaType, string> fMetadata;

        /// <summary>
        /// Keep a handle on the .osz2 file itself to prevent writes while the file is open
        /// </summary>
        private FileStream fHandle;

        private bool fClosed;
        private bool fFiledataChanged;
        private bool fMetadataChanged;
        private bool fNotOnDisk;
        private int fOffsetData;
        private int fOffsetFileinfo;

        public int DataOffset
        {
            get { return fOffsetData; }
        }

        private long brOffset = 0; //used to store binaryReader position used to do postprocessing later. 


        /// <summary>
        /// Open a map package file. Integrity checks will be performed. If the file does not exist, an exception will be thrown.
        /// </summary>
        public MapPackage(string filename) : this(filename, false, false)
        {
        }

        /// <summary>
        /// Open a map package file. Integrity checks will be performed.
        /// <param name="filename">The path to the package file.</param>
        /// <param name="createIfNotFound">This has no effect if the specified file exists. If the file does not exist and this is true, the file will be created when Save() is called. Otherwise, an exception will be thrown.</param>
        /// </summary>
        public MapPackage(string filename, bool createIfNotFound) : this(filename, createIfNotFound, false)
        {
        }

        /// <summary>
        /// Open a map package file. Integrity checks will be performed.
        /// <param name="filename">The path to the package file.</param>
        /// <param name="createIfNotFound">This has no effect if the specified file exists. If the file does not exist and this is true, the file will be created when Save() is called. Otherwise, an exception will be thrown.</param>
        /// </summary>
        public MapPackage(string filename, bool createIfNotFound, bool metadataOnly)
        {
            fNotOnDisk = !File.Exists(filename);
            if (fNotOnDisk && !createIfNotFound)
            {
                throw new IOException("File does not exist.");
            }

            fFilename = filename;
            fMetadata = new Dictionary<MapMetaType, string>();
            fFiles = new Dictionary<string, FileInfo>();
            fMapFiles = new List<string>();
            fMapStreamsOpen = new List<MapStream>();
            fFilesToAdd = new Dictionary<string, byte[]>();

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

            // lock the file from writes
            fHandle = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            // read data and perform integrity checks
            using (BinaryReader br = new BinaryReader(File.OpenRead(filename)))
            {
                // check magic number
                byte[] magic = br.ReadBytes(3);
                if (magic[0] != 0xec || magic[1] != 'H' || magic[2] != 'O')
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
                        short key = br.ReadInt16();
                        string value = br.ReadString();

                        // check the value is clean and add to dictionary
                        if (Enum.IsDefined(typeof (MapMetaType), (int) key))
                            fMetadata.Add((MapMetaType) key, value);

                        writer.Write(key);
                        writer.Write(value);
                    }
                    writer.Flush();

                    // check hash
                    byte[] hash = GetOszHash(memstream.ToArray(), count*3, 0xa7);
                    for (int i = 0; i < 16; i++)
                    {
                        if (hash[i] != hash_meta[i])
                            throw new IOException("File failed integrity check.");
                    }

                    writer.Close();
                }
                
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

            BinaryReader br = new BinaryReader(File.OpenRead(fFilename));
            br.BaseStream.Seek(brOffset, SeekOrigin.Begin);
            doPostProcessing(br);
            br.Close();
        }

        /// <summary>
        /// Loads additional info besides metadata like FileList and aditional Fileinfo.
        /// </summary>
        /// <param name="br">The binaryReader used to open metadata or a new binaryReader if called from outside of this class.</param>
        private void doPostProcessing(BinaryReader br)
        {
            HasPostProcessed = true;

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
                byte[] buffer = br.ReadBytes((int)br.BaseStream.Length - fOffsetData);
                byte[] hash = GetOszHash(buffer, buffer.Length / 2, 0x9f);
                for (int i = 0; i < 16; i++)
                {
                    if (hash[i] != hash_body[i])
                        throw new IOException("File failed integrity check.");
                }

                // 'decode' iv
                for (int i = 0; i < fIV.Length; i++)
                    fIV[i] ^= buffer[i];
            }

            // decrypt fileinfo
            using (Aes aes = new AesManaged())
            {
                // set up decrypter
                aes.IV = fIV;
                aes.Key = KEY; //TODO: key etc etc

                using (MemoryStream memstream = new MemoryStream(fileinfo))
                using (BinaryReader reader = new BinaryReader(new CryptoStream(memstream, aes.CreateDecryptor(), CryptoStreamMode.Read)))
                {
                    // read the encrypted count
                    int count = reader.ReadInt32();

                    // check hash
                    byte[] hash = GetOszHash(fileinfo, count * 4, 0xd1);
                    for (int i = 0; i < 16; i++)
                    {
                        if (hash[i] != hash_info[i])
                            throw new IOException("File failed integrity check.");
                    }

                    // add files and offsets to dictionary and list
                    int offset_cur = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.ReadString();

                        // get next offset in order to calculate length of file
                        int offset_next;
                        if (i + 1 < count)
                            offset_next = reader.ReadInt32();
                        else
                            offset_next = (int)br.BaseStream.Length - fOffsetData;

                        if (IsMapFile(name))
                            fMapFiles.Add(name);

                        fFiles.Add(name, new FileInfo(name, offset_cur, offset_next - offset_cur));

                        offset_cur = offset_next;
                    }

                    reader.Close();
                }

                aes.Clear();
            }

            
        }

        #region Data processing methods

        private static void EncryptData(Dictionary<string, byte[]> files, ICryptoTransform encryptor)
        {
            // we are going to modify the collection, so we can't foreach it directly
            string[] keys = new string[files.Keys.Count];
            files.Keys.CopyTo(keys, 0);

            foreach (string key in keys)
            {
                using (MemoryStream memstream = new MemoryStream())
                using (CryptoStream cstream = new CryptoStream(memstream, encryptor, CryptoStreamMode.Write))
                using (BinaryWriter writer = new BinaryWriter(cstream))
                {
                    // include original length
                    writer.Write(files[key].Length);
                    writer.Write(files[key]);
                    cstream.FlushFinalBlock();

                    files[key] = memstream.ToArray();

                    writer.Close();
                }
            }
        }

        private static byte[] BytifyData(Dictionary<string, byte[]> data)
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

        private static byte[] EncryptFileinfo(Dictionary<string, byte[]> files, ICryptoTransform encryptor)
        {
            using (MemoryStream memstream = new MemoryStream())
            using (CryptoStream cstream = new CryptoStream(memstream, encryptor, CryptoStreamMode.Write))
            using (BinaryWriter writer = new BinaryWriter(cstream))
            {
                // count is encrypted as well
                writer.Write(files.Count);

                int offset = 0;
                foreach (KeyValuePair<string, byte[]> pair in files)
                {
                    writer.Write(offset);
                    writer.Write(pair.Key);
                    offset += pair.Value.Length;
                }

                cstream.FlushFinalBlock();
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
                    writer.Write(pair.Value);
                }

                writer.Close();

                return memstream.ToArray();
            }
        }

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

                return fHasher.ComputeHash(hash);
            }
            catch (Exception)
            {
                throw new IOException("File failed integrity check.");
            }
        }

        private static byte[] GetMD5Hash(byte[] buffer)
        {
            return fHasher.ComputeHash(buffer);
        }

        private static bool IsMapFile(string name)
        {
            return Path.GetExtension(name).ToLower() == "." + EXT_MAP;
        }

        private static bool IsPackageFile(string name)
        {
            return Path.GetExtension(name).ToLower() == "." + EXT_PACKAGE;
        }

        private static string EnsureDirectorySeparatorChar(string dir)
        {
            if (dir[dir.Length - 1] != Path.DirectorySeparatorChar)
                return dir + Path.DirectorySeparatorChar;
            return dir;
        }

        public static MapMetaType GetMetaType(string meta)
        {
            switch (meta.ToLower())
            {
                case "title":
                    return MapMetaType.Title;
                case "artist":
                    return MapMetaType.Artist;
                case "creator":
                    return MapMetaType.Creator;
                case "version":
                    return MapMetaType.Version;
                case "source":
                    return MapMetaType.Source;
                case "tags":
                    return MapMetaType.Tags;
                default:
                    return MapMetaType.Unknown;
            }
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
                return fMapFiles.ToArray();
            }
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

            using (FileStream fs = File.OpenRead(fFilename))
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
            }

            return list;
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
            CheckClosed();

            Stream stream = null;

            if (fFiles.ContainsKey(filename))
            {
                // offset needs +16 to skip hash
                MapStream ms = new MapStream(fFilename, fOffsetData + fFiles[filename].Offset, fFiles[filename].Length, fIV, KEY); //TODO: fix key
                fMapStreamsOpen.Add(ms);
                ms.OnStreamClosed += MapStream_OnStreamClosed;
                stream = ms;
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

            if (fMetadata.ContainsKey(type))
            {
                // if it's the same value, don't need to write anything
                if (fMetadata[type] == data)
                    return;

                fMetadata[type] = data;
            }
            else
            {
                fMetadata.Add(type, data);
            }

            fMetadataChanged = true;
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

                AddFile(p.Replace(path, ""), p);
            }

            fFiledataChanged = true;
        }

        /// <summary>
        /// Add a file to this map package. If there is a name collision, the old file will be overwritten.
        /// </summary>
        /// <param name="filename">The internal path used to reference this file.</param>
        /// <param name="path">The path to this file on disk.</param>
        public void AddFile(string filename, string path)
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

            AddFile(filename, data);

            fFiledataChanged = true;
        }

        /// <summary>
        /// Add a file to this map package. If there is a name collision, the old file will be overwritten.
        /// </summary>
        /// <param name="filename">The internal path used to reference this file.</param>
        /// <param name="data">The contents of the file in a byte array.</param>
        public void AddFile(string filename, byte[] data)
        {
            CheckClosed();

            if (IsPackageFile(filename))
                throw new IOException("Cannot add other map packages to a map package.");

            if (IsMapFile(filename) && !fMapFiles.Contains(filename))
                fMapFiles.Add(filename);

            if (fFilesToAdd.ContainsKey(filename))
            {
                fFilesToAdd[filename] = data;
            }
            else
            {
                fFilesToAdd.Add(filename, data);
            }

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

            bool removed = false;

            if (fFiles.ContainsKey(filename))
            {
                fFiles.Remove(filename);
                removed = true;
            }

            if (fFilesToAdd.ContainsKey(filename))
            {
                fFilesToAdd.Remove(filename);
                removed = true;
            }

            if (removed)
            {
                fFiledataChanged = true;
                if (fMapFiles.Contains(filename))
                    fMapFiles.Remove(filename);
            }
        }

        /// <summary>
        /// Modify the specified osz2 to match the given file table. Headers WILL be broken.
        /// Existing files will be moved/padded/truncated to the correct length. Space for on-existent files will be padded with zeros.
        /// </summary>
        /// <param name="path">Path to the osz2 that needs to be broken.</param>
        /// <param name="filetable">A list containing the details of the new file table.</param>
        /// <param name="dataoffset">Offset to the file data in the new file (fOffsetData).</param>
        public static void Pad(string path, List<FileInfo> filetable, int dataoffset)
        {
            // make sure smallest offset is first
            filetable.Sort((a, b) => a.Offset - b.Offset);

            // read file data from old file
            byte[] file = null;

            using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
            {
                // don't need to write headers
                // hashes will need updating which means the first block will always need a transfer
                byte[] data = new byte[dataoffset];
                bw.Write(data);

                using (MapPackage p = new MapPackage(path))
                {
                    foreach (FileInfo fi in filetable)
                    {
                        // start with a blank buffer
                        data = new byte[fi.Length];

                        // copy existing data (if it exists)
                        if (p.fFiles.ContainsKey(fi.Filename))
                        {
                            FileInfo old = p.fFiles[fi.Filename];

                            // due to encryption, if file lengths don't match, they probably need full update anyway
                            // so in order to speed up the process only copy if lengths are identical
                            if (old.Length == fi.Length)
                            {
                                // read old data only when necessary
                                if (file == null)
                                {
                                    using (FileStream f = File.OpenRead(path))
                                    {
                                        file = new byte[f.Length - p.fOffsetData];
                                        f.Seek(p.fOffsetData, SeekOrigin.Begin);
                                        f.Read(file, 0, file.Length);
                                    }
                                }

                                Array.Copy(file, old.Offset, data, 0, data.Length);
                            }
                        }

                        bw.Write(data);
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
                }
            }
        }

        /// <summary>
        /// Returns a list of FileInfo representing the files in this package. For use with padding.
        /// </summary>
        public List<FileInfo> GetFileInfo()
        {
            Save();

            List<FileInfo> ret = new List<FileInfo>();
            foreach (FileInfo fi in fFiles.Values)
                ret.Add(fi);

            return ret;
        }

        /// <summary>
        /// Save the current map package to disk.
        /// </summary>
        public bool Save()
        {
            CheckClosed();

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
                Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
                byte[] file_data;
                if (fFiledataChanged)
                {
                    // create a new Dictionary<string, byte[]> containing all current files
                    foreach (KeyValuePair<string, FileInfo> pair in fFiles)
                    {
                        byte[] data = new byte[pair.Value.Length];
                        Array.Copy(file, fOffsetData + pair.Value.Offset, data, 0, data.Length);
                        files.Add(pair.Key, data);
                    }

                    EncryptData(fFilesToAdd, aes.CreateEncryptor());
                    foreach (KeyValuePair<string, byte[]> pair in fFilesToAdd)
                    {
                        files.Add(pair.Key, pair.Value);
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
                if (fMetadataChanged)
                {
                    meta_data = BytifyMetadata(fMetadata);
                    bw.Write(meta_data);
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
                    info_data = EncryptFileinfo(files, aes.CreateEncryptor());

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
                hash_body = GetOszHash(file_data, file_data.Length/2, 0x9f);

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
                        fFiles.Add(pair.Key, new FileInfo(pair.Key, offset, pair.Value.Length));
                        offset += pair.Value.Length;
                    }
                }

                fOffsetFileinfo = tOffsetFileinfo; // bad choice of variable names, but w/e
                fOffsetData = tOffsetData;

                fNotOnDisk = false;
                fMetadataChanged = false;
                fFiledataChanged = false;
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
            fMapStreamsOpen.ForEach(s => s.Close());
            Save();

            Array.Clear(fIV, 0, fIV.Length);
            //Array.Clear(KEY, 0, KEY.Length); //TODO: key stuff

            fFiles.Clear();
            fFilesToAdd.Clear();
            fMetadata.Clear();

            fHandle.Close();

            fClosed = true;
        }

        public void Dispose()
        {
            Close();
        }

        private void CheckClosed()
        {
            if (fClosed)
                throw new Exception("Object is closed.");
        }
    }

    public struct FileInfo
    {
        public readonly string Filename;
        public readonly int Length;
        public readonly int Offset;

        public FileInfo(string filename, int offset, int length)
        {
            Filename = filename;
            Offset = offset;
            Length = length;
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
        Unknown = 9999
    }
}