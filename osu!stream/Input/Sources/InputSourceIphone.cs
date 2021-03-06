using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using osum.Helpers;
using osum.Support.iPhone;
using System.Drawing;
using osum.Input;
using osum.Input.Sources;


namespace osum
{
	public class InputSourceIphone : InputSource
	{
        public InputSourceIphone() : base()
		{
		}

        public void HandleTouches(NSSet touches)
        {
            Clock.Update(true);

            if (touches.Count == 1)
                handleUITouch((UITouch)touches.AnyObject);
            else
                foreach (UITouch u in touches.ToArray<UITouch>())
                    handleUITouch(u);
        }

        Dictionary<UITouch,TrackingPoint> touchDictionary = new Dictionary<UITouch, TrackingPoint>();

        private void handleUITouch(UITouch u)
        {

            TrackingPoint point = null;
            var cgLocation = u.LocationInView(EAGLView.Instance);
            PointF location = new PointF((float)cgLocation.X, (float)cgLocation.Y);

            switch (u.Phase)
            {
                case UITouchPhase.Began:
                    if (AppDelegate.UsingViewController) return;
                    point = new TrackingPointIphone(location, u);
                    touchDictionary[u] = point;
                    TriggerOnDown(point);
                    break;
                case UITouchPhase.Cancelled:
                case UITouchPhase.Ended:
                    if (!touchDictionary.TryGetValue(u, out point))
                        return;
                    touchDictionary.Remove(u);
                    TriggerOnUp(point);
                    break;
                case UITouchPhase.Moved:
                    if (!touchDictionary.TryGetValue(u, out point))
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

