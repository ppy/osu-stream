using System;
using UIKit;
using System.Drawing;
using osum.Helpers;

namespace osum.Support.iPhone
{
    public static class ExtensionMethods
    {
        public static RectangleF BoundsCorrected(this UIScreen screen)
        {
            RectangleF actualBounds = screen.Bounds.ToRectangleF();

            if (HardwareDetection.RunningiOS8OrHigher)
            {
                //As of iOS, bounds is orientation specific (http://stackoverflow.com/a/25088478)
                float w = actualBounds.Width;
                actualBounds.Width = actualBounds.Height;
                actualBounds.Height = w;
            }

            return actualBounds;
        }
    }
}

