using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu_common.Bancho;
using osu_common.Helpers;

namespace osum.GameplayElements.Scoring
{
    public class Score : bSerializable
    {
        public ushort count100;
        public ushort count300;
        public ushort count50;
        public ushort countMiss;
        public DateTime date;
        public int maxCombo;
        public int spinnerBonusScore;
        public int hitOffsetMilliseconds;
        public int hitOffsetCount;
        public int comboBonusScore;
        public int accuracyBonusScore;
        public int hitScore;

        public int totalScore
        {
            get { return spinnerBonusScore + hitScore + comboBonusScore + accuracyBonusScore; }
        }

        public Rank Ranking
        {
            get
            {
                if (accuracy == 1)
                    return Rank.X;
                if (totalScore > 950000)
                    return Rank.S;
                if (totalScore > 900000)
                    return Rank.A;
                if (totalScore > 800000)
                    return Rank.B;
                if (totalScore > 500000)
                    return Rank.C;
                return Rank.D;
            }
        }

        internal virtual float accuracy
        {
            get { return totalHits > 0 ? (float)(count50 * 1 + count100 * 2 + count300 * 4) / (totalHits * 4) : 0; }
        }

        internal virtual int totalHits
        {
            get { return count50 + count100 + count300 + countMiss; }
        }

        internal virtual int totalSuccessfulHits
        {
            get { return count50 + count100 + count300; }
        }

        #region bSerializable implementation
        public void ReadFromStream (SerializationReader sr)
        {
            count300 = sr.ReadUInt16();
            count100 = sr.ReadUInt16();
            count50 = sr.ReadUInt16();
            countMiss = sr.ReadUInt16();
            date = sr.ReadDateTime();
            maxCombo = sr.ReadUInt16();
            spinnerBonusScore = sr.ReadInt32();
            comboBonusScore = sr.ReadInt32();
            accuracyBonusScore = sr.ReadInt32();
            hitScore = sr.ReadInt32();
        }

        public void WriteToStream (SerializationWriter sw)
        {
            sw.Write(count300);
            sw.Write(count100);
            sw.Write(count50);
            sw.Write(countMiss);
            sw.Write(date);
            sw.Write(maxCombo);
            sw.Write(spinnerBonusScore);
            sw.Write(comboBonusScore);
            sw.Write(accuracyBonusScore);
            sw.Write(hitScore);

        }
        #endregion
    }

    public enum Rank
    {
        N,
        D,
        C,
        B,
        A,
        S,
        X
    }
}
