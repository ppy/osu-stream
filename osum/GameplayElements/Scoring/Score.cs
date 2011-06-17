using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameplayElements.Scoring
{
    internal class Score
    {
        internal ushort count100;
        internal ushort count300;
        internal ushort count50;
        internal ushort countGeki;
        internal ushort countKatu;
        internal ushort countMiss;
        internal DateTime date;
        internal bool isOnline;
        internal int maxCombo;
        internal bool pass;
        internal bool exit;
        internal int failTime;
        internal bool perfect;
        internal string playerName;
        internal string rawGraph;
        internal byte[] rawReplayCompressed;
        internal List<bool> scoringSectionResults = new List<bool>();
        internal bool submitting;
        internal int totalScore;
        internal int spinnerBonus;
        public int hitOffsetMilliseconds;
        public int hitOffsetCount;
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
            get { return totalHits > 0 ? (float)(count50 * 50 + count100 * 100 + count300 * 300) / (totalHits * 300) : 0; }
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

    internal enum Rank
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
