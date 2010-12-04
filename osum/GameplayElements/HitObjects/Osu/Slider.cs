using System;
using System.Collections.Generic;
using OpenTK;
using osum.Graphics.Primitives;
using osum.GameplayElements;
using osum.GameplayElements.HitObjects;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using Color = OpenTK.Graphics.Color4;
using osum;
using OpenTK;

#if IPHONE
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif
using osum.Graphics.Renderers;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;

namespace osum.GameplayElements.HitObjects.Osu
{
    internal class Slider : HitObjectSpannable
    {
        #region Sprites

        /// <summary>
        /// Sprite for the animated ball (visible during active time).
        /// </summary>
        internal readonly pAnimation spriteFollowBall;

        /// <summary>
        /// Sprite for the follow-circle (visible during tracking).
        /// </summary>
        internal readonly pSprite spriteFollowCircle;

        /// <summary>
        /// Sprite for slider body (path).
        /// </summary>
        internal pSprite spriteSliderBody;

        #endregion

        /// <summary>
        /// Type of curve generation.
        /// </summary>
        internal CurveTypes CurveType;

        /// <summary>
        /// Total length of this slider in gamefield pixels.
        /// </summary>
        internal double PathLength;

        /// <summary>
        /// Number of times the ball rebounds.
        /// </summary>
        internal int RepeatCount;

        /// <summary>
        /// A list of soundTypes for each end-point on the slider.
        /// </summary>
        private List<HitObjectSoundType> SoundTypeList;

        /// <summary>
        /// The raw control points as read from the beatmap file.
        /// </summary>
        internal List<Vector2> controlPoints;

        /// <summary>
        /// Line segments which are to be drawn to the screen (based on smoothPoints).
        /// </summary>
        internal List<Line> drawableSegments;

        /// <summary>
        /// The path texture
        /// </summary>
        internal pTexture sliderBodyTexture;

        /// <summary>
        /// How much of the slider (path) we have drawn.
        /// </summary>
        internal double lengthDrawn;

        /// <summary>
        /// The last segment (from drawableSegments) that has been drawn.
        /// </summary>
        internal int lastDrawnSegmentIndex;

        /// <summary>
        /// Cumulative list of curve lengths up to AND INCLUDING a given DrawableSegment.
        /// </summary>
        internal List<double> cumulativeLengths;

        /// <summary>
        /// Track bounding rectangle measured in SCREEN COORDINATES
        /// </summary>
        internal Rectangle trackBounds;
        internal Rectangle trackBoundsNative;

        /// <summary>
        /// Sprites which are stuck to the start position of the slider path.
        /// </summary>
        private List<pSprite> spriteCollectionStart = new List<pSprite>();

        /// <summary>
        /// Sprites which are stuck to the end position of the slider path. May be used to hide rendering artifacts.
        /// </summary>
        private List<pSprite> spriteCollectionEnd = new List<pSprite>();

        private List<pSprite> spriteCollectionScoringPoints = new List<pSprite>();

        /// <summary>
        /// The points in progress that ticks are to be placed (based on decimal values 0 - 1).
        /// </summary>
        private List<double> scoringPoints = new List<double>();

        const bool NO_SNAKING = false;
        const bool PRERENDER_ALL = false;

        /// <summary>
        /// The start hitcircle is used for initial judging, and explodes as would be expected of a normal hitcircle. Also handles combo numbering.
        /// </summary>
        HitCircle hitCircleStart;

        internal Slider(HitObjectManager hitObjectManager, Vector2 startPosition, int startTime, bool newCombo, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes)
            : base(hitObjectManager, startPosition, startTime, soundType, newCombo)
        {
            CurveType = curveType;

            controlPoints = sliderPoints;

            if (sliderPoints[0] != startPosition)
                sliderPoints.Insert(0, startPosition);

            RepeatCount = Math.Max(1, repeatCount);

            if (soundTypes != null && soundTypes.Count > 0)
                SoundTypeList = soundTypes;

            PathLength = pathLength;

            Type = HitObjectType.Slider;

            spriteFollowCircle =
                new pSprite(TextureManager.Load(OsuTexture.sliderfollowcircle), FieldTypes.Gamefield512x384,
                               OriginTypes.Centre, ClockTypes.Audio, Position, 0.99f, false, Color.White);

            pTexture[] sliderballtextures = TextureManager.LoadAnimation("sliderb");

            spriteFollowBall =
                new pAnimation(sliderballtextures, FieldTypes.Gamefield512x384, OriginTypes.Centre,
                               ClockTypes.Audio, Position, 0.99f, false, Color.White);

            CalculateSplines();

            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1,
                startTime, startTime);
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);

