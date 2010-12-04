using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public class bBeatmapInfo : bSerializable
    {
        public int id;
        public int beatmapId;
        public int beatmapSetId;
        public int threadId;
        public int ranked;
        public Rankings osuRank = Rankings.N;
        public Rankings taikoRank = Rankings.N;
        public Rankings fruitsRank = Rankings.N;
        public string checksum;

        public bBeatmapInfo(int beatmapId, int beatmapSetId, int threadId,string checksum, int ranked, Rankings osuRank, Rankings taikoRank, Rankings fruitsRank)
        {
            this.beatmapId = beatmapId;
            this.beatmapSetId = beatmapSetId;
            this.threadId = threadId;
            this.ranked = ranked;
            this.osuRank = osuRank;
            this.taikoRank = taikoRank;
            this.fruitsRank = fruitsRank;
            this.checksum = checksum;
        }


        public bBeatmapInfo(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);
            id = sr.ReadInt16();
            beatmapId = sr.ReadInt32();
            beatmapSetId = sr.ReadInt32();
            threadId = sr.ReadInt32();
            ranked = sr.ReadByte();
            osuRank = (Rankings) sr.ReadByte();
            fruitsRank = (Rankings) sr.ReadByte();
            taikoRank = (Rankings) sr.ReadByte();
            checksum = sr.ReadString();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write((short)id);
            sw.Write(beatmapId);
            sw.Write(beatmapSetId);
            sw.Write(threadId);
            sw.Write((byte)ranked);
            sw.Write((byte)osuRank);
            sw.Write((byte)fruitsRank);
            sw.Write((byte)taikoRank);
            sw.Write(checksum);
        }

        #endregion
    }
}