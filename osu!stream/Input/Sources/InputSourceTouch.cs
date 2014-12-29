using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace osum.Input.Sources
{
    unsafe class InputSourceTouch : InputSourceRaw
    {
        private class TrackingPointTouch : TrackingPoint
        {
            private GameWindowDesktop window;

            public TrackingPointTouch(PointF location, object tag, GameWindowDesktop window)
                : base(location, tag)
            {
                this.window = window;

                // Trigger UpdatePositions now that our window member is valid.
                Location = location;
            }

            public override void UpdatePositions()
            {
                // This is called in the base constructor sadly and window is not set at this point.
                // To compensate we manually set location again in our own constructor after setting window.
                if (window == null)
                {
                    return;
                }

                Vector2 baseLast = BasePosition;

                PointF clientLocation =
                    window.PointToClient(Point.Round(new PointF(Location.X / 100, Location.Y / 100)));

                BasePosition =
                    new Vector2(
                        GameBase.ScaleFactor * (clientLocation.X / GameBase.NativeSize.Width) * GameBase.BaseSizeFixedWidth.X,
                        GameBase.ScaleFactor * (clientLocation.Y / GameBase.NativeSize.Height) * GameBase.BaseSizeFixedWidth.Y);
                
                WindowDelta = BasePosition - baseLast;
            }
        }

        public InputSourceTouch(GameWindowDesktop window)
            : base(window)
        {
            try
            {
                RegisterTouchWindow(windowHandle, TWF_WANTPALM);
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
                    p = new TrackingPointTouch(new PointF(data.X, data.Y), data.ID, window);
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
