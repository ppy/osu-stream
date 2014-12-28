using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace osum.Input.Sources
{
    unsafe class InputSourceTouch : InputSourceRaw
    {
        class TrackingPointTouch : TrackingPoint
        {
            public TrackingPointTouch(PointF location, object tag)
                : base(location, tag)
            {
            }

            public override void UpdatePositions()
            {
                Vector2 baseLast = BasePosition;

                PointF clientLocation = 
                    GameBaseDesktop.Window.PointToClient(Point.Round(new PointF(Location.X / 100, Location.Y / 100)));

                BasePosition =
                    new Vector2(
                        GameBase.ScaleFactor * (clientLocation.X / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.X,
                        GameBase.ScaleFactor * (clientLocation.Y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Y);
                WindowDelta = BasePosition - baseLast;
            }
        }

        public InputSourceTouch(IntPtr handle)
            : base(handle)
        {
            try
            {
                RegisterTouchWindow(handle, TWF_WANTPALM);
                bindTouch(touchHandler);
            }
            catch { }
        }

        
        private void touchHandler(RawTouchInput data)
        {
            TrackingPoint p = trackingPoints.Find(t => t.Tag.ToString() == data.ID.ToString());

            if ((data.Flags & RawTouchFlags.Down) > 0)
            {
                if (p == null)
                {
                    p = new TrackingPointTouch(new PointF(data.X, data.Y), data.ID);
                    trackingPoints.Add(p);
                }

                TriggerOnDown(p);
            }
            else if (p != null)
            {
                p.Location = new PointF(data.X, data.Y);
                if ((data.Flags & RawTouchFlags.Up) > 0)
                {
                    TriggerOnUp(p);
                }
                else
                {
                    TriggerOnMove(p);
                }
            }
            
        }

    }
}
