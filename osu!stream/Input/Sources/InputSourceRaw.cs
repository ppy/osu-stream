using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace osum.Input.Sources
{
    unsafe class InputSourceRaw : InputSourceRawBase
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

        bool registeredTouch = false;

        public InputSourceRaw(GameWindowDesktop window)
            : base(window)
        {
            RawInputDevice r = new RawInputDevice();

            r.UsagePage = HIDUsagePage.Generic;
            r.Usage = HIDUsage.Mouse;
            r.Flags = RawInputDeviceFlags.InputSink;
            r.WindowHandle = windowHandle;

            if (!RegisterRawInputDevices(new[] { r }, 1, sizeof(RawInputDevice)))
            {
                throw new Exception("Couldn't initialize raw mouse input.");
            }

            bind(RawInputType.Mouse, handler);

            registeredTouch = RegisterTouchWindow(windowHandle, TWF_WANTPALM);
            bindTouch(touchHandler);
        }

        private PointF MousePosition(RawMouse data)
        {
            if ((data.Flags & RawMouseFlags.MoveAbsolute) > 0)
            {
                const int range = 65536;

                Rectangle resolution = Screen.PrimaryScreen.Bounds;

                PointF pos = new PointF(
                    ((float)(data.LastX - range / 2) + range / 2) / range * resolution.Width,
                    ((float)(data.LastY - range / 2) + range / 2) / range * resolution.Height);

                return window.PointToClient(Point.Round(pos));
            }
            else
            {
                // We cheat here and use absolute mouse position from opentk. Raw input is possible by always starting
                // at the absolute mouse position on button down, but there is a slight drift detaching our current position
                // from the windows cursor. Not desired!
                return window.Mouse == null ? new PointF(0, 0) : new PointF(window.Mouse.X, window.Mouse.Y);
            }
        }

        private void handler(RawInput data)
        {
            // This detects WM_INPUT messages that were generated from WM_TOUCH.
            // We don't wanna handle them since we handle WM_TOUCH messages by ourself!
            // If OS tablet support is activated we however don't receive WM_TOUCH messages and DO want to handle WM_INPUT.
            // See http://the-witness.net/news/2012/10/wm_touch-is-totally-bananas/ for more information.
            if (registeredTouch && (data.Mouse.ExtraInformation & 0x82) > 0)
            {
                return;
            }

            TrackingPoint p = trackingPoints.Find(t => (string)t.Tag == "m");
            PointF pos = MousePosition(data.Mouse);

            if ((data.Mouse.ButtonFlags & 
                    (RawMouseButtons.LeftDown |
                     RawMouseButtons.RightDown |
                     RawMouseButtons.MiddleDown |
                     RawMouseButtons.Button4Down |
                     RawMouseButtons.Button5Down)) > 0)
            {
                if (p != null)
                {
                    TriggerOnUp(p);
                }

                p = new TrackingPoint(pos, "m");
                TriggerOnDown(p);
            }

            if (p != null)
            {
                bool isNewPosition = p.Location != pos;

                if (isNewPosition)
                {
                    p.Location = pos;
                }
                
                if ((data.Mouse.ButtonFlags &
                    (RawMouseButtons.LeftUp |
                        RawMouseButtons.RightUp |
                        RawMouseButtons.MiddleUp |
                        RawMouseButtons.Button4Up |
                        RawMouseButtons.Button5Up)) > 0)
                {
                    TriggerOnUp(p);
                }
                else if (isNewPosition)
                {
                    TriggerOnMove(p);
                }
            }
        }
        
        private void touchHandler(RawTouchInput data)
        {
            // Actual touch
            TrackingPoint p = trackingPoints.Find(t => t.Tag.ToString() == data.ID.ToString());

            if ((data.Flags & RawTouchFlags.Down) > 0)
            {
                if (p == null)
                {
                    p = new TrackingPointTouch(new PointF(data.X, data.Y), data.ID, window);
                    TriggerOnDown(p);
                }
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