            hitCircleStart = new HitCircle(null, Position, StartTime, newCombo, SoundTypeList != null ? SoundTypeList[0] : SoundType);

            spriteSliderBody = new pSprite(null, FieldTypes.Native, OriginTypes.TopLeft,
                                   ClockTypes.Audio, Vector2.Zero, SpriteManager.drawOrderBwd(EndTime + 14),
                                   false, Color.White);

            spriteSliderBody.Transform(fadeInTrack);
            spriteSliderBody.Transform(fadeOut);

            spriteFollowBall.Transform(fadeIn);
            spriteFollowBall.Transform(fadeOut);

            SpriteCollection.Add(spriteFollowBall);
            SpriteCollection.Add(spriteFollowCircle);
            SpriteCollection.Add(spriteSliderBody);

            DimCollection.Add(spriteSliderBody);

            //Start and end circles

            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));
            if (repeatCount > 2)
                spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White) { Additive = true });

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));


            spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 12), false, Color.White));
            spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 11), false, Color.White));
            if (repeatCount > 1)
                spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 10), false, Color.White) { Additive = true });

            spriteCollectionEnd.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionEnd.ForEach(s => s.Transform(fadeOut));

            //endpoint angular caltulations

            startAngle = (float)Math.Atan2(drawableSegments[0].p1.Y - drawableSegments[0].p2.Y, drawableSegments[0].p1.X - drawableSegments[0].p2.X);
            endAngle = (float)Math.Atan2(drawableSegments[drawableSegments.Count - 1].p1.Y - drawableSegments[drawableSegments.Count - 1].p2.Y,
                                         drawableSegments[drawableSegments.Count - 1].p1.X - drawableSegments[drawableSegments.Count - 1].p2.X);

            //tick calculations
            double distanceBetweenTicks = hitObjectManager.SliderScoringPointDistance;

            double tickCount = PathLength / distanceBetweenTicks;
            int actualTickCount = (int)Math.Ceiling(Math.Round(tickCount,1)) - 1;

            double tickNumber = 0;
            while (++tickNumber <= actualTickCount)
            {
                double progress = (tickNumber * distanceBetweenTicks) / PathLength;

                scoringPoints.Add(progress);

                pSprite scoringDot =
                                    new pSprite(TextureManager.Load(OsuTexture.sliderscorepoint),
                                                FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, positionAtProgress(progress),
                                                SpriteManager.drawOrderBwd(EndTime + 13), false, Color.White);

                scoringDot.Transform(new Transformation(TransformationType.Fade, 0, 1,
                    startTime - DifficultyManager.PreEmptSnakeStart + (int)((DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd) * progress),
                    startTime - DifficultyManager.PreEmptSnakeStart + (int)((DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd) * progress) + 100));

                spriteCollectionScoringPoints.Add(scoringDot);
            }

            spriteCollectionScoringPoints.ForEach(s => s.Transform(fadeOut));

            SpriteCollection.AddRange(hitCircleStart.SpriteCollection);
            SpriteCollection.AddRange(spriteCollectionStart);
            SpriteCollection.AddRange(spriteCollectionEnd);
            SpriteCollection.AddRange(spriteCollectionScoringPoints);

            DimCollection.AddRange(hitCircleStart.SpriteCollection);
            DimCollection.AddRange(spriteCollectionStart);
            DimCollection.AddRange(spriteCollectionEnd);
            DimCollection.AddRange(spriteCollectionScoringPoints);

            if (PRERENDER_ALL)
                UpdatePathTexture();
        }

        private void CalculateSplines()
        {
            List<Vector2> smoothPoints;

            switch (CurveType)
            {
                case CurveTypes.Bezier:
                default:
                    smoothPoints = new List<Vector2>();

                    int lastIndex = 0;

                    for (int i = 0; i < controlPoints.Count; i++)
                    {
                        bool multipartSegment = i < controlPoints.Count - 2 && controlPoints[i] == controlPoints[i + 1];

                        if (multipartSegment || i == controlPoints.Count - 1)
                        {
                            List<Vector2> thisLength = controlPoints.GetRange(lastIndex, i - lastIndex + 1);

                            smoothPoints.AddRange(pMathHelper.CreateBezier(controlPoints, 10));

                            if (multipartSegment) i++;
                            //Need to skip one point since we consuned an extra.

                            lastIndex = i;
                        }
                    }
                    break;
                case CurveTypes.Catmull:
                    smoothPoints = pMathHelper.CreateCatmull(controlPoints, 10);
                    break;
                case CurveTypes.Linear:
                    smoothPoints = pMathHelper.CreateLinear(controlPoints, 10);
                    break;
            }

            //adjust the line to be of maximum length specified...
            double currentLength = 0;

            drawableSegments = new List<Line>();
            cumulativeLengths = new List<double>();

            for (int i = 1; i < smoothPoints.Count; i++)
            {
                Line l = new Line(smoothPoints[i - 1], smoothPoints[i]);
                drawableSegments.Add(l);

                float lineLength = l.rho;

                if (lineLength + currentLength > PathLength)
                {
                    l.p2 = l.p1 + Vector2.Normalize(l.p2 - l.p1) * (float)(PathLength - currentLength);
                    l.Recalc();

                    currentLength += l.rho;
                    cumulativeLengths.Add(currentLength);
                    break; //we are done.
                }

                currentLength += lineLength;
                cumulativeLengths.Add(currentLength);
            }

            PathLength = currentLength;
            EndTime = StartTime + (int)(1000 * PathLength / m_HitObjectManager.VelocityAt(StartTime) * RepeatCount);
        }

        /// <summary>
        /// Find the extreme values of the given curve in the form of a box.
        /// </summary>
        private static RectangleF FindBoundingBox(List<Line> curve, float radius)
        {
            // TODO: FIX this to use SCREEN coordinates instead of osupixels.

            if (curve.Count == 0) throw new ArgumentException("Curve must have at least one segment.");

            float Left = (int)curve[0].p1.X;
            float Top = (int)curve[0].p1.Y;
            float Right = (int)curve[0].p1.X;
            float Bottom = (int)curve[0].p1.Y;

            foreach (Line l in curve)
            {
                Left = Math.Min(Left, l.p1.X - radius);
                Left = Math.Min(Left, l.p2.X - radius);

                Top = Math.Min(Top, l.p1.Y - radius);
                Top = Math.Min(Top, l.p2.Y - radius);

                Right = Math.Max(Right, l.p1.X + radius);
                Right = Math.Max(Right, l.p2.X + radius);

                Bottom = Math.Max(Bottom, l.p1.Y + radius);
                Bottom = Math.Max(Bottom, l.p2.Y + radius);
            }

            return new System.Drawing.RectangleF(Left, Top, Right - Left, Bottom - Top);
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
                spriteCollectionStart[0].Colour = value;
                spriteCollectionEnd[0].Colour = value;
            }
        }

        internal override int ComboNumber
        {
            get
            {
                return hitCircleStart.ComboNumber;
            }
            set
            {
                hitCircleStart.ComboNumber = value;
            }
        }

        internal override Vector2 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                Vector2 change = value - position;

                base.Position = value;

                drawableSegments.ForEach(d => { d.Move(d.p1 + change, d.p2 + change); });

                hitCircleStart.Position = value;
            }
        }

        internal override Vector2 EndPosition
        {
            get
            {
                return RepeatCount % 2 == 0 ? position : drawableSegments[drawableSegments.Count - 1].p2;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal override bool HitTest(TrackingPoint tracking)
        {
            return hitCircleStart.HitTest(tracking);
        }

        protected override ScoreChange HitAction()
        {
            ScoreChange startCircleChange = hitCircleStart.Hit();

            //triggered on the first hit
            if (startCircleChange > 0)
            {
                hitCircleStart.HitAnimation(startCircleChange);

                scoringEndpointsHit++;
                return ScoreChange.SliderEnd;
            }

            return ScoreChange.Ignore;
        }

        /// <summary>
        /// Tracking point associated with the slider.
        /// </summary>
        TrackingPoint trackingPoint;

        /// <summary>
        /// Gets a value indicating whether this instance is tracking.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is tracking; otherwise, <c>false</c>.
        /// </value>
        bool isTracking { get { return trackingPoint != null; } }

        /// <summary>
        /// Number of successfully hit end-points. Includes the start circle.
        /// </summary>
        int scoringEndpointsHit;

        /// <summary>
        /// Index of the last end-point to be judged. Used to keep track of judging calculations.
        /// </summary>
        int lastJudgedEndpoint;

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal override ScoreChange CheckScoring()
        {
            if (!IsActive) //would be unnecessary to do anything at this point.
                return ScoreChange.Ignore;

            bool wasTracking = isTracking;

            if (trackingPoint == null)
            {
                if (InputManager.IsPressed)
                {
                    //todo: isPressed should *probably* be an attribute of a trackingPoint.
                    //this is only required at the moment with  mouse, an will always WORK correctly even with multiple touches, but logically doesn't make much sense.

                    //check each tracking point to find if any are usable
                    foreach (TrackingPoint p in InputManager.TrackingPoints)
                    {
                        if (pMathHelper.DistanceSquared(p.GamefieldPosition, TrackingPosition) < DifficultyManager.HitObjectRadius * DifficultyManager.HitObjectRadius)
                        {
                            trackingPoint = p;
                            break;
                        }
                    }
                }
            }
            else if (!trackingPoint.Valid || pMathHelper.DistanceSquared(trackingPoint.GamefieldPosition, TrackingPosition) > Math.Pow(DifficultyManager.HitObjectRadius * 2, 2))
                trackingPoint = null;

            //Check is the state of tracking changed.
            if (isTracking != wasTracking)
            {
                if (!isTracking)
                {
                    //End tracking.
                    spriteFollowCircle.Transformations.Clear();
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1, 1.4f, Clock.AudioTime, Clock.AudioTime + 150, EasingTypes.In));
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.AudioTime, Clock.AudioTime + 150, EasingTypes.None));

                }
                else
                {
                    //Begin tracking.
                    spriteFollowCircle.Transformations.Clear();
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 0.4f, 1.05f, Clock.AudioTime, Math.Min(EndTime,Clock.AudioTime + 200), EasingTypes.InHalf));
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1.05f, 1, Clock.AudioTime + 200, Math.Min(EndTime,Clock.AudioTime + 250), EasingTypes.OutHalf));
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 0, 1, Clock.AudioTime, Math.Min(EndTime,Clock.AudioTime + 140), EasingTypes.None));
                    spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 1, 1, Clock.AudioTime + 140, EndTime));
                }
            }

            //Check if we've hit a new endpoint...
            if ((int)progressCurrent != (int)progressLastUpdate)
            {
                lastJudgedEndpoint++;

                if (RepeatCount - lastJudgedEndpoint < 3 && RepeatCount - lastJudgedEndpoint > 0)
                {
                    //we can turn off some repeat arrows...
                    if (lastJudgedEndpoint % 2 == 0)
                        spriteCollectionStart[2].Transformations.Clear();
                    else
                        spriteCollectionEnd[2].Transformations.Clear();
                }

                if (isTracking)
                {
                    PlaySound(SoundTypeList != null ? SoundTypeList[lastJudgedEndpoint] : SoundType);

                    Transformation circleScaleOut = new Transformation(TransformationType.Scale, 1.0F, 1.9F,
                        Clock.Time, (int)(Clock.Time + (DifficultyManager.FadeOut * 0.7)), EasingTypes.In);

                    Transformation circleScaleOut2 = new Transformation(TransformationType.Scale, 1.9F, 2F,
                        (int)(Clock.Time + (DifficultyManager.FadeOut * 0.7)), (Clock.Time + DifficultyManager.FadeOut));

                    Transformation circleFadeOut = new Transformation(TransformationType.Fade, 1, 0,
                        Clock.Time, Clock.Time + DifficultyManager.FadeOut);

                    foreach (pSprite p in lastJudgedEndpoint % 2 == 0 ? spriteCollectionStart : spriteCollectionEnd)
                    {
                        //Burst the endpoint we just reached.
                        pSprite clone = p.Clone();

                        clone.Transformations.Clear();

                        clone.Clocking = ClockTypes.Game;

                        clone.Transform(circleScaleOut);
                        clone.Transform(circleScaleOut2);
                        clone.Transform(circleFadeOut);

                        m_HitObjectManager.spriteManager.Add(clone);
                    }

                    scoringEndpointsHit++;
                }

                if (RepeatCount - lastJudgedEndpoint == 0)
                {
                    //we've hit the end of the slider altogether.
                    spriteFollowBall.RunAnimation = false;
                    spriteFollowCircle.Transformations.Clear();

                    IsEndHit = true;

                    float amountHit = (float)scoringEndpointsHit / (lastJudgedEndpoint + 1);
                    ScoreChange amount;

                    if (amountHit == 1)
                        amount = ScoreChange.Hit300;
                    else if (amountHit > 0.8)
                        amount = ScoreChange.Hit100;
                    else if (amountHit > 0)
                        amount = ScoreChange.Hit50;
                    else
                        amount = ScoreChange.Miss;

                    return amount; //actual judging
                }

                lastJudgedScoringPoint = -1;

                return isTracking ? ScoreChange.SliderRepeat : ScoreChange.MissHpOnly;
            }
            else
            {
                //Check if we've hit a new scoringpoint...

                int judgePointNormalized = isReversing ? scoringPoints.Count - 1 - (lastJudgedScoringPoint + 1) : lastJudgedScoringPoint + 1;

                if (lastJudgedScoringPoint < scoringPoints.Count - 1 &&
                    (
                        (isReversing && normalizeProgress(progressCurrent) < scoringPoints[judgePointNormalized]) ||
                        (!isReversing && normalizeProgress(progressCurrent) > scoringPoints[judgePointNormalized])
                    )
                   )
                {
                    lastJudgedScoringPoint++;

                    if (isTracking)
                    {
                        AudioEngine.PlaySample(OsuSamples.SliderTick);

                        pSprite point = spriteCollectionScoringPoints[judgePointNormalized];

                        point.Alpha = 0;

                        spriteFollowCircle.Transformations.RemoveAll(t => t.Type == TransformationType.Scale);
                        spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1.05f, 1, Clock.AudioTime, Clock.AudioTime + 100, EasingTypes.OutHalf));

                        if (RepeatCount > progressCurrent + 1)
                        {
                            //we still have more repeats to go.
                            int nextRepeatStartTime = (int)(StartTime + (EndTime - StartTime) * (((int)progressCurrent + 1) / (float)RepeatCount));

                            spriteCollectionScoringPoints[judgePointNormalized].Transform(
                                new Transformation(TransformationType.Fade, 0, 1, nextRepeatStartTime - 100, nextRepeatStartTime));
                            spriteCollectionScoringPoints[judgePointNormalized].Transform(
                                new Transformation(TransformationType.Scale, 0, 1, nextRepeatStartTime - 100, nextRepeatStartTime));
                        }
                        else
                        {
                            //done with the point for good.
                            point.Transformations.Clear();
                        }

                        return ScoreChange.SliderTick;
                    }
                }
            }

            return ScoreChange.Ignore;
        }

        internal Vector2 TrackingPosition;
        private float startAngle;
        private float endAngle;

        /// <summary>
        /// Floating point progress from the previous update (used during scoring for checking scoring milestones).
        /// </summary>
        float progressLastUpdate;

        /// <summary>
        /// Floating point progress through the slider (0..1 for first length, 1..x for futher repeats)
        /// </summary>
        float progressCurrent;

        private double normalizeProgress(double progress)
        {
            while (progress > 2)
                progress -= 2;
            if (progress > 1)
                progress = 2 - progress;

            return progress;
        }

        private Line lineAtProgress(double progress)
        {
            progress = normalizeProgress(progress);

            double aimLength = PathLength * progress;

            //index is the index of the line segment that exceeds the required length (so we need to cut it back)
            int index = Math.Max(0, cumulativeLengths.FindIndex(l => l >= aimLength));

            double lengthAtIndex = cumulativeLengths[index];
            return drawableSegments[index];
        }
        
        private Vector2 positionAtProgress(double progress)
        {
            progress = normalizeProgress(progress);

            double aimLength = PathLength * progress;

            //index is the index of the line segment that exceeds the required length (so we need to cut it back)
            int index = Math.Max(0, cumulativeLengths.FindIndex(l => l >= aimLength));

            double lengthAtIndex = cumulativeLengths[index];
            Line currentLine = drawableSegments[index];

            //cut back the line to required exact length
            return currentLine.p1 + Vector2.Normalize(currentLine.p2 - currentLine.p1) * (float)(currentLine.rho - Math.Abs(lengthAtIndex - aimLength));
        }

        bool isReversing { get { return progressCurrent % 2 >= 1; } }

        /// <summary>
        /// Update all elements of the slider which aren't affected by user input.
        /// </summary>
        public override void Update()
        {
            progressLastUpdate = progressCurrent;
            progressCurrent = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime)) * RepeatCount;

            spriteFollowBall.Reverse = isReversing;

            //cut back the line to required exact length
            TrackingPosition = positionAtProgress(progressCurrent);

            if (IsVisible && (lengthDrawn < PathLength || sliderBodyTexture == null) && (Clock.AudioTime > StartTime - DifficultyManager.PreEmptSnakeStart))
                UpdatePathTexture();

            spriteFollowBall.Position = TrackingPosition;
            spriteFollowBall.Rotation = lineAtProgress(progressCurrent).theta;

            spriteFollowCircle.Position = TrackingPosition;

            //Adjust the angles of the end arrows
            if (RepeatCount > 1)
                spriteCollectionEnd[2].Rotation = endAngle + (float)((MathHelper.Pi / 32) * ((Clock.AudioTime % 300) / 300f - 0.5) * 2);
            if (RepeatCount > 2)
                spriteCollectionStart[2].Rotation = 3 + startAngle + (float)((MathHelper.Pi / 32) * ((Clock.AudioTime % 300) / 300f - 0.5) * 2);

            base.Update();
        }

        internal void DisposePathTexture()
        {
            if (sliderBodyTexture != null)
                sliderBodyTexture.Dispose();
            sliderBodyTexture = null;

#if IPHONE
            if (fbo > 0)
            {
                GL.Oes.DeleteFramebuffers(1,ref fbo);
                fbo = 0;
            }
#endif
        }

        /// <summary>
        /// Counter for number of frames skipped since last slider path render.
        /// </summary>
        int pathTextureUpdateSkippedFrames;
        private int lastJudgedScoringPoint = -1;

        uint fbo;

        /// <summary>
        /// Updates the slider's path texture if required.
        /// </summary>
        internal void UpdatePathTexture()
        {
            if (sliderBodyTexture == null) // Perform setup to begin drawing the slider track.
                CreatePathTexture();

            if (lengthDrawn == PathLength) return; //finished drawing already.

            // Snaking animation is IN PROGRESS
#if FBO
                int FirstSegmentIndex = lastSegmentIndex + 1;

                throw new NotImplementedException();
#else
            int FirstSegmentIndex = 0;

            // Length of the curve we're drawing up to.
            lengthDrawn = PathLength *
                          (double)(Clock.AudioTime - StartTime + DifficultyManager.PreEmptSnakeStart) /
                          (double)(DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd);

            while (lastDrawnSegmentIndex < cumulativeLengths.Count && cumulativeLengths[lastDrawnSegmentIndex] < lengthDrawn)
                lastDrawnSegmentIndex++;

            if (lastDrawnSegmentIndex >= cumulativeLengths.Count || NO_SNAKING)
            {
                lengthDrawn = PathLength;
                lastDrawnSegmentIndex = drawableSegments.Count - 1;
            }

            Line prev = null;
            if (FirstSegmentIndex > 0) prev = drawableSegments[FirstSegmentIndex - 1];

            if (lastDrawnSegmentIndex >= FirstSegmentIndex)
            {
                List<Line> partialDrawable = drawableSegments.GetRange(FirstSegmentIndex, lastDrawnSegmentIndex - FirstSegmentIndex + 1);

                Vector2 drawEndPosition = positionAtProgress(lengthDrawn / PathLength);
                spriteCollectionEnd.ForEach(s => s.Position = drawEndPosition);

                if (pathTextureUpdateSkippedFrames++ % 3 == 0 || lengthDrawn == PathLength)
                {
                    GL.PushMatrix();

#if IPHONE
                    int oldFBO = 0;
                    GL.GetInteger(All.FramebufferBindingOes, ref oldFBO);

                    GL.Oes.BindFramebuffer(All.FramebufferOes, fbo);


                    GL.Viewport(0, 0, trackBoundsNative.Width, trackBoundsNative.Height);
                    GL.MatrixMode(MatrixMode.Projection);

                    GL.LoadIdentity();
                    GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Top, trackBounds.Bottom, -1, 1);

                    GL.Clear((int)ClearBufferMask.ColorBufferBit);

                    m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                              DifficultyManager.HitObjectRadius, ColourIndex, prev);

                    //GL.TexParameter(TextureGl.SURFACE_TYPE, All.TextureMinFilter, (int)All.Nearest);
                    //GL.TexParameter(TextureGl.SURFACE_TYPE, All.TextureMagFilter, (int)All.Nearest);

                    GL.Oes.BindFramebuffer(All.FramebufferOes, oldFBO);

                    GL.Clear((int)ClearBufferMask.ColorBufferBit);
#else
                    GL.Viewport(0, 0, trackBoundsNative.Width, trackBoundsNative.Height);
                    GL.MatrixMode(MatrixMode.Projection);

                    GL.LoadIdentity();
                    GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Top, trackBounds.Bottom, -1, 1);

                    m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                              DifficultyManager.HitObjectRadius, ColourIndex, prev);


                    GL.BindTexture(TextureGl.SURFACE_TYPE, sliderBodyTexture.TextureGl.Id);
                    GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                    GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, sliderBodyTexture.TextureGl.potWidth, sliderBodyTexture.TextureGl.potHeight, 0);

                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
