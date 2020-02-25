using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using osum.Input.Sources.UsbHID;

namespace osum.Input.Sources
{
    internal class TrackingPointBaanto : TrackingPoint
    {
        public TrackingPointBaanto(PointF location, object tag) : base(location, tag)
        {
        }

        public override void UpdatePositions()
        {
            Vector2 baseLast = BasePosition;
            BasePosition = new Vector2(GameBase.ScaleFactor * Location.X * GameBase.BaseSizeFixedWidth.X, GameBase.ScaleFactor * Location.Y * GameBase.BaseSizeFixedWidth.Y);
            WindowDelta = BasePosition - baseLast;
        }
    }

    internal class InputSourceBaanto : InputSource
    {
        private enum TouchType
        {
            Release,
            Touch
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TouchPoint
        {
            public readonly byte active;

            public readonly byte id;
            private readonly byte xposition;
            private readonly byte xsection;
            private readonly byte yposition;
            private readonly byte ysection;
            private readonly byte unknown1;
            private readonly byte threshold;
            private readonly byte unknown2;
            private readonly byte threshold2;

            private const int max_length = 16 * 256;
            private const int section_size = 256;

            public TouchType State => active > 0 ? TouchType.Touch : TouchType.Release;
            public float X => (float)(xposition + section_size * xsection) / max_length;
            public float Y => (float)(yposition + section_size * ysection) / max_length;
        }

        public InputSourceBaanto()
        {
            USBInterface usb = new USBInterface("vid_2453", "pid_0100");

            usb.Connect();
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
            }
        }
    }
}