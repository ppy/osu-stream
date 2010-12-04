using System;
using System.IO;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public struct bMatchJoin : bSerializable
    {
        public readonly int MatchId;
        public readonly string Password;

        public bMatchJoin(int matchId, string password)
        {
            MatchId = matchId;
            Password = password;
        }

        public bMatchJoin(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);
            MatchId = sr.ReadInt32();
            Password = sr.ReadString();
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(MatchId);
            sw.Write(Password);
        }

        #endregion
    }
}