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
        OffsetX
    }

    internal static class TransformStore
    {
        static Queue<Transformation> transformations = new Queue<Transformation>();

        public static void Initialize()
        {
            for (int i = 0; i < 1000; i++)
                transformations.Enqueue(new Transformation());
        }

        public static Transformation Make()
        {
            return transformations.Dequeue();
        }
    }

    internal class TransformationC : Transformation
    {
        internal Color4 StartColour;
        internal Color4 EndColour;

        internal TransformationC(Color4 source, Color4 destination, int start, int end, EasingTypes easing = EasingTypes.None)
        {
            Type = TransformationType.Colour;
            StartColour = source;
            EndColour = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
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
    }

    internal class TransformationV : Transformation
    {
        internal Vector2 StartVector;
        internal Vector2 EndVector;

        internal TransformationV(Vector2 source, Vector2 destination, int start, int end, EasingTypes easing = EasingTypes.None)
        {
            Type = TransformationType.Movement;
            StartVector = source;
            EndVector = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        public TransformationV()
        {
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
    }

    internal class TransformationF : Transformation
    {
        internal float StartFloat;
        internal float EndFloat;

        internal TransformationF(TransformationType type, float source, float destination, int start, int end, EasingTypes easing = EasingTypes.None)
        {
            Type = type;
            StartFloat = source;
            EndFloat = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        public TransformationF()
        {
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
    }

    internal class Transformation : IComparable<Transformation>
    {
        public bool Looping;
        public ushort LoopDelay;

        internal EasingTypes Easing;


        internal int StartTime;
        internal int EndTime;
        internal TransformationType Type;

        internal bool Initiated;
        internal bool Terminated;

        /// <summary>
        /// Current time for transformation. Updated by running Update().
        /// </summary>
        protected int now;

        internal int Duration
        {
            get { return EndTime - StartTime; }
        }

        protected virtual float CalculateCurrent(float start, float end)
        {
            float progress = (float)(now - StartTime) / Duration;

            switch (Easing)
            {
                case EasingTypes.InDouble:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - progress, 4));
                case EasingTypes.In:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - progress, 2));
                case EasingTypes.InHalf:
                    return pMathHelper.Lerp(end, start, (float)Math.Pow(1 - progress, 1.5));
                case EasingTypes.Out:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow(progress, 2));
                case EasingTypes.OutHalf:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow(progress, 1.5));
                case EasingTypes.OutDouble:
                    return pMathHelper.Lerp(start, end, (float)Math.Pow(progress, 4));
                case EasingTypes.InOut:
                    return start + (float)(-2 * Math.Pow(progress, 3) + 3 * Math.Pow(progress, 2)) * (end - start);
                default:
                case EasingTypes.None:
                    return pMathHelper.Lerp(start, end, progress);
            }
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
            return (Transformation)MemberwiseClone();
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
            return string.Format("{4} {0}-{1}", StartTime, EndTime, Type);
        }

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

    internal class TransformationBounce : TransformationF
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
                    return Math.Max(0, EndFloat + diminishingMagnitude * rawSine);
                else
                    return EndFloat + diminishingMagnitude * rawSine;
            }
        }
    }

    internal class NullTransform : Transformation
    {
        public NullTransform(int startTime, int endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }
    }


}
