using System;
using System.Drawing;
using OpenTK;
namespace osum
{
    public class TrackingPoint : ICloneable
    {
        public object Tag;
        
        private PointF location;
        /// <summary>
        /// The raw screen location 
        /// </summary>
        public PointF Location
        {
            get	{ return location; }
            set { 
                if (location != Point.Empty)
                    Delta = new PointF(value.X - location.X, value.Y - location.Y);
                location = value;

                UpdatePositions();
            }
        }

        public virtual void UpdatePositions()
        {
            BasePosition = new Vector2(GameBase.ScaleFactor * Location.X/GameBase.NativeSize.Width * GameBase.BaseSizeFixedWidth.Width, GameBase.ScaleFactor * Location.Y/GameBase.NativeSize.Height * GameBase.BaseSizeFixedWidth.Height);
            WindowDelta = new Vector2(GameBase.ScaleFactor * Delta.X/GameBase.NativeSize.Width * GameBase.BaseSizeFixedWidth.Width, GameBase.ScaleFactor * Delta.Y/GameBase.NativeSize.Height * GameBase.BaseSizeFixedWidth.Height);
        }
        
        protected PointF Delta;

        /// <summary>
        /// Increased for every press that is associated with the tracking point.
        /// </summary>
        int validity;
        
        public object HoveringObject;
        
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

