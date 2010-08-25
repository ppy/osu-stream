using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using osum.Helpers;
namespace osum
{
	public static class InputManager
	{
		public static List<InputSource> RegisteredSources = new List<InputSource>();
		
		public static Vector2 MainPointerPosition;
		
		public static bool IsTracking { 
			get
			{
				if (RegisteredSources.Count == 0) return false;
				
				return RegisteredSources[0].trackingPoints.Count > 0;
			}
		}
		
		public static void Initialize()
		{
			
		}
		
		public static bool AddSource(InputSource source)
		{
		    if (RegisteredSources.Contains(source))
    		    return false;
    		    
		    source.OnDown += ReceiveDown;
			source.OnUp += ReceiveUp;
			source.OnClick += ReceiveClick;
			source.OnMove += ReceiveMove;
		    
		    RegisteredSources.Add(source);
			
			return true;
	    }
		
		private static void ReceiveDown(InputSource source, TrackingPoint point)
		{
			Console.WriteLine("input: down");
			MainPointerPosition = source.trackingPoints[0].GamePosition;

            TriggerOnDown(source, point);
		}

        private static void ReceiveUp(InputSource source, TrackingPoint point)
		{
			Console.WriteLine("input: up");
            TriggerOnUp(source, point);
		}

        private static void ReceiveClick(InputSource source, TrackingPoint point)
		{
			Console.WriteLine("input: click");
            TriggerOnClick(source, point);
		}

        private static void ReceiveMove(InputSource source, TrackingPoint point)
		{
			Console.WriteLine("input: move");
			MainPointerPosition = source.trackingPoints[0].GamePosition;
            TriggerOnMove(source, point);
		}

        public static bool IsPressed
        {
            get
            {
                return RegisteredSources[0].IsPressed;
            }
        }

        public static event InputHandler OnDown;
        private static void TriggerOnDown(InputSource source, TrackingPoint point)
        {
            if (OnDown != null)
                OnDown(source, point);
        }

        public static event InputHandler OnUp;
        private static void TriggerOnUp(InputSource source, TrackingPoint point)
        {
            if (OnUp != null)
                OnUp(source, point);
        }

        public static event InputHandler OnClick;
        private static void TriggerOnClick(InputSource source, TrackingPoint point)
        {
            if (OnClick != null)
                OnClick(source, point);
        }

        public static event InputHandler OnMove;
        private static void TriggerOnMove(InputSource source, TrackingPoint point)
        {
            if (OnMove != null)
                OnMove(source, point);
        }
    }
	
	
}

