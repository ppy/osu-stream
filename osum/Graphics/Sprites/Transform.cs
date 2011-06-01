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
        OutDouble
    }

    [Flags]
    internal enum TransformationType
    {
        None = 0,
        Movement = 1,
        Fade = 2,
        Scale = 4,
        Rotation = 8,
        Colour = 16,
        ParameterFlipHorizontal = 32,
        ParameterFlipVertical = 64,
        MovementX = 128,
        MovementY = 256,
        VectorScale = 512,
        ParameterAdditive = 1024
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

        internal ClockTypes Clocking { get; set; }

        internal int Duration
        {
            get { return EndTime - StartTime; }
        }

        internal bool Initiated
        {
            get { return Clock.GetTime(Clocking) >= StartTime; }
        }

        internal bool Terminated
        {
            get { return Clock.GetTime(Clocking) >= EndTime && !Looping; }
        }

        internal bool Is(TransformationType type)
        {
            return (Type & type) > 0;
        }

        internal Vector2 CurrentVector
        {
            get
            {
                if (!this.Initiated)
                    return StartVector;

                if (this.Terminated)
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
                if (!this.Initiated)
                    return StartColour;

                if (this.Terminated)
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
                if (!this.Initiated)
                    return StartFloat;

                if (this.Terminated)
                    return EndFloat;

                return CalculateCurrent(StartFloat, EndFloat);
            }
        }

        private float CalculateCurrent(float start, float end)
        {
            int now = Clock.GetTime(Clocking);

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
            Clocking = ClockTypes.Game;
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
            Clocking = ClockTypes.Game;
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
            Clocking = ClockTypes.Game;
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
            Transformation t = (Transformation)this.MemberwiseClone();

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

        internal void Update()
        {
            int now = Clock.GetTime(Clocking);

            if (Looping && EndTime < now)
            {
                int endTimeBefore = EndTime;
                Offset(((now - EndTime) / (Duration + LoopDelay) + 1) * (Duration + LoopDelay));

                Console.WriteLine("Looping from " + endTimeBefore + " to " + StartTime + "-" + EndTime);
            }
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
                int now = Clock.GetTime(Clocking);

                float progress = pMathHelper.ClampToOne((float)(now - StartTime) / Duration);

                float rawSine = (float)Math.Sin(Pulses * Math.PI * (progress - 0.5f / Pulses));

                float diminishingMagnitude = (float)(Magnitude * Math.Pow(1 - progress, 2));

                return Math.Max(0, StartFloat + diminishingMagnitude * rawSine);
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
