using Android.Views;
using osum.Helpers;
using System.Collections.Generic;
using System.Drawing;

namespace osum.Input.Sources
{
    internal class InputSourceAndroid : InputSource
    {
        Dictionary<int, TrackingPoint> touchDictionary = new Dictionary<int, TrackingPoint>();

        public InputSourceAndroid() : base()
        {
        }

        public void HandleTouches(MotionEvent e)
        {
            Clock.Update(true);

            TrackingPoint point = null;

            int pointerIndex = e.ActionIndex;
            int id = e.GetPointerId(pointerIndex);

            PointF pointerLocation = new PointF(e.GetX(pointerIndex), e.GetY(pointerIndex));

            switch (e.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    point = new TrackingPointAndroid(pointerLocation, e);

                    touchDictionary[id] = point;

                    TriggerOnDown(point);
                    break;

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (!touchDictionary.TryGetValue(id, out point))
                        return;

                    touchDictionary.Remove(id);

                    TriggerOnUp(point);
                    break;

                case MotionEventActions.Move:
                    for (pointerIndex = 0; pointerIndex < e.PointerCount; pointerIndex++)
                    {
                        id = e.GetPointerId(pointerIndex);

                        pointerLocation = new PointF(e.GetX(pointerIndex), e.GetY(pointerIndex));

                        if (!touchDictionary.TryGetValue(id, out point))
                            return;

                        point.Location = pointerLocation;

                        TriggerOnMove(point);
                    }
                    break;
            }
        }
    }
}