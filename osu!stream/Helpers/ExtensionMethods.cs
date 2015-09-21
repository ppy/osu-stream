using System;
using CoreGraphics;
using System.Drawing;

namespace osum.Helpers
{
    public static class ExtensionMethods
    {
        public static RectangleF ToRectangleF(this CGRect rect)
        {
            return new RectangleF ((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }
    }
}

