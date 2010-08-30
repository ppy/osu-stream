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
using osum;
using OpenTK.Graphics.OpenGL;
using osu.Graphics.Renderers;
using OpenTK.Graphics;

namespace osu.GameplayElements.HitObjects.Osu
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
        /// Points after smoothing/curve-generation has been applied.
        /// </summary>
        internal List<Vector2> smoothPoints;

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
        internal List<double> cumuLengths;

        /// <summary>
        /// Track bounding rectangle measured in SCREEN COORDINATES
        /// </summary>
        internal System.Drawing.Rectangle trackBounds;
        
        HitCircle hitCircleStart;

        internal Slider(HitObjectManager hitObjectManager, Vector2 startPosition, int startTime, bool newCombo, HitObjectSoundType soundType,
                        CurveTypes curveType, int repeatCount, double pathLength, List<Vector2> sliderPoints,
                        List<HitObjectSoundType> soundTypes)
            : base(hitObjectManager, startPosition, startTime, soundType, newCombo)
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

            hitCircleStart = new HitCircle(hitObjectManager, Position, StartTime, newCombo, soundType);

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
            double currentLength = 0;

            drawableSegments = new List<Line>();
            cumuLengths = new List<double>();

            for (int i = 1; i < smoothPoints.Count; i++)
            {
                Line l = new Line(smoothPoints[i], smoothPoints[i - 1]);
                drawableSegments.Add(l);

                float lineLength = l.rho;

                if (lineLength + currentLength > PathLength)
                {
                    l.p2 = l.p1 + Vector2.Normalize((l.p2 - l.p1) * (float)(l.rho - (PathLength - currentLength)));
                    l.Recalc();

                    //currentLength += l.rho;
                    //break; //we are done. // Just fall through ~mm
                }

                currentLength += l.rho;
                cumuLengths.Add(currentLength);
            }

            PathLength = currentLength;
            EndTime = StartTime + (int)(1000 * PathLength / DifficultyManager.SliderVelocity);
        }

        /// <summary>
        /// Find the extreme values of the given curve in the form of a box.
        /// </summary>
        private static System.Drawing.RectangleF FindBoundingBox(List<Line> curve, float radius)
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

            if (trackTexture == null) // Perform setup to begin drawing the slider track.
            {
                // Allocate the track's texture resources.
                System.Drawing.RectangleF rectf = FindBoundingBox(drawableSegments, DifficultyManager.HitObjectRadius);
                trackBounds.X = (int)(rectf.X * GameBase.WindowRatio);
                trackBounds.Y = (int)(rectf.Y * GameBase.WindowRatio);

                // Round these up so nothing gets clipped.
                trackBounds.Width = (int)(rectf.Right * GameBase.WindowRatio + 1.0f) - trackBounds.X;
                trackBounds.Height = (int)(rectf.Bottom * GameBase.WindowRatio + 1.0f) - trackBounds.Y;

                lengthDrawn = 0;
                lastSegmentIndex = -1;

                int newtexid = GL.GenTexture();
                TextureGl gl = new TextureGl(trackBounds.Width, trackBounds.Height);
                gl.SetData(newtexid);
                trackTexture = new pTexture(gl, trackBounds.Width, trackBounds.Height);

                spriteSliderBody = new pSprite(trackTexture, FieldTypes.Native, OriginTypes.TopLeft,
                                   ClockTypes.Audio, new Vector2(trackBounds.X, trackBounds.Y), SpriteManager.drawOrderBwd(EndTime + 10),
                                   false, Color.White);

                spriteSliderBody.Transformations.Clear();

                spriteSliderBody.Transformations.Add(
                    new Transformation(TransformationType.Fade, 0, 0.97F, StartTime - DifficultyManager.PreEmpt,
                                       StartTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn));
                spriteSliderBody.Transformations.Add(
                    new Transformation(TransformationType.Fade, 0.97F, 0, EndTime,
                                       EndTime + DifficultyManager.FadeOut));

                SpriteCollection.Add(spriteSliderBody);
            }

            if (IsVisible && (lengthDrawn < PathLength) && (Clock.AudioTime > StartTime - DifficultyManager.PreEmptSnakeStart))
            {
                Draw();
            }

            spriteFollowBall.Position = currentSegment.p1;
            spriteFollowBall.Rotation = currentSegment.theta + (float)Math.PI;

            spriteFollowCircle.Position = currentSegment.p1;
        }

        internal void Draw()
        {
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

            lastSegmentIndex = cumuLengths.FindLastIndex(d => d < lengthDrawn);
            if (lastSegmentIndex == -1)
            {
                lengthDrawn = PathLength;
                lastSegmentIndex = drawableSegments.Count - 1;
            }

            Line prev = null;
            if (FirstSegmentIndex > 0) prev = drawableSegments[FirstSegmentIndex - 1];

            if (lastSegmentIndex >= FirstSegmentIndex)
            {

                GL.Viewport(0, 0, trackBounds.Width, trackBounds.Height);

                GL.MatrixMode(MatrixMode.Modelview);

                GL.LoadIdentity();

                GL.MatrixMode(MatrixMode.Projection);

                GL.LoadIdentity();
                GL.Ortho(trackBounds.Left, trackBounds.Right, trackBounds.Bottom, trackBounds.Top, -1.0d, 2.0d);

                GL.Clear(ClearBufferMask.ColorBufferBit);

                m_HitObjectManager.sliderTrackRenderer.Draw(drawableSegments.GetRange(FirstSegmentIndex, lastSegmentIndex - FirstSegmentIndex + 1),
                                                          DifficultyManager.HitObjectRadius, 0, prev);

                GL.Enable((EnableCap)TextureGl.SURFACE_TYPE);

                GL.BindTexture(TextureGl.SURFACE_TYPE, trackTexture.TextureGl.Id);
                GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.CopyTexImage2D(TextureGl.SURFACE_TYPE, 0, PixelInternalFormat.Rgba, 0, 0, trackBounds.Width, trackBounds.Height, 0);
                GL.Disable((EnableCap)TextureGl.SURFACE_TYPE);

                //restore viewport (can make this more efficient but not much point?)
                GameBase.Instance.SetupScreen();

            }

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