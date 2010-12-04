using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public enum Completeness
    {
        StatusOnly,
        Statistics,
        Full
    }

    public class bUserStats : bSerializable
    {
        public float accuracy;
        public string avatarFilename;
        public Completeness completeness;
        public int level;
        public string location;
        public readonly Permissions permission;
        public int playcount;
        public int rank;
        public long rankedScore;
        public bStatusUpdate status;
        public int timezone;
        public long totalScore;
        public int userId;
        public string username;
        public float longitude;
        public float latitude;

        public bUserStats(int userId, string username, long rankedScore, float accuracy, int playcount,
                          long totalScore,
                          int rank, string avatarFilename, bStatusUpdate status, int timezone, string location, Permissions permission, float longitude, float latitude)
        {
            this.userId = userId;
            this.username = username;
            this.rankedScore = rankedScore;
            this.accuracy = accuracy;
            this.playcount = playcount;
            this.totalScore = totalScore;
            this.rank = rank;
            this.avatarFilename = avatarFilename;
            this.status = status;
            this.timezone = timezone;
            this.location = location;
            this.permission = permission;
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public bUserStats(Stream s)
            : this(s, false)
        {
        }

        public bUserStats(Stream s, bool forceFull)
        {
            SerializationReader sr = new SerializationReader(s);

            userId = sr.ReadInt32();

            Completeness comp = (Completeness) sr.ReadByte();

            completeness = forceFull ? Completeness.Full : comp;

            status = new bStatusUpdate(s);

            if (completeness > Completeness.StatusOnly)
            {
                rankedScore = sr.ReadInt64();
                accuracy = sr.ReadSingle();
                playcount = sr.ReadInt32();
                totalScore = sr.ReadInt64();
                rank = sr.ReadInt32();
            }
            
            if (completeness != Completeness.Full) return;
            
            username = sr.ReadString();
            avatarFilename = sr.ReadString();
            timezone = sr.ReadByte() - 24;
            location = sr.ReadString();
            permission = (Permissions) sr.ReadByte();
            
            if (OsuCommon.ProtocolVersion >= 5)
            {
                longitude = sr.ReadSingle();
                latitude = sr.ReadSingle();
            }
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            WriteToStream(sw, false);
        }

        #endregion

        public void WriteToStream(SerializationWriter sw, bool forceFull)
        {
            sw.Write(userId);
            sw.Write((byte)completeness);
            status.WriteToStream(sw);
            if (completeness > Completeness.StatusOnly || forceFull)
            {
                sw.Write(rankedScore);
                sw.Write(accuracy);
                sw.Write(playcount);
                sw.Write(totalScore);
                sw.Write(rank);
            }
            if (completeness == Completeness.Full || forceFull)
            {
                sw.Write(username);
                sw.Write(avatarFilename);
                sw.Write((byte)(timezone + 24));
                sw.Write(location);
                sw.Write((byte)permission);
                sw.Write(longitude);
                sw.Write(latitude);
            }

            
        }

        public bUserStats Clone()
        {
            MemoryStream ms = new MemoryStream();
            WriteToStream(new SerializationWriter(ms), true);
            ms.Position = 0;
            return new bUserStats(ms, true);
        }
    }
}