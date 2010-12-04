using System;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public class bStatusUpdate : bSerializable
    {
        public string beatmapChecksum;
        public int beatmapId;
        public bool beatmapUpdate;
        public Mods currentMods;
        public PlayModes playMode;
        public bStatus status;
        public string statusText;


        public bStatusUpdate(bStatus status, bool beatmapUpdate, string statusText, string songChecksum, int beatmapId, Mods mods,
                             PlayModes playMode)
        {
            this.status = status;
            this.beatmapUpdate = beatmapUpdate;
            beatmapChecksum = songChecksum;
            this.statusText = statusText;
            currentMods = mods;
            this.playMode = playMode;
            this.beatmapId = beatmapId;
        }

        public bStatusUpdate(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            status = (bStatus) sr.ReadByte();
            beatmapUpdate = sr.ReadBoolean();

            if (!beatmapUpdate) return;

            statusText = sr.ReadString();
            beatmapChecksum = sr.ReadString();
            currentMods = (Mods) sr.ReadUInt16();
            playMode = (PlayModes)sr.ReadByte();
            beatmapId = sr.ReadInt32();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write((byte)status);
            sw.Write(beatmapUpdate);

            if (!beatmapUpdate) return;

            sw.Write(statusText);
            sw.Write(beatmapChecksum);
            sw.Write((ushort)currentMods);
            
            sw.Write((byte)playMode);
            sw.Write(beatmapId);
        }

        #endregion
    }
}