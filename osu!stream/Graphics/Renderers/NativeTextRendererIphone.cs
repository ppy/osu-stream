//  NativeTextRendererIphone.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using UIKit;
using CoreGraphics;
using Foundation;
using OpenTK.Graphics.ES11;
using System.Drawing;
using OpenTK;
using osum.Graphics.Sprites;
using System.Runtime.InteropServices;

namespace osum.Graphics.Renderers
{
    unsafe internal class NativeTextRendererIphone : NativeTextRenderer
    {
        public NativeTextRendererIphone()
        {

        }

        internal override pTexture CreateText(string text, float size, OpenTK.Vector2 restrictBounds, OpenTK.Graphics.Color4 Color4, bool shadow, bool bold, bool underline, TextAlignment alignment, bool forceAa, out OpenTK.Vector2 measured, OpenTK.Graphics.Color4 background, OpenTK.Graphics.Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {
            //UIFont font = bold ? UIFont.FromName("GillSans-Bold", size) : UIFont.FromName("GillSans",size);
            //UIFont font = bold ? UIFont.BoldSystemFontOfSize(size) : UIFont.SystemFontOfSize(size);
            UIFont font = UIFont.FromName(bold ? "Futura-CondensedExtraBold" : "Futura-Medium",size);

            NSString nsstr = new NSString(text);
            CGSize actualSize = CGSize.Empty;

			if (restrictBounds == Vector2.Zero)
            {
                actualSize = nsstr.StringSize(font, GameBase.NativeSize, UILineBreakMode.WordWrap);
                restrictBounds = new Vector2((float)actualSize.Width, (float)actualSize.Height);
            }
            else if (restrictBounds.Y == 0)
            {
                actualSize = nsstr.StringSize(font, new SizeF(restrictBounds.X, GameBase.NativeSize.Height), UILineBreakMode.WordWrap);
                restrictBounds = new Vector2((float)actualSize.Width + 5, (float)actualSize.Height);
                //note: 5px allowance is givn because when wordwarpping it seems to get cut off otherwise.
            }

            int width = TextureGl.GetPotDimension((int)restrictBounds.X);
            int height = TextureGl.GetPotDimension((int)restrictBounds.Y);

            IntPtr data = Marshal.AllocHGlobal(width * height);
            unsafe {
                byte* bytes = (byte*)data;
                for (int i = width * height - 1; i >= 0; i--) bytes[i] = 0;
            }

            using (CGColorSpace colorSpace = CGColorSpace.CreateDeviceGray())
            using (CGBitmapContext context = new CGBitmapContext(data, width, height, 8, width, colorSpace,CGImageAlphaInfo.None))
            {
                context.SetFillColor(1, 1);
                context.TranslateCTM(0, height);
                context.ScaleCTM(1, -1);

                UIGraphics.PushContext(context);

                UITextAlignment align = UITextAlignment.Left;
                switch (alignment)
                {
                    case TextAlignment.Centre:
                        align = UITextAlignment.Center;
                        break;
                    case TextAlignment.Right:
                        align = UITextAlignment.Right;
                        break;
                }

                actualSize = nsstr.DrawString(new RectangleF(0,0,restrictBounds.X,restrictBounds.Y),font, UILineBreakMode.WordWrap, align);

                UIGraphics.PopContext();

                nsstr.Dispose();

                measured = new Vector2((float)actualSize.Width, (float)actualSize.Height);

    			SpriteManager.TexturesEnabled = true;

                TextureGl gl = new TextureGl(width, height);
                gl.SetData(data, 0, All.Alpha);

                Marshal.FreeHGlobal(data);

                return new pTexture(gl, (int)actualSize.Width, (int)actualSize.Height);
            }
        }
    }
}

