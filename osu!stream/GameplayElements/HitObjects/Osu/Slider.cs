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
using osum.GameplayElements.Beatmaps;

namespace osum.GameplayElements.HitObjects.Osu
{
    public class Slider : HitObjectSpannable
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
        /// Sliderball's glossy layer
        /// </summary>
        internal pSprite spriteFollowBallOverlay;

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
        protected List<HitObjectSoundType> SoundTypeList;

        /// <summary>
        /// A list of the samplesets used for each slider end
        /// </summary>
        protected List<SampleSetInfo> SampleSets;

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
        /// Track bounding rectangle measured in native (screen) coordinates
        /// </summary>
        internal Rectangle trackBounds;

        /// <summary>
        /// Gameplay time when this slider begins snaking out
        /// </summary>
        internal int snakingBegin;

        /// <summary>
        /// Gameplay time when this slider ends its snake
        /// </summary>
        internal int snakingEnd;

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

        /// <summary>
        /// cuz snackin' iz chestin'
        /// </summary>
        const bool NO_SNAKING = false;
        const bool PRERENDER_ALL = false;

        /// <summary>
        /// The start hitcircle is used for initial judging, and explodes as would be expected of a normal hitcircle. Also handles combo numbering.
        /// </summary>
        internal HitCircle HitCircleStart;

        internal Slider(HitObjectManager hitObjectManager, Vector2 startPosition, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes, double velocity, double tickDistance, List<SampleSetInfo> sampleSets)
            : base(hitObjectManager, startPosition, startTime, soundType, newCombo, comboOffset)
        {
            CurveType = curveType;

            SnakingEndPosition = Position;

            controlPoints = sliderPoints;

            if (sliderPoints[0] != startPosition)
                sliderPoints.Insert(0, startPosition);

            RepeatCount = Math.Max(1, repeatCount);

            if (soundTypes != null && soundTypes.Count > 0)
                SoundTypeList = soundTypes;

            PathLength = pathLength;
            Velocity = velocity;
            TickDistance = tickDistance;
            SampleSets = sampleSets;

            Type = HitObjectType.Slider;

            CalculateSplines();

            CalculateSnakingTimes();
            initializeSprites();
            initializeStartCircle();
        }

        internal Slider(HitObjectManager hitObjectManager, Vector2 startPosition, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes, double velocity, double tickDistance)
            : this(hitObjectManager, startPosition, startTime, newCombo, comboOffset, soundType,
                curveType, repeatCount, pathLength, sliderPoints,
                soundTypes, velocity, tickDistance, null)
        {
        }

        protected virtual void initializeStartCircle()
        {
            HitCircleStart = new HitCircle(null, Position, StartTime, NewCombo, ComboOffset, SoundTypeList != null ? SoundTypeList[0] : SoundType);
            Sprites.AddRange(HitCircleStart.Sprites);
        }

