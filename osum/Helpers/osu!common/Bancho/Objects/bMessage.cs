using System;
using System.IO;
using osu_common.Tencho;
using osu_common.Helpers;

namespace osu_common.Tencho.Objects
{
    public class bMessage : bSerializable
    {
        public object sendingClient;
        public string message;
        public string target;
        public bool isprivate { get { return target.Length == 0 || target[0] != '#'; } }

        public bMessage(object sender, string target, string message)
        {
            sendingClient = sender ?? string.Empty;
            this.message = message;
            this.target = target;
        }


        public bMessage(Stream s)
            : this(new SerializationReader(s))
        {
        }

        public bMessage(SerializationReader sr)
        {
            sendingClient = sr.ReadString();
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
            sw.Write(sendingClient.ToString());
            sw.Write(message);

            //cloak multiplayer channels before serialising any messages.
            sw.Write((target != null && target.StartsWith("#mphaxjax")) ? "#multiplayer" : target);
        }

        #endregion
    }
}