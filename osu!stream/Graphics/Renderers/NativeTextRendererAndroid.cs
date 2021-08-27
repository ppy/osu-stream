using System;
using System.IO;
using Android.Graphics;
using Android.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.AssetManager;
using osum.Helpers;

namespace osum.Graphics.Renderers
{
    internal class NativeTextRendererAndroid : NativeTextRenderer
    {
        /// <summary>
        /// Creates a pTexture containing text with the specified parameters,
        /// all variables marked with (leftover) are not used and are from when this function was taken from a very old version of osu!stable
        /// </summary>
        /// <param name="text">The text to render</param>
        /// <param name="size">The text size</param>
        /// <param name="restrictBounds">The bounds of the text</param>
        /// <param name="Color4">The colour of the text (leftover as we set the colour on render of pText)</param>
        /// <param name="shadow">Whether to render it with a shadow (leftover as we draw the shadow in pText)</param>
        /// <param name="bold">Whether to render with bold</param>
        /// <param name="underline">Whether to render the text with underline (leftover but implemented just in case)</param>
        /// <param name="alignment">The alignment to render with</param>
        /// <param name="forceAa">Whether to force anti aliasing (leftover as Aa is always on, likely mattered with the old version of System.Drawing in use at the time)</param>
        /// <param name="measured">The size of the text</param>
        /// <param name="background">The colour to render as the background of the text (leftover)</param>
        /// <param name="border">The colour of the text border (leftover)</param>
        /// <param name="borderWidth">The width of the border (leftover)</param>
        /// <param name="measureOnly">Whether to only measure the text and not render it (leftover but if implemented would return a null pTexture)</param>
        /// <param name="fontFace">The font face to render with (leftover as it is forced to Futura in rendering, this will always be set to Tahoma)</param>
        /// <returns>The pTexture that contains the rendered text</returns>
        internal override pTexture CreateText(string text, float size, Vector2 restrictBounds, Color4 Color4, bool shadow,
            bool bold, bool underline, TextAlignment alignment, bool forceAa,
            out Vector2 measured,
            Color4 background, Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {
            try {
                //Create a bitmap with a bogus width and height equal to the size of the display
                Bitmap bitmap = Bitmap.CreateBitmap(GameBase.NativeSize.Width, GameBase.NativeSize.Height, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(bitmap);
                //Create a new TextPaint with the right size (note the colour is always set to white as we change the colour when rendering)
                TextPaint paint = new TextPaint {
                    Color    = Color.White,
                    TextSize = size,
                    AntiAlias = true,
                    Dither = true
                };

                // Sets some parameters of the TextPaint
                paint.SetTypeface(Typeface.CreateFromAsset(NativeAssetManagerAndroid.manager, bold ? @"Skins/Default/Futura-CondensedExtraBold.ttf" : @"Skins/Default/Futura-Medium.ttf"));
                paint.UnderlineText = underline;
                // Gets the first iteration of the text width
                // If restrictBounds 
                int textWidth  = restrictBounds != Vector2.Zero ? (int) Math.Ceiling(restrictBounds.X) : canvas.Width;
                // Create the first iteration of the StaticLayout (with a bunked width)
                // only used for measuring
                StaticLayout textLayout = new StaticLayout(text, paint, textWidth, ConvertAlignment(alignment), 1.0f, 0.0f, false);

                // Get the real width and height of the text
                float widestLine = 0;
                for (int i = 0; i < textLayout.LineCount; i++) {
                    float temp = textLayout.GetLineWidth(i);
                    
                    if (temp > widestLine) widestLine = temp;
                } 
                int textHeight = textLayout.Height;
                textWidth      = (int) Math.Ceiling(widestLine);
                
                // Create the second iteration of the StaticLayout with a proper width
                // Actually used for rendering
                textLayout = new StaticLayout(text, paint, textWidth, ConvertAlignment(alignment), 1.0f, 0.0f, false);

                canvas.Save();
                textLayout.Draw(canvas);
                canvas.Restore();

                // Crop the bitmap to the proper size
                bitmap = Bitmap.CreateBitmap(bitmap, 0, 0, textWidth, textHeight);

                measured = new Vector2(textWidth, textHeight);
                
                return BitmapToTex(bitmap);
            }
            catch (Exception)
            {
                measured = Vector2.Zero;
                return null;
            }
        }

        /// <summary>
        /// Converts a TextAlignment to a Layout.Alignment
        /// </summary>
        /// <param name="oldAlignment">The TextAlignment</param>
        /// <returns>The converted Layout.Alignment</returns>
        private static Layout.Alignment ConvertAlignment(TextAlignment oldAlignment) {
            switch (oldAlignment) {
                case TextAlignment.Centre: {
                    return Layout.Alignment.AlignCenter;
                }
                case TextAlignment.Left: {
                    return Layout.Alignment.AlignNormal;
                }
                default: return Layout.Alignment.AlignNormal;
            }
        }
        
        /// <summary>
        /// Converts a Bitmap to a pTexture
        /// </summary>
        /// <param name="bitmap">The Bitmap</param>
        /// <returns>The converted pTexture</returns>
        private static pTexture BitmapToTex(Bitmap bitmap) 
        {
            pTexture tex = pTexture.FromRawBytes(bitmap.LockPixels(), bitmap.Width, bitmap.Height);
            bitmap.UnlockPixels();
            return tex;
        }
    }
}