        protected virtual void initializeSprites()
        {
            spriteFollowCircle =
    new pSprite(TextureManager.Load(OsuTexture.sliderfollowcircle), FieldTypes.GamefieldSprites,
                   OriginTypes.Centre, ClockTypes.Audio, Position, 0.98f, false, Color.White) { ExactCoordinates = false };

            pTexture[] sliderballtextures = TextureManager.LoadAnimation(OsuTexture.sliderb_0, 10);

            spriteFollowBall =
                new pAnimation(sliderballtextures, FieldTypes.GamefieldSprites, OriginTypes.Centre,
                               ClockTypes.Audio, Position, SpriteManager.drawOrderFwdPrio(EndTime), false, Color.White) { ExactCoordinates = false };
            spriteFollowBall.FramesPerSecond = Velocity / 6;

            Transformation fadeInBall = new TransformationF(TransformationType.Fade, 0, 1,
                StartTime, StartTime);
            Transformation fadeInHead = new TransformationF(TransformationType.Fade, 0, 1,
                StartTime - DifficultyManager.PreEmpt, StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);
            Transformation fadeOut = new TransformationF(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.FadeOut / 2);
            Transformation fadeOutInstant = new TransformationF(TransformationType.Fade, 1, 0, EndTime, EndTime);
            Transformation fadeInTail = new TransformationF(TransformationType.Fade, 0, 1,
                snakingBegin - DifficultyManager.FadeInReboundArrow,
                snakingBegin);

            spriteSliderBody = new pSprite(null, FieldTypes.NativeScaled, OriginTypes.TopLeft,
                                   ClockTypes.Audio, Vector2.Zero, GameBase.IsSlowDevice ? 0.01f : SpriteManager.drawOrderBwd(EndTime + 14),
                                   false, Color.White);

            spriteSliderBody.Transform(fadeInHead);
            spriteSliderBody.Transform(fadeOut);

            spriteFollowBall.Transform(fadeInBall);
            spriteFollowBall.Transform(fadeOutInstant);

            spriteFollowCircle.Transform(new NullTransform(StartTime, EndTime + DifficultyManager.HitWindow50));

            spriteFollowBallOverlay =
new pSprite(TextureManager.Load(OsuTexture.sliderballoverlay), FieldTypes.GamefieldSprites,
       OriginTypes.TopCentre, ClockTypes.Audio, Position + new Vector2(0, -DifficultyManager.HitObjectRadiusGamefield * 59 / 64), 0.99f, false, Color.White) { ExactCoordinates = false };

            spriteFollowBallOverlay.Transform(fadeInBall);
            spriteFollowBallOverlay.Transform(fadeOutInstant);

            Sprites.Add(spriteSliderBody);
            Sprites.Add(spriteFollowBall);
            Sprites.Add(spriteFollowCircle);
            Sprites.Add(spriteFollowBallOverlay);

            spriteSliderBody.TagNumeric = HitObject.DIMMABLE_TAG;

            //Start and end circles

            spriteCollectionStart.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle0), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));

            spriteCollectionStart.ForEach(s => s.Transform(fadeInHead));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));

            if (RepeatCount > 2)
            {
                pSprite headArrow = new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White) { Additive = true };
                headArrow.Transform(fadeInTail);
                headArrow.Transform(fadeOut);
                spriteCollectionStart.Add(headArrow);
            }

            spriteCollectionEnd.Add(new pSprite(TextureManager.Load(OsuTexture.hitcircle0), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 12), false, Color.White));

            spriteCollectionEnd.ForEach(s => s.Transform(fadeInTail));
            spriteCollectionEnd.ForEach(s => s.Transform(fadeOut));

            if (RepeatCount > 1)
            {
                pSprite tailArrow = new pSprite(TextureManager.Load(OsuTexture.sliderarrow), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 10), false, Color.White) { Additive = true };
                tailArrow.Transform(fadeInTail);
                tailArrow.Transform(fadeOut);
                spriteCollectionEnd.Add(tailArrow);
            }


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

                Line l;

                pSprite scoringDot =
                                    new pSprite(TextureManager.Load(OsuTexture.sliderscorepoint),
                                                FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, positionAtProgress(progress, out l),
                                                SpriteManager.drawOrderBwd(EndTime + 13), false, Color.White);

                scoringDot.Transform(new TransformationF(TransformationType.Fade, 0, 1,
                    snakingBegin + (int)((snakingEnd - snakingBegin) * progress),
                    snakingBegin + (int)((snakingEnd - snakingBegin) * progress) + 100));

                spriteCollectionScoringPoints.Add(scoringDot);
            }

            spriteCollectionScoringPoints.ForEach(s => s.Transform(fadeOut));

            Sprites.AddRange(spriteCollectionStart);
            Sprites.AddRange(spriteCollectionEnd);
            Sprites.AddRange(spriteCollectionScoringPoints);

            spriteCollectionStart.ForEach(s => s.TagNumeric = HitObject.DIMMABLE_TAG);
            spriteCollectionEnd.ForEach(s => s.TagNumeric = HitObject.DIMMABLE_TAG);
            spriteCollectionScoringPoints.ForEach(s => s.TagNumeric = HitObject.DIMMABLE_TAG);
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

                            // remove the last point of the previous curve since it's the same as the first point of this curve
                            if (smoothPoints.Count > 0) smoothPoints.RemoveAt(smoothPoints.Count - 1);

                            smoothPoints.AddRange(pMathHelper.CreateBezier(thisLength));

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
                case CurveTypes.PerfectCurve:

                    // we may have 2 points when building the circle.
                    if (controlPoints.Count < 3)
                        goto case CurveTypes.Linear;
                    // more than 3 -> ignore them.
                    if (controlPoints.Count > 3)
                        goto case CurveTypes.Bezier;

                    Vector2 A = controlPoints[0];
                    Vector2 B = controlPoints[1];
                    Vector2 C = controlPoints[2];
                    // all 3 points are on a straight line, avoid undefined behaviour:
                    if ((B.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (B.Y - A.Y) == 0.0f)
                        goto case CurveTypes.Linear;

                    Vector2 centre;
                    float radius;
                    double t_initial, t_final;

                    pMathHelper.CircleThroughPoints(A, B, C, out centre, out radius, out t_initial, out t_final);

                    double curveLength = Math.Abs((t_final - t_initial) * radius);
                    int _segments = (int)(curveLength * 0.125f);

                    Vector2 lastPoint = A;

                    smoothPoints = new List<Vector2>();

                    for (int i = 0; i < _segments; i++)
                    {
                        double progress = (double)i / (double)_segments;
                        double t = t_final * progress + t_initial * (1 - progress);
                        smoothPoints.Add(pMathHelper.CirclePoint(centre, radius, t));
                    }

                    break;
            }

            //adjust the line to be of maximum length specified...
            double currentLength = 0;

            for (int i = 1; i < smoothPoints.Count; i++)
            {
                Line l = new Line(smoothPoints[i - 1], smoothPoints[i]);
                drawableSegments.Add(l);

                float lineLength = l.rho;

                if (lineLength + currentLength > PathLength || i + 1 == smoothPoints.Count)
                {
                    l.p2 = l.p1 + (l.p2 - l.p1) * (float)(PathLength - currentLength) / lineLength;
                    l.Recalc();

                    cumulativeLengths.Add(PathLength);
                    break; //we are done.
                }

                currentLength += lineLength;
                cumulativeLengths.Add(currentLength);
            }

            EndTime = StartTime + (int)(1000 * PathLength / Velocity * RepeatCount);
        }

        /// <summary>
        /// Find the extreme values of the given curve in the form of a box.
        /// </summary>
        private static RectangleF FindBoundingBox(List<Line> curve, float radius)
        {
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

        private void CalculateSnakingTimes()
        {
            // time this slider wants to snake at
            int DesiredTime = (int)(PathLength * (double)DifficultyManager.SnakeSpeedInverse);

            // time our difficulty allows for const speed snaking
            int AllowedTime = DifficultyManager.SnakeStart - DifficultyManager.SnakeEndDesired;

            // time it must finish within no matter what
            int RequiredTime = DifficultyManager.SnakeStart - DifficultyManager.SnakeEndLimit;

            if (DesiredTime < AllowedTime)
            {
                // we have ample time, so end at the desired end time, and work our way back
                snakingBegin = StartTime - DifficultyManager.SnakeEndDesired - DesiredTime;
                snakingEnd = StartTime - DifficultyManager.SnakeEndDesired;
            }
            else if (DesiredTime < RequiredTime)
            {
                // we take more time than the desired window so allow the snaking to continue a little later
                snakingBegin = StartTime - DifficultyManager.SnakeStart;
                snakingEnd = StartTime - DifficultyManager.SnakeStart + DesiredTime;
            }
            else
            {
                // we are way over limit so speed up the snaking
                snakingBegin = StartTime - DifficultyManager.SnakeStart;
                snakingEnd = StartTime - DifficultyManager.SnakeEndLimit;
            }
        }

        public override bool IsVisible
        {
            get
            {
                int now = ClockingNow;
                return
                    now >= StartTime - DifficultyManager.PreEmpt &&
                    now <= EndTime + DifficultyManager.FadeOut;
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
            }
        }

        internal override int ColourIndex
        {
            get
            {
                return base.ColourIndex;
            }
            set
            {
                HitCircleStart.ColourIndex = value;
                if (spriteCollectionStart.Count > 0) ((pSprite)spriteCollectionStart[0]).Texture = TextureManager.Load((OsuTexture)(OsuTexture.hitcircle0 + value));
                if (spriteCollectionEnd.Count > 0) ((pSprite)spriteCollectionEnd[0]).Texture = TextureManager.Load((OsuTexture)(OsuTexture.hitcircle0 + value));
                base.ColourIndex = value;
            }
        }

        internal override int ComboNumber
        {
            get
            {
                return HitCircleStart.ComboNumber;
            }
            set
            {
                HitCircleStart.ComboNumber = value;
            }
        }

        public override Vector2 Position
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

                HitCircleStart.Position = value;
            }
        }

        public override Vector2 EndPosition
        {
            get
            {
                return RepeatCount % 2 == 0 ? Position : Position2;
            }
        }

        public override Vector2 Position2
        {
            get
            {
                return drawableSegments[drawableSegments.Count - 1].p2;
            }
        }

        internal override bool HitTestInitial(TrackingPoint tracking)
        {
            return Player.Autoplay || HitCircleStart.HitTestInitial(tracking);
        }

        protected override ScoreChange HitActionInitial()
        {
            //todo: this is me being HORRIBLY lazy.
            HitCircleStart.SampleSet = SampleSets == null ? SampleSet : SampleSets[0];

            ScoreChange startCircleChange = HitCircleStart.Hit();

            if (startCircleChange == ScoreChange.Ignore)
                return startCircleChange;

            //triggered on the first hit
            if (startCircleChange > 0)
            {
                switch (startCircleChange)
                {
                    case ScoreChange.Hit300:
                        totalScoreValue += 3;
                        break;
                    case ScoreChange.Hit100:
                        totalScoreValue += 2;
                        break;
                    case ScoreChange.Hit50:
                        totalScoreValue += 1;
                        break;

                }

                HitCircleStart.HitAnimation(startCircleChange, true);

                totalScoreValue++;
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
        public bool IsTracking { get { return (Player.Autoplay && ClockingNow >= StartTime) || trackingPoint != null; } }

        bool wasTracking;

        /// <summary>
        /// Number of successfully hit end-points. Includes the start circle.
        /// </summary>
        int totalScoreValue;

        /// <summary>
        /// Index of the last end-point to be judged. Used to keep track of judging calculations.
        /// </summary>
        int lastJudgedEndpoint;

        public override bool IsHit
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

        internal Vector2 trackingPosition; //todo: make this based on input adjusted clocking.
        public override Vector2 TrackingPosition { get { return trackingPosition; } }

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal override ScoreChange CheckScoring()
        {
            if (!HitCircleStart.IsHit)
                base.CheckScoring();

            if (IsEndHit || Clock.AudioTimeInputAdjust < StartTime)
                return ScoreChange.Ignore;

            int now = ClockingNow;

            if (!Player.Autoplay)
            {
                float radius = DifficultyManager.HitObjectRadiusSolidGamefieldHittable * 1.1f;

                if (trackingPoint == null)
                {

                    if (InputManager.IsPressed)
                    {
                        //todo: isPressed should *probably* be an attribute of a trackingPoint.
                        //this is only required at the moment with  mouse, an will always WORK correctly even with multiple touches, but logically doesn't make much sense.

                        //check each tracking point to find if any are usable
                        foreach (TrackingPoint p in InputManager.TrackingPoints)
                        {
                            if (pMathHelper.DistanceSquared(p.GamefieldPosition, TrackingPosition) < radius * radius)
                            {
                                trackingPoint = p;
                                break;
                            }
                        }
                    }
                }
                else if (!trackingPoint.Valid || pMathHelper.DistanceSquared(trackingPoint.GamefieldPosition, TrackingPosition) > radius * radius * 4)
                    trackingPoint = null;
            }

            //Check is the state of tracking changed.
            if (IsTracking != wasTracking)
            {
                wasTracking = IsTracking;

                if (!IsTracking)
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

                bool finished = RepeatCount - lastJudgedEndpoint == 0;

                if (IsTracking)
                {
                    playRebound(lastJudgedEndpoint);
                    if (!finished)
                        burstEndpoint();
                    totalScoreValue++;
                }

                if (finished)
                {
                    //we've hit the end of the slider altogether.
                    lastEndpoint();

                    IsEndHit = true;

                    //Start is worth 1 for a hit .
                    //Start is worth 1-3 depending on hit accuracy (50/100/300).
                    //Repeat endpoints are worth 1.
                    //Ticks are worth 1.

                    float amountHit = (float)totalScoreValue / (lastJudgedEndpoint + 4 + scoringPoints.Count * RepeatCount);
                    ScoreChange amount;

                    if (amountHit == 1)
                        amount = ScoreChange.Hit300;
                    else if (amountHit > 0.7)
                        amount = ScoreChange.Hit100;
                    else if (amountHit > 0)
                        amount = ScoreChange.Hit50;
                    else
                        amount = ScoreChange.Miss;

                    return amount; //actual judging
                }

                lastJudgedScoringPoint = -1;

                return IsTracking ? ScoreChange.SliderRepeat : ScoreChange.MissMinor;
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
                    if (!HitCircleStart.IsHit)
                    {
                        HitCircleStart.IsHit = true;
                        return ScoreChange.MissMinor;
                    }

                    lastJudgedScoringPoint++;

                    if (IsTracking)
                    {
                        totalScoreValue++;
                        playTick();

                        pDrawable point = spriteCollectionScoringPoints[judgePointNormalized];

                        point.Alpha = 0;


                        if (spriteFollowCircle.Transformations.Find(t => t.Type == TransformationType.Scale) == null)
                            spriteFollowCircle.Transform(new TransformationF(TransformationType.Scale, 1.05f, 1, now, now + 100, EasingTypes.OutHalf));

                        if (RepeatCount > progressCurrent + 1)
                        {
                            //we still have more repeats to go.
                            int nextRepeatStartTime = (int)(StartTime + (EndTime - StartTime) * (((int)progressCurrent + 1) / (float)RepeatCount));

                            spriteCollectionScoringPoints[judgePointNormalized].Transform(
                                new TransformationF(TransformationType.Fade, 0, 1, nextRepeatStartTime - 100, nextRepeatStartTime));
                            spriteCollectionScoringPoints[judgePointNormalized].Transform(
                                new TransformationF(TransformationType.Scale, 0, 1, nextRepeatStartTime - 100, nextRepeatStartTime));
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

        protected virtual void playTick()
        {
            AudioEngine.PlaySample(OsuSamples.SliderTick, SampleSet.SampleSet, SampleSet.Volume);
        }

        protected virtual void playRebound(int lastJudgedEndpoint)
        {
            PlaySound(SoundTypeList != null ? SoundTypeList[lastJudgedEndpoint] : SoundType,
                      SampleSets != null ? SampleSets[lastJudgedEndpoint] : SampleSet);
        }

        internal override void StopSound(bool done = true)
        {
            stopSliding(this);
            base.StopSound();
        }

        protected virtual void lastEndpoint()
        {
            StopSound();

            spriteFollowBall.RunAnimation = false;

            spriteFollowCircle.Transformations.Clear();

            if (spriteFollowCircle.Alpha > 0 && IsTracking)
            {
                int now = ClockingNow;
                spriteFollowCircle.Transform(new TransformationF(TransformationType.Scale, 1f, 0.8f, now, now + 240, EasingTypes.In));
                spriteFollowCircle.Transform(new TransformationF(TransformationType.Fade, 1, 0, now, now + 240, EasingTypes.None));
            }
        }

        static Dictionary<SampleSet, Source> slidingSources = new Dictionary<SampleSet, Source>();
        static void startSliding(Slider s)
        {
            if (AudioEngine.Effect == null) return;

            Source source = null;

            if (slidingSources.TryGetValue(s.SampleSet.SampleSet, out source))
            {
                source.TagNumeric++; //increment the number of sliders emitting this sound.
                return;
            }

            source = AudioEngine.Effect.LoadBuffer(AudioEngine.LoadSample(OsuSamples.SliderSlide, s.SampleSet.SampleSet), s.SampleSet.Volume * 0.8f, true, true);

            if (source == null) return;

            source.TagNumeric++;
            source.Play();

            slidingSources.Add(s.SampleSet.SampleSet, source);
        }

        static void stopSliding(Slider s)
        {
            if (AudioEngine.Effect == null) return;

            Source source = null;

            if (!slidingSources.TryGetValue(s.SampleSet.SampleSet, out source))
                return;

            if (--source.TagNumeric <= 0)
            {
                source.Stop();
                source.Reserved = false;
                slidingSources.Remove(s.SampleSet.SampleSet);
            }
        }

        protected virtual void newEndpoint()
        {
            if (RepeatCount - lastJudgedEndpoint < 3 && RepeatCount - lastJudgedEndpoint > 0)
            {
                //we can turn off one repeat arrow...
                pDrawable arrow;

                if (lastJudgedEndpoint % 2 == 0)
                    arrow = spriteCollectionStart[1];
                else
                    arrow = spriteCollectionEnd[1];

                arrow.Alpha = 0;
                arrow.Transformations.Clear();
            }
        }

        protected virtual void beginTracking()
        {
            startSliding(this);

            //Begin tracking.
            spriteFollowCircle.Transformations.RemoveAll(t => t.Type != TransformationType.None);

            int now = ClockingNow;

            spriteFollowCircle.Transform(new TransformationBounce(now, Math.Min(EndTime, now + 350), 1, 0.5f, 2));
            spriteFollowCircle.Transform(new TransformationF(TransformationType.Fade, 0, 1, now, Math.Min(EndTime, now + 100), EasingTypes.None));
        }

        protected virtual void endTracking()
        {
            if (IsEndHit)
                return;

            StopSound(false);

            int now = ClockingNow;

            spriteFollowCircle.Transformations.RemoveAll(t => t.Type != TransformationType.None);

            spriteFollowCircle.Transform(new TransformationF(TransformationType.Scale, 1, 1.4f, now, now + 150, EasingTypes.In));
            spriteFollowCircle.Transform(new TransformationF(TransformationType.Fade, spriteFollowCircle.Alpha, 0, now, now + 150, EasingTypes.None));
        }

        internal virtual void burstEndpoint()
        {
            int now = Clock.Time;

            Transformation circleScaleOut = new TransformationF(TransformationType.Scale, 1.1F, 1.4F,
                    now, now + DifficultyManager.FadeOut, EasingTypes.InHalf);

            Transformation circleFadeOut = new TransformationF(TransformationType.Fade, 1, 0,
                now, now + DifficultyManager.FadeOut);

            if (lastJudgedEndpoint % 2 == 0)
            {
                foreach (pSprite p in spriteCollectionStart)
                {
                    if (p.Alpha == 0) continue;

                    //Burst the endpoint we just reached.
                    pDrawable clone = p.Clone();

                    clone.Transformations.Clear();

                    clone.Clocking = ClockTypes.Game;
                    clone.AlwaysDraw = false;

                    clone.Transform(circleScaleOut);
                    clone.Transform(circleFadeOut);

                    Sprites.Add(clone);
                    usableSpriteManager.Add(clone);
                }
            }

            if (lastJudgedEndpoint % 2 == 1)
            {
                foreach (pSprite p in spriteCollectionEnd)
                {
                    if (p.Alpha == 0) continue;

                    //Burst the endpoint we just reached.
                    pDrawable clone = p.Clone();

                    clone.Transformations.Clear();

                    clone.Clocking = ClockTypes.Game;

                    clone.Transform(circleScaleOut);
                    clone.Transform(circleFadeOut);

                    Sprites.Add(clone);
                    usableSpriteManager.Add(clone);
                }
            }

        }

        private float startAngle;
        private float endAngle;

        /// <summary>
        /// Floating point progress from the previous update (used during scoring for checking scoring milestones).
        /// </summary>
        protected int progressEndpointProcessed;

        /// <summary>
        /// Floating point progress through the slider (0..1 for first length, 1..x for futher repeats)
        /// </summary>
        internal float progressCurrent;

        private double normalizeProgress(double progress)
        {
            if (progress > 2) progress -= (Math.Ceiling(progress / 2d) - 1) * 2;
            if (progress > 1) progress = 2 - progress;
            return progress;
        }

        protected Vector2 positionAtProgress(double progress, int hint, out Line line)
        {
            double aimLength = PathLength * normalizeProgress(progress);

            //index is the index of the line segment that exceeds the required length (so we need to cut it back)
            int count = cumulativeLengths.Count;
            int index = 0;
            if (hint >= 0)
            {
                while (hint < count && cumulativeLengths[hint] < aimLength)
                    hint++;

                index = Math.Min(hint, count - 1);
            }
            else
            {
                index = cumulativeLengths.BinarySearch(aimLength);
                if (index < 0) index = (~index);

                index = Math.Min(index, count - 1);
            }

            double lengthAtIndex = cumulativeLengths[index];
            line = drawableSegments[index];

            //cut back the line to required exact length
            return line.p1 + Vector2.Normalize(line.p2 - line.p1) * (float)(aimLength - (index > 0 ? cumulativeLengths[index - 1] : 0));
        }

        protected virtual Vector2 positionAtProgress(double progress, out Line line)
        {
            return positionAtProgress(progress, -1, out line);
        }

        bool isReversing { get { return progressCurrent % 2 >= 1; } }

        /// <summary>
        /// Update all elements of the slider which aren't affected by user input.
        /// </summary>
        public override void Update()
        {
            int now = ClockingNow;

            progressCurrent = pMathHelper.ClampToOne((float)(now - StartTime) / (EndTime - StartTime)) * RepeatCount;

            spriteFollowBall.Reverse = isReversing;

            Line line;
            //cut back the line to required exact length
            trackingPosition = positionAtProgress(progressCurrent, out line);

            if (IsVisible && ClockingNow > snakingBegin)
                UpdatePathTexture();

            spriteFollowBall.Position = trackingPosition;
            spriteFollowBall.Rotation = line.theta;

            spriteFollowCircle.Position = trackingPosition;
            spriteFollowBallOverlay.Position = trackingPosition + new Vector2(0, -DifficultyManager.HitObjectRadiusGamefield * 59 / 64);

            //Adjust the angles of the end arrows
            if (RepeatCount > 1)
                spriteCollectionEnd[1].Rotation = (lastDrawnSegmentIndex >= 0 ? drawableSegments[lastDrawnSegmentIndex].theta + MathHelper.Pi : endAngle)
                                                + (float)((MathHelper.Pi / 32) * ((now % 300) / 300f - 0.5) * 2);
            if (RepeatCount > 2)
                spriteCollectionStart[1].Rotation = MathHelper.Pi + startAngle + (float)((MathHelper.Pi / 32) * ((now % 300) / 300f - 0.5) * 2);

            base.Update();
        }

        internal override void Dispose()
        {
            StopSound();
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

        public Vector2 SnakingEndPosition;

#if iOS
        static int oldFboId = -1;
#endif

        /// <summary>
        /// Updates the slider's path texture if required.
        /// </summary>
        internal virtual void UpdatePathTexture()
        {
            if (lengthDrawn >= PathLength || IsHit) return; //finished drawing already.

            // Snaking animation is IN PROGRESS
            int FirstSegmentIndex = lastDrawnSegmentIndex + 1;

            double drawProgress = Math.Max(0, (double)(ClockingNow - snakingBegin) / (double)(snakingEnd - snakingBegin));

            if (drawProgress <= 0) return; //haven't started drawing yet.

            if (sliderBodyTexture == null || sliderBodyTexture.IsDisposed) // Perform setup to begin drawing the slider track.
            {
                CreatePathTexture();

                lastDrawnSegmentIndex = -1;
                FirstSegmentIndex = 0;
                if (sliderBodyTexture == null)
                    return; //creation failed
            }

            if (sliderBodyTexture.fboId < 0)
            {
                lastDrawnSegmentIndex = -1;
                FirstSegmentIndex = 0;
            }

            // Length of the curve we're drawing up to.
            lengthDrawn = PathLength * drawProgress;

            // this is probably faster than a binary search since it runs so few times and the result is very close
            while (lastDrawnSegmentIndex < cumulativeLengths.Count - 1 && cumulativeLengths[lastDrawnSegmentIndex + 1] <= lengthDrawn)
                lastDrawnSegmentIndex++;

            if (lastDrawnSegmentIndex >= cumulativeLengths.Count - 1 || NO_SNAKING)
            {
                lengthDrawn = PathLength;
                lastDrawnSegmentIndex = drawableSegments.Count - 1;
                drawProgress = 1.0d;
            }

            Line l;
            SnakingEndPosition = positionAtProgress(drawProgress, lastDrawnSegmentIndex + 1, out l);
            foreach (pDrawable p in spriteCollectionEnd)
                p.Position = SnakingEndPosition;

            Line prev = FirstSegmentIndex > 0 ? drawableSegments[FirstSegmentIndex - 1] : null;
            Line next = lastDrawnSegmentIndex + 1 < drawableSegments.Count ? drawableSegments[lastDrawnSegmentIndex + 1] : null;

            if (lastDrawnSegmentIndex >= FirstSegmentIndex || FirstSegmentIndex == 0)
            {
                List<Line> partialDrawable = drawableSegments.GetRange(FirstSegmentIndex, lastDrawnSegmentIndex - FirstSegmentIndex + 1);

                sliderBodyTexture.BindFramebuffer();
#if iOS
                GL.Oes.BindFramebuffer(All.FramebufferOes, sliderBodyTexture.fboId);
                DrawPath(partialDrawable, prev, next);
                GL.Oes.BindFramebuffer(All.FramebufferOes, oldFboId);
#else
                if (sliderBodyTexture.fboId >= 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, sliderBodyTexture.fboId);

                    DrawPath(partialDrawable, prev, next);

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
                else
                {
                    GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);

                    DrawPath(partialDrawable, prev, next);

                    GL.BindTexture(TextureGl.SURFACE_TYPE, sliderBodyTexture.TextureGl.Id);
                    GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, sliderBodyTexture.TextureGl.potWidth, sliderBodyTexture.TextureGl.potHeight, 0);
                    GL.BindTexture(TextureGl.SURFACE_TYPE, TextureGl.lastDrawTexture);
                }
#endif
                GameBase.Instance.SetViewport();
            }
        }

        private void DrawPath(List<Line> partialDrawable, Line prev, Line next)
        {
            GL.Viewport(0, 0, trackBounds.Width, trackBounds.Height);
            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadIdentity();
            GL.Ortho(trackBounds.Left / GameBase.BaseToNativeRatioAligned - GameBase.GamefieldOffsetVector1.X,
                        trackBounds.Right / GameBase.BaseToNativeRatioAligned - GameBase.GamefieldOffsetVector1.X,
                        trackBounds.Top / GameBase.BaseToNativeRatioAligned - GameBase.GamefieldOffsetVector1.Y,
                        trackBounds.Bottom / GameBase.BaseToNativeRatioAligned - GameBase.GamefieldOffsetVector1.Y,
                        -1, 1);

            m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                        DifficultyManager.HitObjectRadiusGamefield, ColourIndex, prev, next);
        }

        /// <summary>
        /// Creates the texture which will hold the slider's path.
        /// </summary>
        private void CreatePathTexture()
        {
            //resign any old FBO assignments first.
            DisposePathTexture();

            RectangleF rectf = FindBoundingBox(drawableSegments, DifficultyManager.HitObjectRadiusGamefield);

            trackBounds.X = (int)((rectf.X + GameBase.GamefieldOffsetVector1.X) * GameBase.BaseToNativeRatioAligned);
            trackBounds.Y = (int)((rectf.Y + GameBase.GamefieldOffsetVector1.Y) * GameBase.BaseToNativeRatioAligned);
            trackBounds.Width = (int)(rectf.Width * GameBase.BaseToNativeRatioAligned) + 1;
            trackBounds.Height = (int)(rectf.Height * GameBase.BaseToNativeRatioAligned) + 1;

            lengthDrawn = 0;
            lastDrawnSegmentIndex = -1;

            sliderBodyTexture = TextureManager.RequireTexture(trackBounds.Width, trackBounds.Height);

            if (sliderBodyTexture == null)
                return;

#if iOS
            spriteSliderBody.Premultiplied = true;
#endif
            spriteSliderBody.Texture = sliderBodyTexture;

            sliderBodyTexture.BindFramebuffer();
            if (sliderBodyTexture.fboId >= 0)
            {
#if iOS
                //clear the full buffer before changing the viewport.
                if (oldFboId < 0) GL.GetInteger(All.FramebufferBindingOes, ref oldFboId);
                GL.Oes.BindFramebuffer(All.FramebufferOes, sliderBodyTexture.fboId);
                GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);
                GL.Oes.BindFramebuffer(All.FramebufferOes, oldFboId);
#else
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, sliderBodyTexture.fboId);
                GL.Clear(Constants.COLOR_DEPTH_BUFFER_BIT);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
#endif
            }

            spriteSliderBody.DrawWidth = trackBounds.Width;
            spriteSliderBody.DrawHeight = trackBounds.Height;

            spriteSliderBody.Position = new Vector2(trackBounds.X, trackBounds.Y);
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
        Linear,
        PerfectCurve
    } ;
}