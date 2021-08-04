using System;
using System.IO;
using Android.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osum.AssetManager;
using osum.Helpers;

namespace osum.Graphics.Renderers
{
    internal class NativeTextRendererAndroid : NativeTextRenderer
    {
        internal override pTexture CreateText(string text, float size, Vector2 restrictBounds, Color4 Color4, bool shadow,
            bool bold, bool underline, TextAlignment alignment, bool forceAa,
            out Vector2 measured,
            Color4 background, Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {
            try
            {
                Canvas canvas = new Canvas();
                Paint  paint  = new Paint();
                
                paint.SetARGB(0, 0, 0, 0);
                paint.SetStyle(Paint.Style.Fill);
                
                paint.SetTypeface(Typeface.CreateFromAsset(NativeAssetManagerAndroid.manager, bold ? @"Skins/Default/Futura-CondensedExtraBold.ttf" : @"Skins/Default/Futura-Medium.ttf"));
                paint.SetARGB(255, 255, 255, 255);
                paint.TextSize = size;
                
                canvas.DrawPaint(paint);

                Rect textBounds = new Rect();
                paint.GetTextBounds(text, 0, text.Length, textBounds);

                if (restrictBounds != Vector2.Zero)
                    measured = new Vector2(Math.Min(textBounds.Width(), restrictBounds.X), Math.Min(textBounds.Height(), restrictBounds.Y));
                else
                    measured = new Vector2(textBounds.Width(), textBounds.Height());
                
                using (Bitmap bitmap = Bitmap.CreateBitmap((int) measured.X, (int) measured.Y, Bitmap.Config.Argb8888))
                {
                    canvas.SetBitmap(bitmap);
                    canvas.DrawText(text, 0, measured.Y, paint);
                    
                    return BitmapToTex(bitmap);
                }
            }
            catch (Exception)
            {
                measured = Vector2.Zero;
                return null;
            }
        }

        private static pTexture BitmapToTex(Bitmap bitmap) 
        {
            pTexture tex = pTexture.FromRawBytes(bitmap.LockPixels(), bitmap.Width, bitmap.Height);
            bitmap.UnlockPixels();
            return tex;
        }

        private static float dpiRatio;
    }
}