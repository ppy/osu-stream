#if iOS || ANDROID
using OpenTK.Graphics.ES11;
#if iOS
using Foundation;
using ObjCRuntime;
using OpenGLES;
#endif

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
#if iOS
using UIKit;
using CoreGraphics;
#endif
#else
using OpenTK.Graphics.OpenGL;
#endif
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osum.GameplayElements;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    public partial class pDrawable : IComparable<pDrawable>
    {
        internal float Alpha;

        internal bool AlwaysDraw;
        internal ClockTypes Clocking = ClockTypes.Mode;

        internal Color4 Colour = Color4.White;

        internal bool Disposable;
        internal float Rotation;
        internal Vector2 Scale = Vector2.One;
        internal Vector2 Offset;
        public object Tag;
        public int TagNumeric;
        internal FieldTypes Field = FieldTypes.Standard;
        internal OriginTypes Origin;

        private int StartTime;

        internal Vector2 OriginVector;

        internal virtual void UpdateOriginVector()
        {
            Vector2 scale = AlignToSprites ? new Vector2(Scale.X, Scale.Y * 960f / GameBase.SpriteResolution) : Scale;

            switch (Origin)
            {
                default:
                    OriginVector = Vector2.Zero;
                    break;
                case OriginTypes.TopCentre:
                    OriginVector = new Vector2(scale.X / 2, 0);
                    break;
                case OriginTypes.TopRight:
                    OriginVector = new Vector2(scale.X, 0);
                    break;
                case OriginTypes.CentreLeft:
                    OriginVector = new Vector2(0, scale.Y / 2);
                    break;
                case OriginTypes.Centre:
                    OriginVector = new Vector2(scale.X / 2, scale.Y / 2);
                    break;
                case OriginTypes.CentreRight:
                    OriginVector = new Vector2(scale.X, scale.Y / 2);
                    break;
                case OriginTypes.BottomLeft:
                    OriginVector = new Vector2(0, scale.Y);
                    break;
                case OriginTypes.BottomCentre:
                    OriginVector = new Vector2(scale.X / 2, scale.Y);
                    break;
                case OriginTypes.BottomRight:
                    OriginVector = new Vector2(scale.X, scale.Y);
                    break;
            }
        }

        internal Vector2 Position;
        internal BlendingFactorDest BlendingMode = BlendingFactorDest.OneMinusSrcAlpha;

        internal virtual bool IsOnScreen
        {
            get
            {
                Box2 rect = DisplayRectangle;

                if (ContainingSpriteManager != null)
                {
                    Vector2 offset = ContainingSpriteManager.ViewOffset * GameBase.InputToFixedWidthAlign;
                    if (rect.Left > GameBase.BaseSizeFixedWidth.X + 1 - offset.X || rect.Right < -offset.X ||
                        rect.Top > GameBase.BaseSizeFixedWidth.Y + 1 - offset.Y || rect.Bottom < -offset.Y)
                        return false;
                }
                else
                {
                    if (rect.Left > GameBase.BaseSizeFixedWidth.X + 1 || rect.Right < 0 ||
                        rect.Top > GameBase.BaseSizeFixedWidth.Y + 1 || rect.Bottom < 0)
                        return false;
                }

                return true;
            }
        }

        public virtual pDrawable Clone()
        {
            pDrawable clone = (pDrawable)MemberwiseClone();
            clone.Transformations = new pList<Transformation> { UseBackwardsSearch = true };
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
        internal pList<Transformation> Transformations = new pList<Transformation> { UseBackwardsSearch = true };

        internal virtual bool IsRemovable => !AlwaysDraw && noTransformationsLeft;

        internal bool UsesTextures;

        internal float ScaleScalar
        {
            get => Scale.X;
            set
            {
                if (Scale.X == value && Scale.Y == value)
                    return;

                Scale = new Vector2(value, value);
            }
        }

        internal bool Additive
        {
            get => BlendingMode == BlendingFactorDest.One;
            set => BlendingMode = value ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha;
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

        internal virtual bool ExactCoordinates
        {
            get => !exactCoordinatesOverride && UsesTextures && !hasMovement;
            set => exactCoordinatesOverride = !value;
        }

        internal Vector2 FieldPosition;
        internal Vector2 FieldScale;

        internal virtual void UpdateFieldPosition()
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
                    FieldPosition = pos;
                    return;
            }

            switch (Field)
            {
                case FieldTypes.StandardSnapCentre:
                    pos = new Vector2(GameBase.NativeSize.Width / 2f + pos.X, GameBase.NativeSize.Height / 2f + pos.Y);
                    break;
                case FieldTypes.StandardSnapBottomCentre:
                    pos = new Vector2(GameBase.NativeSize.Width / 2f + pos.X, GameBase.NativeSize.Height - pos.Y);
                    break;
                case FieldTypes.StandardSnapTopCentre:
                    pos = new Vector2(GameBase.NativeSize.Width / 2f + pos.X, pos.Y);
                    break;
                case FieldTypes.StandardSnapCentreRight:
                    pos = new Vector2(GameBase.NativeSize.Width - pos.X, GameBase.NativeSize.Height / 2f + pos.Y);
                    break;
                case FieldTypes.StandardSnapCentreLeft:
                    pos = new Vector2(pos.X, GameBase.NativeSize.Height / 2f + pos.Y);
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
            }

            if (ExactCoordinates)
            {
                pos.X = (int)(pos.X + 0.5f);
                pos.Y = (int)(pos.Y + 0.5f);
            }

            FieldPosition = pos;
        }

        /// <summary>
        /// Because the resolution of sprites is not 1:1 to the resizing of the window (ie. between 960-1024 widths, where it stays constant)
        /// an extra ratio calculation must be applied to keep sprites aligned.
        ///
        /// Use FALSE to align to the cursor, or TRUE when aligning with other sprites.
        /// </summary>
        internal bool AlignToSprites = true;

        internal virtual void UpdateFieldScale()
        {
            switch (Field)
            {
                case FieldTypes.GamefieldExact:
                    FieldScale = Scale * DifficultyManager.HitObjectRadius;
                    break;
                case FieldTypes.GamefieldSprites:
                    FieldScale = Scale * (DifficultyManager.HitObjectSizeModifier * GameBase.SpriteToNativeRatio);
                    break;
                case FieldTypes.Native:
                case FieldTypes.NativeScaled:
                    FieldScale = Scale;
                    break;
                default:
                    if (UsesTextures)
                    {
                        FieldScale = Scale * GameBase.SpriteToNativeRatio;
                        return;
                    }


                    if (AlignToSprites)
                    {
                        if (Scale.X != GameBase.BaseSizeFixedWidth.X)
                        {
                            FieldScale = Scale * GameBase.BaseToNativeRatioAligned;
                            return;
                        }

                        //special case for drawables which take up the full screen width.
                        FieldScale = new Vector2(Scale.X * GameBase.BaseToNativeRatio, Scale.Y * GameBase.BaseToNativeRatioAligned);
                        return;
                    }

                    FieldScale = Scale * GameBase.BaseToNativeRatio;
                    break;
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

        internal void ResetInitialTransformationRead()
        {
            readInitialTransformationsOnce = false;
        }

        private bool readInitialTransformationsOnce;

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
                                Colour = ((TransformationC)t).StartColour;
                                break;

                            case TransformationType.Fade:
                                Alpha = ((TransformationF)t).StartFloat;
                                break;

                            case TransformationType.Movement:
                                Position = ((TransformationV)t).StartVector;
                                break;

                            case TransformationType.MovementX:
                                Position.X = ((TransformationF)t).StartFloat;
                                break;

                            case TransformationType.MovementY:
                                Position.Y = ((TransformationF)t).StartFloat;
                                break;

                            case TransformationType.Rotation:
                                Rotation = ((TransformationF)t).StartFloat;
                                break;

                            case TransformationType.Scale:
                                ScaleScalar = ((TransformationF)t).StartFloat;
                                break;

                            case TransformationType.VectorScale:
                                Scale = ((TransformationV)t).StartVector;
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
                            Colour = ((TransformationC)t).EndColour;
                            break;

                        case TransformationType.Fade:
                            Alpha = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.Movement:
                            Position = ((TransformationV)t).EndVector;
                            break;

                        case TransformationType.MovementX:
                            Position.X = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.OffsetX:
                            Offset.X = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.Rotation:
                            Rotation = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.Scale:
                            ScaleScalar = ((TransformationF)t).EndFloat;
                            break;

                        case TransformationType.VectorScale:
                            Scale = ((TransformationV)t).EndVector;
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
                            Colour = ((TransformationC)t).CurrentColour;
                            break;

                        case TransformationType.Fade:
                            Alpha = ((TransformationF)t).CurrentFloat;
                            break;

                        case TransformationType.Movement:
                            Position = ((TransformationV)t).CurrentVector;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementX:
                            Position.X = ((TransformationF)t).CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.MovementY:
                            Position.Y = ((TransformationF)t).CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.OffsetX:
                            Offset.X = ((TransformationF)t).CurrentFloat;
                            hasMovement = true;
                            break;

                        case TransformationType.Rotation:
                            Rotation = ((TransformationF)t).CurrentFloat;
                            break;

                        case TransformationType.Scale:
                            ScaleScalar = ((TransformationF)t).CurrentFloat;
                            break;

                        case TransformationType.VectorScale:
                            Scale = ((TransformationV)t).CurrentVector;
                            break;
                    }
                }
            }
        }

        internal int ClockingNow => Clock.GetTime(Clocking);

        internal void FadeIn(int duration, float finalAlpha = 1)
        {
            int count = Transformations.Count;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && ((TransformationF)t).EndFloat == finalAlpha && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (finalAlpha - Alpha < float.Epsilon)
                return;

            if (duration == 0)
            {
                Alpha = finalAlpha;
                return;
            }

            int now = ClockingNow;
            Transform(new TransformationF(TransformationType.Fade, Alpha, finalAlpha, now, now + duration));
        }

        internal void FadeInFromZero(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = ClockingNow;
            Transform(new TransformationF(TransformationType.Fade,
                0, (Colour.A != 0 ? Colour.A : 1),
                now, now + duration));
        }

        internal void FadeOut(int duration, float finalAlpha = 0)
        {
            int count = Transformations.Count;

            if (count == 1)
            {
                Transformation t = Transformations[0];
                if (t.Type == TransformationType.Fade && ((TransformationF)t).EndFloat == finalAlpha && t.Duration == duration)
                    return;
            }

            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            if (Alpha - finalAlpha < float.Epsilon)
                return;

            if (duration == 0)
            {
                Alpha = finalAlpha;
                return;
            }

            int now = ClockingNow;
            Transform(new TransformationF(TransformationType.Fade, Alpha, finalAlpha, now, now + duration));
        }

        internal void FadeOutFromOne(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = ClockingNow;
            Transform(new TransformationF(TransformationType.Fade, 1, 0, now, now + duration));
        }

        internal SpriteManager ContainingSpriteManager;
        public bool AlphaBlend = true;
        protected bool noTransformationsLeft;

        internal pDrawable AdditiveFlash(int duration, float brightness, bool keepTransformations = false)
        {
            pDrawable clone = Clone();

            clone.UnbindAllEvents();

            if (keepTransformations)
                clone.Transformations.AddRange(Transformations);

            clone.Alpha *= brightness;
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
                new TransformationC(Colour, colour,
                    ClockingNow - (int)Clock.ElapsedMilliseconds,
                    ClockingNow + duration));
        }

        internal Transformation FlashColour(Color4 colour, int duration)
        {
            Color4 end = Colour;

            if (Transformations.FindLast(t => t is TransformationC) is TransformationC last)
            {
                end = last.EndColour;
                Transformations.RemoveAll(t => t.Type == TransformationType.Colour);
            }

            Transformation flash = new TransformationC(colour, end,
                ClockingNow,
                ClockingNow + duration);
            Transform(flash);

            return flash;
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

            Transform(new TransformationV(Position, destination, now, now + duration, easing));
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

            Transform(new TransformationF(TransformationType.Scale, ScaleScalar, target, now, now + duration, easing));

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

            Transform(new TransformationF(TransformationType.Rotation, Rotation, target, now, now + duration, easing));

            return this;
        }

        #region IDrawable Members

        public virtual bool Draw()
        {
            if (Bypass) return false;

            if (Alpha != 0 && (AlwaysDraw || !noTransformationsLeft) && ((ContainingSpriteManager == null || !ContainingSpriteManager.CheckSpritesAreOnScreenBeforeRendering) || IsOnScreen))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IUpdateable Members

        protected bool drawThisFrame;
        public bool Premultiplied;

        public virtual void Update()
        {
            if (Bypass)
            {
                drawThisFrame = false;
                return;
            }

            bool onScreenFailed = !(ContainingSpriteManager == null || !ContainingSpriteManager.CheckSpritesAreOnScreenBeforeRendering || IsOnScreen);

            UpdateTransformations();

            drawThisFrame = (Alpha != 0 && (AlwaysDraw || !noTransformationsLeft) && !onScreenFailed);

            if (drawThisFrame || onScreenFailed)
            {
                UpdateFieldPosition();
                UpdateFieldScale();
                UpdateOriginVector();
            }
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