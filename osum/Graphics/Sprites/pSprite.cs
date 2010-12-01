using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Helpers;

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
using osu_common.Helpers;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
using osum.Helpers;
using osu_common.Helpers;
#endif

namespace osum.Graphics.Sprites
{
    internal partial class pSprite : IDrawable, IDisposable
    {
        protected pList<Transformation> transformations;

        internal Vector2 StartPosition;

        protected pTexture texture;
        internal Vector2 OriginVector;
        protected SpriteEffect effect;
        protected BlendingFactorDest blending;

        internal Vector2 Position, Scale;
        internal FieldTypes Field;
        internal OriginTypes Origin;
        internal ClockTypes Clocking;
        internal Color4 Colour;
        internal float DrawDepth;
        internal float Rotation;
        internal bool AlwaysDraw;
        internal bool Reverse;

        internal int DrawTop;
        internal int DrawLeft;
        internal int DrawWidth;
        internal int DrawHeight;

        internal bool Disposable;

        internal int TextureWidth { get { return texture != null ? texture.Width : 0; } }
        internal int TextureHeight { get { return texture != null ? texture.Height : 0; } }
        internal int TextureX { get { return texture != null ? texture.X : 0; } }
        internal int TextureY { get { return texture != null ? texture.Y : 0; } }

        internal float ScaleScalar { get { return Scale.X; } set { Scale = new Vector2(value, value); } }

        internal bool Additive { get { return blending == BlendingFactorDest.One; } set { blending = value ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha; } }

        internal float Alpha;

        public object Tag;
        public int TagNumeric;

        /// <summary>
        /// Determines whether the sprite automatically remove past transformations.
        /// </summary>
        internal bool RemoveOldTransformations = true;

        internal virtual pTexture Texture
        {
            get { return texture; }
            set
            {
                if (value == texture)
                    return;

                texture = value;

                UpdateTextureSize();
                UpdateTextureAlignment();
            }
        }

        internal pSprite(pTexture texture, OriginTypes origin, Vector2 position, Color4 colour)
            : this(texture, FieldTypes.Standard, origin, ClockTypes.Game, position, 1, false, Color4.White)
        {
        }

        /// <summary>
        /// Important: don't use this to add new transformations, use pSprite.Transform() for that.
        /// </summary>
        public pList<Transformation> Transformations
        {
            get { return transformations; }
        }

        internal pSprite(pTexture texture, FieldTypes field, OriginTypes origin, ClockTypes clocking, Vector2 position, float depth, bool alwaysDraw, Color4 colour)
        {
            this.transformations = new pList<Transformation>();

            this.Field = field;
            this.Origin = origin;
            this.Clocking = clocking;

            this.Position = position;
            this.StartPosition = position;
            this.Colour = colour;

            this.Scale = Vector2.One;
            this.Rotation = 0;
            this.effect = SpriteEffect.None;
            this.blending = BlendingFactorDest.OneMinusSrcAlpha;
            this.DrawDepth = depth;
            this.AlwaysDraw = alwaysDraw;

            if (!alwaysDraw)
                Alpha = 0;
            else
                Alpha = 1;

            this.Texture = texture;
        }

        internal virtual void UpdateTextureAlignment()
        {
            //if (Type == SpriteTypes.NativeText || Type == SpriteTypes.SpriteText)
            //    return;
            if (texture == null)
                return;

            switch (Origin)
            {
                case OriginTypes.TopLeft:
                    OriginVector = Vector2.Zero;
                    break;
                case OriginTypes.TopCentre:
                    OriginVector = new Vector2(TextureWidth / 2, 0);
                    break;
                case OriginTypes.TopRight:
                    OriginVector = new Vector2(TextureWidth, 0);
                    break;
                case OriginTypes.CentreLeft:
                    OriginVector = new Vector2(0, TextureHeight / 2);
                    break;
                case OriginTypes.Centre:
                    OriginVector = new Vector2(TextureWidth / 2, TextureHeight / 2);
                    break;
                case OriginTypes.CentreRight:
                    OriginVector = new Vector2(TextureWidth, TextureHeight / 2);
                    break;
                case OriginTypes.BottomLeft:
                    OriginVector = new Vector2(0, TextureHeight);
                    break;
                case OriginTypes.BottomCentre:
                    OriginVector = new Vector2(TextureWidth / 2, TextureHeight);
                    break;
                case OriginTypes.BottomRight:
                    OriginVector = new Vector2(TextureWidth, TextureHeight);
                    break;
            }
        }

