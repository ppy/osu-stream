using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Skins;
using osu_common.Helpers;
using System.IO;
using osu_common.Bancho;
using osum.GameplayElements.Beatmaps;

namespace osum.GameplayElements
{
    internal static class BeatmapDatabase
    {
        const int DATABASE_VERSION = 1;
        const string filename = "osu!.db";
        
        private static bool initialized;
        private static int Version = -1;

        public static List<BeatmapInfo> BeatmapInfo = new List<BeatmapInfo>();

        internal static void Initialize()
        {
            initialized = true;
            if (!File.Exists("osu!.db"))
                return;

            using (FileStream fs = File.OpenRead(filename))
            using (SerializationReader reader = new SerializationReader(fs))
            {
                Version = reader.ReadInt32();
                BeatmapInfo = reader.ReadBList<BeatmapInfo>();
            }


            Version = DATABASE_VERSION;
        }

        internal static void Write()
        {
            Initialize();

            using (FileStream fs = File.OpenWrite(filename))
            using (SerializationWriter writer = new SerializationWriter(fs))
            {
                writer.Write(Version);
                writer.Write(BeatmapInfo);
            }
        }

        internal static BeatmapInfo GetBeatmapInfo(Beatmap b, Difficulty d)
        {
            BeatmapInfo i = BeatmapInfo.Find(bmi => { return bmi.filename == b.ContainerFilename && bmi.difficulty == d; });
            if (i == null)
            {
                i = new BeatmapInfo() { filename = b.ContainerFilename, difficulty = d };
                BeatmapInfo.Add(i);
            }

            return i;
        }
    }

    internal class BeatmapInfo : bSerializable
    {
        public string filename;
        public Difficulty difficulty;
        public int HighScore;
        public int Playcount;

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            HighScore = sr.ReadInt32();
            Playcount = sr.ReadInt32();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(HighScore);
            sw.Write(Playcount);
        }

        #endregion
    }
}
