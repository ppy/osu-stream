using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace osum.Support.iPhone
{
    [MonoTouch.Foundation.Register("OverlayViewController")]
    public class OverlayViewController : UIViewController
    {
        [Export("initWithCoder:")]
        public OverlayViewController(NSCoder coder) : base(coder)
        {

        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            switch (toInterfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeRight:
                    return true;
                default:
                    return false;
            }
        }
    }
}

