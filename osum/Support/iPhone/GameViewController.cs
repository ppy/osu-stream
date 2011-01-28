using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
namespace osum.Support.iPhone
{
	public partial class GameViewController : UIViewController
	{
		[Export("initWithCoder:")]
		public GameViewController(NSCoder coder) : base(coder)
		{
			
		}
			
		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation orientation)
		{
			return orientation == UIInterfaceOrientation.LandscapeLeft || orientation == UIInterfaceOrientation.LandscapeRight;
		}
	}
}

