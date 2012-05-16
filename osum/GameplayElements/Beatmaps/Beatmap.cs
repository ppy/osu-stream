//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using System.IO;
using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;
using osu_common.Libraries.Osz2;
using System.Globalization;
using osu_common.Helpers;
namespace osum.GameplayElements.Beatmaps
{
    public partial class Beatmap : IDisposable, IComparable<Beatmap>
    {
        public string ContainerFilename;

        public byte DifficultyOverall;
        public byte DifficultyCircleSize;
        public byte DifficultyHpDrainRate;
        public int StackLeniency = 1;
        public int CountdownOffset = 0;

        public string BeatmapFilename
        {
            get
            {
                MapPackage package = Package;

                if (package == null) return null;

                return package.MapFiles[0];
            }
        }
        public string StoryboardFilename { get { return ""; } }

        public Dictionary<Difficulty, BeatmapDifficultyInfo> DifficultyInfo = new Dictionary<Difficulty, BeatmapDifficultyInfo>();

        private MapPackage package;
        public MapPackage Package
        {
            get
            {
                try
                {
                    if (package == null)
                    {

                        if (ContainerFilename == null) return null;
#if iOS && DIST
                        if (ContainerFilename.EndsWith("osf2"))
                            package = new MapPackage(ContainerFilename);
                        else
                            package = new MapPackage(ContainerFilename, hash, false, false);
#else
                        package = new MapPackage(ContainerFilename);
#endif
                    }
                }
                catch
                {
                    return null;
                }

                return package;
            }

            set
            {
                package = value;
            }

        }

#if DIST || M4A
        public string AudioFilename = "audio.m4a";
#else
        public string AudioFilename
        {
            get
            {
                return "audio.m4a";
            }
        }
#endif

        public string PackageIdentifier
        {
            get
            {
                return Artist + Title;
            }
        }

        public List<int> StreamSwitchPoints;

        public Beatmap()
        {
        }

        ~Beatmap()
        {
            Dispose();
        }

        private byte[] hash
        {
            get
            {
                string deviceId = GameBase.Instance.DeviceIdentifier;
                string str = (char)0x6f + Path.GetFileName(ContainerFilename) + (char)0x73 + deviceId.Substring(0, 2) + (char)0x75 + deviceId.Substring(2) + (char)0x6d;
#if DEBUG
                Console.WriteLine("key: " + str);
#endif
                return CryptoHelper.GetMd5ByteArrayString(str);
            }
        }

        public Beatmap(string containerFilename)
        {
            ContainerFilename = containerFilename;
        }

        public Stream GetFileStream(string filename)
        {
            MapPackage p = Package;
            if (p == null) return null;
            return p.GetFile(filename);
        }

        internal byte[] GetFileBytes(string filename)
        {
            byte[] data = null;

            Stream stream = GetFileStream(filename);
            {
                if (stream != null)
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    //stream.Close();
                }

            }

            return data;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (package != null)
            {
                package.Dispose();
                package = null;
            }
        }

        #endregion

        BeatmapInfo beatmapInfo;
        public BeatmapInfo BeatmapInfo
        {
            get
            {
                if (beatmapInfo == null)
                    beatmapInfo = BeatmapDatabase.PopulateBeatmap(this);
                return beatmapInfo;
            }
            set
            {
                beatmapInfo = value;
            }
        }

        public string Artist
        {
            get
            {
                try
                {
                    return Package.GetMetadata(MapMetaType.Artist);
                }
                catch { return "error"; }
            }
        }

        public string PackId
        {
            get
            {
                try
                {
                    return Package.GetMetadata(MapMetaType.PackId);
                }
                catch { return "error"; }
            }
        }

        public string Creator { get { return Package.GetMetadata(MapMetaType.Creator); } }

        public string Title
        {
            get
            {
                try
                {
                    return Package.GetMetadata(MapMetaType.Title);
                }
                catch { return "error"; }
            }
        }

        public double HpStreamAdjustmentMultiplier = 1;

        private int difficultyStars = -1;
        public int DifficultyStars
        {
            get
            {
                try
                {
                    if (difficultyStars == -1)
                        Int32.TryParse(Package.GetMetadata(MapMetaType.Difficulty), out difficultyStars);
                }
                catch { difficultyStars = 0; }

                return difficultyStars;
            }
        }

        private int previewPoint = -1;
        public int PreviewPoint
        {
            get
            {
                if (previewPoint == -1)
                    Int32.TryParse(Package.GetMetadata(MapMetaType.PreviewTime), out previewPoint);

                if (previewPoint < 10000)
                    previewPoint = 30000;

                return previewPoint;
            }
        }


        #region IComparable<Beatmap> Members

        public int CompareTo(Beatmap other)
        {
            int comp = this.DifficultyStars.CompareTo(other.DifficultyStars);
            if (comp == 0)
                return this.ContainerFilename.CompareTo(other.ContainerFilename);
            return comp;
        }

        #endregion
    }
}

