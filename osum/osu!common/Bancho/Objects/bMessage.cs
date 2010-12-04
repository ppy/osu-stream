using System;
using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public class bMessage : bSerializable
    {
        public string sender;
        public string message;
        public string target;
        public bool isprivate { get { return target.Length == 0 || target[0] != '#';}}

        public bMessage(string sender, string target, string message)
        {
            this.sender = sender;
            this.message = message;
            this.target = target;
            
        }

        public bMessage(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            sender = sr.ReadString();
            message = sr.ReadString();
            target = sr.ReadString();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(sender);
            sw.Write(message);
            sw.Write(target);
        }

        #endregion
    }
}