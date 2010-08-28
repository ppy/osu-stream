using System;
using System.Collections.Generic;
using OpenTK;
using osu.Graphics.Primitives;
using osum.GameplayElements;
using osum.GameplayElements.HitObjects;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using Color = OpenTK.Graphics.Color4;

namespace osu.GameplayElements.HitObjects.Osu
{
    internal class Slider : HitObjectSpannable
    {
        internal readonly pAnimation spriteFollowBall;
        internal readonly pAnimation spriteFollowCircle;

        internal CurveTypes CurveType;
        internal double PathLength;

        /// <summary>
        /// Number of times the ball rebounds.
        /// </summary>
        internal int RepeatCount;

        private List<HitObjectSoundType> SoundTypeList;

        internal List<Vector2> controlPoints;
        private bool fullyDrawn;
        internal List<Vector2> smoothPoints;
        internal List<Line> drawableSegments;

        internal pSprite spriteSliderBody;

        HitCircle hitCircleStart;

        internal Slider(Vector2 startPosition, int startTime, bool newCombo, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes)
            : base(startPosition, startTime, soundType, newCombo)
        {
            CurveType = curveType;

            controlPoints = sliderPoints;

            RepeatCount = Math.Max(1, repeatCount);

            if (soundTypes != null && soundTypes.Count > 0)
                SoundTypeList = soundTypes;

            PathLength = pathLength;

            Type = HitObjectType.Slider;

            spriteFollowCircle =
                new pAnimation(SkinManager.LoadAll("sliderfollowcircle"), FieldTypes.Gamefield512x384,
                               OriginTypes.Centre, ClockTypes.Audio, Position, 0.99f, false, Color.White);
            spriteFollowCircle.SetFramerateFromSkin();

            pTexture[] sliderballtextures = SkinManager.LoadAll("sliderb");

            spriteFollowBall =
                new pAnimation(sliderballtextures, FieldTypes.Gamefield512x384, OriginTypes.Centre,
                               ClockTypes.Audio, Position, 0.99f, false, Color.White);

            CalculateSplines();

            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1,
                startTime, startTime);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);

            spriteFollowBall.Transform(fadeIn);
            spriteFollowBall.Transform(fadeOut);

            spriteFollowCircle.Transform(fadeIn);
            spriteFollowCircle.Transform(fadeOut);

            SpriteCollection.Add(spriteFollowBall);
            SpriteCollection.Add(spriteFollowCircle);

            hitCircleStart = new HitCircle(Position, StartTime, newCombo, soundType);
            
            SpriteCollection.AddRange(hitCircleStart.SpriteCollection);
        }

        internal override bool IsVisible
        {
            get
            {
                return
                    Clock.AudioTime >= StartTime - DifficultyManager.PreEmpt &&
                    Clock.AudioTime <= EndTime + DifficultyManager.FadeOut;
            }
        }

        internal override Color Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                base.Colour = value;
                hitCircleStart.Colour = value;
            }
        }

        private void CalculateSplines()
        {
            smoothPoints = pMathHelper.CreateBezier(controlPoints, 30);

            //adjust the line to be of maximum length specified...
            float currentLength = 0;

            drawableSegments = new List<Line>();

            for (int i = 1; i < smoothPoints.Count; i++)
            {
                Line l = new Line(smoothPoints[i], smoothPoints[i - 1]);
                drawableSegments.Add(l);

                float lineLength = l.rho;

                if (lineLength + currentLength > PathLength)
                {
                    l.p2 = l.p1 + Vector2.Normalize((l.p2 - l.p1) * (float)(l.rho - (PathLength - currentLength)));
                    l.Recalc();

                    currentLength += l.rho;
                    break; //we are done.
                }

                currentLength += l.rho;
            }

            PathLength = currentLength;
            EndTime = StartTime + (int)(1000 * PathLength / DifficultyManager.SliderVelocity);
        }


        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public override void Update()
        {
            if (!IsVisible)
                return;

            float progress = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime));

            int currentSegmentIndex = (int)(drawableSegments.Count * progress);

            Line currentSegment = drawableSegments[Math.Min(drawableSegments.Count - 1, currentSegmentIndex)];

            spriteFollowBall.Position = currentSegment.p1;
            spriteFollowBall.Rotation = currentSegment.theta + (float)Math.PI;

            spriteFollowCircle.Position = currentSegment.p1;
        }
    }

    internal enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear
    } ;
}