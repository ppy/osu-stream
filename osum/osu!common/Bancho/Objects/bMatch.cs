using System;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    [Flags]
    public enum SlotStatus
    {
        Open = 1,
        Locked = 2,
        NotReady = 4,
        Ready = 8,
        NoMap = 16,
        Playing = 32,
        Complete = 64,
        HasPlayer = NotReady | Ready | NoMap | Playing | Complete,
        Quit = 128
    }

    public class bMatch : bSerializable
    {
        public string gameName;
        public int matchId;
        public MatchTypes matchType;
        public SlotStatus[] slotStatus = new SlotStatus[slotCount];
        public int[] slotId = new int[slotCount];
        public SlotTeams[] slotTeam = new SlotTeams[slotCount];
        public string beatmapName;
        public string beatmapChecksum;
        public int beatmapId = -1;
        public bool inProgress;
        public Mods activeMods;
        public int hostId;
        public PlayModes playMode;
        public MatchScoringTypes matchScoringType;
        public MatchTeamTypes matchTeamType;
        private readonly bool sendPassword;
        public string gamePassword;
        public const int slotCount = 8;
        public bool passwordRequired { get { return gamePassword != null; } }

        public bMatch(MatchTypes matchType, MatchScoringTypes matchScoringType, MatchTeamTypes matchTeamType, PlayModes playMode, string gameName, string gamePassword, int initialSlotCount, string beatmapName, string beatmapChecksum, int beatmapId, Mods activeMods, int hostId)
        {
            this.matchType = matchType;
            this.playMode = playMode;
            this.matchScoringType = matchScoringType;
            this.matchTeamType = matchTeamType;
            this.gameName = gameName;
            this.gamePassword = gamePassword;
            this.beatmapName = beatmapName;
            this.beatmapChecksum = beatmapChecksum;
            this.beatmapId = beatmapId;
            this.activeMods = activeMods;
            this.hostId = hostId;

            sendPassword = true;

            for (int i = 0; i < slotCount; i++)
            {
                slotStatus[i] = i < initialSlotCount ? SlotStatus.Open : SlotStatus.Locked;
                slotId[i] = -1;
            }
        }

        public bMatch(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);

            sendPassword = false;

            matchId = sr.ReadByte();
            inProgress = sr.ReadBoolean();
            matchType = (MatchTypes) sr.ReadByte();
            activeMods = (Mods)sr.ReadInt16();
            gameName = sr.ReadString();
            gamePassword = sr.ReadString();
            beatmapName = sr.ReadString();
            beatmapId = sr.ReadInt32();
            beatmapChecksum = sr.ReadString();
            for (int i = 0; i < slotCount; i++)
                slotStatus[i] = (SlotStatus)sr.ReadByte();

            if (OsuCommon.ProtocolVersion > 3)
                for (int i = 0; i < slotCount; i++)
                    slotTeam[i] = (SlotTeams)sr.ReadByte();

            for (int i = 0; i < slotCount; i++)
                slotId[i] = (slotStatus[i] & SlotStatus.HasPlayer) > 0 ? sr.ReadInt32() : -1;

            hostId = sr.ReadInt32();

            playMode = (PlayModes)sr.ReadByte();

            if (OsuCommon.ProtocolVersion > 2)
            {
                matchScoringType = (MatchScoringTypes)sr.ReadByte();
                matchTeamType = (MatchTeamTypes)sr.ReadByte();
            }
        }

        public int slotUsedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotCount; i++)
                    if ((slotStatus[i] & SlotStatus.HasPlayer) > 0)
                        count++;
                return count;
            }
        }

        public int slotPlayingCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotCount; i++)
                    if ((slotStatus[i] & SlotStatus.Playing) > 0)
                        count++;
                return count;
            }
        }

        public int slotOpenCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotCount; i++)
                    if (slotStatus[i] != SlotStatus.Locked)
                        count++;
                return count;
            }
        }

        public int slotFreeCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotCount; i++)
                    if (slotStatus[i] == SlotStatus.Open)
                        count++;
                return count;
            }
        }

        public int slotReadyCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < slotCount; i++)
                    if (slotStatus[i] == SlotStatus.Ready)
                        count++;
                return count;
            }
        }

        public bool TeamMode
        {
            get {
                return matchTeamType == MatchTeamTypes.TagTeamVs || matchTeamType == MatchTeamTypes.TeamVs;
            }
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader sr)
        {
            throw new System.NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write((byte) matchId);
            sw.Write(inProgress);
            sw.Write((byte) matchType);
            sw.Write((short)activeMods);
            sw.Write(gameName);
            sw.Write(!sendPassword && gamePassword != null ? "" : gamePassword);
            sw.Write(beatmapName);
            sw.Write(beatmapId);
            sw.Write(beatmapChecksum);
            for (int i = 0; i < slotCount; i++)
                sw.Write((byte)slotStatus[i]);

            if (OsuCommon.ProtocolVersion > 3)
                for (int i = 0; i < slotCount; i++)
                    sw.Write((byte)slotTeam[i]);

            for (int i = 0; i < slotCount; i++)
                if ((slotStatus[i] & SlotStatus.HasPlayer) > 0)
                    sw.Write(slotId[i]);
            sw.Write(hostId);

            sw.Write((byte)playMode);

            sw.Write((byte)matchScoringType);
            sw.Write((byte)matchTeamType);
        }

        private static byte bools2byte(bool[] bools)
        {
            byte outbyte = 0;

            for (int i = 7; i >= 0; i--)
            {
                if (bools[i])
                    outbyte |= 1;
                if (i > 0)
                    outbyte <<= 1;
            }
            return outbyte;
        }

        private static bool[] byte2bools(byte inbyte)
        {
            bool[] bools = new bool[slotCount];

            for (int i = 0; i < slotCount; i++)
                bools[i] = ((inbyte >> i) & 1) > 0;
            return bools;
        }

        public int findPlayerFromId(int userId)
        {
            int pos = 0;
            while (pos < slotCount && slotId[pos] != userId)
                pos++;
            if (pos > 7)
                return -1;
            return pos;
        }



        #endregion
    }
}