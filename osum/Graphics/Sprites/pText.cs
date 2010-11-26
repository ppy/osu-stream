using System;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using OpenTK;
using osum.Helpers;
using osu.Graphics.Renderers;

namespace osum.Graphics.Sprites
{
    internal class pText : pSprite
    {
        internal Color4 BackgroundColour;
        internal Color4 BorderColour;

        public int BorderWidth = 1;
        private int renderingResolution = GameBase.WindowSize.Width;
        internal bool TextAntialiasing = true;
        internal TextAlignment TextAlignment;

        public bool TextBold;
        internal Vector2 TextBounds;
        internal Color4 TextColour;
        internal bool TextRenderSpecific;
        internal bool TextShadow;
        internal float TextSize;
        public bool TextUnderline;
        private bool aggressiveCleanup;
        internal string FontFace = "Tahoma";

        internal string Text;

        private bool textChanged = true;
        private bool exactCoordinates = true;

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
            TextAntialiasing = true;

            Field = FieldTypes.StandardNoScale;

            TextRenderSpecific = true;
        }

        public pText(string text, float textSize, Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour) :
            this(text, textSize, startPosition, Vector2.Zero, drawDepth, alwaysDraw, colour, false)
        {
        }

        internal override pTexture Texture
        {
            get
            {
                if (internalTexture != null && !internalTexture.IsDisposed && !textChanged && GameBase.WindowSize.Width == renderingResolution)
                    return internalTexture;

                MeasureText();

                return internalTexture;
            }
            set { }
        }

        internal Vector2 MeasureText(int startIndex, int length)
        {
            float size = (TextRenderSpecific ? (float) GameBase.WindowRatio : 1)*TextSize;

            if (Text.Length == 0)
                return Vector2.Zero;

            Vector2 measure;
            NativeText.CreateText(Text.Substring(startIndex, length), size, Vector2.Zero, TextColour, TextShadow, TextBold,
                                  TextUnderline, TextAlignment,
                                  TextAntialiasing, out measure, BackgroundColour, BorderColour, BorderWidth, true, FontFace);
            if (TextRenderSpecific)
                return measure/GameBase.WindowRatio;
            return measure*0.625f;
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

            if (TextRenderSpecific)
                return lastMeasure/GameBase.WindowRatio;
            return lastMeasure*0.625f; // *GameBase.WindowRatio;
        }

        /// <summary>
        /// don't call this directly for the moment; we need MeasureText to be called to set DrawWidth/DrawHeight
        /// (this could do with some tidying)
        /// </summary>
        /// <returns></returns>
        private pTexture refreshTexture()
        {
            bool existed = false;

            if (internalTexture != null && !internalTexture.IsDisposed)
            {
                internalTexture.Dispose();
                internalTexture = null;
                existed = true;
            }
            
            textChanged = false;

            if (string.IsNullOrEmpty(Text) && TextBounds.X == 0)
            {
                lastMeasure = TextBounds;
                return null;
            }

            renderingResolution = GameBase.WindowSize.Height;

            float size = (TextRenderSpecific ? (float)GameBase.WindowRatio : 1) * TextSize;
            Vector2 bounds = (TextRenderSpecific ? GameBase.WindowRatio : 1)*TextBounds;
            internalTexture =
                NativeText.CreateText(Text, size, bounds, TextColour, TextShadow, TextBold, TextUnderline, TextAlignment,
                                      TextAntialiasing, out lastMeasure, BackgroundColour, BorderColour, BorderWidth, false, FontFace);
            
            if (aggressiveCleanup)
            {
                internalTexture.TrackAccessTime = true;
                //if (!existed) DynamicSpriteCache.Load(this,true);
            }

            UpdateTextureSize();
            UpdateTextureAlignment();

            return internalTexture;
        }
    }
}