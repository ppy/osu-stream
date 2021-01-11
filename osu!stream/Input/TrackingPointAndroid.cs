using System.Drawing;
using OpenTK;

namespace osum.Input
{
    class TrackingPointAndroid : TrackingPoint
    {
        public TrackingPointAndroid(PointF location, object tag) : base(location, tag)
        {
        }

        public override void UpdatePositions()
        {
            float x = Location.X * GameBase.ScaleFactor;
            float y = Location.Y * GameBase.ScaleFactor;

            if (GameBase.Instance.FlipView)
                y = GameBase.NativeSize.Height - y;

            Vector2 oldBase = BasePosition;
            BasePosition = new Vector2(
                (x / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.X,
                GameBase.BaseSizeFixedWidth.Y - ((y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Y));
            WindowDelta = BasePosition - oldBase;
        }
    }
}