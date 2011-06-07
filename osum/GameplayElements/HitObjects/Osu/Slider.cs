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

#if iOS
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
using osum.Input;
#endif

using System.Drawing;
using osum.Graphics.Renderers;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameModes;

namespace osum.GameplayElements.HitObjects.Osu
{
    internal class Slider : HitObjectSpannable
    {
        #region Sprites

        /// <summary>
        /// Sprite for the animated ball (visible during active time).
        /// </summary>
        internal pAnimation spriteFollowBall;

        /// <summary>
        /// Sprite for the follow-circle (visible during tracking).
        /// </summary>
        internal pSprite spriteFollowCircle;

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
        internal List<Line> drawableSegments = new List<Line>();

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
        internal List<double> cumulativeLengths = new List<double>();

        /// <summary>
        /// Track bounding rectangle measured in SCREEN COORDINATES
        /// </summary>
        internal Rectangle trackBounds;
        internal Rectangle trackBoundsNative;

        /// <summary>
        /// Sprites which are stuck to the start position of the slider path.
        /// </summary>
        protected List<pDrawable> spriteCollectionStart = new List<pDrawable>();

        /// <summary>
        /// Sprites which are stuck to the end position of the slider path. May be used to hide rendering artifacts.
        /// </summary>
        protected List<pDrawable> spriteCollectionEnd = new List<pDrawable>();

        private List<pDrawable> spriteCollectionScoringPoints = new List<pDrawable>();

        /// <summary>
        /// The points in progress that ticks are to be placed (based on decimal values 0 - 1).
        /// </summary>
        private List<double> scoringPoints = new List<double>();

        const bool NO_SNAKING = false;
        const bool PRERENDER_ALL = false;

        /// <summary>
        /// The start hitcircle is used for initial judging, and explodes as would be expected of a normal hitcircle. Also handles combo numbering.
        /// </summary>
        protected HitCircle hitCircleStart;

        internal Slider(HitObjectManager hitObjectManager, Vector2 startPosition, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes, double velocity, double tickDistance)
            : base(hitObjectManager, startPosition, startTime, soundType, newCombo, comboOffset)
        {
            CurveType = curveType;

            controlPoints = sliderPoints;

            if (sliderPoints[0] != startPosition)
                sliderPoints.Insert(0, startPosition);

            RepeatCount = Math.Max(1, repeatCount);

            if (soundTypes != null && soundTypes.Count > 0)
                SoundTypeList = soundTypes;

            PathLength = pathLength;
            Velocity = velocity;
            TickDistance = tickDistance;

            Type = HitObjectType.Slider;

            CalculateSplines();

            initializeSprites();
            initializeStartCircle();

            if (PRERENDER_ALL)
                UpdatePathTexture();
        }

        protected virtual void initializeStartCircle()
        {
            hitCircleStart = new HitCircle(null, Position, StartTime, NewCombo, ComboOffset, SoundTypeList != null ? SoundTypeList[0] : SoundType);
            Sprites.AddRange(hitCircleStart.Sprites);
            SpriteCollectionDim.AddRange(hitCircleStart.Sprites);
        }

        protected virtual void initializeSprites()
        {
            spriteFollowCircle =
    new pSprite(TextureManager.Load(OsuTexture.sliderfollowcircle), FieldTypes.GamefieldSprites,
                   OriginTypes.Centre, ClockTypes.Audio, Position, 0.99f, false, Color.White);

            pTexture[] sliderballtextures = TextureManager.LoadAnimation(OsuTexture.sliderb_0, 10);

            spriteFollowBall =
                new pAnimation(sliderballtextures, FieldTypes.GamefieldSprites, OriginTypes.Centre,
                               ClockTypes.Audio, Position, 0.99f, false, Color.White);
            spriteFollowBall.FramesPerSecond = Velocity / 6;

            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1,
                StartTime, StartTime);
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
                StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);


            spriteSliderBody = new pSprite(null, FieldTypes.NativeScaled, OriginTypes.TopLeft,
                                   ClockTypes.Audio, Vector2.Zero, SpriteManager.drawOrderBwd(EndTime + 14),
                                   false, Color.White);

            spriteSliderBody.Transform(fadeInTrack);
            spriteSliderBody.Transform(fadeOut);

            spriteFollowBall.Transform(fadeIn);
            spriteFollowBall.Transform(fadeOut);

            spriteFollowCircle.Transform(new NullTransform(StartTime, EndTime + DifficultyManager.HitWindow50));

