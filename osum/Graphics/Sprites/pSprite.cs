using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    internal enum FieldType
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

    internal enum OriginType
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

    internal class pSprite : ISpriteable
    {
        protected List<Transform> transformations;

        internal Color4 OriginalColour;
        internal Vector2 OriginalPosition;

        protected pTexture texture;
        protected Vector2 originVector;
        protected Color4 colour;
        protected float rotation;
        protected SpriteEffect effect;
        protected BlendingFactorDest blending;
        protected float depth;

        internal Vector2 Position, Scale;
        internal FieldType Field;
        internal OriginType Origin;
        internal ClockType Clocking;
        internal bool AlwaysDraw;
        internal bool Reverse;

        internal float ScaleScalar
        {
            set { Scale = new Vector2(value, value); }
        }

        internal virtual pTexture Texture
        {
            get { return texture; }
            set
            {
                if (value == texture)
                    return;

                if (texture != null) // && Disposable)
                    texture.Dispose();

                texture = value;
                //UpdateTextureSize();
                //UpdateTextureAlignment();
            }
        }

        internal pSprite(pTexture texture, OriginType origin, Vector2 position, Color4 colour)
            : this(texture, FieldType.Standard, origin, ClockType.Game, position, 1, false, Color4.White)
        {
        }

        /// <summary>
        /// Important: don't use this to add new transformations, use pSprite.Transform() for that.
        /// </summary>
        public List<Transform> Transformations
        {
            get { return transformations; }
        }

        internal pSprite(pTexture texture, FieldType field, OriginType origin, ClockType clocking, Vector2 position, float depth, bool alwaysDraw, Color4 colour)
        {
            this.transformations = new List<Transform>();

            this.texture = texture;
            this.Field = field;
            this.Origin = origin;
            this.Clocking = clocking;
            this.Position = position;
            this.colour = colour;
            this.Scale = Vector2.One;
            this.rotation = 0;
            this.effect = SpriteEffect.None;
            this.blending = BlendingFactorDest.OneMinusSrcAlpha;
            this.Clocking = ClockType.Game;
            this.depth = depth;

            if (texture != null)
                UpdateTextureAlignment();
        }

        internal void UpdateTextureAlignment()
        {
            //if (Type == SpriteTypes.NativeText || Type == SpriteTypes.SpriteText)
            //    return;

            switch (Origin)
            {
                case OriginType.TopLeft:
                    originVector = Vector2.Zero;
                    break;
                case OriginType.TopCentre:
                    originVector = new Vector2(texture.Width / 2, 0);
                    break;
                case OriginType.TopRight:
                    originVector = new Vector2(texture.Width, 0);
                    break;
                case OriginType.CentreLeft:
                    originVector = new Vector2(0, texture.Height / 2);
                    break;
                case OriginType.Centre:
                    originVector = new Vector2(texture.Width / 2, texture.Height / 2);
                    break;
                case OriginType.CentreRight:
                    originVector = new Vector2(texture.Width, texture.Height / 2);
                    break;
                case OriginType.BottomLeft:
                    originVector = new Vector2(0, texture.Height);
                    break;
                case OriginType.BottomCentre:
                    originVector = new Vector2(texture.Width / 2, texture.Height);
                    break;
                case OriginType.BottomRight:
                    originVector = new Vector2(texture.Width, texture.Height);
                    break;
            }
        }

        internal void Transform(Transform transform)
        {
            transform.Clocking = this.Clocking;
            transformations.Add(transform);
        }

        internal void Transform(IEnumerable<Transform> transforms)
        {
            foreach (Transform t in transforms)
                this.Transform(t);
        }

        public void Update()
        {
            // remove old transformations
            for (int i = 0; i < transformations.Count; i++)
            {
                if (transformations[i].Terminated)
                    transformations.RemoveAt(i);
            }

            // modify variables based on transformations
            for (int i = 0; i < transformations.Count; i++)
            {
                Transform t = transformations[i];

                // reset some values
                effect = SpriteEffect.None;
                blending = BlendingFactorDest.OneMinusSrcAlpha;

                if (t.Initiated)
                {
                    switch (t.Type)
                    {
                        case TransformType.Colour:
                            Color4 c = t.CurrentColour;
                            colour = new Color4(c.R, c.G, c.B, colour.A);
                            break;

                        case TransformType.Fade:
                            colour = new Color4(colour.R, colour.G, colour.B, t.CurrentFloat);
                            break;

                        case TransformType.Movement:
                            Position = t.CurrentVector;
                            break;

                        case TransformType.MovementX:
                            Position.X = t.CurrentFloat;
                            break;

                        case TransformType.MovementY:
                            Position.Y = t.CurrentFloat;
                            break;

                        case TransformType.ParameterAdditive:
                            blending = BlendingFactorDest.One;
                            break;

                        case TransformType.ParameterFlipHorizontal:
                            effect |= SpriteEffect.FlipHorizontally;
                            break;

                        case TransformType.ParameterFlipVertical:
                            effect |= SpriteEffect.FlipVertically;
                            break;

                        case TransformType.Rotation:
                            rotation = t.CurrentFloat;
                            break;

                        case TransformType.Scale:
                            Scale = new Vector2(t.CurrentFloat, t.CurrentFloat);
                            break;

                        case TransformType.VectorScale:
                            Scale = t.CurrentVector;
                            break;
                    }
                }
            }
        }

        public void Draw()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, blending);
            if (texture != null)
                texture.TextureGl.Draw(Position, originVector, colour, Scale, rotation, null, effect);
        }

        public virtual pSprite Clone()
        {
            pSprite clone = new pSprite(Texture, Field, Origin, Clocking, OriginalPosition, depth, AlwaysDraw, OriginalColour);
            clone.Position = Position;
            /*
            clone.DrawLeft = DrawLeft;
            clone.DrawTop = DrawTop;
            clone.DrawWidth = DrawWidth;
            clone.DrawHeight = DrawHeight;
            */
            clone.Scale = Scale;

            foreach (Transform t in Transformations)
                //if (!t.IsLoopStatic) 
                clone.Transformations.Add(t.Clone());

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
    }
}
