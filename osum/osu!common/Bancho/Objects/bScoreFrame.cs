using System;
using System.IO;
using osu_common.Helpers;

namespace osu_common.Bancho.Objects
{
    public struct bScoreFrame : bSerializable
    {
        public ushort count100;
        public ushort count300;
        public ushort count50;
        public ushort countGeki;
        public ushort countKatu;
        public ushort countMiss;
        public ushort currentCombo;
        public int currentHp;
        public byte id;
        public ushort maxCombo;
        public bool pass;
        public bool perfect;
        public int time;
        public int totalScore;
        public int tagByte;
        //public string checksum;

        public bScoreFrame(Stream s)
        {
            SerializationReader sr = new SerializationReader(s);
            //checksum = sr.ReadString();
            time = sr.ReadInt32();
            id = sr.ReadByte();
            count300 = sr.ReadUInt16();
            count100 = sr.ReadUInt16();
            count50 = sr.ReadUInt16();
            countGeki = sr.ReadUInt16();
            countKatu = sr.ReadUInt16();
            countMiss = sr.ReadUInt16();
            totalScore = sr.ReadInt32();
            maxCombo = sr.ReadUInt16();
            currentCombo = sr.ReadUInt16();
            perfect = sr.ReadBoolean();
            currentHp = sr.ReadByte();
            tagByte = sr.ReadByte();
            if (currentHp == 254)
            {
                currentHp = 0;
                pass = false;
            }
            else
                pass = true;
        }

        #region bSerializable Members

        public void ReadFromStream(SerializationReader s)
        {
            throw new NotImplementedException();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            //sw.Write(checksum);
            sw.Write(time);
            sw.Write(id);
            sw.Write(count300);
            sw.Write(count100);
            sw.Write(count50);
            sw.Write(countGeki);
            sw.Write(countKatu);
            sw.Write(countMiss);
            sw.Write(totalScore);
            sw.Write(maxCombo);
            sw.Write(currentCombo);
            sw.Write(perfect);
            sw.Write((byte) (pass ? currentHp : 254));
            sw.Write(tagByte);
        }

        #endregion

        public bool IsValid()
        {
/*            if (checksum != null)
                return checksum == MakeChecksum();
            else*/
            return true;
        }

        public string MakeChecksum()
        {
            return
                CryptoHelper.GetMd5String(time + pass.ToString() + count300 + count50 + countGeki + countKatu +
                                          countMiss + currentCombo + maxCombo + currentHp);
        }
    }
}