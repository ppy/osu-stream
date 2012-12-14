using System;
using System.Drawing;
using osu_common.Helpers;
using Rectangle=System.Drawing.Rectangle;
using osum.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;

namespace osum.Graphics.Renderers
{
    internal enum TextAlignment
    {
        Left,
        LeftFixed,
        Centre,
        Right
    }

    internal class NativeTextRenderer
    {
        internal virtual pTexture CreateText(string text, float size, Vector2 restrictBounds, Color4 Color4, bool shadow,
                                            bool bold, bool underline, TextAlignment alignment, bool forceAa,
                                            out Vector2 measured,
                                            Color4 background, Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {
            byte[] bytes = new byte[text.Length * 32 * 26 * 4];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = 200;

            pTexture tex = pTexture.FromRawBytes(bytes,text.Length * 32, 26);
            measured = new Vector2(tex.Width,tex.Height);
            return tex;
        }
    }
}