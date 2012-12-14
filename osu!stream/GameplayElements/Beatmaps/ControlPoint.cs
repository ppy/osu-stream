using System;
using System.IO;
using System.Runtime.Serialization;
using osum.GameplayElements.Beatmaps;

namespace osum.GameplayElements.Beatmaps
{
    public enum CustomSampleSet
    {
        Default = 0,
        Custom1 = 1,
        Custom2 = 2
    }

    public enum SampleSet
    {
        None = 0,
        Normal = 1,
        Soft = 2,
        Drum = 3
    } ;

    public enum TimeSignatures
    {
        SimpleQuadruple = 4,
        SimpleTriple = 3
    }

    public struct SampleSetInfo
    {
        public SampleSet SampleSet;
        public CustomSampleSet CustomSampleSet;
        public float Volume;
        public SampleSet AdditionSampleSet;
    }

    public class ControlPoint : IComparable<ControlPoint>, ICloneable//, bSerializable
    {
        public double beatLength;
        public CustomSampleSet customSamples;
        public double offset;
        public SampleSet sampleSet;
        public TimeSignatures timeSignature;
        public int volume;

        private bool timingChange = true;
        public bool TimingChange
        {
            get { return timingChange; }
            set
            {
                if (!value && beatLength >= 0) beatLength = -100;
                //If we change to an inheriting section, force the beatLength to a sane value.

                timingChange = value;
            }
        }
        public bool kiaiMode;

        public float bpmMultiplier
        {
            get
            {
                if (beatLength > 0) return 1;

                return (float)(-beatLength / 100);
            }
        }

        public ControlPoint(double offset, double beatLength, TimeSignatures timeSignature, SampleSet sampleSet,
                             CustomSampleSet customSamples, int volume, bool timingChange, bool kiaiMode)
        {
            this.offset = offset;
            this.beatLength = beatLength;
            this.timeSignature = timeSignature;
            this.sampleSet = sampleSet == SampleSet.None ? SampleSet.Soft : sampleSet;
            this.customSamples = customSamples;
            this.volume = volume;
            this.TimingChange = timingChange;
            this.kiaiMode = kiaiMode;
        }

        public double bpm
        {
            get { return beatLength == 0 ? 0 : 60000 / beatLength; }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new ControlPoint(offset, beatLength, timeSignature, sampleSet, customSamples, volume, TimingChange, kiaiMode);
        }

        #endregion

        #region IComparable<TimingPoint> Members

        public int CompareTo(ControlPoint other)
        {
            if (offset == other.offset)
                return other.TimingChange.CompareTo(TimingChange);

            return offset.CompareTo(other.offset);
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{5}{0:00}:{1:00}:{2:00} {3}/4 {4}bpm {6}{7}{8}", ((int)offset / 60000), (int)offset % 60000 / 1000,
                (int)offset % 1000 / 10, (int)timeSignature, beatLength < 0 ? Math.Round(-100f / beatLength, 1) + "x " : (60000 / beatLength).ToString("N"),
                                 !TimingChange ? "^ " : "",
                                 sampleSet == SampleSet.Soft ? "S" : "N",
                                 customSamples == CustomSampleSet.Custom1 ? ":C1" : (customSamples == CustomSampleSet.Custom2 ? ":C2" : ""),
                                 kiaiMode ? " Ki" : "");
        }

        /*public void ReadFromStream(SerializationReader sr)
        {
            beatLength = sr.ReadDouble();
            offset = sr.ReadDouble();
            TimingChange = sr.ReadBoolean();
        }

        public void WriteToStream(SerializationWriter sw)
        {
            sw.Write(beatLength);
            sw.Write(offset);
            sw.Write(TimingChange);
        }*/
    }
}