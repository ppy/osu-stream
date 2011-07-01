using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu_common.Helpers;
using osum.GameplayElements;
using osum.Helpers;
using osum.Graphics.Skins;
using osum.Graphics.Drawables;

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
    internal partial class pSprite : pDrawable
    {
        internal int DrawHeight;
        internal int DrawLeft;
        internal int DrawTop;
        internal int DrawWidth;

        protected pTexture texture;

        internal pSprite(pTexture texture, FieldTypes field, OriginTypes origin, ClockTypes clocking, Vector2 position,
                         float depth, bool alwaysDraw, Color4 colour)
        {
            Field = field;
            Origin = origin;
            Clocking = clocking;

            Position = position;
            StartPosition = position;
            Colour = colour;

            Scale = Vector2.One;
            Rotation = 0;
            DrawDepth = depth;
            AlwaysDraw = alwaysDraw;

            if (!alwaysDraw)
                Alpha = 0;
            else
                Alpha = 1;

            Texture = texture;
        }

        public pSprite(pTexture tex, Vector2 pos) :
            this(tex, FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, pos, 1, true, Color4.White)
        {
        }

        internal override bool UsesTextures
        {
            get
            {
                return true;
            }
        }

        internal int TextureWidth
        {
            get { return texture != null ? texture.Width : 0; }
        }

        internal int TextureHeight
        {
            get { return texture != null ? texture.Height : 0; }
        }

        internal int TextureX
        {
            get { return texture != null ? texture.X : 0; }
        }

        internal int TextureY
        {
            get { return texture != null ? texture.Y : 0; }
        }

        internal virtual pTexture Texture
        {
            get { return texture; }
            set
            {
                if (value == texture)
                    return;

                texture = value;

                if (texture != null)
                {
                    Premultiplied = texture.Premultiplied;
                    UpdateTextureSize();
                }
            }
        }

        internal Box2 TextureRectangle
        {
            get { return new Box2(DrawLeft, DrawTop, DrawWidth + DrawLeft, DrawHeight + DrawTop); }
        }

        internal override Box2 DisplayRectangle
        {
            get
            {
                Vector2 pos = FieldPosition / GameBase.BaseToNativeRatio - Vector2.Multiply(OriginVector,Scale) * GameBase.SpriteToBaseRatio;
                //Vector2 scale = FieldScale / GameBase.BaseToNativeRatio;

                return new Box2(pos.X, pos.Y,
                    pos.X + (float)DrawWidth * GameBase.SpriteToBaseRatio * Scale.X,
                    pos.Y + (float)DrawHeight * GameBase.SpriteToBaseRatio * Scale.Y);
            }
        }

        internal static pDrawable FullscreenWhitePixel
        {
            get
            {
                pDrawable whiteLayer =
                    new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, GameBase.BaseSizeFixedWidth.Height), false, 1, Color4.White);
                whiteLayer.AlignToSprites = false;
                return whiteLayer;
            }
        }

        #region IDisposable Members

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion

        #region IDrawable Members

        public override void Update()
        {
            base.Update();
        }

        public override bool Draw()
        {
            if (!base.Draw()) return false;

            pTexture texture = Texture;
            if (texture == null || texture.TextureGl == null)
                return false;

            texture.TextureGl.Draw(FieldPosition, OriginVector, AlphaAppliedColour, FieldScale, Rotation, TextureRectangle);
            return true;
        }

        #endregion

        internal virtual void UpdateTextureSize()
        {
            DrawWidth = TextureWidth;
            DrawHeight = TextureHeight;
            DrawTop = TextureY;
            DrawLeft = TextureX;
        }

        internal override Vector2 OriginVector
        {
            get
            {
                if (texture == null)
                    return Vector2.Zero;

                switch (Origin)
                {
                    default:
                    case OriginTypes.TopLeft:
                        return Vector2.Zero;
                    case OriginTypes.TopCentre:
                        return new Vector2(DrawWidth / 2, 0);
                    case OriginTypes.TopRight:
                        return new Vector2(DrawWidth, 0);
                    case OriginTypes.CentreLeft:
                        return new Vector2(0, DrawHeight / 2);
                    case OriginTypes.Centre:
                        return new Vector2(DrawWidth / 2, DrawHeight / 2);
                    case OriginTypes.CentreRight:
                        return new Vector2(DrawWidth, DrawHeight / 2);
                    case OriginTypes.BottomLeft:
                        return new Vector2(0, DrawHeight);
                    case OriginTypes.BottomCentre:
                        return new Vector2(DrawWidth / 2, DrawHeight);
                    case OriginTypes.BottomRight:
                        return new Vector2(DrawWidth, DrawHeight);
                    case OriginTypes.Custom:
                        return Offset;
                }
            }
        }

        public override string ToString()
        {
            if (texture != null)
                return texture.assetName + "(" + texture.OsuTextureInfo + ")";
            else
                return "unknown";
        }
    }

    internal enum FieldTypes
    {
        /// <summary>
        ///   The gamefield resolution.  Used for hitobjects and anything which needs to align with gameplay elements.
        ///   This is scaled in proportion to the native resolution and aligned accordingly.
        /// </summary>
        GamefieldSprites,

        /// <summary>
        /// A field where 1 pixel at 1.0f scale becomes the width of a hitObject. Used for primitives that are based around
        /// gamefield objects.
        /// </summary>
        GamefieldExact,

        GamefieldStandardScale,
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

        NativeScaled,

        /// <summary>
        ///   Native screen resolution with 1024x768-native sprite scaling.
        /// </summary>
        NativeStandardScale,

        /// <summary>
        ///   Native screen resolution aligned from the right-hand side of the screen, where an X position of 0 is translated to Standard(WindowWidth).
        /// </summary>
        NativeSnapRight,

        StandardSnapTopCentre,
        StandardSnapCentreLeft,
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