        internal virtual void UpdateTextureSize()
        {
            DrawWidth = TextureWidth;
            DrawHeight = TextureHeight;
            DrawTop = TextureY;
            DrawLeft = TextureX;
        }

        internal void Transform(Transformation transform)
        {
            transform.Clocking = this.Clocking;
            Transformations.AddInPlace(transform);
        }

        internal void Transform(IEnumerable<Transformation> transforms)
        {
            foreach (Transformation t in transforms)
                this.Transform(t);
        }

        protected Box2 TextureRectangle
        {
            get { return new Box2(DrawLeft, DrawTop, DrawWidth + DrawLeft, DrawHeight + DrawTop); }
        }

        protected Box2 DisplayRectangle
        {
            get
            {
                Vector2 topLeft = FieldPosition - OriginVector;
                Vector2 bottomRight = new Vector2(FieldPosition.X + DrawWidth * FieldScale.X, FieldPosition.Y + DrawHeight * FieldScale.Y);

                return new Box2(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
            }
        }

        public virtual void Update()
        {
            UpdateTransformations();
        }

        /// <summary>
        /// Iterates through each tansformation and applies where necessary.
        /// </summary>
        private void UpdateTransformations()
        {
            bool hasColour = false;
            bool hasAlpha = false;
            bool hasRotation = false;
            bool hasScale = false;
            bool hasMovement = false;
            bool hasMovementX = false;

            for (int i = 0; i < transformations.Count; i++)
            {
                Transformation t = transformations[i];

                // remove old transformations
                if (t.Terminated)
                {
                    switch (t.Type)
                    {
                        case TransformationType.Colour:
                            Colour = t.EndColour;
                            hasColour = true;
                            break;

                        case TransformationType.Fade:
                            Alpha = t.EndFloat;
                            hasAlpha = true;
                            break;

                        case TransformationType.Movement:
                            Position = t.EndVector;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementX:
                            Position.X = t.EndFloat;
                            hasMovementX = true;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = t.EndFloat;
                            break;

                        case TransformationType.ParameterAdditive:
                            blending = BlendingFactorDest.One;
                            break;

                        case TransformationType.ParameterFlipHorizontal:
                            effect |= SpriteEffect.FlipHorizontally;
                            break;

                        case TransformationType.ParameterFlipVertical:
                            effect |= SpriteEffect.FlipVertically;
                            break;

                        case TransformationType.Rotation:
                            Rotation = t.EndFloat;
                            hasRotation = true;
                            break;

                        case TransformationType.Scale:
                            Scale = new Vector2(t.EndFloat, t.EndFloat);
                            hasScale = true;
                            break;

                        case TransformationType.VectorScale:
                            Scale = t.EndVector;
                            hasScale = true;
                            break;
                    }

                    if (RemoveOldTransformations)
                        transformations.RemoveAt(i);
                    continue;
                }

                // reset some values
                effect = SpriteEffect.None;

                // update current transformations
                if (t.Initiated)
                {
                    switch (t.Type)
                    {
                        case TransformationType.Colour:
                            Colour = t.CurrentColour;
                            hasColour = true;
                            break;

                        case TransformationType.Fade:
                            Alpha = t.CurrentFloat;
                            hasAlpha = true;
                            break;

                        case TransformationType.Movement:
                            Position = t.CurrentVector;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementX:
                            Position.X = t.CurrentFloat;
                            hasMovementX = true;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = t.CurrentFloat;
                            break;

                        case TransformationType.ParameterAdditive:
                            blending = BlendingFactorDest.One;
                            break;

                        case TransformationType.ParameterFlipHorizontal:
                            effect |= SpriteEffect.FlipHorizontally;
                            break;

                        case TransformationType.ParameterFlipVertical:
                            effect |= SpriteEffect.FlipVertically;
                            break;

                        case TransformationType.Rotation:
                            Rotation = t.CurrentFloat;
                            hasRotation = true;
                            break;

                        case TransformationType.Scale:
                            Scale = new Vector2(t.CurrentFloat, t.CurrentFloat);
                            hasScale = true;
                            break;

                        case TransformationType.VectorScale:
                            Scale = t.CurrentVector;
                            hasScale = true;
                            break;
                    }

                    continue;
                }

                switch (t.Type)
                {
                    case TransformationType.Colour:
                        if (!hasColour)
                        {
                            hasColour = true;
                            Colour = t.CurrentColour;
                        }
                        break;

                    case TransformationType.Fade:
                        if (!hasAlpha)
                        {
                            hasAlpha = true;
                            Alpha = t.CurrentFloat;
                        }
                        break;

                    case TransformationType.Movement:
                        if (!hasMovement)
                            Position = t.CurrentVector;
                        break;

                    case TransformationType.MovementX:
                        if (!hasMovementX)
                            Position.X = t.CurrentFloat;
                        break;

                    case TransformationType.MovementY:
                        Position.Y = t.CurrentFloat;
                        break;

                    case TransformationType.ParameterAdditive:
                        blending = BlendingFactorDest.One;
                        break;

                    case TransformationType.ParameterFlipHorizontal:
                        effect |= SpriteEffect.FlipHorizontally;
                        break;

                    case TransformationType.ParameterFlipVertical:
                        effect |= SpriteEffect.FlipVertically;
                        break;

                    case TransformationType.Rotation:
                        if (!hasRotation)
                        {
                            hasRotation = true;
                            Rotation = t.CurrentFloat;
                        }
                        break;

                    case TransformationType.Scale:
                        if (!hasScale)
                        {
                            hasScale = true;
                            Scale = new Vector2(t.CurrentFloat, t.CurrentFloat);
                        }
                        break;

                    case TransformationType.VectorScale:
                        if (!hasScale)
                        {
                            hasScale = true;
                            Scale = t.CurrentVector;
                        }
                        break;
                }
            }
        }

        protected Vector2 FieldPosition
        {
            get
            {
                Vector2 fieldPosition;

                switch (Field)
                {
                    case FieldTypes.StandardSnapCentre:
                        fieldPosition = new Vector2(GameBase.WindowBaseSize.Width / 2 + Position.X, GameBase.WindowBaseSize.Height / 2 + Position.Y);
                        break;
                    case FieldTypes.StandardSnapBottomCentre:
                        fieldPosition = new Vector2(GameBase.WindowBaseSize.Width / 2 + Position.X, GameBase.WindowBaseSize.Height - Position.Y);
                        break;
                    case FieldTypes.StandardSnapRight:
                        fieldPosition = new Vector2(GameBase.WindowBaseSize.Width - Position.X, Position.Y);
                        break;
                    case FieldTypes.StandardSnapBottomLeft:
                        fieldPosition = new Vector2(Position.X, GameBase.WindowBaseSize.Height - Position.Y);
                        break;
                    case FieldTypes.StandardSnapBottomRight:
                        fieldPosition = new Vector2(GameBase.WindowBaseSize.Width - Position.X, GameBase.WindowBaseSize.Height - Position.Y);
                        break;
                    case FieldTypes.Gamefield512x384:
                        fieldPosition = Position;
                        GameBase.GamefieldToStandard(ref fieldPosition);
                        break;
                    case FieldTypes.Native:
                        return Position / GameBase.WindowRatio;
                    default:
                        fieldPosition = Position;
                        break;
                }

                return fieldPosition;
            }
        }

        protected Vector2 FieldScale
        {
            get
            {
                switch (Field)
                {
                    case FieldTypes.Gamefield512x384:
                        return Scale * GameBase.SpriteRatioToWindowBase;
                    case FieldTypes.Native:
                        return Scale / GameBase.WindowRatio;
                    default:
                        return Scale * GameBase.SpriteRatioToWindowBase;
                }
            }
        }

        protected Color4 AlphaAppliedColour
        {
            get
            {
                if (SpriteManager.UniversalDim > 0)
                    return ColourHelper.Darken(new Color4(Colour.R, Colour.G, Colour.B, Alpha * Colour.A), SpriteManager.UniversalDim);

                return Alpha < 1 ? new Color4(Colour.R, Colour.G, Colour.B, Alpha * Colour.A) : Colour;
            }
        }

        public virtual void Draw()
        {
            pTexture texture = Texture;
            if (texture == null || texture.TextureGl == null)
                return;

            if (transformations.Count != 0 || AlwaysDraw)
            {
                if (Alpha != 0)
                {
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, blending);
                    texture.TextureGl.Draw(FieldPosition, OriginVector, AlphaAppliedColour, FieldScale, Rotation, TextureRectangle, effect);
                }
            }

        }

