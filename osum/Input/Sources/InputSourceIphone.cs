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
		
		private static List<UITouch> NSSetToList(NSSet set)
		{
			// Make full-aot aware of the needed ICollection<UITouch> types
				ICollection<UITouch> touches_col = (ICollection<UITouch>)set.ToArray<UITouch>();
				
				#pragma warning disable 0219
				// this is a tiny hack, make sure the AOT compiler knows about
				// UICollection.Count, as it will need it when we instantiate the list below
				int touches_count = touches_col.Count;
				#pragma warning restore 0219
				
				List<UITouch> touches = new List<UITouch>(touches_col);
			
				return touches;
		}
		
		public void HandleTouchesBegan (NSSet touches, UIEvent evt)
		{
			Console.WriteLine("touch began");
			
			foreach (UITouch u in NSSetToList(touches))
				trackingPoints.Add(new TrackingPointIphone(u.LocationInView(gameWindow), u));
			
			Console.WriteLine("total touches: " + trackingPoints.Count);
			
			if (trackingPoints.Count == 1)
				TriggerOnDown();
		}
		
		public void HandleTouchesMoved (NSSet touches, UIEvent evt)
		{
			
			
			foreach (UITouch u in NSSetToList(touches))
				trackingPoints.Find(t => t.Tag == u).Location = u.LocationInView(gameWindow);
			
			Console.WriteLine("touch moved");
			Console.WriteLine("total touches: " + trackingPoints.Count);
			
			TriggerOnMove();
		}
		
		public void HandleTouchesEnded (NSSet touches, UIEvent evt)
		{
			foreach (UITouch u in NSSetToList(touches))
				trackingPoints.RemoveAll(t => t.Tag == u);
			
			Console.WriteLine("touch ended");
			Console.WriteLine("total touches: " + trackingPoints.Count);
			
			if (trackingPoints.Count == 0)
				TriggerOnUp();
			
		}
		
		public void HandleTouchesCancelled (NSSet touches, UIEvent evt)
		{
			foreach (UITouch u in NSSetToList(touches))
				trackingPoints.RemoveAll(t => t.Tag == u);
			
			Console.WriteLine("touch cancelled");
			Console.WriteLine("total touches: " + trackingPoints.Count);
			
			if (trackingPoints.Count == 0)
				TriggerOnUp();
		}

	}
}

