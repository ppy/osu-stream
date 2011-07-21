using System;
using System.Drawing;
using OpenTK;
namespace osum
{
    public class TrackingPoint : ICloneable
    {
        private PointF location;
        /// <summary>
        /// The raw screen location 
        /// </summary>
        public PointF Location
        {
            get	{ return location; }
            set { 
                location = value;
                UpdatePositions();
            }
        }

        public virtual void UpdatePositions()
        {
            Vector2 baseLast = BasePosition;
            BasePosition = new Vector2(GameBase.ScaleFactor * Location.X/GameBase.NativeSize.Width * GameBase.BaseSizeFixedWidth.Width, GameBase.ScaleFactor * Location.Y/GameBase.NativeSize.Height * GameBase.BaseSizeFixedWidth.Height);
            WindowDelta = BasePosition - baseLast;
        }

        /// <summary>
        /// Increased for every press that is associated with the tracking point.
        /// </summary>
        int validity;
        
        public object HoveringObject;

        /// <summary>
        /// Each frame this will be set to false, and set to true when the previously hovering object
        /// is confirmed to still be the "highest" hovering object.
        /// </summary>
        public bool HoveringObjectConfirmed;
        
        /// <summary>
        /// Is this point still valid (active)?
        /// </summary>
        public bool Valid { get { return validity > 0; } }
        
        public TrackingPoint(PointF location) : this(location,null)
        {}
            
        public TrackingPoint(PointF location, object tag)
        {
            Location = location;
            WindowDelta = Vector2.Zero; //no delta on first ctor.
        }

        public Vector2 BasePosition;
        public Vector2 WindowDelta;

        public virtual Vector2 GamefieldPosition
        {
            get
            {
                return GameBase.StandardToGamefield(BasePosition);
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

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}

