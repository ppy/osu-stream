using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using osum.Helpers;
using osu_common.Helpers;
using Rectangle = System.Drawing.Rectangle;
using osum.Graphics;
using OpenTK;
using OpenTK.Graphics;
using System.IO;

namespace osum.Graphics.Renderers
{
    internal class NativeTextRendererDesktop : NativeTextRenderer
    {
        static readonly Brush blackBrush = Brushes.Black;

        internal override pTexture CreateText(string text, float size, Vector2 restrictBounds, Color4 Color4, bool shadow,
                                            bool bold, bool underline, TextAlignment alignment, bool forceAa,
                                            out Vector2 measured,
                                            Color4 background, Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {

            size *= 0.72f;
            //todo: this is a temporary hack to make sizes the same on desktop and iphone.
            //need to switch this around so desktop is the base size and iphone matches instead.

            try
            {
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                using (StringFormat sf = new StringFormat())
                {
                    if (dpiRatio == 0) dpiRatio = 96 / graphics.DpiX;

                    size *= dpiRatio;

                    if (text == null)
                    {
                        measured = Vector2.Zero;
                        return null;
                    }

                    SizeF measuredSize;

                    FontStyle fs = FontStyle.Bold;
                    //if (bold)
                    //    fs |= FontStyle.Bold;
                    //if (underline)
                    //    fs |= FontStyle.Underline;

                    switch (alignment)
                    {
                        case TextAlignment.Left:
                        case TextAlignment.LeftFixed:
                            sf.Alignment = StringAlignment.Near;
                            break;
                        case TextAlignment.Centre:
                            sf.Alignment = StringAlignment.Center;
                            break;
                        case TextAlignment.Right:
                            sf.Alignment = StringAlignment.Far;
                            break;
                    }

                    PrivateFontCollection fontCollection = new PrivateFontCollection();
                    fontCollection.AddFontFile(Path.GetFullPath(bold ? @"Skins/Default/Futura-CondensedExtraBold.ttf" : @"Skins/Default/Futura-Medium.ttf"));
                    FontFamily tFont = fontCollection.Families[0];

                    using (Font f = new Font(tFont, size, fs))
                    {
                        {
                            try
                            {
                                if (text.Length == 0) text = " ";
                                measuredSize = restrictBounds != Vector2.Zero
                                                   ? graphics.MeasureString(text, f, new SizeF(restrictBounds.X, restrictBounds.Y), sf)
                                                   : graphics.MeasureString(text, f);
                            }
                            catch (InvalidOperationException)
                            {
                                measured = Vector2.Zero;
                                return null;
                            }
                        }

                        int width = (int)(measuredSize.Width + 1);
                        int height = (int)(measuredSize.Height + 1);

                        if (restrictBounds.Y != 0)
                            height = (int)restrictBounds.Y;

                        if (restrictBounds.X != 0)
                            width = (int)restrictBounds.X;

                        if (measureOnly)
                        {
                            int startSpace = 0;
                            int endSpace = 0;

                            int i = 0;
                            while (i < text.Length && text[i++] == ' ')
                                startSpace++;
                            int j = text.Length - 1;
                            while (j >= i && text[j--] == ' ')
                                endSpace++;
                            if (startSpace == text.Length)
                                endSpace += startSpace;

                            measured = new Vector2(width + (endSpace * 5.145f * size / 12), height);

                            return null;
                        }

                        measured = new Vector2(width, height);

                        using (Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                        {
                            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
                            {
                                //Quality settings
                                g.TextRenderingHint = forceAa ? TextRenderingHint.AntiAlias : TextRenderingHint.AntiAliasGridFit;
                                g.SmoothingMode = SmoothingMode.HighQuality;
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;


                                if (background.A > 0)
                                    g.Clear(ColourHelper.CConvert(background));
                                if (background.A > 0 && borderWidth > 0)
                                    g.DrawRectangle(new Pen(ColourHelper.CConvert(border), borderWidth),
                                                    new Rectangle(0 + borderWidth / 2, 0 + borderWidth / 2,
                                                                  width - borderWidth, height - borderWidth));


                                using (Brush brush = new SolidBrush(ColourHelper.CConvert(Color4)))
                                {
                                    //if (shadow)
                                    //    g.DrawString(text, f, blackBrush,
                                    //                 new RectangleF(1, 1, measuredSize.Width, measuredSize.Height), sf);
                                    g.DrawString(text, f, brush, new RectangleF(0, 0, measuredSize.Width, measuredSize.Height), sf);
                                }
                            }

                            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, b.PixelFormat);
                            pTexture tex = pTexture.FromRawBytes(data.Scan0, width, height);
                            b.UnlockBits(data);
                            return tex;
                        }
                    }
                }
            }
            catch (Exception)
            {
                measured = Vector2.Zero;
                return null;
            }
        }

        private static float dpiRatio = 0;
    }
}