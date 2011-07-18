using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using osum.Helpers;
using osum.Support.iPhone;
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
            TrackingPoint point = null;

            foreach (UITouch u in touches.ToArray<UITouch>())
            {
                switch (u.Phase)
                {
                    case UITouchPhase.Began:
                        if (AppDelegate.UsingViewController) return;
                        point = new TrackingPointIphone(u.LocationInView(EAGLView.Instance), u);
                        trackingPoints.Add(point);
                        TriggerOnDown(point);
                        break;
                    case UITouchPhase.Cancelled:
                    case UITouchPhase.Ended:
                        point = trackingPoints.Find(t => t.Tag == u);
                         if (point != null)
                         {
                             trackingPoints.Remove(point);
                             TriggerOnUp(point);
                         }
                        break;
                    case UITouchPhase.Moved:
                        point = trackingPoints.Find(t => t.Tag == u);
                        if (point != null)
                        {
                            point.Location = u.LocationInView(EAGLView.Instance);
                            TriggerOnMove(point);
                        }
                        break;
                }
            }
        }

        public void ReleaseAllTouches()
        {
            foreach (TrackingPoint t in trackingPoints)
                TriggerOnUp(t);
            trackingPoints.Clear();
        }
	}
}

