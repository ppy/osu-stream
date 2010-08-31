using System;
using System.Drawing;
using OpenTK;
namespace osum
{
	public class TrackingPoint
	{
		public object Tag;

        /// <summary>
        /// The raw screen location 
        /// </summary>
		public PointF Location;

        /// <summary>
        /// Is this point still valid (active)?
        /// </summary>
        public bool Valid;
		
		public TrackingPoint(PointF location) : this(location,null)
		{}
			
		public TrackingPoint(PointF location, object tag)
		{
			Location = location;
			Tag = tag;
            Valid = true;
		}

		public virtual Vector2 WindowPosition
		{
			get
			{
				return new Vector2(Location.X/GameBase.WindowSize.Width * GameBase.WindowBaseSize.Width, Location.Y/GameBase.WindowSize.Height * GameBase.WindowBaseSize.Height);	
			}
		}

        public virtual Vector2 GamefieldPosition
        {
            get
            {
                return GameBase.StandardToGamefield(WindowPosition);
            }
        }


	}
}

