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

        public override void UpdatePositions()
        {
            float x = GameBase.Instance.FlipView ? GameBase.NativeSize.Width - Location.Y : Location.Y;
            float y = GameBase.Instance.FlipView ? GameBase.NativeSize.Height - Location.X : Location.X;

            Vector2 oldBase = BasePosition;
            BasePosition = new Vector2(
                (GameBase.ScaleFactor * x / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.Width,
                GameBase.BaseSizeFixedWidth.Height - ((GameBase.ScaleFactor * y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Height));
            WindowDelta = BasePosition - oldBase;
        }
	}
}

