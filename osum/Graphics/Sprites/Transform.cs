using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    public class Transform
    {
        public EasingType Easing { get; private set; }

        public Vector2 StartVector { get; private set; }
        public Color4 StartColour { get; private set; }
        public float StartFloat { get; private set; }

        public Vector2 EndVector { get; private set; }
        public Color4 EndColour { get; private set; }
        public float EndFloat { get; private set; }

        public int StartTime { get; private set; }
        public int EndTime { get; private set; }
        public TransformType Type { get; private set; }

        public int Duration
        {
            get { return EndTime - StartTime; }
        }

        public bool Initiated
        {
            get { return Clock.Time >= StartTime; }
        }

        public bool Terminated
        {
            get { return Clock.Time > EndTime; }
        }

        public bool Is(TransformType type)
        {
            return (Type & type) > 0;
        }

        public Vector2 CurrentVector
        {
            get
            {
                if (Clock.Time <= StartTime)
                    return StartVector;

                if (Clock.Time >= EndTime)
                    return EndVector;

                return new Vector2(
                    CalculateCurrent(StartVector.X, EndVector.X),
                    CalculateCurrent(StartVector.Y, EndVector.Y)
                );
            }
        }

        public Color4 CurrentColour
        {
            get
            {
                if (Clock.Time <= StartTime)
                    return StartColour;

                if (Clock.Time >= EndTime)
                    return EndColour;

                return new Color4(
                    CalculateCurrent(StartColour.R, EndColour.R),
                    CalculateCurrent(StartColour.G, EndColour.G),
                    CalculateCurrent(StartColour.B, EndColour.B),
                    1
                );
            }
        }

        public float CurrentFloat
        {
            get
            {
                if (Clock.Time <= StartTime)
                    return StartFloat;

                if (Clock.Time >= EndTime)
                    return EndFloat;

                return CalculateCurrent(StartFloat, EndFloat);
            }
        }

        private float CalculateCurrent(float start, float end)
        {
            switch (Easing)
            {
                case EasingType.In:
                    return OsumMathHelper.Lerp(end, start, (float)Math.Pow(1 - (float)(Clock.Time - StartTime) / Duration, 2));

                case EasingType.Out:
                    return OsumMathHelper.Lerp(start, end, (float)Math.Pow((float)(Clock.Time - StartTime) / Duration, 2));

                default:
                case EasingType.None:
                    return OsumMathHelper.Lerp(start, end, (float)(Clock.Time - StartTime) / Duration);
            }
        }

        public Transform(Vector2 source, Vector2 destination, int start, int end)
            : this(TransformType.Movement, source, destination, start, end, EasingType.None)
        {
        }

        public Transform(Vector2 source, Vector2 destination, int start, int end, EasingType easing)
            : this(TransformType.Movement, source, destination, start, end, easing)
        {
        }

        public Transform(TransformType type, Vector2 source, Vector2 destination, int start, int end)
            : this(type, source, destination, start, end, EasingType.None)
        {
        }

        public Transform(TransformType type, Vector2 source, Vector2 destination, int start, int end, EasingType easing)
        {
            Type = type;
            StartVector = source;
            EndVector = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        public Transform(Color4 source, Color4 destination, int start, int end)
            : this(source, destination, start, end, EasingType.None)
        {
        }

        public Transform(Color4 source, Color4 destination, int start, int end, EasingType easing)
        {
            Type = TransformType.Colour;
            StartColour = source;
            EndColour = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }

        public Transform(TransformType type, float source, float destination, int start, int end)
            : this(type, source, destination, start, end, EasingType.None)
        {
        }

        public Transform(TransformType type, float source, float destination, int start, int end, EasingType easing)
        {
            Type = type;
            StartFloat = source;
            EndFloat = destination;
            StartTime = start;
            EndTime = end;
            Easing = easing;
        }
    }

    public enum EasingType
    {
        None,
        In,
        Out
    }

    [Flags]
    public enum TransformType
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
    } ;
}
