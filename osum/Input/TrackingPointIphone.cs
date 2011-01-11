using System;
using System.Drawing;
using OpenTK;
namespace osum
{
	public class TrackingPointIphone : TrackingPoint
	{
		public TrackingPointIphone (PointF location, object tag) : base(location,tag)
		{			
		}
		
		public override OpenTK.Vector2 WindowDelta {
			get
			{
				return new Vector2(-(Delta.Y / GameBase.WindowSize.Width) * GameBase.WindowBaseSize.Width,
					                   -((Delta.X / GameBase.WindowSize.Height) * GameBase.WindowBaseSize.Height));	
			}
		}
		
		public override OpenTK.Vector2 WindowPosition {
			get
			{
				return new Vector2((Location.Y / GameBase.WindowSize.Width) * GameBase.WindowBaseSize.Width,
				                   GameBase.WindowBaseSize.Height - ((Location.X / GameBase.WindowSize.Height) * GameBase.WindowBaseSize.Height));
			}
		}
	}
}

