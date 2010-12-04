using System.Collections.Generic;
using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public class bBeatmapInfoRequest : bSerializable
    {
        public List<string> filenames;
        public List<int> ids;

        public bBeatmapInfoRequest()
        {
            filenames = new List<string>();
            ids = new List<int>();
        }

        public bBeatmapInfoRequest(List<string> checksums, List<int> ids)
        {
            this.filenames = checksums;
            this.ids = ids;
            
        }

        public bBeatmapInfoRequest(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            filenames = new List<string>();
            ids = new List<int>();
            
            int count = sr.ReadInt32();
            for (int i = 0; i < count; i++) filenames.Add(sr.ReadString());

            count = sr.ReadInt32();
            for (int i = 0; i < count; i++) ids.Add(sr.ReadInt32());
        }

        public bBeatmapInfoRequest(int lengthFilenames, int lengthIds)
        {
            filenames = new List<string>(lengthFilenames);
            ids = new List<int>(lengthIds);
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(filenames.Count);
            foreach (string cs in filenames) sw.Write(cs);

            sw.Write(ids.Count);
            foreach (int id in ids) sw.Write(id);
        }

        #endregion
    }
}