        internal void FadeIn(int duration)
        {
            int count = Transformations.Count;

            if (count == 0 && Alpha == 1)
                return;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && t.EndFloat == 1 && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (1 - Alpha < float.Epsilon)
                return;

            int now = Clock.GetTime(Clocking);
            this.Transform(new Transformation(TransformationType.Fade,
                            (float)Alpha, (Colour.A != 0 ? (float)Colour.A : 1),
                            now, now + duration));
        }

        internal void FadeInFromZero(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = Clock.GetTime(Clocking);
            this.Transform(new Transformation(TransformationType.Fade,
                            0, (Colour.A != 0 ? (float)Colour.A : 1),
                            now, now + duration));
        }

        internal void FadeOut(int duration)
        {
            int count = Transformations.Count;

            if (count == 0 && Alpha == 0)
                return;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && t.EndFloat == 0 && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (Alpha < float.Epsilon)
                return;

            int now = Clock.GetTime(Clocking);
            this.Transform(new Transformation(TransformationType.Fade, (float)Alpha, 0, now, now + duration));
        }

        internal void FadeOutFromOne(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = Clock.GetTime(Clocking);
            this.Transform(new Transformation(TransformationType.Fade, 1, 0, now, now + duration));
        }

        internal void FadeColour(Color4 colour, int duration)
        {
            FadeColour(colour, duration, false);
        }

