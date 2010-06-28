using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    internal enum EasingType
    {
        None,
        In,
        Out
    }

    [Flags]
    internal enum TransformType
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

    internal class Transform : IComparable<Transform>
    {
        internal EasingType Easing { get; private set; }

        internal Vector2 StartVector { get; private set; }
        internal Color4 StartColour { get; private set; }
        internal float StartFloat { get; private set; }

        internal Vector2 EndVector { get; private set; }
        internal Color4 EndColour { get; private set; }
        internal float EndFloat { get; private set; }

        internal int StartTime { get; private set; }
        internal int EndTime { get; private set; }
        internal TransformType Type { get; private set; }

        internal ClockType Clocking { get; set; }

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
            get { return Clock.GetTime(Clocking) >= EndTime; }
        }

        internal bool Is(TransformType type)
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
                    1
                );
            }
        }

        internal float CurrentFloat
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
                case EasingType.In:
                    return OsumMathHelper.Lerp(end, start, (float)Math.Pow(1 - (float)(now - StartTime) / Duration, 2));

                case EasingType.Out:
                    return OsumMathHelper.Lerp(start, end, (float)Math.Pow((float)(now - StartTime) / Duration, 2));

                default:
                case EasingType.None:
                    return OsumMathHelper.Lerp(start, end, (float)(now - StartTime) / Duration);
            }
        }

        internal Transform(Vector2 source, Vector2 destination, int start, int end)
            : this(TransformType.Movement, source, destination, start, end, EasingType.None)
        {
        }

        internal Transform(Vector2 source, Vector2 destination, int start, int end, EasingType easing)
            : this(TransformType.Movement, source, destination, start, end, easing)
        {
        }

        internal Transform(TransformType type, Vector2 source, Vector2 destination, int start, int end)
            : this(type, source, destination, start, end, EasingType.None)
        {
        }

        internal Transform(TransformType type, Vector2 source, Vector2 destination, int start, int end, EasingType easing)
        {
            Type = type;
            StartVector = source;
            EndVector = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
            Clocking = ClockType.Game;
        }

        internal Transform(Color4 source, Color4 destination, int start, int end)
            : this(source, destination, start, end, EasingType.None)
        {
        }

        internal Transform(Color4 source, Color4 destination, int start, int end, EasingType easing)
        {
            Type = TransformType.Colour;
            StartColour = source;
            EndColour = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
            Clocking = ClockType.Game;
        }

        internal Transform(TransformType type, float source, float destination, int start, int end)
            : this(type, source, destination, start, end, EasingType.None)
        {
        }

        internal Transform(TransformType type, float source, float destination, int start, int end, EasingType easing)
        {
            Type = type;
            StartFloat = source;
            EndFloat = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
            Clocking = ClockType.Game;
        }

        private Transform()
        {
        }

        #region IComparable<Transformation> Members

        public int CompareTo(Transform other)
        {
            int compare;

            if ((compare = StartTime.CompareTo(other.StartTime)) != 0) 
                return compare;

            if ((compare = EndTime.CompareTo(other.EndTime)) != 0) 
                return compare;

            return Type.CompareTo(other.Type);
        }

        #endregion

        internal Transform Clone()
        {
            Transform t = new Transform();
            t.StartFloat = StartFloat;
            t.StartColour = StartColour;
            t.StartVector = StartVector;
            t.EndFloat = EndFloat;
            t.EndColour = EndColour;
            t.EndVector = EndVector;
            t.Easing = Easing;
            t.StartTime = StartTime;
            t.EndTime = EndTime;
            t.Type = Type;

            return t;
        }

        internal Transform CloneReverse()
        {
            Transform t = new Transform();
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
                case EasingType.In:
                    t.Easing = EasingType.Out;
                    break;
                case EasingType.Out:
                    t.Easing = EasingType.In;
                    break;
                default:
                    t.Easing = EasingType.None;
                    break;
            }

            return t;
        }
    }
}
