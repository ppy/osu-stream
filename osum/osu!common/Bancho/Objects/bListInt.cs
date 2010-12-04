using System;
using System.Collections.Generic;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public struct bListInt : bSerializable
    {
        public readonly List<int> list;

        public bListInt(List<int> list)
        {
            this.list = list;
        }

        public bListInt(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            list = new List<int>();

            int count = sr.ReadInt32();

            for (int i = 0; i < count; i++)
                list.Add(sr.ReadInt32());
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            int count = list.Count;

            sw.Write(count);
            for (int i = 0; i < count; i++)
                sw.Write(list[i]);
        }

        #endregion
    }
}