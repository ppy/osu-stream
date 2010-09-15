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
        /// Increased for every press that is associated with the tracking point.
        /// </summary>
        int validity;
        
        /// <summary>
        /// Is this point still valid (active)?
        /// </summary>
        public bool Valid { get { return validity > 0; } }
		
		public TrackingPoint(PointF location) : this(location,null)
		{}
			
		public TrackingPoint(PointF location, object tag)
		{
			Location = location;
			Tag = tag;
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



        internal void IncreaseValidity()
        {
            validity++;
        }

        internal void DecreaseValidity()
        {
            validity--;
        }
    }
}

