using Android.Views;
using osum.Helpers;
using System.Collections.Generic;
using System.Drawing;

namespace osum.Input.Sources
{
    internal class InputSourceAndroid : InputSource
    {
        public InputSourceAndroid() : base()
        {
        }

        public void HandleTouches(MotionEvent e)
        {
            Clock.Update(true);

            for (int i = 0; i < e.PointerCount; i++)
            {
                handleMotionEvent(i, e);
            }
        }

        Dictionary<int, TrackingPoint> touchDictionary = new Dictionary<int, TrackingPoint>();

        private void handleMotionEvent(int i, MotionEvent e)
        {
            TrackingPoint point = null;
            PointF location = new PointF(e.GetX(i)/* / 2.25f*/, e.GetY(i)/* / 2.25f*/);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    point = new TrackingPointAndroid(location, e);
                    touchDictionary[e.GetPointerId(i)] = point;
                    TriggerOnDown(point);
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (!touchDictionary.TryGetValue(e.GetPointerId(i), out point))
                        return;
                    touchDictionary.Remove(e.GetPointerId(i));
                    TriggerOnUp(point);
                    break;
                case MotionEventActions.Move:
                    if (!touchDictionary.TryGetValue(e.GetPointerId(i), out point))
                        return;
                    point.Location = location;
                    TriggerOnMove(point);
                    break;
            }
        }

        public void ReleaseAllTouches()
        {
            foreach (TrackingPoint t in trackingPoints.ToArray())
                TriggerOnUp(t);
            trackingPoints.Clear();
            touchDictionary.Clear();
        }
    }
}