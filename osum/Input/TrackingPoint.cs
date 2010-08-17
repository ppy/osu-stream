using System;
using System.Drawing;
using OpenTK;
namespace osum
{
	public class TrackingPoint
	{
		public object Tag;
		public PointF Location;
		
		public TrackingPoint(PointF location) : this(location,null)
		{}
			
		public TrackingPoint(PointF location, object tag)
		{
			Location = location;
			Tag = tag;
		}
		
		public virtual Vector2 GamePosition
		{
			get
			{
				return new Vector2(Location.X/GameBase.WindowSize.Width * GameBase.StandardSize.Width, Location.Y/GameBase.WindowSize.Height * GameBase.StandardSize.Height);	
			}
		}
	}
}

