using System;
using System.Drawing;
using OpenTK;

namespace osum.Input
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
            get => location;
            set
            {
                location = value;
                UpdatePositions();
            }
        }

        public virtual void UpdatePositions()
        {
            Vector2 baseLast = BasePosition;
            BasePosition = new Vector2(GameBase.ScaleFactor * Location.X / GameBase.NativeSize.Width * GameBase.BaseSizeFixedWidth.X, GameBase.ScaleFactor * Location.Y / GameBase.NativeSize.Height * GameBase.BaseSizeFixedWidth.Y);
            WindowDelta = BasePosition - baseLast;
        }

        /// <summary>
        /// Increased for every press that is associated with the tracking point.
        /// </summary>
        private int validity;

        public object HoveringObject;

        public TrackingPoint originalTrackingPoint;

        /// <summary>
        /// Each frame this will be set to false, and set to true when the previously hovering object
        /// is confirmed to still be the "highest" hovering object.
        /// </summary>
        public bool HoveringObjectConfirmed;

        /// <summary>
        /// Is this point still valid (active)?
        /// </summary>
        public bool Valid => validity > 0;

        public TrackingPoint(PointF location) : this(location, null)
        {
        }

        public TrackingPoint(PointF location, object tag)
        {
            Tag = tag;
            Location = location;
            WindowDelta = Vector2.Zero; //no delta on first ctor.
            originalTrackingPoint = this;
        }

        public Vector2 BasePosition;
        public Vector2 WindowDelta;

        public virtual Vector2 GamefieldPosition => GameBase.StandardToGamefield(BasePosition);

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
            TrackingPoint clone = MemberwiseClone() as TrackingPoint;
            clone.originalTrackingPoint = originalTrackingPoint;
            return clone;
        }

        #endregion
    }
}