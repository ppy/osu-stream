using System;
using System.Drawing;
using System.Windows.Forms;
using osum.Support;

namespace osum.Input.Sources
{
    internal unsafe class InputSourceRaw : InputSourceRawBase
    {
        private readonly bool registeredTouch;

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
            bindPointer(pointerHandler);

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

            // We cheat here and use absolute mouse position from opentk. Raw input is possible by always starting
            // at the absolute mouse position on button down, but there is a slight drift detaching our current position
            // from the windows cursor. Not desired!
            return window.Mouse == null ? new PointF(0, 0) : new PointF(window.Mouse.X, window.Mouse.Y);
        }

        private void handler(RawInput data)
        {
            if (registeredTouch && (trackingPoints.Count > 1 || (trackingPoints.Count == 1 && trackingPoints[0].Tag.ToString() != "m")))
            {
                return;
            }

            TrackingPoint p = trackingPoints.Find(t => t.Tag.ToString() == "m");
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
            Point pos = window.PointToClient(Point.Round(new PointF((float)data.X / 100, (float)data.Y / 100)));

            if ((data.Flags & RawTouchFlags.Down) > 0)
            {
                if (p == null)
                {
                    p = new TrackingPoint(new PointF(pos.X, pos.Y), data.ID);
                    TriggerOnDown(p);
                }
            }
            else if (p != null)
            {
                p.Location = new PointF(pos.X, pos.Y);
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


        private void pointerHandler(RawPointerInput data)
        {
            // Actual touch
            TrackingPoint p = trackingPoints.Find(t => t.Tag.ToString() == data.ID.ToString());
            Point pos = window.PointToClient(data.PixelLocationRaw);

            if ((data.Flags & RawPointerFlags.Down) > 0)
            {
                if (p == null)
                {
                    p = new TrackingPoint(new PointF(pos.X, pos.Y), data.ID);
                    TriggerOnDown(p);
                }
            }
            else if (p != null)
            {
                p.Location = new PointF(pos.X, pos.Y);
                if ((data.Flags & RawPointerFlags.Up) > 0 || (data.Flags & RawPointerFlags.CaptureChanged) > 0)
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