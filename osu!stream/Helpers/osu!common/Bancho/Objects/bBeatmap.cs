using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_common.Tencho;
using osu_common.Helpers;

namespace osum.Helpers.osu_common.Tencho.Objects
{
    public class bBeatmap : bSerializable
    {
        private string filename;
        public virtual string Filename { get { return filename; } }

        public bBeatmap()
        {

        }

        public bBeatmap(SerializationReader sr)
        {
            ReadFromStream(sr);
        }

        public void ReadFromStream(SerializationReader sr)
        {
            filename = sr.ReadString();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(Filename);
        }
    }
}
