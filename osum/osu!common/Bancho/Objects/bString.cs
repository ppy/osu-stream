using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public struct bString : bSerializable
    {
        public readonly string text;

        public bString(string text)
        {
            this.text = text;
        }

        public bString(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);
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