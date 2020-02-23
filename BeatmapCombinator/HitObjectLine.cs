using System;

namespace BeatmapCombinator
{
    public class HitObjectLine : IComparable<HitObjectLine>
    {
        internal string StringRepresentation;
        internal int Time;

        public int CompareTo(HitObjectLine h)
        {
            return Time.CompareTo(h.Time);
        }
    }
}