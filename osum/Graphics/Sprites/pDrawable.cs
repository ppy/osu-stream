using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using osum.Helpers;
using OpenTK;
using osu_common.Helpers;
using osum.GameplayElements;
#if IPHONE
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace osum.Graphics.Sprites
{
    internal class pDrawable : IDrawable, IDisposable
    {
        internal float Alpha;

        private bool alwaysDraw;
        internal bool AlwaysDraw
        {
            get { return alwaysDraw; }
            set
            {
                alwaysDraw = value;
                Alpha = alwaysDraw ? 1 : 0;
            }
        }
        internal ClockTypes Clocking;

        internal Color4 Colour;
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
        protected pList<Transformation> transformations = new pList<Transformation>();
        public object Tag;
        public int TagNumeric;
        internal FieldTypes Field;
        internal OriginTypes Origin;
        internal Vector2 OriginVector;
        internal Vector2 Position;
        protected BlendingFactorDest blending = BlendingFactorDest.OneMinusSrcAlpha;

        internal float DrawDepth;

        /// <summary>
        /// Determines whether the sprite automatically remove past transformations.
        /// </summary>
        internal bool RemoveOldTransformations = true;

        /// <summary>
        /// Important: don't use this to add new transformations, use pSprite.Transform() for that.
        /// </summary>
        public pList<Transformation> Transformations
        {
            get { return transformations; }
        }

        internal virtual bool IsRemovable
        {
            get { return !AlwaysDraw && Transformations.Count == 0; }
        }
		
		internal float ScaleScalar
        {
            get { return Scale.X; }
            set { Scale = new Vector2(value, value); }
        }

        internal bool Additive
        {
            get { return blending == BlendingFactorDest.One; }
            set { blending = value ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha; }
        }

        protected virtual Vector2 FieldPosition
        {
            get
            {
                Vector2 fieldPosition;

                Vector2 pos = Position * GameBase.WindowRatio;

                switch (Field)
                {
                    case FieldTypes.StandardSnapCentre:
                        fieldPosition = new Vector2(GameBase.WindowSize.Width / 2 + pos.X,
                                                    GameBase.WindowSize.Height / 2 + pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomCentre:
                        fieldPosition = new Vector2(GameBase.WindowSize.Width / 2 + pos.X,
                                                    GameBase.WindowSize.Height - pos.Y);
                        break;
                    case FieldTypes.StandardSnapRight:
                        fieldPosition = new Vector2(GameBase.WindowSize.Width - pos.X, pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomLeft:
                        fieldPosition = new Vector2(pos.X, GameBase.WindowSize.Height - pos.Y);
                        break;
                    case FieldTypes.StandardSnapBottomRight:
                        fieldPosition = new Vector2(GameBase.WindowSize.Width - pos.X,
                                                    GameBase.WindowSize.Height - pos.Y);
                        break;
                    case FieldTypes.Gamefield512x384:
                        fieldPosition = Position;
                        GameBase.GamefieldToStandard(ref fieldPosition);
                        Vector2.Multiply(ref fieldPosition, GameBase.WindowRatio, out fieldPosition);
                        break;
                    case FieldTypes.NativeScaled:
                        return Position;
                    case FieldTypes.Native:
                    default:
                        fieldPosition = pos;
                        break;
                }

                return fieldPosition;
            }
        }

        internal Vector2 FieldScale
        {
            get
            {
                switch (Field)
                {
                    case FieldTypes.Gamefield512x384:
                        return Scale * GameBase.SpriteRatioToWindow *
                               (DifficultyManager.HitObjectRadius / DifficultyManager.HitObjectRadiusDefault);
                    case FieldTypes.Native:
                    case FieldTypes.NativeScaled:
                        return Scale;
                    default:
                        return Scale * GameBase.SpriteRatioToWindow;
                }
            }
        }

        protected Color4 AlphaAppliedColour
        {
            get
            {
                if (SpriteManager.UniversalDim > 0)
                    return ColourHelper.Darken(new Color4(Colour.R, Colour.G, Colour.B, Alpha * Colour.A),
                                               SpriteManager.UniversalDim);

                return Alpha < 1 ? new Color4(Colour.R, Colour.G, Colour.B, Alpha * Colour.A) : Colour;
            }
        }

        internal void Transform(Transformation transform)
        {
            transform.Clocking = Clocking;
            Transformations.AddInPlace(transform);
        }

        internal void Transform(IEnumerable<Transformation> transforms)
        {
            foreach (Transformation t in transforms)
                Transform(t);
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

        internal int ClockingNow
        {
            get { return Clock.GetTime(Clocking); }
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
            Transform(new Transformation(TransformationType.Fade,
                                         Alpha, (Colour.A != 0 ? Colour.A : 1),
                                         now, now + duration));
        }

        internal void FadeInFromZero(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = Clock.GetTime(Clocking);
            Transform(new Transformation(TransformationType.Fade,
                                         0, (Colour.A != 0 ? Colour.A : 1),
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
            Transform(new Transformation(TransformationType.Fade, Alpha, 0, now, now + duration));
        }

        internal void FadeOutFromOne(int duration)
        {
            Transformations.RemoveAll(t => t.Type == TransformationType.Fade);

            int now = Clock.GetTime(Clocking);
            Transform(new Transformation(TransformationType.Fade, 1, 0, now, now + duration));
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

        internal const int TRANSFORMATION_TAG_FLASH = 51458;

        internal void FlashColour(Color4 colour, int duration)
        {
            if (Colour == colour)
                return;

            Color4 end = Colour;

            Transformation start = Transformations.Find(t => t.Tag == TRANSFORMATION_TAG_FLASH);
            if (start != null)
            {
                end = start.EndColour;
                Transformations.Remove(start);
            }

            Transformation flash = new Transformation(colour, end,
                                   ClockingNow,
                                   ClockingNow + duration);
            flash.Tag = TRANSFORMATION_TAG_FLASH;
            Transform(flash);
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

            if (duration == 0)
                Position = destination;

            int now = Clock.GetTime(Clocking);

            Transformation tr =
                new Transformation(Position, destination,
                                   now - (int)Math.Max(1, GameBase.ElapsedMilliseconds),
                                   now + duration, easing);
            Transform(tr);
        }

        #region IDrawable Members

        public virtual bool Draw()
        {
            if (transformations.Count != 0 || AlwaysDraw)
            {
                if (Alpha != 0)
                {
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, (BlendingFactorDest)blending);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IUpdateable Members

        public virtual void Update()
        {
            UpdateTransformations();
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            throw new NotImplementedException();
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
