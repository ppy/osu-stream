using System;
using System.IO;
using osu_common.Tencho;
using osu_common.Helpers;

namespace osu_common.Tencho.Objects
{
    public struct bInt : bSerializable
    {
        public int number;

        public bInt(int number)
        {
            this.number = number;
        }

        public bInt(Stream s) : this(new SerializationReader(s))
        {
            
        }

        public bInt(SerializationReader sr)
        {
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