            Sprites.Add(spriteSliderBody);
            Sprites.Add(spriteFollowBall);
            Sprites.Add(spriteFollowCircle);

            SpriteCollectionDim.Add(spriteSliderBody);

            //Start and end circles

            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));
            if (RepeatCount > 2)
                spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White) { Additive = true });

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));


            spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 12), false, Color.White));
            spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 11), false, Color.White));
            if (RepeatCount > 1)
                spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 10), false, Color.White) { Additive = true });

            spriteCollectionEnd.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionEnd.ForEach(s => s.Transform(fadeOut));

            //endpoint angular calculations
            if (drawableSegments.Count > 0)
            {
                startAngle = (float)Math.Atan2(drawableSegments[0].p1.Y - drawableSegments[0].p2.Y, drawableSegments[0].p1.X - drawableSegments[0].p2.X);
                endAngle = (float)Math.Atan2(drawableSegments[drawableSegments.Count - 1].p1.Y - drawableSegments[drawableSegments.Count - 1].p2.Y,
                                             drawableSegments[drawableSegments.Count - 1].p1.X - drawableSegments[drawableSegments.Count - 1].p2.X);
            }

            //tick calculations
            double tickCount = PathLength / TickDistance;
            int actualTickCount = (int)Math.Ceiling(Math.Round(tickCount, 1)) - 1;

            double tickNumber = 0;
            while (++tickNumber <= actualTickCount)
            {
                double progress = (tickNumber * TickDistance) / PathLength;

                scoringPoints.Add(progress);

                pSprite scoringDot =
                                    new pSprite(TextureManager.Load(OsuTexture.sliderscorepoint),
                                                FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, positionAtProgress(progress),
                                                SpriteManager.drawOrderBwd(EndTime + 13), false, Color.White);

                scoringDot.Transform(new Transformation(TransformationType.Fade, 0, 1,
                    StartTime - DifficultyManager.PreEmptSnakeStart + (int)((DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd) * progress),
                    StartTime - DifficultyManager.PreEmptSnakeStart + (int)((DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd) * progress) + 100));

                spriteCollectionScoringPoints.Add(scoringDot);
            }

            spriteCollectionScoringPoints.ForEach(s => s.Transform(fadeOut));

            Sprites.AddRange(spriteCollectionStart);
            Sprites.AddRange(spriteCollectionEnd);
            Sprites.AddRange(spriteCollectionScoringPoints);

            SpriteCollectionDim.AddRange(spriteCollectionStart);
            SpriteCollectionDim.AddRange(spriteCollectionEnd);
            SpriteCollectionDim.AddRange(spriteCollectionScoringPoints);
        }

        protected virtual void CalculateSplines()
        {
            List<Vector2> smoothPoints;

            switch (CurveType)
            {
                case CurveTypes.Bezier:
                default:
                    smoothPoints = new List<Vector2>();

                    int lastIndex = 0;

                    int count = controlPoints.Count;

                    for (int i = 0; i < count; i++)
                    {
                        bool multipartSegment = i + 1 < count && controlPoints[i] == controlPoints[i + 1];

                        if (multipartSegment || i == count - 1)
                        {
                            List<Vector2> thisLength = controlPoints.GetRange(lastIndex, i - lastIndex + 1);

                            smoothPoints.AddRange(pMathHelper.CreateBezier(thisLength, (int)Math.Max(1, ((float)thisLength.Count / count * PathLength) / 10)));

                            if (multipartSegment) i++;
                            //Need to skip one point since we consumed an extra.

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
            EndTime = StartTime + (int)(1000 * PathLength / Velocity * RepeatCount);
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
                if (spriteCollectionStart.Count > 0) spriteCollectionStart[0].Colour = value;
                if (spriteCollectionEnd.Count > 0) spriteCollectionEnd[0].Colour = value;
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

        internal override bool HitTestInitial(TrackingPoint tracking)
        {
            return Player.Autoplay || hitCircleStart.HitTestInitial(tracking);
        }

        protected override ScoreChange HitActionInitial()
        {
            //todo: this is me being HORRIBLY lazy.
            hitCircleStart.SampleSet = SampleSet;
            hitCircleStart.Volume = Volume;

            ScoreChange startCircleChange = hitCircleStart.Hit();

            if (startCircleChange == ScoreChange.Ignore)
                return startCircleChange;

            //triggered on the first hit
            if (startCircleChange > 0)
            {
                hitCircleStart.HitAnimation(startCircleChange);

                scoringEndpointsHit++;
                return ScoreChange.SliderEnd;
            }

            return ScoreChange.MissMinor;
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
        bool isTracking { get { return (Player.Autoplay && Clock.AudioTime >= StartTime) || trackingPoint != null; } }

        bool wasTracking;

        /// <summary>
        /// Number of successfully hit end-points. Includes the start circle.
        /// </summary>
        int scoringEndpointsHit;

        /// <summary>
        /// Index of the last end-point to be judged. Used to keep track of judging calculations.
        /// </summary>
        int lastJudgedEndpoint;

        internal override bool IsHit
        {
            get
            {
                return IsEndHit;
            }
            set
            {
                base.IsHit = value;
            }
        }

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal override ScoreChange CheckScoring()
        {
            if (!hitCircleStart.IsHit)
                base.CheckScoring();

            if (IsEndHit || Clock.AudioTime < StartTime)
                return ScoreChange.Ignore;

            if (trackingPoint == null)
            {
                if (InputManager.IsPressed)
                {
                    //todo: isPressed should *probably* be an attribute of a trackingPoint.
                    //this is only required at the moment with  mouse, an will always WORK correctly even with multiple touches, but logically doesn't make much sense.

                    //check each tracking point to find if any are usable
                    foreach (TrackingPoint p in InputManager.TrackingPoints)
                    {
                        if (pMathHelper.DistanceSquared(p.GamefieldPosition, TrackingPosition) < DifficultyManager.HitObjectRadiusSolidGamefieldHittable * DifficultyManager.HitObjectRadiusSolidGamefieldHittable)
                        {
                            trackingPoint = p;
                            break;
                        }
                    }
                }
            }
            else if (!trackingPoint.Valid || pMathHelper.DistanceSquared(trackingPoint.GamefieldPosition, TrackingPosition) > Math.Pow(DifficultyManager.HitObjectRadiusSolidGamefieldHittable * 2, 2))
                trackingPoint = null;

            //Check is the state of tracking changed.
            if (isTracking != wasTracking)
            {
                wasTracking = isTracking;

                if (!isTracking)
                {
                    //End tracking.
                    endTracking();
                }
                else
                {
                    beginTracking();
                }
            }

            //Check if we've hit a new endpoint...
            if ((int)progressCurrent != progressEndpointProcessed)
            {
                lastJudgedEndpoint++;
                progressEndpointProcessed++;

                newEndpoint();

                if (isTracking)
                {
                    PlaySound(SoundTypeList != null ? SoundTypeList[lastJudgedEndpoint] : SoundType);

                    burstEndpoint();

                    scoringEndpointsHit++;
                }

                if (RepeatCount - lastJudgedEndpoint == 0)
                {
                    //we've hit the end of the slider altogether.
                    lastEndpoint();

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

                return isTracking ? ScoreChange.SliderRepeat : ScoreChange.MissMinor;
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
                        AudioEngine.PlaySample(OsuSamples.SliderTick, SampleSet, Volume);

                        pDrawable point = spriteCollectionScoringPoints[judgePointNormalized];

                        point.Alpha = 0;


                        if (spriteFollowCircle.Transformations.Find(t => t.Type == TransformationType.Scale) == null)
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

                    return ScoreChange.MissMinor;
                }
            }

            return ScoreChange.Ignore;
        }

        protected virtual void lastEndpoint()
        {
            spriteFollowBall.RunAnimation = false;

            spriteFollowCircle.Transformations.Clear();

            if (spriteFollowCircle.Alpha > 0 && isTracking)
            {
                spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1.05f, 0.8f, Clock.AudioTime, Clock.AudioTime + 240, EasingTypes.In));
                spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.AudioTime, Clock.AudioTime + 240, EasingTypes.None));
            }
        }

        protected virtual void newEndpoint()
        {
            if (RepeatCount - lastJudgedEndpoint < 3 && RepeatCount - lastJudgedEndpoint > 0)
            {
                //we can turn off some repeat arrows...
                if (lastJudgedEndpoint % 2 == 0)
                    spriteCollectionStart[2].Transformations.Clear();
                else
                    spriteCollectionEnd[2].Transformations.Clear();
            }
        }

        protected virtual void beginTracking()
        {
            //Begin tracking.
            spriteFollowCircle.Transformations.RemoveAll(t => t.Type != TransformationType.None);

            spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 0.4f, 1.05f, Clock.AudioTime, Math.Min(EndTime, Clock.AudioTime + 200), EasingTypes.InHalf));
            spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1.05f, 1, Clock.AudioTime + 200, Math.Min(EndTime, Clock.AudioTime + 250), EasingTypes.OutHalf));
            spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 0, 1, Clock.AudioTime, Math.Min(EndTime, Clock.AudioTime + 140), EasingTypes.None));
        }

        protected virtual void endTracking()
        {
            if (IsEndHit)
                return;

            spriteFollowCircle.Transformations.RemoveAll(t => t.Type != TransformationType.None);

            spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1, 1.4f, Clock.AudioTime, Clock.AudioTime + 150, EasingTypes.In));
            spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, spriteFollowCircle.Alpha, 0, Clock.AudioTime, Clock.AudioTime + 150, EasingTypes.None));
        }

        protected virtual void burstEndpoint()
        {
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
        }

        internal Vector2 TrackingPosition;
        private float startAngle;
        private float endAngle;

        /// <summary>
        /// Floating point progress from the previous update (used during scoring for checking scoring milestones).
        /// </summary>
        protected int progressEndpointProcessed;

        /// <summary>
        /// Floating point progress through the slider (0..1 for first length, 1..x for futher repeats)
        /// </summary>
        protected float progressCurrent;

        private double normalizeProgress(double progress)
        {
            while (progress > 2)
                progress -= 2;
            if (progress > 1)
                progress = 2 - progress;

            return progress;
        }

        protected virtual Line lineAtProgress(double progress)
        {
            double aimLength = PathLength * normalizeProgress(progress);

            //index is the index of the line segment that exceeds the required length (so we need to cut it back)
            int index = 0;
            while (index < cumulativeLengths.Count && cumulativeLengths[index] < aimLength)
                index++;

            return drawableSegments[index];
        }

        protected virtual Vector2 positionAtProgress(double progress)
        {
            double aimLength = PathLength * normalizeProgress(progress);

            //index is the index of the line segment that exceeds the required length (so we need to cut it back)
            int index = 0;
            while (index < cumulativeLengths.Count && cumulativeLengths[index] < aimLength)
                index++;

            double lengthAtIndex = cumulativeLengths[index];
            Line currentLine = drawableSegments[index];

            //cut back the line to required exact length
            return currentLine.p1 + Vector2.Normalize(currentLine.p2 - currentLine.p1) * (float)(aimLength - (index > 0 ? cumulativeLengths[index - 1] : 0));
        }

        bool isReversing { get { return progressCurrent % 2 >= 1; } }

        /// <summary>
        /// Update all elements of the slider which aren't affected by user input.
        /// </summary>
        public override void Update()
        {
            progressCurrent = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime)) * RepeatCount;

            spriteFollowBall.Reverse = isReversing;

            //cut back the line to required exact length
            TrackingPosition = positionAtProgress(progressCurrent);

            if (IsVisible && Clock.AudioTime > StartTime - DifficultyManager.PreEmptSnakeStart)
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

        internal override void Dispose()
        {
            DisposePathTexture();
            base.Dispose();
        }

        internal void DisposePathTexture()
        {
            if (sliderBodyTexture != null)
            {
                TextureManager.ReturnTexture(sliderBodyTexture);
                sliderBodyTexture = null;

                lengthDrawn = 0;
                lastDrawnSegmentIndex = -1;
            }
        }

        /// <summary>
        /// Counter for number of frames skipped since last slider path render.
        /// </summary>
        private int lastJudgedScoringPoint = -1;

        private bool IsEndHit;
        protected double TickDistance;

        /// <summary>
        /// Used by both sliders and hold circles
        /// </summary>
        protected double Velocity;

        bool waitingForPathTextureClear;

        /// <summary>
        /// Updates the slider's path texture if required.
        /// </summary>
        internal virtual void UpdatePathTexture()
        {
            if (lengthDrawn == PathLength) return; //finished drawing already.

            // Snaking animation is IN PROGRESS
            int FirstSegmentIndex = lastDrawnSegmentIndex + 1;

            double drawProgress = Math.Max(0, (double)(Clock.AudioTime - StartTime + DifficultyManager.PreEmptSnakeStart) /
                          (double)(DifficultyManager.PreEmptSnakeStart - DifficultyManager.PreEmptSnakeEnd));

            if (drawProgress <= 0) return; //haven't started drawing yet.

            if (sliderBodyTexture == null || sliderBodyTexture.IsDisposed) // Perform setup to begin drawing the slider track.
            {
                CreatePathTexture();

                if (sliderBodyTexture == null)
                    //creation failed
                    return;
            }

            // Length of the curve we're drawing up to.
            lengthDrawn = PathLength * drawProgress;

            // this is probably faster than a binary search since it runs so few times and the result is very close
            while (lastDrawnSegmentIndex < cumulativeLengths.Count - 1 && cumulativeLengths[lastDrawnSegmentIndex + 1] < lengthDrawn)
                lastDrawnSegmentIndex++;

            if (lastDrawnSegmentIndex >= cumulativeLengths.Count - 1 || NO_SNAKING)
            {
                lengthDrawn = PathLength;
                lastDrawnSegmentIndex = drawableSegments.Count - 1;
            }

            Vector2 drawEndPosition = positionAtProgress(lengthDrawn / PathLength);
            spriteCollectionEnd.ForEach(s => s.Position = drawEndPosition);

            Line prev = FirstSegmentIndex > 0 ? drawableSegments[FirstSegmentIndex - 1] : null;

            if (lastDrawnSegmentIndex >= FirstSegmentIndex || FirstSegmentIndex == 0)
            {
                List<Line> partialDrawable = drawableSegments.GetRange(FirstSegmentIndex, lastDrawnSegmentIndex - FirstSegmentIndex + 1);
#if iOS
                int oldFBO = 0;
                GL.GetInteger(All.FramebufferBindingOes, ref oldFBO);
                
                GL.Oes.BindFramebuffer(All.FramebufferOes, sliderBodyTexture.fboId);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                
                GL.Viewport(0, 0, trackBoundsNative.Width, trackBoundsNative.Height);
                GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Top, trackBounds.Bottom, -1, 1);

                if (waitingForPathTextureClear)
                {
                    GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);
                    waitingForPathTextureClear = false;
                }

                m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                            DifficultyManager.HitObjectRadiusGamefield, ColourIndex, prev);

                GL.Oes.BindFramebuffer(All.FramebufferOes, oldFBO);
