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

            CGSize actualSize = CGSize.Empty;

            // Render the text to a UILabel to calculate sizing
            // and line-wrapping, and then copy the pixels to our texture buffer.
            UILabel textLabel = new UILabel();
            textLabel.Font = font;
            textLabel.BackgroundColor = UIColor.Clear;
            textLabel.TextColor = UIColor.White;
            textLabel.LineBreakMode = UILineBreakMode.WordWrap;
            textLabel.Lines = 0; // Needed for multiple lines
            textLabel.Text = text;

            textLabel.TextAlignment = UITextAlignment.Left;
            switch (alignment) {
            case TextAlignment.Centre:
                textLabel.TextAlignment = UITextAlignment.Center;
                break;
            case TextAlignment.Right:
                textLabel.TextAlignment = UITextAlignment.Right;
                break;
            }

            if (restrictBounds == Vector2.Zero)
            {
                textLabel.SizeToFit();
                actualSize = textLabel.Frame.Size;

                restrictBounds = new Vector2((float)actualSize.Width, (float)actualSize.Height);
            }
            else if (restrictBounds.Y == 0)
            {
                SizeF boundsSize = new SizeF (restrictBounds.X, GameBase.NativeSize.Height);
                actualSize = textLabel.SizeThatFits(boundsSize);
                textLabel.Frame = new CGRect(CGPoint.Empty, actualSize);

                restrictBounds = new Vector2((float)actualSize.Width, (float)actualSize.Height);
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
                context.TranslateCTM(0, height);
                context.ScaleCTM(1, -1);

                UIGraphics.PushContext(context);

                textLabel.SetNeedsDisplay();
                textLabel.Layer.DrawInContext (context);
                
                UIGraphics.PopContext();

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

