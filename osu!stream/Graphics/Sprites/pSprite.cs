using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;

#if iOS
using OpenTK.Graphics.ES11;
using Foundation;
using ObjCRuntime;
using OpenGLES;

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
using UIKit;
using CoreGraphics;
#else

#endif


namespace osum.Graphics.Sprites
{
    internal class pSprite : pDrawable
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
            UsesTextures = true;
        }

        public pSprite(pTexture tex, Vector2 pos) :
            this(tex, FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, pos, 1, true, Color4.White)
        {
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
            get {
                return texture;
            }
            set
            {
                if (value == texture)
                    return;

                texture = value;

                if (texture != null)
                {
#if iOS
                    Premultiplied |= texture.OsuTextureInfo != OsuTexture.None;
#endif
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
                Vector2 pos = FieldPosition / GameBase.BaseToNativeRatio - new Vector2(OriginVector.X * Scale.X * GameBase.SpriteToBaseRatioAligned, OriginVector.Y * Scale.Y * GameBase.SpriteToBaseRatioAligned);

                return new Box2(pos.X, pos.Y,
                    pos.X + DrawWidth * GameBase.SpriteToBaseRatioAligned * Scale.X,
                    pos.Y + DrawHeight * GameBase.SpriteToBaseRatioAligned * Scale.Y);
            }
        }

        internal static pDrawable FullscreenWhitePixel
        {
            get
            {
                pDrawable whiteLayer =
                    new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.X + 1, GameBase.BaseSizeFixedWidth.Y + 1), false, 1, Color4.White);
                whiteLayer.AlignToSprites = false;
                whiteLayer.AlphaBlend = false;
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

        public override bool Draw()
        {
            if (!base.Draw()) return false;

            pTexture texture = Texture;
            if (texture == null || texture.TextureGl == null)
                return false;

            if (texture.TextureGl.Id == -1) texture.ReloadIfPossible();

            texture.TextureGl.Draw(FieldPosition, OriginVector, AlphaAppliedColour, FieldScale, Rotation, TextureRectangle);
            return true;
        }

        #endregion

        internal virtual void UpdateTextureSize()
        {
            //if (texture != null && SpriteInfo != null)
            //{
            //    DrawWidth = SpriteInfo.Width;
            //    DrawHeight = SpriteInfo.Height;
            //    DrawTop = SpriteInfo.Y;
            //    DrawLeft = SpriteInfo.X;
            //}
            {
                DrawWidth = TextureWidth;
                DrawHeight = TextureHeight;
                DrawTop = TextureY;
                DrawLeft = TextureX;
            }
        }

        internal override void UpdateOriginVector()
        {
            Vector2 origin = Vector2.Zero;

            if (texture == null || Origin == OriginTypes.TopLeft)
            {
                OriginVector = origin;
                return;
            }

            switch (Origin)
            {
                case OriginTypes.TopCentre:
                    origin.X = DrawWidth / 2;
                    break;
                case OriginTypes.TopRight:
                    origin.X = DrawWidth;
                    break;
                case OriginTypes.CentreLeft:
                    origin.Y = DrawHeight / 2;
                    break;
                case OriginTypes.Centre:
                    origin = new Vector2(DrawWidth / 2, DrawHeight / 2);
                    break;
                case OriginTypes.CentreRight:
                    origin = new Vector2(DrawWidth, DrawHeight / 2);
                    break;
                case OriginTypes.BottomLeft:
                    origin.Y = DrawHeight;
                    break;
                case OriginTypes.BottomCentre:
                    origin = new Vector2(DrawWidth / 2, DrawHeight);
                    break;
                case OriginTypes.BottomRight:
                    origin = new Vector2(DrawWidth, DrawHeight);
                    break;
                case OriginTypes.Custom:
                    origin = Offset;
                    break;
            }

            if (!exactCoordinatesOverride)
            {
                if (origin.X % 2 != 0) origin.X--;
                if (origin.Y % 2 != 0) origin.Y--;
            }

            OriginVector = origin;
        }

        public override string ToString()
        {
            if (texture != null)
                return texture.assetName + "(" + texture.OsuTextureInfo + ")";
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
        StandardSnapCentreLeft
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