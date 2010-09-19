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

#if IPHONE

#else
using OpenTK.Graphics.OpenGL;
#endif

using osum.Graphics.Renderers;
using OpenTK.Graphics;
using System.Drawing;

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
        internal readonly pAnimation spriteFollowCircle;

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

        internal pTexture trackTexture;

        internal double lengthDrawn;
        internal int lastSegmentIndex;

        /// <summary>
        /// Cumulative list of curve lengths up to AND INCLUDING a given DrawableSegment.
        /// </summary>
        internal List<double> cumulativeLengths;

        /// <summary>
        /// Track bounding rectangle measured in SCREEN COORDINATES
        /// </summary>
        internal Rectangle trackBounds;
        internal Rectangle trackBoundsNative;

        private List<pSprite> spriteCollectionStart = new List<pSprite>();

        private List<pSprite> spriteCollectionEnd = new List<pSprite>();

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
            Transformation fadeInTrack = new Transformation(TransformationType.Fade, 0, 1,
                startTime - DifficultyManager.PreEmpt - DifficultyManager.HitWindow50, startTime - DifficultyManager.PreEmpt);
            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                EndTime, EndTime + DifficultyManager.HitWindow50);

            hitCircleStart = new HitCircle(null, Position, StartTime, newCombo, soundType);

            spriteSliderBody = new pSprite(null, FieldTypes.Native, OriginTypes.TopLeft,
                                   ClockTypes.Audio, Vector2.Zero, SpriteManager.drawOrderBwd(EndTime + 10),
                                   false, Color.White);

            spriteSliderBody.Transform(fadeInTrack);
            spriteSliderBody.Transform(fadeOut);

            spriteFollowBall.Transform(fadeIn);
            spriteFollowBall.Transform(fadeOut);

            SpriteCollection.Add(spriteFollowBall);
            SpriteCollection.Add(spriteFollowCircle);
            SpriteCollection.Add(spriteSliderBody);

            //Start and end circles

            spriteCollectionStart.Add(new pSprite(SkinManager.Load("hitcircle"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionStart.Add(new pSprite(SkinManager.Load("hitcircleoverlay"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));
            if (repeatCount > 2)
                spriteCollectionStart.Add(new pSprite(SkinManager.Load("reversearrow"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White));

            spriteCollectionStart.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionStart.ForEach(s => s.Transform(fadeOut));


            spriteCollectionEnd.Add(new pSprite(SkinManager.Load("hitcircle"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 9), false, Color.White));
            spriteCollectionEnd.Add(new pSprite(SkinManager.Load("hitcircleoverlay"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 8), false, Color.White));
            if (repeatCount > 1)
                spriteCollectionEnd.Add(new pSprite(SkinManager.Load("reversearrow"), FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(EndTime + 7), false, Color.White));

            spriteCollectionEnd.ForEach(s => s.Transform(fadeInTrack));
            spriteCollectionEnd.ForEach(s => s.Transform(fadeOut));

            SpriteCollection.AddRange(hitCircleStart.SpriteCollection);
            SpriteCollection.AddRange(spriteCollectionStart);
            SpriteCollection.AddRange(spriteCollectionEnd);

            startAngle = (float)Math.Atan2(drawableSegments[0].p1.Y - drawableSegments[0].p2.Y, drawableSegments[0].p1.X - drawableSegments[0].p2.X);
            endAngle = (float)Math.Atan2(drawableSegments[drawableSegments.Count - 1].p1.Y - drawableSegments[drawableSegments.Count - 1].p2.Y,
                                         drawableSegments[drawableSegments.Count - 1].p1.X - drawableSegments[drawableSegments.Count - 1].p2.X);
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
            //triggered on the first hit
            if (hitCircleStart.Hit() > 0)
            {
                scoringEndpointsHit++;
                return ScoreChange.SliderEnd;
            }

            return ScoreChange.Ignore;
        }

        TrackingPoint trackingPoint;
        bool isTracking;

        int scoringEndpointsHit;

        internal override ScoreChange CheckScoring()
        {
            if (!IsActive)
                return ScoreChange.Ignore;

            float radius = DifficultyManager.HitObjectRadius;

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
                            Console.WriteLine("got point");
                            break;
                        }
                    }
                }
            }
            else if (!trackingPoint.Valid || pMathHelper.DistanceSquared(trackingPoint.GamefieldPosition, TrackingPosition) > Math.Pow(radius * 2, 2))
                trackingPoint = null;

            if (trackingPoint == null && isTracking)
            {
                //End tracking.
                isTracking = false;

                spriteFollowCircle.Transformations.Clear();
                spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1, 1.4f, Clock.AudioTime, Clock.AudioTime + 200, EasingTypes.In));
                spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.AudioTime, Clock.AudioTime + 200, EasingTypes.None));

            }
            else if (trackingPoint != null && !isTracking)
            {
                //Begin tracking.
                isTracking = true;

                spriteFollowCircle.Transformations.Clear();
                spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 0.6f, 1.05f, Clock.AudioTime, Clock.AudioTime + 230, EasingTypes.InHalf));
                spriteFollowCircle.Transform(new Transformation(TransformationType.Scale, 1.05f, 1, Clock.AudioTime + 230, Clock.AudioTime + 270, EasingTypes.OutHalf));
                spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 0, 1, Clock.AudioTime, Clock.AudioTime + 200, EasingTypes.In));
                spriteFollowCircle.Transform(new Transformation(TransformationType.Fade, 1, 1, Clock.AudioTime + 200, EndTime));

            }

            //Check if we've hit a new endpoint...
            if ((int)progressCurrent != (int)progressLastUpdate)
            {
                lastScoredEndpoint++;

                if (RepeatCount - lastScoredEndpoint < 3 && RepeatCount - lastScoredEndpoint > 0)
                {
                    //we can turn off some repeat arrows...
                    if (lastScoredEndpoint % 2 == 0)
                        spriteCollectionStart[2].Transformations.Clear();
                    else
                        spriteCollectionEnd[2].Transformations.Clear();
                }

                if (isTracking)
                {
                    Transformation circleScaleOut = new Transformation(TransformationType.Scale, 1.0F, 1.9F,
                        Clock.Time, (int)(Clock.Time + (DifficultyManager.FadeOut * 0.7)), EasingTypes.In);

                    Transformation circleScaleOut2 = new Transformation(TransformationType.Scale, 1.9F, 2F,
                        (int)(Clock.Time + (DifficultyManager.FadeOut * 0.7)), (Clock.Time + DifficultyManager.FadeOut));

                    Transformation circleFadeOut = new Transformation(TransformationType.Fade, 1, 0,
                        Clock.Time, Clock.Time + DifficultyManager.FadeOut);

                    foreach (pSprite p in lastScoredEndpoint % 2 == 0 ? spriteCollectionStart : spriteCollectionEnd)
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

                if (RepeatCount - lastScoredEndpoint == 0)
                {
                    //we've hit the end of the slider altogether.
                    spriteFollowBall.RunAnimation = false;
                    spriteFollowCircle.Transformations.Clear();

                    IsEndHit = true;

                    float amountHit = (float)scoringEndpointsHit / (lastScoredEndpoint + 1);
                    ScoreChange amount;

                    if (amountHit == 1)
                        amount = ScoreChange.Hit300;
                    else if (amountHit > 0.8)
                        amount = ScoreChange.Hit100;
                    else if (amountHit > 0)
                        amount = ScoreChange.Hit50;
                    else
                        amount = ScoreChange.Miss;

                    HitAnimation(amount);
                    return amount; //actual judging
                }

                return isTracking ? ScoreChange.SliderRepeat : ScoreChange.MissHpOnly;
            }

            return ScoreChange.Ignore;
        }

        private void CalculateSplines()
        {
            List<Vector2> smoothPoints;

            switch (CurveType)
            {
                case CurveTypes.Bezier:
                default:
                    smoothPoints = pMathHelper.CreateBezier(controlPoints, 10);
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
                Line l = new Line(smoothPoints[i], smoothPoints[i - 1]);
                drawableSegments.Add(l);

                float lineLength = l.rho;

                if (lineLength + currentLength > PathLength)
                {
                    l.p2 = l.p1 + Vector2.Normalize((l.p2 - l.p1) * (float)(l.rho - (PathLength - currentLength)));
                    l.Recalc();

                    currentLength += l.rho;
                    cumulativeLengths.Add(currentLength);
                    break; //we are done.
                }

                currentLength += l.rho;
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

        internal Vector2 TrackingPosition;
        private float startAngle;
        private float endAngle;


        float progressLastUpdate;
        float progressCurrent;
        int lastScoredEndpoint;

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public override void Update()
        {
            if (!IsVisible)
                return;

            progressLastUpdate = progressCurrent;

            float progress = pMathHelper.ClampToOne((float)(Clock.AudioTime - StartTime) / (EndTime - StartTime)) * RepeatCount;
            progressCurrent = progress;

            bool backwards = false;


            while (progress > 1)
            {
                backwards = !backwards;
                progress -= 1;
            }

            if (backwards)
                progress = 1 - progress;

            spriteFollowBall.Reverse = backwards;

            //length we are looking to achieve based on time progress through slider
            double aimLength = PathLength * progress;

            int index = Math.Max(0, cumulativeLengths.FindIndex(l => l >= aimLength) - 1);
            double lengthAtIndex = cumulativeLengths[index];

            //we need to finish off the current position using a bit of the line length
            Line currentLine = drawableSegments[index];
            TrackingPosition = currentLine.p1 + Vector2.Normalize((currentLine.p2 - currentLine.p1) * (float)(currentLine.rho - (aimLength - lengthAtIndex)));

            if (IsVisible && (lengthDrawn < PathLength || trackTexture == null) && (Clock.AudioTime > StartTime - DifficultyManager.PreEmptSnakeStart))
                UpdatePathTexture();

            spriteFollowBall.Position = TrackingPosition;
            spriteFollowBall.Rotation = currentLine.theta + (float)Math.PI;

            spriteFollowCircle.Position = TrackingPosition;

            //Adjust the angles of the end arrows
            if (RepeatCount > 1)
                spriteCollectionEnd[2].Rotation = 3 + endAngle + (float)((MathHelper.Pi / 32) * ((Clock.AudioTime % 300) / 300f - 0.5) * 4);
            if (RepeatCount > 2)
                spriteCollectionStart[2].Rotation = startAngle + (float)((MathHelper.Pi / 32) * ((Clock.AudioTime % 300) / 300f - 0.5) * 4);
        }

        internal void DisposePathTexture()
        {
            if (trackTexture != null)
                trackTexture.Dispose();
            trackTexture = null;
        }

        int pathTextureUpdateSkippedFrames;
        internal void UpdatePathTexture()
        {
            if (trackTexture == null) // Perform setup to begin drawing the slider track.
                CreatePathTexture();

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

            while (lastSegmentIndex < cumulativeLengths.Count && cumulativeLengths[lastSegmentIndex] < lengthDrawn)
                lastSegmentIndex++;

            if (lastSegmentIndex >= cumulativeLengths.Count)
            {
                lengthDrawn = PathLength;
                lastSegmentIndex = drawableSegments.Count - 1;
            }

            Line prev = null;
            if (FirstSegmentIndex > 0) prev = drawableSegments[FirstSegmentIndex - 1];

            if (lastSegmentIndex >= FirstSegmentIndex)
            {
#if !IPHONE
                List<Line> partialDrawable = drawableSegments.GetRange(FirstSegmentIndex, lastSegmentIndex - FirstSegmentIndex + 1);
                Vector2 drawEndPosition = partialDrawable[partialDrawable.Count - 1].p2;
                spriteCollectionEnd.ForEach(s => s.Position = drawEndPosition);

                if (pathTextureUpdateSkippedFrames++ % 3 == 0 || lengthDrawn == PathLength)
                {
                    GL.Viewport(0, 0, trackBoundsNative.Width, trackBoundsNative.Height);
                    GL.MatrixMode(MatrixMode.Projection);

                    GL.LoadIdentity();
                    GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Top, trackBounds.Bottom, -1, 1);
                    /*GL.Ortho(-GameBase.GamefieldOffsetVector1.X,
                             1024 / GameBase.WindowRatio - GameBase.GamefieldOffsetVector1.X,
                             -GameBase.GamefieldOffsetVector1.Y,
                             1024 / GameBase.WindowRatio - GameBase.GamefieldOffsetVector1.Y,
                             -1, 1);*/

                    

                    m_HitObjectManager.sliderTrackRenderer.Draw(partialDrawable,
                                                              DifficultyManager.HitObjectRadius, ColourIndex, prev);


                    GL.Disable((EnableCap)TextureGl.SURFACE_TYPE);

                    GL.BindTexture(TextureGl.SURFACE_TYPE, trackTexture.TextureGl.Id);
                    GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                    GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, trackTexture.TextureGl.potWidth, trackTexture.TextureGl.potHeight, 0);
                    GL.Disable((EnableCap)TextureGl.SURFACE_TYPE);

                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    GameBase.Instance.SetViewport();
                }
#endif
            }
#endif
        }

        private void CreatePathTexture()
        {
            // Allocate the track's texture resources.
            RectangleF rectf = FindBoundingBox(drawableSegments, DifficultyManager.HitObjectRadius);

            trackBounds.X = (int)(rectf.X);
            trackBounds.Y = (int)(rectf.Y);
            trackBounds.Width = (int)rectf.Width + 1;// (int)(rectf.Right * GameBase.WindowRatio + 1.0f) - trackBounds.X;
            trackBounds.Height = (int)rectf.Height + 1;// (int)(rectf.Bottom * GameBase.WindowRatio + 1.0f) - trackBounds.Y;

            trackBoundsNative.X = (int)((rectf.X + GameBase.GamefieldOffsetVector1.X) * GameBase.WindowRatio);
            trackBoundsNative.Y = (int)((rectf.Y + GameBase.GamefieldOffsetVector1.Y) * GameBase.WindowRatio);
            trackBoundsNative.Width = (int)(rectf.Width * GameBase.WindowRatio) + 1;
            trackBoundsNative.Height = (int)(rectf.Height * GameBase.WindowRatio) + 1;

            lengthDrawn = 0;
            lastSegmentIndex = 0;

#if !IPHONE
            int newtexid = GL.GenTexture();
            TextureGl gl = new TextureGl(trackBoundsNative.Width, trackBoundsNative.Height);
            gl.SetData(newtexid);
            trackTexture = new pTexture(gl, trackBoundsNative.Width, trackBoundsNative.Height);

            spriteSliderBody.Texture = trackTexture;
            spriteSliderBody.Position = new Vector2(trackBoundsNative.X, trackBoundsNative.Y);
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