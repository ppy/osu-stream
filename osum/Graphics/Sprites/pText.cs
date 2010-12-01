using System;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;
using osum.Helpers;
using osum.Graphics.Renderers;

namespace osum.Graphics.Sprites
{
    internal class pText : pSprite
    {
        internal Color4 BackgroundColour;
        internal Color4 BorderColour;

        public int BorderWidth = 1;
        internal bool TextAntialiasing = true;
        internal TextAlignment TextAlignment;

        public bool TextBold;
        internal Vector2 TextBounds;
        internal Color4 TextColour;
        internal bool TextShadow;
        internal float TextSize;
        public bool TextUnderline;
        private bool aggressiveCleanup;
        internal string FontFace = "Tahoma";

        internal string Text;

        private bool textChanged = true;
        private bool exactCoordinates = true;

#if IPHONE
        private static NativeTextRenderer TextRenderer = new NativeTextRendererIphone();
#else
        private static NativeTextRenderer TextRenderer = new NativeTextRendererDesktop();
#endif

        private pTexture internalTexture;

        public override pSprite Clone()
        {
            throw new NotImplementedException();
        }

        internal pText(string text, float textSize, Vector2 startPosition, Vector2 bounds, float drawDepth,
                       bool alwaysDraw, Color4 colour, bool shadow)
            : base(
                null, FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Game, startPosition, drawDepth, alwaysDraw,
                colour)
        {
            Text = text;
            Disposable = true;
            TextColour = Color4.White;
            TextShadow = shadow;
            TextSize = textSize;
            TextAlignment = TextAlignment.Left;
            TextBounds = bounds;

            Field = FieldTypes.Native;

            GameBase.OnScreenLayoutChanged += new VoidDelegate(GameBase_OnScreenLayoutChanged);
        }

        public override void Dispose()
        {
            GameBase.OnScreenLayoutChanged -= new VoidDelegate(GameBase_OnScreenLayoutChanged);

            base.Dispose();
        }

        void GameBase_OnScreenLayoutChanged()
        {
            textChanged = true;
        }

        public pText(string text, float textSize, Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour) :
            this(text, textSize, startPosition, Vector2.Zero, drawDepth, alwaysDraw, colour, false)
        {
        }

        internal override pTexture Texture
        {
            get
            {
                if (internalTexture != null && !internalTexture.IsDisposed && !textChanged)
                    return internalTexture;

                textChanged = true;

                MeasureText();

                return internalTexture;
            }
            set { }
        }

        Vector2 lastMeasure;

        internal Vector2 MeasureText()
        {
            if (textChanged)
            {
                refreshTexture();
                
                DrawWidth = (int)Math.Round(lastMeasure.X);
                DrawHeight = (int)Math.Round(lastMeasure.Y);
            }

            return lastMeasure;
        }

        /// <summary>
        /// don't call this directly for the moment; we need MeasureText to be called to set DrawWidth/DrawHeight
        /// (this could do with some tidying)
        /// </summary>
        /// <returns></returns>
        private pTexture refreshTexture()
        {
            if (internalTexture != null && !internalTexture.IsDisposed)
            {
                internalTexture.Dispose();
                internalTexture = null;
            }

            textChanged = false;

            if (string.IsNullOrEmpty(Text) && TextBounds.X == 0)
            {
                lastMeasure = TextBounds;
                return null;
            }

            float size = GameBase.WindowRatio * TextSize;

            internalTexture = TextRenderer.CreateText(Text, size, TextBounds, TextColour, TextShadow, TextBold, TextUnderline, TextAlignment,
                                      TextAntialiasing, out lastMeasure, BackgroundColour, BorderColour, BorderWidth, false, FontFace);


            UpdateTextureSize();
            UpdateTextureAlignment();

            return internalTexture;
        }
    }
}