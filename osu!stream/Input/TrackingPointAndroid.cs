using System.Drawing;
using OpenTK;

namespace osum.Input
{
    // based on TrackingPointIphone.
    class TrackingPointAndroid : TrackingPoint
    {
        private const bool FIX_POSITION = true;

        public TrackingPointAndroid(PointF location, object tag) : base(location, tag)
        {
        }

        public override void UpdatePositions()
        {
            float x = (FIX_POSITION ? Location.X : Location.Y) * GameBase.ScaleFactor;
            float y = (FIX_POSITION ? Location.Y : Location.X) * GameBase.ScaleFactor;

            if (GameBase.Instance.FlipView || FIX_POSITION)
                y = GameBase.NativeSize.Height - y;
            if (GameBase.Instance.FlipView && !FIX_POSITION)
                x = GameBase.NativeSize.Width - x;

            Vector2 oldBase = BasePosition;
            BasePosition = new Vector2(
                (x / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.X,
                GameBase.BaseSizeFixedWidth.Y - ((y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Y));
            WindowDelta = BasePosition - oldBase;
        }
    }
}