        internal void FadeColour(Color4 colour, int duration, bool force)
        {
            if (!force && Colour == colour && Transformations.Count == 0)
                return;

            Transformations.RemoveAll(t => t.Type == TransformationType.Colour);

            if (duration == 0)
            {
                //Shortcut a duration of 0.
                Colour = colour;
                return;
            }

            Transform(
                new Transformation(Colour, colour,
                                   Clock.GetTime(Clocking) - (int)GameBase.ElapsedMilliseconds,
                                   Clock.GetTime(Clocking) + duration));
        }

        internal void MoveTo(Vector2 destination, int duration)
        {
            MoveTo(destination, duration, EasingTypes.None);
        }

        internal void MoveTo(Vector2 destination, int duration, EasingTypes easing)
        {
            Transformations.RemoveAll(t => (t.Type & TransformationType.Movement) > 0);

            if (destination == Position)
                return;

            int now = Clock.GetTime(Clocking);

            Transformation tr =
                new Transformation(Position, destination,
                                   now - (int)Math.Max(1, GameBase.ElapsedMilliseconds),
                                   now + duration, easing);
            Transform(tr);
        }

        public virtual pSprite Clone()
        {
            pSprite clone = new pSprite(Texture, Field, Origin, Clocking, StartPosition, DrawDepth, AlwaysDraw, Colour);
            clone.Position = Position;

            clone.DrawLeft = DrawLeft;
            clone.DrawTop = DrawTop;
            clone.DrawWidth = DrawWidth;
            clone.DrawHeight = DrawHeight;

            clone.OriginVector = OriginVector;

            clone.Scale = Scale;
            clone.Rotation = Rotation;

            foreach (Transformation t in Transformations)
                //if (!t.IsLoopStatic) 
                clone.Transform(t.Clone());

            /*
            if (Loops != null)
            {
                clone.Loops = new List<TransformationLoop>(Loops.Count);
                foreach (TransformationLoop tl in Loops)
                    clone.Loops.Add(tl.Clone());
            }
            */

            return clone;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            //todo: kill texture if possible

            UnbindAllEvents();

        }

