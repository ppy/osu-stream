using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    internal enum EasingTypes
    {
        None,
        In,
        Out,
        InHalf,
        OutHalf,
        InDouble,
        OutDouble,
        InOut
    }

    [Flags]
    internal enum TransformationType
    {
        None = 0,
        Movement,
        Fade,
        Scale,
        Rotation,
        Colour,
        ParameterFlipHorizontal,
        ParameterFlipVertical,
        MovementX,
        MovementY,
        VectorScale,
        ParameterAdditive,
        OffsetX
    }

    internal class Transformation : IComparable<Transformation>
    {
        internal int Tag;
        public bool Looping;
        public int LoopDelay;

        internal EasingTypes Easing { get; set; }

        internal Vector2 StartVector;
        internal Color4 StartColour;
        internal float StartFloat;

        internal Vector2 EndVector;
        internal Color4 EndColour;
        internal float EndFloat;

        internal int StartTime;
        internal int EndTime;
        internal TransformationType Type;

        internal int Duration
        {
            get { return EndTime - StartTime; }
        }

        internal bool Initiated;
        internal bool Terminated;

        internal bool Is(TransformationType type)
        {
            return (Type & type) > 0;
        }

        internal Vector2 CurrentVector
        {
            get
            {
                if (!Initiated)
                    return StartVector;

                if (Terminated)
                    return EndVector;

                return new Vector2(
                    CalculateCurrent(StartVector.X, EndVector.X),
                    CalculateCurrent(StartVector.Y, EndVector.Y)
                );
            }
        }

        internal Color4 CurrentColour
        {
            get
            {
                if (!Initiated)
                    return StartColour;

                if (Terminated)
                    return EndColour;

                return new Color4(
                    CalculateCurrent(StartColour.R, EndColour.R),
                    CalculateCurrent(StartColour.G, EndColour.G),
                    CalculateCurrent(StartColour.B, EndColour.B),
                    CalculateCurrent(StartColour.A, EndColour.A)
                );
            }
        }

        internal virtual float CurrentFloat
        {
            get
            {
                if (!Initiated)
                    return StartFloat;

                if (Terminated)
                    return EndFloat;

                return CalculateCurrent(StartFloat, EndFloat);
            }
        }

        protected virtual float CalculateCurrent(float start, float end)
        {
            switch (Easing)
            {
                case EasingTypes.InDouble:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - (float)(now - StartTime) / Duration, 4));
                case EasingTypes.In:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - (float)(now - StartTime) / Duration, 2));
                case EasingTypes.InHalf:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - (float)(now - StartTime) / Duration, 1.5));

                case EasingTypes.Out:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow((float)(now - StartTime) / Duration, 2));
                case EasingTypes.OutHalf:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow((float)(now - StartTime) / Duration, 1.5));
                case EasingTypes.OutDouble:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow((float)(now - StartTime) / Duration, 4));
                case EasingTypes.InOut:
                    float progress = pMathHelper.ClampToOne((float)(now - StartTime) / Duration);
                    return start + (float)(-2 * Math.Pow(progress, 3) + 3 * Math.Pow(progress, 2)) * (end - start);
                default:
                case EasingTypes.None:
                    return pMathHelper.Lerp(start, end, (float)(now - StartTime) / Duration);
            }
        }

        internal Transformation(Vector2 source, Vector2 destination, int start, int end)
            : this(TransformationType.Movement, source, destination, start, end, EasingTypes.None)
        {
        }

        internal Transformation(Vector2 source, Vector2 destination, int start, int end, EasingTypes easing)
            : this(TransformationType.Movement, source, destination, start, end, easing)
        {
        }

        internal Transformation(TransformationType type, Vector2 source, Vector2 destination, int start, int end)
            : this(type, source, destination, start, end, EasingTypes.None)
        {
        }

        internal Transformation(TransformationType type, Vector2 source, Vector2 destination, int start, int end, EasingTypes easing)
        {
            Type = type;
            StartVector = source;
            EndVector = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        internal Transformation(Color4 source, Color4 destination, int start, int end)
            : this(source, destination, start, end, EasingTypes.None)
        {
        }

        internal Transformation(Color4 source, Color4 destination, int start, int end, EasingTypes easing)
        {
            Type = TransformationType.Colour;
            StartColour = source;
            EndColour = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        internal Transformation(TransformationType type, float source, float destination, int start, int end)
            : this(type, source, destination, start, end, EasingTypes.None)
        {
        }

        protected Transformation(TransformationType type)
        {
            Type = type;
        }

        internal Transformation(TransformationType type, float source, float destination, int start, int end, EasingTypes easing)
        {
            Type = type;
            StartFloat = source;
            EndFloat = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        public Transformation()
        {
        }

        #region IComparable<Transformation> Members

        public int CompareTo(Transformation other)
        {
            int compare;

            if ((compare = StartTime.CompareTo(other.StartTime)) != 0)
                return compare;

            if ((compare = EndTime.CompareTo(other.EndTime)) != 0)
                return compare;

            return Type.CompareTo(other.Type);
        }

        #endregion

        internal Transformation Clone()
        {
            Transformation t = (Transformation)MemberwiseClone();

            return t;
        }

        internal Transformation CloneReverse()
        {
            Transformation t = new Transformation();
            t.StartFloat = EndFloat;
            t.StartColour = EndColour;
            t.StartVector = EndVector;
            t.EndFloat = StartFloat;
            t.EndColour = StartColour;
            t.EndVector = StartVector;
            t.StartTime = StartTime;
            t.EndTime = EndTime;
            t.Type = Type;

            switch (Easing)
            {
                case EasingTypes.In:
                    t.Easing = EasingTypes.Out;
                    break;
                case EasingTypes.Out:
                    t.Easing = EasingTypes.In;
                    break;
                default:
                    t.Easing = EasingTypes.None;
                    break;
            }

            return t;
        }

        /// <summary>
        /// Offsets the transformation by the specified amount.
        /// </summary>
        /// <param name="amount">Number of milliseconds to offset by.</param>
        internal void Offset(int amount)
        {
            EndTime += amount;
            StartTime += amount;
        }

        public override string ToString()
        {
            return string.Format("{4} {0}-{1} {2} to {3}",
                StartTime, EndTime, StartFloat, EndFloat, Type);
        }

        /// <summary>
        /// Current time for transformation. Updated by running Update().
        /// </summary>
        protected int now;

        internal void Update(int time)
        {
            now = time;

            if (Looping)
            {
                if (EndTime < now)
                {
                    int duration = Duration + LoopDelay;
                    Offset(((now - EndTime) / duration + 1) * duration);
                }
            }

            Initiated = now >= StartTime;
            Terminated = Initiated && now > EndTime && !Looping;
        }
    }

    internal class TransformationBounce : Transformation
    {
        private float Magnitude;
        private float Pulses;

        internal TransformationBounce(int startTime, int endTime, float aimSize, float magnitude, float pulses)
            : this(TransformationType.Scale, startTime, endTime, aimSize, magnitude, pulses)
        { }

        internal TransformationBounce(TransformationType type, int startTime, int endTime, float aimSize, float magnitude, float pulses)
            : base(type, aimSize, aimSize, startTime, endTime)
        {
            Magnitude = magnitude;
            Pulses = pulses;
        }

        internal override float CurrentFloat
        {
            get
            {
                float progress = pMathHelper.ClampToOne((float)(now - StartTime) / Duration);

                float rawSine = (float)Math.Sin(Pulses * Math.PI * (progress - 0.5f / Pulses));

                float diminishingMagnitude = (float)(Magnitude * Math.Pow(1 - progress, 2));

                if (Type == TransformationType.Scale)
                    return Math.Max(0, StartFloat + diminishingMagnitude * rawSine);
                else
                    return StartFloat + diminishingMagnitude * rawSine;
            }
        }
    }

    internal class NullTransform : Transformation
    {
        public NullTransform(int startTime, int endTime)
            : base(TransformationType.None, 0, 0, startTime, endTime)
        {
        }
    }


}
