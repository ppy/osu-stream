using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
namespace osum
{
	public class InputSourceIphone : InputSource
	{
		private GameWindowIphone gameWindow;

		public InputSourceIphone(GameWindowIphone gameWindow) : base()
		{
			this.gameWindow = gameWindow;
		}

		private static List<UITouch> NSSetToList(NSSet @set)
		{
			// Make full-aot aware of the needed ICollection<UITouch> types
			ICollection<UITouch> touches_col = (ICollection<UITouch>)@set.ToArray<UITouch>();
			
			#pragma warning disable 0219
			// this is a tiny hack, make sure the AOT compiler knows about
			// UICollection.Count, as it will need it when we instantiate the list below
			int touches_count = touches_col.Count;
			#pragma warning restore 0219
			
			List<UITouch> touches = new List<UITouch>(touches_col);
			
			return touches;
		}

		public void HandleTouchesBegan(NSSet touches, UIEvent evt)
		{
			TrackingPoint newPoint = null;
			
			foreach (UITouch u in NSSetToList(touches)) {
				newPoint = new TrackingPointIphone(u.LocationInView(gameWindow), u);
				trackingPoints.Add(newPoint);
			}
			
			//if (trackingPoints.Count == 1)
			TriggerOnDown(newPoint);
		}

		public void HandleTouchesMoved(NSSet touches, UIEvent evt)
		{
			TrackingPoint point = null;

			foreach (UITouch u in NSSetToList(touches)) {
				point = trackingPoints.Find(t => t.Tag == u);
				if (point != null)
					point.Location = u.LocationInView(gameWindow);
			}
			
			TriggerOnMove(point);
		}

		public void HandleTouchesEnded(NSSet touches, UIEvent evt)
		{
			TrackingPoint point = null;
			
			foreach (UITouch u in NSSetToList(touches)) {
				point = trackingPoints.Find(t => t.Tag == u);
				if (point != null)
					trackingPoints.Remove(point);
			}
			
			TriggerOnUp(point);
		}

		public void HandleTouchesCancelled(NSSet touches, UIEvent evt)
		{
			HandleTouchesEnded(touches, evt);
		}		
	}
}