#endif

                    GameBase.Instance.SetViewport();

                    GL.PopMatrix();

                }
            }
#endif
        }

        /// <summary>
        /// Creates the texture which will hold the slider's path.
        /// </summary>
        private void CreatePathTexture()
        {
            RectangleF rectf = FindBoundingBox(drawableSegments, DifficultyManager.HitObjectRadius);

            trackBounds.X = (int)(rectf.X);
            trackBounds.Y = (int)(rectf.Y);
            trackBounds.Width = (int)rectf.Width + 1;
            trackBounds.Height = (int)rectf.Height + 1;

            trackBoundsNative.X = (int)((rectf.X + GameBase.GamefieldOffsetVector1.X) * GameBase.WindowRatio);
            trackBoundsNative.Y = (int)((rectf.Y + GameBase.GamefieldOffsetVector1.Y) * GameBase.WindowRatio);
            trackBoundsNative.Width = (int)(rectf.Width * GameBase.WindowRatio) + 1;
            trackBoundsNative.Height = (int)(rectf.Height * GameBase.WindowRatio) + 1;

            lengthDrawn = 0;
            lastDrawnSegmentIndex = 0;

            TextureGl gl = new TextureGl(trackBoundsNative.Width, trackBoundsNative.Height);

#if IPHONE
            gl.SetData(IntPtr.Zero, 0, All.Rgba);

#else
            int newtexid = GL.GenTexture();
            gl.SetData(newtexid);
#endif

            sliderBodyTexture = new pTexture(gl, trackBoundsNative.Width, trackBoundsNative.Height);

            spriteSliderBody.Texture = sliderBodyTexture;
            spriteSliderBody.Position = new Vector2(trackBoundsNative.X, trackBoundsNative.Y);

#if IPHONE
            int oldFBO = 0;
            GL.GetInteger(All.FramebufferBindingOes, ref oldFBO);

            // create framebuffer
            GL.Oes.GenFramebuffers(1, ref fbo);
            GL.Oes.BindFramebuffer(All.FramebufferOes, fbo);

            // attach renderbuffer
            GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes, All.Texture2D, gl.Id, 0);

            // unbind frame buffer
            GL.Oes.BindFramebuffer(All.FramebufferOes, oldFBO);
#endif
        }
    }


    internal enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear
    } ;
}