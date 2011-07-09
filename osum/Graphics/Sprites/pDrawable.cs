using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using osum.Helpers;
using OpenTK;
using osu_common.Helpers;
using osum.GameplayElements;
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
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#else
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif

namespace osum.Graphics.Sprites
{
    public partial class pDrawable : IDrawable, IDisposable, IComparable<pDrawable>
    {
        internal float Alpha;

        internal bool AlwaysDraw;
        internal ClockTypes Clocking;

        internal Color4 Colour = Color4.White;
        /*protected Color4 colour;
        internal Color4 Colour
        {
            get { return colour; }
            set { colour = value; StartColour = value; }
        }
        
        internal Color4 StartColour;*/

        internal bool Disposable;
        internal float Rotation;
        internal Vector2 Scale = Vector2.One;
        internal Vector2 StartPosition;
        internal Vector2 Offset;
        public object Tag;
        public int TagNumeric;
        internal FieldTypes Field = FieldTypes.Standard;
        internal OriginTypes Origin;

        int StartTime;

        internal virtual Vector2 OriginVector
        {
            get
            {
                Vector2 scale = AlignToSprites ? new Vector2(Scale.X, Scale.Y * 960f / GameBase.SpriteResolution) : Scale;

                switch (Origin)
                {
                    default:
                    case OriginTypes.TopLeft:
                        return Vector2.Zero;
                    case OriginTypes.TopCentre:
                        return new Vector2(scale.X / 2, 0);
                    case OriginTypes.TopRight:
                        return new Vector2(scale.X, 0);
                    case OriginTypes.CentreLeft:
                        return new Vector2(0, scale.Y / 2);
                    case OriginTypes.Centre:
                        return new Vector2(scale.X / 2, scale.Y / 2);
                    case OriginTypes.CentreRight:
                        return new Vector2(scale.X, scale.Y / 2);
                    case OriginTypes.BottomLeft:
                        return new Vector2(0, scale.Y);
                    case OriginTypes.BottomCentre:
                        return new Vector2(scale.X / 2, scale.Y);
                    case OriginTypes.BottomRight:
                        return new Vector2(scale.X, scale.Y);
                }
            }
        }

        internal Vector2 Position;
        internal BlendingFactorDest BlendingMode = BlendingFactorDest.OneMinusSrcAlpha;

        internal bool Premultiplied;

        internal virtual bool IsOnScreen
        {
            get
            {
                Box2 rect = DisplayRectangle;

                if (ContainingSpriteManager != null)
                {
                    Vector2 offset = ContainingSpriteManager.ViewOffset;
                    if (rect.Left > GameBase.BaseSizeFixedWidth.Width + 1 - offset.X || rect.Right < -offset.X ||
                        rect.Top > GameBase.BaseSizeFixedWidth.Height + 1 - offset.Y || rect.Bottom < -offset.Y)
                        return false;
                }
                else
                {
                    if (rect.Left > GameBase.BaseSizeFixedWidth.Width + 1 || rect.Right  < 0 ||
                        rect.Top > GameBase.BaseSizeFixedWidth.Height + 1 || rect.Bottom < 0)
                        return false;
                }

                return true;
            }
        }

        public virtual pDrawable Clone()
        {

            pDrawable clone = (pDrawable)this.MemberwiseClone();
            clone.Transformations = new pList<Transformation>();
            clone.readInitialTransformationsOnce = false;

            return clone;
        }

        internal float DrawDepth;

        /// <summary>
        /// Determines whether the sprite automatically remove past transformations.
        /// </summary>
        internal bool RemoveOldTransformations = true;

        /// <summary>
        /// Important: don't use this to add new transformations, use pSprite.Transform() for that.
        /// </summary>
        internal pList<Transformation> Transformations = new pList<Transformation>() { UseBackwardsSearch = true };

        internal virtual bool IsRemovable
        {
            get { return !AlwaysDraw && noTransformationsLeft; }
        }

        internal virtual bool UsesTextures
        {
            get { return false; }
        }

        internal float ScaleScalar
        {
            get { return Scale.X; }
            set { Scale = new Vector2(value, value); }
        }

        internal bool Additive
        {
            get { return BlendingMode == BlendingFactorDest.One; }
            set { BlendingMode = value ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha; }
        }

        /// <summary>
        /// Gets the display rectangle (base size).
        /// </summary>
        internal virtual Box2 DisplayRectangle
        {
            get
            {
                Vector2 pos = FieldPosition / GameBase.BaseToNativeRatio - OriginVector;
                Vector2 scale = FieldScale / GameBase.BaseToNativeRatio;
                return new Box2(pos.X, pos.Y, pos.X + scale.X, pos.Y + scale.Y);
            }
        }

