using System.IO;
using osu_common.Tencho;
using osu_common.Helpers;

namespace osu_common.Tencho.Objects
{
    public struct bString : bSerializable
    {
        public readonly string text;

        public bString(string text)
        {
            this.text = text;
        }

        public bString(Stream s)
            : this(new SerializationReader(s))
        {
        }

        public bString(SerializationReader sr)
        {
            text = sr.ReadString();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(text);
        }

        #endregion
    }
}