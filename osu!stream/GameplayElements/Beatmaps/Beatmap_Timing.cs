//  Beatmap.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert

using System.Collections.Generic;

namespace osum.GameplayElements.Beatmaps
{
    public partial class Beatmap
    {
        public List<ControlPoint> ControlPoints = new List<ControlPoint>();

        public double DifficultySliderMultiplier;
        public double DifficultySliderTickRate;

        /// <summary>
        /// Beats the offset at.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public double beatOffsetCloseToZeroAt(double time)
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

        public double beatOffsetAt(double time)
        {
            if (ControlPoints.Count == 0)
                return 0;

            int point = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
                if (ControlPoints[i].TimingChange && ControlPoints[i].offset <= time)
                    point = i;

            return ControlPoints[point].offset;
        }

        public double beatLengthAt(double time)
        {
            return beatLengthAt(time, false);
        }

        public double beatLengthAt(double time, bool allowMultiplier)
        {
            if (ControlPoints.Count == 0)
                return 0;

            int point = 0;
            int samplePoint = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].offset <= time)
                {
                    if (ControlPoints[i].TimingChange)
                        point = i;
                    else
                        samplePoint = i;
                }
                else
                    break;
            }

            double mult = 1;

            if (allowMultiplier && samplePoint > point && ControlPoints[samplePoint].beatLength < 0)
                mult = -ControlPoints[samplePoint].beatLength / 100;

            return ControlPoints[point].beatLength * mult;
        }

        public float bpmMultiplierAt(double time)
        {
            ControlPoint pt = controlPointAt(time);

            if (pt == null) return 1.0f;

            return pt.bpmMultiplier;
        }

        public ControlPoint controlPointAt(double time)
        {
            if (ControlPoints.Count == 0) return null;

            int point = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].offset <= time) point = i;
            }

            return ControlPoints[point];
        }
    }
}