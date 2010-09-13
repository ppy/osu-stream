//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using System.IO;
using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;
namespace osum.GameplayElements.Beatmaps
{
    public class Beatmap
    {
        public string ContainerFilename;

        public string BeatmapFilename { get { return ContainerFilename + "/beatmap.osu"; } }
        public string StoryboardFilename { get { return ""; } }


        public Beatmap(string containerFilename)
        {
            ContainerFilename = containerFilename;
        }


        public Stream GetFileStream(string filename)
        {
            return File.OpenRead(filename);
        }

        #region Timing

        internal List<ControlPoint> ControlPoints = new List<ControlPoint>();
        
        public double DifficultySliderMultiplier;
        public double DifficultySliderTickRate;
        public byte DifficultyOverall;
        public byte DifficultyCircleSize;
        public byte DifficultyHpDrainRate;

        /// <summary>
        /// Beats the offset at.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        internal double beatOffsetCloseToZeroAt(double time)
        {
            if (ControlPoints.Count == 0)
                return 0;

            int point = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
                if (ControlPoints[i].TimingChange && ControlPoints[i].offset <= time)
                    point = i;

            double length = ControlPoints[point].beatLength;
            double offset = ControlPoints[point].offset;
            if (point == 0 && length > 0)
                while (offset > 0)
                    offset -= length;
            return offset;
        }

        internal double beatOffsetAt(double time)
        {
            if (ControlPoints.Count == 0)
                return 0;

            int point = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
                if (ControlPoints[i].TimingChange && ControlPoints[i].offset <= time)
                    point = i;

            return ControlPoints[point].offset;
        }

        internal double beatLengthAt(double time)
        {
            return beatLengthAt(time, true);
        }

        internal double beatLengthAt(double time, bool allowMultiplier)
        {
            if (ControlPoints.Count == 0)
                return 0;

            int point = 0;
            int samplePoint = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
                if (ControlPoints[i].offset <= time)
                {
                    if (ControlPoints[i].TimingChange)
                        point = i;
                    else
                        samplePoint = i;
                }

            double mult = 1;

            if (allowMultiplier && samplePoint > point && ControlPoints[samplePoint].beatLength < 0)
                mult = -ControlPoints[samplePoint].beatLength / 100;

            return ControlPoints[point].beatLength * mult;
        }

        internal float bpmMultiplierAt(double time)
        {
            ControlPoint pt = controlPointAt(time);

            if (pt == null) return 1.0f;
            else return pt.bpmMultiplier;
        }

        internal ControlPoint controlPointAt(double time)
        {
            if (ControlPoints.Count == 0) return null;

            int point = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].offset <= time) point = i;
            }

            return ControlPoints[point];
        }

        #endregion

    }
}

