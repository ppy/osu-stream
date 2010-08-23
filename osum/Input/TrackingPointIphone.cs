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
		
		public override OpenTK.Vector2 GamePosition {
			get
			{
				return new Vector2((Location.Y / GameBase.WindowSize.Width) * GameBase.WindowBaseSize.Width,
				                   GameBase.WindowBaseSize.Height - ((Location.X / GameBase.WindowSize.Height) * GameBase.WindowBaseSize.Height));
			}
		}
	}
}

