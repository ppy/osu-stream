using System;
using System.Drawing;
using OpenTK;
using osum.Support.iPhone;


namespace osum
{
	public class TrackingPointIphone : TrackingPoint
	{
		public TrackingPointIphone (PointF location, object tag) : base(location,tag)
		{			
		}

        public override void UpdatePositions()
        {
            bool ios8 = HardwareDetection.RunningiOS8OrHigher;

            float x = (ios8 ? Location.X : Location.Y) * GameBase.ScaleFactor;
            float y = (ios8 ? Location.Y : Location.X) * GameBase.ScaleFactor;

            if (GameBase.Instance.FlipView || ios8)
                y = GameBase.NativeSize.Height - y;
            if (GameBase.Instance.FlipView && !ios8)
                x = GameBase.NativeSize.Width - x;

            Vector2 oldBase = BasePosition;
            BasePosition = new Vector2(
                (x / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.X,
                GameBase.BaseSizeFixedWidth.Y - ((y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Y));
            WindowDelta = BasePosition - oldBase;
        }
	}
}

