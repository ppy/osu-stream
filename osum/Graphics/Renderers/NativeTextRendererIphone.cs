//  NativeTextRendererIphone.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using OpenTK.Graphics.ES11;
using System.Drawing;
using OpenTK;
namespace osum.Graphics.Renderers
{
    unsafe internal class NativeTextRendererIphone : NativeTextRenderer
    {
        public NativeTextRendererIphone()
        {

        }

        internal override pTexture CreateText(string text, float size, OpenTK.Vector2 restrictBounds, OpenTK.Graphics.Color4 Color4, bool shadow, bool bold, bool underline, TextAlignment alignment, bool forceAa, out OpenTK.Vector2 measured, OpenTK.Graphics.Color4 background, OpenTK.Graphics.Color4 border, int borderWidth, bool measureOnly, string fontFace)
        {
            UIFont font = UIFont.SystemFontOfSize(size);
			
			if (restrictBounds == Vector2.Zero)
				restrictBounds = new Vector2(512,64);

            int width = TextureGl.GetPotDimension((int)restrictBounds.X);
            int height = TextureGl.GetPotDimension((int)restrictBounds.Y);



            CGColorSpace colorSpace = CGColorSpace.CreateDeviceGray();

            byte[] data = new byte[height * width];

            SizeF actualSize = SizeF.Empty;

            fixed (byte* dataPtr = data)
            {

                CGBitmapContext context = new CGBitmapContext((IntPtr)dataPtr, width, height, 8, width, colorSpace,CGImageAlphaInfo.None);

                colorSpace.Dispose();
    
                context.SetGrayFillColor(1, 1);
                context.TranslateCTM(0, height);
                context.ScaleCTM(1, -1);

                UIGraphics.PushContext(context);
    
                string[] lines = text.Split('\n');

                if (lines.Length > 1)
                {
    
                }
                else
                {
                    actualSize = new NSString(text).DrawString(new RectangleF(0,0,width,height),font, UILineBreakMode.TailTruncation,  UITextAlignment.Left);
                }
    
                UIGraphics.PopContext();

                measured = new OpenTK.Vector2(actualSize.Width, actualSize.Height);
    
                TextureGl gl = new TextureGl(width, height);
                gl.SetData((IntPtr)dataPtr, 0, All.Alpha);
    
                return new pTexture(gl, width, height);
            }
        }
    }
}

