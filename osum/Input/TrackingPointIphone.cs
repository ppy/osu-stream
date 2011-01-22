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
				return new Vector2(-(GameBase.WindowScaleFactor * Delta.Y / GameBase.WindowSize.Width) * GameBase.WindowBaseSize.Width,
					                   -((GameBase.WindowScaleFactor * Delta.X / GameBase.WindowSize.Height) * GameBase.WindowBaseSize.Height));	
			}
		}
		
		public override OpenTK.Vector2 WindowPosition {
			get
			{
				return new Vector2((GameBase.WindowScaleFactor * Location.Y / GameBase.WindowSize.Width) * GameBase.WindowBaseSize.Width,
				                   GameBase.WindowBaseSize.Height - ((GameBase.WindowScaleFactor * Location.X / GameBase.WindowSize.Height) * GameBase.WindowBaseSize.Height));
			}
		}
	}
}