        protected bool exactCoordinatesOverride;
        internal virtual bool ExactCoordinates {
            get { return !exactCoordinatesOverride && UsesTextures && !hasMovement; }
            set {
                exactCoordinatesOverride = !value;
            }
        }


        internal virtual Vector2 FieldPosition
        {
            get
            {
                Vector2 pos = Position;

                if (Origin != OriginTypes.Custom && Offset != Vector2.Zero)
                    pos += Offset;

                switch (Field)
                {
                    default:
                        pos *= AlignToSprites ? GameBase.BaseToNativeRatioAligned : GameBase.BaseToNativeRatio;
                        break;
                    case FieldTypes.GamefieldStandardScale:
                    case FieldTypes.GamefieldSprites:
                    case FieldTypes.GamefieldExact:
                        break;
                    case FieldTypes.NativeScaled:
                        return pos;
                }

                switch (Field)
                {
                    case FieldTypes.StandardSnapCentre:
                        pos = new Vector2(GameBase.NativeSize.Width / 2 + pos.X,
                                                    GameBase.NativeSize.Height / 2 + pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomCentre:
                        pos = new Vector2(GameBase.NativeSize.Width / 2 + pos.X,
                                                    GameBase.NativeSize.Height - pos.Y);
                        break;
                    case FieldTypes.StandardSnapTopCentre:
                        pos = new Vector2(GameBase.NativeSize.Width / 2 + pos.X,
                                                    pos.Y);
                        break;
                    case FieldTypes.StandardSnapCentreRight:
                        pos = new Vector2(GameBase.NativeSize.Width - pos.X, GameBase.NativeSize.Height / 2 + pos.Y);
                        break;
                    case FieldTypes.StandardSnapCentreLeft:
                        pos = new Vector2(pos.X, GameBase.NativeSize.Height / 2 + pos.Y);
                        break;
                    case FieldTypes.StandardSnapRight:
                        pos = new Vector2(GameBase.NativeSize.Width - pos.X, pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomLeft:
                        pos = new Vector2(pos.X, GameBase.NativeSize.Height - pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomRight:
                        pos = new Vector2(GameBase.NativeSize.Width - pos.X,
                                                    GameBase.NativeSize.Height - pos.Y);
                        break;
                    case FieldTypes.GamefieldStandardScale:
                    case FieldTypes.GamefieldSprites:
                    case FieldTypes.GamefieldExact:
                        pos += GameBase.GamefieldOffsetVector1;
                        pos *= AlignToSprites ? GameBase.BaseToNativeRatioAligned : GameBase.BaseToNativeRatio;
                        break;
                    case FieldTypes.Native:
                    default:
                        break;
                }

                if (ExactCoordinates)
                {
                    pos.X = (int)Math.Round(pos.X);
                    pos.Y = (int)Math.Round(pos.Y);
                }

                return pos;
            }
        }

        /// <summary>
        /// Because the resolution of sprites is not 1:1 to the resizing of the window (ie. between 960-1024 widths, where it stays constant)
        /// an extra ratio calculation must be applied to keep sprites aligned.
        ///
        /// Use FALSE to align to the cursor, or TRUE when aligning with other sprites.
        /// </summary>
        internal bool AlignToSprites = true;

        internal virtual Vector2 FieldScale
        {
            get
            {
                switch (Field)
                {
                    case FieldTypes.GamefieldExact:
                        return Scale * DifficultyManager.HitObjectRadius;
                    case FieldTypes.GamefieldSprites:
                        return Scale * (DifficultyManager.HitObjectSizeModifier * GameBase.SpriteToNativeRatio);
                    case FieldTypes.Native:
                    case FieldTypes.NativeScaled:
                        return Scale;
                    default:
                        if (UsesTextures)
                            return Scale * GameBase.SpriteToNativeRatio;

                        if (AlignToSprites)
                        {
                            if (Scale.X != GameBase.BaseSizeFixedWidth.Width)
                                return Scale * GameBase.BaseToNativeRatioAligned;

                            //special case for drawables which take up the full screen width.
                            return new Vector2(Scale.X * GameBase.BaseToNativeRatio, Scale.Y * GameBase.BaseToNativeRatioAligned);
                        }

                        return Scale * GameBase.BaseToNativeRatio;

                }
            }
        }

        /// <summary>
        /// If true, this sprite is not affected by universal dimming.
        /// </summary>
        internal bool DimImmune;
        public bool Bypass;

        internal Color4 AlphaAppliedColour
        {
            get
            {
                float alpha = Alpha * Colour.A;

                if (SpriteManager.UniversalDim > 0 && !DimImmune)
                {
                    float dim = (1 - SpriteManager.UniversalDim);
                    if (Premultiplied)
                        dim *= alpha;

                    return new Color4(Colour.R * dim, Colour.G * dim, Colour.B * dim, alpha);
                }

                if (Premultiplied)
                    return new Color4(Colour.R * alpha, Colour.G * alpha, Colour.B * alpha, alpha);
                else
                    return new Color4(Colour.R, Colour.G, Colour.B, alpha);
            }
        }

        internal void Transform(Transformation transform)
        {
            noTransformationsLeft = false;
            if (Transformations.AddInPlace(transform) == 0)
                StartTime = transform.StartTime;
        }

        /// <summary>
        /// Assumes correct clocking and order.
        /// </summary>
        /// <param name="transforms"></param>
        internal void Transform(params Transformation[] transforms)
        {
            Transformations.AddRange(transforms);
        }

        internal void Transform(IEnumerable<Transformation> transforms)
        {
            foreach (Transformation t in transforms)
                Transform(t);
        }

        protected bool hasMovement;

        bool readInitialTransformationsOnce;

        /// <summary>
        /// Iterates through each tansformation and applies where necessary.
        /// </summary>
        private void UpdateTransformations()
        {
            hasMovement = false;

            int count = Transformations.Count;

            int now = ClockingNow;

            noTransformationsLeft = count == 0;

            if (!readInitialTransformationsOnce)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    Transformation t = Transformations[i];

                    t.Update(now);

                    if (!t.Initiated)
                    {
                        switch (t.Type)
                        {
                            case TransformationType.Colour:
                                Colour = t.StartColour;
                                break;

                            case TransformationType.Fade:
                                Alpha = t.StartFloat;
                                break;
    
                            case TransformationType.Movement:
                                Position = t.StartVector;
                                break;
    
                            case TransformationType.MovementX:
                                Position.X = t.StartFloat;
                                break;

                            case TransformationType.MovementY:
                                Position.Y = t.StartFloat;
                                break;

                            case TransformationType.Rotation:
                                Rotation = t.StartFloat;
                                break;

                            case TransformationType.Scale:
                                Scale = new Vector2(t.StartFloat, t.StartFloat);
                                break;

                            case TransformationType.VectorScale:
                                Scale = t.StartVector;
                                break;
                        }
                    }
                }

                readInitialTransformationsOnce = true;
            }
            else if (noTransformationsLeft || StartTime > now)
                return;

            for (int i = 0; i < count; i++)
            {
                Transformation t = Transformations[i];
                t.Update(now);

                // remove old transformations
                if (t.Terminated)
                {
                    switch (t.Type)
                    {
                        case TransformationType.Colour:
                            Colour = t.EndColour;
                            break;

                        case TransformationType.Fade:
                            Alpha = t.EndFloat;
                            break;

                        case TransformationType.Movement:
                            Position = t.EndVector;
                            break;

                        case TransformationType.MovementX:
                            Position.X = t.EndFloat;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = t.EndFloat;
                            break;

                        case TransformationType.OffsetX:
                            Offset.X = t.EndFloat;
                            break;

                        case TransformationType.Rotation:
                            Rotation = t.EndFloat;
                            break;

                        case TransformationType.Scale:
                            Scale = new Vector2(t.EndFloat, t.EndFloat);
                            break;

                        case TransformationType.VectorScale:
                            Scale = t.EndVector;
                            break;
                    }

                    if (RemoveOldTransformations)
                    {
                        Transformations.RemoveAt(i--);
                        count--;
                    }
                }
                // update current transformations
                else if (t.Initiated)
                {
                    switch (t.Type)
                    {
                        case TransformationType.Colour:
                            Colour = t.CurrentColour;
                            break;

                        case TransformationType.Fade:
                            Alpha = t.CurrentFloat;
                            break;

                        case TransformationType.Movement:
                            Position = t.CurrentVector;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementX:
                            Position.X = t.CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = t.CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.OffsetX:
                            Offset.X = t.CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.Rotation:
                            Rotation = t.CurrentFloat;
                            break;

                        case TransformationType.Scale:
                            Scale = new Vector2(t.CurrentFloat, t.CurrentFloat);
                            break;

                        case TransformationType.VectorScale:
                            Scale = t.CurrentVector;
                            break;
                    }
                }
            }
        }

        internal int ClockingNow
        {
            get { return Clock.GetTime(Clocking); }
        }

        internal void FadeIn(int duration, float finalAlpha = 1)
        {
            int count = Transformations.Count;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && t.EndFloat == finalAlpha && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (finalAlpha - Alpha < float.Epsilon)
                return;

            int now = ClockingNow;
            Transform(new Transformation(TransformationType.Fade, Alpha, finalAlpha, now, now + duration));
        }

        internal void FadeInFromZero(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = ClockingNow;
            Transform(new Transformation(TransformationType.Fade,
                                         0, (Colour.A != 0 ? Colour.A : 1),
                                         now, now + duration));
        }

        internal void FadeOut(int duration, float finalAlpha = 0)
        {
            int count = Transformations.Count;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && t.EndFloat == finalAlpha && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (Alpha - finalAlpha < float.Epsilon)
                return;

            int now = ClockingNow;
            Transform(new Transformation(TransformationType.Fade, Alpha, finalAlpha, now, now + duration));
        }

        internal void FadeOutFromOne(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = ClockingNow;
            Transform(new Transformation(TransformationType.Fade, 1, 0, now, now + duration));
        }

        internal SpriteManager ContainingSpriteManager;
        public bool AlphaBlend = true;
        protected bool noTransformationsLeft;

        internal pDrawable AdditiveFlash(int duration, float brightness, bool keepTransformations = false)
        {
            pDrawable clone = this.Clone();

            clone.UnbindAllEvents();

            if (!keepTransformations)
                clone.Transformations.Clear();

            clone.Alpha *= brightness;
            clone.Clocking = ClockTypes.Game;
            clone.DrawDepth = Math.Min(1, DrawDepth + 0.001f);
            clone.Additive = true;
            clone.FadeOut(duration);
            clone.AlwaysDraw = false;

            ContainingSpriteManager.Add(clone);

            return clone;
        }

        internal void FadeColour(Color4 colour, int duration, bool force = false)
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
                                         ClockingNow - (int)GameBase.ElapsedMilliseconds,
                                         ClockingNow + duration));
        }

        internal void FlashColour(Color4 colour, int duration)
        {
            Color4 end = Colour;

            Transformation last = Transformations.FindLast(t => t.Type == TransformationType.Colour);

            if (last != null)
            {
                end = last.EndColour;
                Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
            }

            Transformation flash = new Transformation(colour, end,
                                   ClockingNow,
                                   ClockingNow + duration);
            Transform(flash);
        }

        /// <summary>
        /// Moves the sprite to a specified desintation, using the current location as the source.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="easing">The easing.</param>
        internal void MoveTo(Vector2 destination, int duration, EasingTypes easing = EasingTypes.None)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Movement);

            if (destination == Position)
                return;

            if (duration == 0)
            {
                Position = destination;
                return;
            }

            int now = ClockingNow;

            Transform(new Transformation(Position, destination, now, now + duration, easing));
        }

        /// <summary>
        /// Scales the sprite to a specified desintation, using the current location as the source.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="easing">The easing.</param>
        internal pDrawable ScaleTo(float target, int duration, EasingTypes easing = EasingTypes.None)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Scale);

            if (target == ScaleScalar)
                return this;

            if (duration == 0)
                ScaleScalar = target;

            int now = ClockingNow;

            Transform(new Transformation(TransformationType.Scale, ScaleScalar, target, now, now + duration, easing));

            return this;
        }

        /// <summary>
        /// Rotates the sprite to a specified desintation, using the current location as the source.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="easing">The easing.</param>
        internal pDrawable RotateTo(float target, int duration, EasingTypes easing = EasingTypes.None)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Rotation);

            if (target == Rotation)
                return this;

            if (duration == 0)
                Rotation = target;

            int now = ClockingNow;

            Transform(new Transformation(TransformationType.Rotation, Rotation, target, now, now + duration, easing));

            return this;
        }

        #region IDrawable Members

        public virtual bool Draw()
        {
            if (Bypass) return false;

            if (Alpha != 0 && //Colour.A != 0 &&
                (AlwaysDraw || !noTransformationsLeft) && ((ContainingSpriteManager == null || !ContainingSpriteManager.CheckSpritesAreOnScreenBeforeRendering) || IsOnScreen))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IUpdateable Members

        public virtual void Update()
        {
            if (Bypass) return;

            UpdateTransformations();
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            UnbindAllEvents();
        }

        #endregion

        #region IComparable<pDrawable> Members

        public int CompareTo(pDrawable other)
        {
            return StartTime.CompareTo(other.StartTime);
        }

        #endregion
    }

    internal class pDrawableDepthComparer : IComparer<pDrawable>
    {
        #region IComparer<pDrawable> Members

        public int Compare(pDrawable x, pDrawable y)
        {
            return x.DrawDepth.CompareTo(y.DrawDepth);
        }

        #endregion
    }
}
