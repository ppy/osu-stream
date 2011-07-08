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
            float x = GameBase.Instance.FlipView ? GameBase.NativeSize.Width - GameBase.ScaleFactor * Location.Y : GameBase.ScaleFactor * Location.Y;
            float y = GameBase.Instance.FlipView ? GameBase.NativeSize.Height - GameBase.ScaleFactor * Location.X : GameBase.ScaleFactor * Location.X;

            Vector2 oldBase = BasePosition;
            BasePosition = new Vector2(
                (x / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.Width,
                GameBase.BaseSizeFixedWidth.Height - ((y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Height));
            WindowDelta = BasePosition - oldBase;
        }
	}
}