#else
                if (sliderBodyTexture.fboId >= 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, sliderBodyTexture.fboId);

                    GL.Viewport(0, 0, trackBoundsNative.Width, trackBoundsNative.Height);
                    GL.MatrixMode(MatrixMode.Projection);

                    GL.LoadIdentity();
                    GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Top, trackBounds.Bottom, -1, 1);

                    if (waitingForPathTextureClear)
                    {
                        GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);
                        waitingForPathTextureClear = false;
                    }

                    m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                                DifficultyManager.HitObjectRadiusGamefield, ColourIndex, prev);

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
#endif

                GameBase.Instance.SetViewport();
            }
        }

        /// <summary>
        /// Creates the texture which will hold the slider's path.
        /// </summary>
        private void CreatePathTexture()
        {
            //resign any old FBO assignments first.
            DisposePathTexture();

            RectangleF rectf = FindBoundingBox(drawableSegments, DifficultyManager.HitObjectRadiusGamefield);

            trackBounds.X = (int)(rectf.X);
            trackBounds.Y = (int)(rectf.Y);
            trackBounds.Width = (int)rectf.Width + 1;
            trackBounds.Height = (int)rectf.Height + 1;

            trackBoundsNative.X = (int)((rectf.X + GameBase.GamefieldOffsetVector1.X) * GameBase.BaseToNativeRatioAligned);
            trackBoundsNative.Y = (int)((rectf.Y + GameBase.GamefieldOffsetVector1.Y) * GameBase.BaseToNativeRatioAligned);
            trackBoundsNative.Width = (int)(rectf.Width * GameBase.BaseToNativeRatioAligned) + 1;
            trackBoundsNative.Height = (int)(rectf.Height * GameBase.BaseToNativeRatioAligned) + 1;

            lengthDrawn = 0;
            lastDrawnSegmentIndex = -1;

            sliderBodyTexture = TextureManager.RequireTexture(trackBoundsNative.Width, trackBoundsNative.Height);

            if (sliderBodyTexture == null)
                return;

            spriteSliderBody.Texture = sliderBodyTexture;
            spriteSliderBody.Position = new Vector2(trackBoundsNative.X, trackBoundsNative.Y);

            waitingForPathTextureClear = true;
        }

        internal override void Shake()
        {
            if (spriteSliderBody == null || spriteSliderBody.Texture == null)
                return; //don't try and shake before we have drawn the body textre; it will animate in the wrong place.
            base.Shake();
        }
    }


    internal enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear
    } ;
}