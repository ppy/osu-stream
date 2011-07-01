using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameplayElements.Scoring
{
    public class Score
    {
        public ushort count100;
        public ushort count300;
        public ushort count50;
        public ushort countGeki;
        public ushort countKatu;
        public ushort countMiss;
        public DateTime date;
        public bool isOnline;
        public int maxCombo;
        public bool pass;
        public bool exit;
        public int failTime;
        public string playerName;
        public string rawGraph;
        public int totalScore
        {
            get { return spinnerBonusScore + hitScore + comboBonusScore + accuracyBonusScore; }
        }

        public int spinnerBonusScore;
        public int hitOffsetMilliseconds;
        public int hitOffsetCount;
        public int comboBonusScore;
        public int accuracyBonusScore;
        public int hitScore;
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
