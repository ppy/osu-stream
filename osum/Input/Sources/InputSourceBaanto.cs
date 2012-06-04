using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using System.Drawing;
using System.Threading;
using USBHIDDRIVER;
using System.Runtime.InteropServices;
using OpenTK;

namespace osum.Input.Sources
{
    class TrackingPointBaanto : TrackingPoint
    {
        public TrackingPointBaanto(PointF location, object tag)
            : base(location, tag)
        {
        }

        public override void UpdatePositions()
        {
            Vector2 baseLast = BasePosition;
            BasePosition = new Vector2(GameBase.ScaleFactor * Location.X * GameBase.BaseSizeFixedWidth.Width, GameBase.ScaleFactor * Location.Y * GameBase.BaseSizeFixedWidth.Height);
            WindowDelta = BasePosition - baseLast;
        }
    }

    class InputSourceBaanto : InputSource
    {
        enum TouchType
        {
            Release,
            Touch
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TouchPoint
        {
            public byte active;

            public byte id;
            byte xposition;
            byte xsection;
            byte yposition;
            byte ysection;
            byte unknown1;
            byte threshold;
            byte unknown2;
            byte threshold2;

            const int max_length = 16 * 256;
            const int section_size = 256;

            public TouchType State { get { return active > 0 ? TouchType.Touch : TouchType.Release; } }
            public float X { get { return (float)(xposition + section_size * xsection) / max_length; } }
            public float Y { get { return (float)(yposition + section_size * ysection) / max_length; } }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CustomTouchPoint
        {
            byte xsection;
            byte xposition;
            byte ysection;
            byte yposition;
            byte zsection;
            byte zposition;

            const int max_length = 4094;
            const int section_size = 256;

            public TouchType State { get { return xposition + section_size * xsection <= max_length ? TouchType.Touch : TouchType.Release; } }
            public float X { get { return (float)(xposition + section_size * xsection) / max_length; } }
            public float Y { get { return (float)(yposition + section_size * ysection) / max_length; } }
        }

        public InputSourceBaanto()
            : base()
        {
            USBInterface usb = new USBInterface("vid_2453", "pid_0100");

            if (!usb.Connect())
                GameBase.Notify("Couldn't connect with touchscreen.");

            usb.enableUsbBufferEvent(incomingData);
            usb.startRead();
        }

        private void incomingData(object sender, EventArgs args)
        {
            if (USBInterface.usbBuffer.Count > 0)
            {
                byte[] buffer = null;
                int counter = 0;
                while ((byte[])USBInterface.usbBuffer[counter] == null)
                    //Remove this report from list
                    lock (USBInterface.usbBuffer.SyncRoot)
                        USBInterface.usbBuffer.RemoveAt(0);

                //since the remove statement at the end of the loop take the first element
                buffer = (byte[])USBInterface.usbBuffer[0];
                lock (USBInterface.usbBuffer.SyncRoot)
                    USBInterface.usbBuffer.RemoveAt(0);

#if WM_TOUCH
                if (buffer.Length != 56) return;
             
                const int touch_point_count = 5;
                const int touch_point_size = 10;
                const int byte_offset = 1;

                List<TouchPoint> points = new List<TouchPoint>();

                IntPtr ptr = Marshal.AllocHGlobal(touch_point_size * touch_point_count);
                Marshal.Copy(buffer, 0, ptr, touch_point_size * touch_point_count);

                for (int i = 0; i < touch_point_count; i++)
                {
                    TouchPoint point = (TouchPoint)Marshal.PtrToStructure(new IntPtr(ptr.ToInt32() + touch_point_size * i + byte_offset), typeof(TouchPoint));
                    if (point.id > 0)
                        points.Add(point);
                }

                Marshal.FreeHGlobal(ptr);

                foreach (TouchPoint p in points)
                {
                    TrackingPoint tp = trackingPoints.Find(t => t.Tag.ToString() == p.id.ToString());
                    if (tp != null)
                    {
                        tp.Location = new PointF(p.X, p.Y);
                        if (p.State == TouchType.Touch)
                            TriggerOnMove(tp);
                        else
                            TriggerOnUp(tp);
                    }
                    else
                    {
                        tp = new TrackingPointBaanto(new PointF(p.X, p.Y), p.id.ToString());
                        TriggerOnDown(tp);
                    }
                }
#else
                if (buffer.Length != 64 || buffer[0] != 0x0f) return;

                const int touch_point_count = 5;
                const int touch_point_size = 6;
                const int byte_offset = 1;

                List<CustomTouchPoint> points = new List<CustomTouchPoint>();

                IntPtr ptr = Marshal.AllocHGlobal(touch_point_size * touch_point_count);
                Marshal.Copy(buffer, 0, ptr, touch_point_size * touch_point_count);

                for (int i = 0; i < touch_point_count; i++)
                {
                    CustomTouchPoint point = (CustomTouchPoint)Marshal.PtrToStructure(new IntPtr(ptr.ToInt32() + touch_point_size * i + byte_offset), typeof(CustomTouchPoint));
                    points.Add(point);
                }

                Marshal.FreeHGlobal(ptr);

                int id = 0;
                foreach (CustomTouchPoint p in points)
                {
                    TrackingPoint tp = trackingPoints.Find(t => t.Tag.ToString() == id.ToString());
                    if (tp != null)
                    {
                        tp.Location = new PointF(p.X, p.Y);
                        if (p.State == TouchType.Touch)
                            TriggerOnMove(tp);
                        else
                            TriggerOnUp(tp);
                    }
                    else if (p.State == TouchType.Touch)
                    {
                        tp = new TrackingPointBaanto(new PointF(p.X, p.Y), id.ToString());
                        TriggerOnDown(tp);
                    }

                    id++;
                }
#endif
            }
        }
    }
}
