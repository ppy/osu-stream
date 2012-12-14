using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_common.Tencho;
using osu_common.Helpers;

namespace osum.Helpers.osu_common.Tencho.Objects
{
    public class bMatch : bSerializable
    {
        public int MatchId;
        public virtual MatchState State { get; set; }
        public bBeatmap Beatmap = new bBeatmap();
        public pList<bPlayerData> Players = new pList<bPlayerData>();

        public bMatch()
        {
        }

        public bMatch(SerializationReader sr)
        {
            ReadFromStream(sr);
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            MatchId = sr.ReadInt32();
            State = (MatchState)sr.ReadByte();
            Beatmap = new bBeatmap(sr);
            Players = sr.ReadBList<bPlayerData>();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(MatchId);
            sw.Write((byte)State);
            Beatmap.WriteToStream(sw);
            sw.Write<bPlayerData>(Players);
        }

        #endregion
    }

    public enum MatchState
    {
        Gathering,
        SongSelect,
        DifficultySelect,
        Preparing,
        Playing,
        Results
    }

    public enum PlayerState
    {
        Waiting,
        DifficultySelected,
        ReadyToPlay,
        Playing,
        FinishedPlaying
    }

    public class bPlayerData : bSerializable, IComparable<bPlayerData>
    {
        public string Username;
        public PlayerState State;
        public pList<TrackingPoint> Input = new pList<TrackingPoint>();

        public bPlayerData()
        {

        }

        public bPlayerData(SerializationReader sr)
        {
            ReadFromStream(sr);
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            Username = sr.ReadString();
            State = (PlayerState)sr.ReadByte();
            Input = sr.ReadBList<TrackingPoint>();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(Username);
            sw.Write((byte)State);
            sw.Write(Input);
        }

        #endregion

        #region IComparable<bPlayerData> Members

        public int CompareTo(bPlayerData other)
        {
            return 0;
        }

        #endregion
    }
}
