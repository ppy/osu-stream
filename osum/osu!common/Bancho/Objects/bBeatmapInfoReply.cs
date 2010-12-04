using System;
using System.Collections.Generic;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public class bBeatmapInfoReply : bSerializable
    {
        public List<bBeatmapInfo> beatmapInfo = new List<bBeatmapInfo>();


        public bBeatmapInfoReply()
        {
        }

        public bBeatmapInfoReply(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            int count = sr.ReadInt32();
            for (int i = 0; i < count; i++) beatmapInfo.Add(new bBeatmapInfo(s));
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            int count = beatmapInfo.Count;

            sw.Write(count);
            for (int i = 0; i < count; i++) beatmapInfo[i].WriteToStream(sw);
        }

        #endregion
    }
}