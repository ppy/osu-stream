using System;
using System.Collections.Generic;
using System.IO;
using osum.GameModes.SongSelect;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements.Scoring;
using osum.Helpers;

#if iOS
using osum.Support.iPhone;
#endif

namespace osum.GameplayElements
{
    internal static class BeatmapDatabase
    {
        internal const int DATABASE_VERSION = 12;
        private const string FILENAME = "osu!.db";

        private static string databasePath => GameBase.Instance.PathConfig + FILENAME;

        internal static int Version = -1;

        public static pList<BeatmapInfo> BeatmapInfo;

        internal static void Initialize()
        {
            if (BeatmapInfo != null)
                return;

            BeatmapInfo = new pList<BeatmapInfo>();

            if (File.Exists(databasePath))
            {
                try
                {
                    using (FileStream fs = File.OpenRead(databasePath))
                    using (SerializationReader reader = new SerializationReader(fs))
                    {
                        Version = reader.ReadInt32();
                        if (Version > 3)
                            BeatmapInfo = reader.ReadBList<BeatmapInfo>();
                    }
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine("Error while reading database! " + e);
#endif
                }
            }

#if iOS && DIST
            //move beatmaps from Documents to Library/Cache/ as per new storage guidelines (see https://www.marco.org/2011/10/13/ios5-caches-cleaning)
            if (Version < 8)
            {
                string newLocation = SongSelectMode.BeatmapPath;
                foreach (string file in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"*.os*"))
                {
                    string newFile = newLocation + "/" + Path.GetFileName(file);
                    File.Delete(newFile);
                    File.Move(file, newFile);
                }
            }
            else if (Version < 9)
            {
                if (HardwareDetection.RunningiOS5OrHigher)
                    foreach (string file in Directory.GetFiles(SongSelectMode.BeatmapPath, "*.os*"))
                        Foundation.NSFileManager.SetSkipBackupAttribute(file,true);
            }
#endif

#if DEBUG
            Console.WriteLine("Read beatmap database: " + BeatmapInfo.Count);
#endif
        }

        internal static void Write()
        {
            Initialize();

            Version = DATABASE_VERSION;

            string filename = databasePath;
            string tempFilename = databasePath + "_";

            //write to a new file and then move, just in case something bad was to happen when writing.
            using (FileStream fs = File.Create(tempFilename))
            using (SerializationWriter writer = new SerializationWriter(fs))
            {
                writer.Write(Version);
                writer.Write(BeatmapInfo);
            }

            File.Delete(filename);
            File.Move(tempFilename, filename);

#if DEBUG
            Console.WriteLine("Wrote beatmap database to " + filename + " with count " + BeatmapInfo.Count);
#endif
        }

        internal static DifficultyScoreInfo GetDifficultyInfo(Beatmap b, Difficulty d)
        {
            if (b == null) return null;
            return PopulateBeatmap(b).DifficultyScores[d];
        }

        internal static BeatmapInfo PopulateBeatmap(Beatmap beatmap)
        {
            if (beatmap == null) return null;
            Initialize();

            string filename = Path.GetFileName(beatmap.ContainerFilename);

            BeatmapInfo i = BeatmapInfo.Find(bmi => bmi.Filename == filename);

            if (i == null)
            {
                i = new BeatmapInfo(filename);
                BeatmapInfo.AddInPlace(i);
            }

            return i;
        }

        internal static void Erase(Beatmap b)
        {
            Initialize();

            string filename = Path.GetFileName(b.ContainerFilename);

            BeatmapInfo i = BeatmapInfo.Find(bmi => bmi.Filename == filename);

            if (i != null)
                BeatmapInfo.Remove(i);
        }
    }

    public class DifficultyScoreInfo : bSerializable
    {
        public Difficulty difficulty;
        public Score HighScore;
        public ushort Playcount;

        public void ReadFromStream(SerializationReader sr)
        {
            if (BeatmapDatabase.Version < 5)
                sr.ReadByte();

            if (sr.ReadBoolean()) //has score
            {
                HighScore = new Score();
                HighScore.ReadFromStream(sr);
            }

            Playcount = sr.ReadUInt16();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(HighScore != null);

            if (HighScore != null)
                HighScore.WriteToStream(sw);

            sw.Write(Playcount);
        }
    }

    public class BeatmapInfo : bSerializable, IComparable<BeatmapInfo>
    {
        public string Filename;

        public Dictionary<Difficulty, DifficultyScoreInfo> DifficultyScores = new Dictionary<Difficulty, DifficultyScoreInfo>();

        public BeatmapInfo(string filename)
            : this()
        {
            Filename = Path.GetFileName(filename);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="osum.GameplayElements.BeatmapInfo"/> class.
        /// Do NOT use this for anything other than serialisation; it does not properly initialise scores.
        /// </summary>
        public BeatmapInfo()
        {
            DifficultyScores[Difficulty.Easy] = new DifficultyScoreInfo { difficulty = Difficulty.Easy };
            DifficultyScores[Difficulty.Normal] = new DifficultyScoreInfo { difficulty = Difficulty.Normal };
            DifficultyScores[Difficulty.Expert] = new DifficultyScoreInfo { difficulty = Difficulty.Expert };
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            //the GetFileName call is not necessary after old (pre-v1.11) databases are updated.
            Filename = Path.GetFileName(sr.ReadString());

            DifficultyScores[Difficulty.Easy].ReadFromStream(sr);
            DifficultyScores[Difficulty.Normal].ReadFromStream(sr);
            DifficultyScores[Difficulty.Expert].ReadFromStream(sr);
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(Filename);

            DifficultyScores[Difficulty.Easy].WriteToStream(sw);
            DifficultyScores[Difficulty.Normal].WriteToStream(sw);
            DifficultyScores[Difficulty.Expert].WriteToStream(sw);
        }

        public Beatmap GetBeatmap()
        {
            if (Filename == null) return null;

            string path = SongSelectMode.BeatmapPath + "/" + Filename;
            if (Filename.EndsWith(".osf2") && !File.Exists(path))
                path = "Beatmaps/" + Filename;

            return new Beatmap(path) { BeatmapInfo = this };
        }

        #endregion

        #region IComparable<BeatmapInfo> Members

        public int CompareTo(BeatmapInfo other)
        {
            using (Beatmap beatmapThis = GetBeatmap())
            using (Beatmap beatmapOther = other.GetBeatmap())
            {
                if (beatmapThis == null) return 1;
                if (beatmapOther == null) return -1;
                return beatmapThis.CompareTo(beatmapOther);
            }
        }

        #endregion
    }
}
