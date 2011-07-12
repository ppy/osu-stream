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
        const int DATABASE_VERSION = 3;
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
                    if (Version > 2)
                        BeatmapInfo = reader.ReadBList<BeatmapInfo>();
                }
            }
            catch { }

            Version = DATABASE_VERSION;
        }

        internal static void Write()
        {
            Initialize();

            using (FileStream fs = File.Create(fullPath))
            using (SerializationWriter writer = new SerializationWriter(fs))
            {
                writer.Write(Version);
                writer.Write(BeatmapInfo);
            }
        }

        internal static BeatmapInfo GetBeatmapInfo(Beatmap b, Difficulty d)
        {
            if (b == null) return null;

            string filename = Path.GetFileName(b.ContainerFilename);

            BeatmapInfo i = BeatmapInfo.Find(bmi => { return bmi.filename == filename && bmi.difficulty == d; });

            if (i == null)
            {
                i = new BeatmapInfo() { filename = filename, difficulty = d, HighScore = new Score() };
                BeatmapInfo.Add(i);
            }

            return i;
        }
    }

    internal class BeatmapInfo : bSerializable
    {
        public string filename;
        public Difficulty difficulty;
        public Score HighScore;
        public ushort Playcount;

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            filename = sr.ReadString();
            difficulty = (Difficulty)sr.ReadByte();

            HighScore = new Score();
            HighScore.ReadFromStream(sr);

            Playcount = sr.ReadUInt16();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(filename);
            sw.Write((byte)difficulty);

            HighScore.WriteToStream(sw);

            sw.Write(Playcount);
        }

        #endregion
    }
}
