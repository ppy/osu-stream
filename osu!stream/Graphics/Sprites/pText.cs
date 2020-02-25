using System;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Renderers;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    internal class pText : pSprite
    {
        public int BorderWidth = 1;
        internal bool TextAntialiasing = true;
        internal TextAlignment TextAlignment;

        internal Vector2 TextBounds;
        internal Color4 TextColour;
        internal bool TextShadow;
        internal float TextSize;
        internal bool Bold;
        internal string FontFace = "Tahoma";

        private string text;

        internal string Text
        {
            get => text;
            set
            {
                if (text == value) return;

                text = value;
                textChanged = true;
            }
        }

        private bool textChanged = true;

#if iOS
        private static NativeTextRenderer TextRenderer = new NativeTextRendererIphone();
#else
        private static readonly NativeTextRenderer TextRenderer = new NativeTextRendererDesktop();
#endif

        internal pText(string text, float textSize, Vector2 startPosition, Vector2 bounds, float drawDepth,
            bool alwaysDraw, Color4 colour, bool shadow)
            : base(
                null, FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, startPosition, drawDepth, alwaysDraw,
                colour)
        {
            Text = text;
            Disposable = true;
            TextColour = Color4.White;
            TextShadow = shadow;
            TextSize = textSize;
            TextAlignment = TextAlignment.Left;
            TextBounds = bounds;
        }

        public override bool Draw()
        {
            if (TextShadow)
            {
                if (Bypass) return false;

                if (drawThisFrame)
                {
                    pTexture texture = Texture;
                    if (texture == null || texture.TextureGl == null)
                        return false;

                    texture.TextureGl.Draw(FieldPosition + Vector2.One, OriginVector, new Color4(0, 0, 0, AlphaAppliedColour.A), FieldScale, Rotation, TextureRectangle);
                    texture.TextureGl.Draw(FieldPosition, OriginVector, AlphaAppliedColour, FieldScale, Rotation, TextureRectangle);
                    return true;
                }

                return false;
            }

            return base.Draw();
        }

        public pText(string text, float textSize, Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour) :
            this(text, textSize, startPosition, Vector2.Zero, drawDepth, alwaysDraw, colour, false)
        {
        }

        internal override pTexture Texture
        {
            get
            {
                if (texture != null && !texture.IsDisposed && !textChanged)
                    return texture;

                textChanged = true;

                MeasureText();

                return texture;
            }
            set => base.Texture = value;
        }

        private Vector2 lastMeasure;

        internal Vector2 MeasureText()
        {
            if (textChanged)
                refreshTexture();

            return lastMeasure;
        }

        internal override Box2 DisplayRectangle
        {
            get
            {
                Vector2 pos = FieldPosition / GameBase.BaseToNativeRatio - OriginVector * GameBase.SpriteToBaseRatio;

                return new Box2(pos.X, pos.Y,
                    pos.X + DrawWidth / GameBase.BaseToNativeRatio * Scale.X,
                    pos.Y + DrawHeight / GameBase.BaseToNativeRatio * Scale.Y);
            }
        }

        internal override void UpdateFieldScale()
        {
            FieldScale = Scale;
        }

        internal override void UpdateTextureSize()
        {
            DrawWidth = (int)Math.Round(lastMeasure.X);
            DrawHeight = (int)Math.Round(lastMeasure.Y);
            DrawTop = TextureY;
            DrawLeft = TextureX;
        }

        /// <summary>
        /// don't call this directly for the moment; we need MeasureText to be called to set DrawWidth/DrawHeight
        /// (this could do with some tidying)
        /// </summary>
        /// <returns></returns>
        private void refreshTexture()
        {
            if (texture != null && !texture.IsDisposed)
            {
                TextureManager.DisposableTextures.Remove(texture);

                texture.Dispose();
                texture = null;
            }

            textChanged = false;

            if (string.IsNullOrEmpty(Text) && TextBounds.X == 0)
            {
                lastMeasure = TextBounds;
                return;
            }

            float size = GameBase.BaseToNativeRatio * TextSize * 960f / GameBase.SpriteResolution;

            Vector2 bounds = TextBounds * GameBase.BaseToNativeRatio;

            Texture = TextRenderer.CreateText(Text, size, bounds, TextColour, TextShadow, Bold, false, TextAlignment,
                TextAntialiasing, out lastMeasure, Color4.Transparent, Color4.Transparent, BorderWidth, false, FontFace);

            if (texture != null)
                TextureManager.RegisterDisposable(texture);

            Update();
        }
    }
}