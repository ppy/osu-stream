using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu_common.Helpers;
using osum.GameplayElements;
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
        

        internal pSprite(pTexture texture, OriginTypes origin, Vector2 position, Color4 colour)
            : this(texture, FieldTypes.Standard, origin, ClockTypes.Game, position, 1, false, Color4.White)
        {
        }

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
		
		internal override bool UsesTextures {
			get {
				return true;
			}
		}
		
		internal override bool IsOnScreen {
			get {
				
				Box2 rect = DisplayRectangle;

				if (rect.Left > GameBase.WindowSize.Width || rect.Right < 0 ||
				    rect.Top > GameBase.WindowSize.Height || rect.Bottom < 0)
					return false;
				
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

                UpdateTextureSize();
                UpdateTextureAlignment();
            }
        }
        
        protected Box2 TextureRectangle
        {
            get { return new Box2(DrawLeft, DrawTop, DrawWidth + DrawLeft, DrawHeight + DrawTop); }
        }

        protected Box2 DisplayRectangle
        {
            get
            {
                Vector2 pos = FieldPosition/GameBase.WindowRatio;
                Vector2 scale = FieldScale/GameBase.WindowRatio;

                return new Box2(pos.X - OriginVector.X, pos.Y - OriginVector.Y, pos.X + DrawWidth*scale.X,
                                pos.Y + DrawHeight*scale.Y);
            }
        }

        internal static pSprite FullscreenWhitePixel
        {
            get
            {
                pSprite whiteLayer =
                    new pSprite(pTexture.FromRawBytes(new byte[] {255, 255, 255, 255}, 1, 1), FieldTypes.Standard,
                                OriginTypes.TopLeft, ClockTypes.Mode, Vector2.Zero, 1, false, Color4.White);
                whiteLayer.Scale = new Vector2(GameBase.WindowBaseSize.Width, GameBase.WindowBaseSize.Height)/
                                   GameBase.SpriteRatioToWindowBase;
                return whiteLayer;
            }
        }

        #region IDisposable Members

        public override void Dispose()
        {
            //todo: kill texture if possible

            UnbindAllEvents();
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
                    OriginVector = new Vector2(TextureWidth/2, 0);
                    break;
                case OriginTypes.TopRight:
                    OriginVector = new Vector2(TextureWidth, 0);
                    break;
                case OriginTypes.CentreLeft:
                    OriginVector = new Vector2(0, TextureHeight/2);
                    break;
                case OriginTypes.Centre:
                    OriginVector = new Vector2(TextureWidth/2, TextureHeight/2);
                    break;
                case OriginTypes.CentreRight:
                    OriginVector = new Vector2(TextureWidth, TextureHeight/2);
                    break;
                case OriginTypes.BottomLeft:
                    OriginVector = new Vector2(0, TextureHeight);
                    break;
                case OriginTypes.BottomCentre:
                    OriginVector = new Vector2(TextureWidth/2, TextureHeight);
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