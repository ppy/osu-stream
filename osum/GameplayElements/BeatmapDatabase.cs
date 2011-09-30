using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osu_common.Helpers;
using System.IO;
using osu_common.Bancho;
using osum.GameplayElements.Beatmaps;
using osum.GameModes;
using osum.GameplayElements.Scoring;

namespace osum.GameplayElements
{
    internal static class BeatmapDatabase
    {
        const int DATABASE_VERSION = 6;
        const string FILENAME = "osu!.db";

        private static string fullPath { get { return GameBase.Instance.PathConfig + FILENAME; } }

        private static bool initialized;
        internal static int Version = -1;

        public static List<BeatmapInfo> BeatmapInfo = new List<BeatmapInfo>();

        internal static void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            if (!File.Exists(fullPath))
                return;

            try
            {
                using (FileStream fs = File.OpenRead(fullPath))
                using (SerializationReader reader = new SerializationReader(fs))
                {
                    Version = reader.ReadInt32();
                    if (Version > 3)
                        BeatmapInfo = reader.ReadBList<BeatmapInfo>();
                }
            }
            catch { }

            Version = DATABASE_VERSION;
        }

        internal static void Write()
        {
            Initialize();

            string filename = fullPath;
            string tempFilename = fullPath + "_";

            //write to a new file and then move, just in case something bad was to happen when writing.
            using (FileStream fs = File.Create(tempFilename))
            using (SerializationWriter writer = new SerializationWriter(fs))
            {
                writer.Write(Version);
                writer.Write(BeatmapInfo);
            }

            File.Delete(filename);
            File.Move(tempFilename, filename);
        }

        internal static DifficultyScoreInfo GetDifficultyInfo(Beatmap b, Difficulty d)
        {
            if (b == null) return null;

            BeatmapInfo i = PopulateBeatmap(b);

            return i.DifficultyScores[d];
        }

        internal static BeatmapInfo PopulateBeatmap(Beatmap beatmap)
        {
            string filename = Path.GetFileName(beatmap.ContainerFilename);

            BeatmapInfo i = BeatmapInfo.Find(bmi => bmi.Filename == filename);

            if (i == null)
            {
                i = new BeatmapInfo() { FullPath = beatmap.ContainerFilename, Filename = filename };
                BeatmapInfo.Add(i);
            }

            return i;
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
        public string FullPath;

        public Dictionary<Difficulty,DifficultyScoreInfo> DifficultyScores = new Dictionary<Difficulty,DifficultyScoreInfo>();

        public BeatmapInfo()
        {
            DifficultyScores[Difficulty.Easy] = new DifficultyScoreInfo() { difficulty = Difficulty.Easy };
            DifficultyScores[Difficulty.Normal] = new DifficultyScoreInfo() { difficulty = Difficulty.Normal };
            DifficultyScores[Difficulty.Expert] = new DifficultyScoreInfo() { difficulty = Difficulty.Expert };
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            FullPath = sr.ReadString();
            Filename = Path.GetFileName(FullPath);
            

            DifficultyScores[Difficulty.Easy].ReadFromStream(sr);
            DifficultyScores[Difficulty.Normal].ReadFromStream(sr);
            DifficultyScores[Difficulty.Expert].ReadFromStream(sr);
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(FullPath);

            DifficultyScores[Difficulty.Easy].WriteToStream(sw);
            DifficultyScores[Difficulty.Normal].WriteToStream(sw);
            DifficultyScores[Difficulty.Expert].WriteToStream(sw);
        }

        public Beatmap GetBeatmap()
        {
            return new Beatmap(FullPath) { BeatmapInfo = this };
        }

        #endregion

        #region IComparable<BeatmapInfo> Members

        public int CompareTo(BeatmapInfo other)
        {
            return GetBeatmap().CompareTo(other.GetBeatmap());
        }

        #endregion
    }
}
