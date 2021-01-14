using System;
using System.IO;
using Android.Graphics;
using OpenTK;
using OpenTK.Graphics;
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

                using (Bitmap bitmap = Bitmap.CreateBitmap(1024, 1024, Bitmap.Config.Argb8888))
                {
                    canvas.SetBitmap(bitmap);

                    Paint paint = new Paint();

                    paint.SetARGB(0, 0, 0, 0);
                    paint.SetStyle(Paint.Style.Fill);

                    canvas.DrawPaint(paint);

                    paint.SetARGB(255, 255, 255, 255);
                    paint.TextSize = size;

                    Rect textBounds = new Rect();

                    paint.GetTextBounds(text, 0, text.Length, textBounds);

                    measured = new Vector2(textBounds.Width(), textBounds.Height());

                    canvas.DrawText(text, 0, textBounds.Height(), paint);

                    using (Bitmap resized = Bitmap.CreateBitmap(bitmap, 0, 0, textBounds.Width(), textBounds.Height()))
                    {
                        resized.SetConfig(Bitmap.Config.Argb8888);

                        pTexture tex = pTexture.FromRawBytes(resized.LockPixels(), resized.Width, resized.Height);
                        resized.UnlockPixels();
                        return tex;
                    }
                }
            }
            catch (Exception)
            {
                measured = Vector2.Zero;
                return null;
            }
        }

        private static float dpiRatio;
    }
}