        #endregion
    }

    internal class pSpriteDepthComparer : IComparer<pSprite>
    {
        #region IComparer<pSprite> Members

        public int Compare(pSprite x, pSprite y)
        {
            return x.DrawDepth.CompareTo(y.DrawDepth);
        }

        #endregion
    }

    internal enum FieldTypes
    {
        /// <summary>
        ///   The gamefield resolution.  Used for hitobjects and anything which needs to align with gameplay elements.
        ///   This is scaled in proportion to the native resolution and aligned accordingly.
        /// </summary>
        Gamefield512x384,
        /// <summary>
        ///   Gamefield "container" resolution.  This is where storyboard and background events sits, and is the same
        ///   scaling/positioning as Standard when in play mode.  It differs in editor design mode where this field
        ///   will be shrunk to allow for editing.
        /// </summary>
        Gamefield640x480,

        /// <summary>
        ///   The standard 640x480 window resolution.  Sprites are scaled down from 1024x768 at a ratio of 5/8, then scaled
        ///   back up to native.
        /// </summary>
        Standard,

        /// <summary>
        ///   Standard window resolution, but ignoring sprite rescaling.  Sprites will be displayed at their raw dimensions
        ///   no matter what client resolution.
        /// </summary>
        StandardNoScale,

        /// <summary>
        ///   Aligns from the right-hand side of the screen, where an X position of 0 is translated to Standard(640).
        /// </summary>
        StandardSnapRight,

        StandardSnapBottomRight,

        StandardSnapBottomLeft,

        /// <summary>
        ///   Aligns from the exact centre, where a position of 0 is translated to Standard(320,240).
        /// </summary>
        StandardSnapCentre,

        /// <summary>
        ///   Aligns from the bottom centre, where a position of 0 is translated to Standard(320,480).
        /// </summary>
        StandardSnapBottomCentre,

        /// <summary>
        ///   Aligns from the centre-right point of the screen, where a position of 0 is translated to Standard(640,320).
        /// </summary>
        StandardSnapCentreRight,

        /// <summary>
        ///   Aligns from the right-hand side of the screen but ignores scaling, rendering at raw dimensions (used for pText when
        ///   rendering native resolution text).
        /// </summary>
        StandardNoScaleSnapRight,

        /// <summary>
        ///   Standard 640x480 field at raw dimensions rounded to nearest int to avoid interpolation artifacts.
        /// </summary>
        StandardNoScaleExactCoordinates,

        StandardGamefieldScale,

        /// <summary>
        ///   Native screen resolution.
        /// </summary>
        Native,

        /// <summary>
        ///   Native screen resolution with 1024x768-native sprite scaling.
        /// </summary>
        NativeStandardScale,

        /// <summary>
        ///   Native screen resolution aligned from the right-hand side of the screen, where an X position of 0 is translated to Standard(WindowWidth).
        /// </summary>
        NativeSnapRight,
    }

    internal enum OriginTypes
    {
        TopLeft,
        Centre,
        CentreLeft,
        TopRight,
        BottomCentre,
        TopCentre,
        Custom,
        CentreRight,
        BottomLeft,
        BottomRight
    }
}

