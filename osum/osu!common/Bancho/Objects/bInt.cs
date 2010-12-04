using System;
using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public struct bInt : bSerializable
    {
        public int number;

        public bInt(int number)
        {
            this.number = number;
        }

        public bInt(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);
            number = sr.ReadInt32();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(number);
        }

        #endregion
    }
}