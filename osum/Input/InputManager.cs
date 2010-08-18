using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
namespace osum
{
    public delegate void InputHandler(InputSource source);
	
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
		
		private static void ReceiveDown(InputSource source)
		{
			Console.WriteLine("input: down");
			MainPointerPosition = source.trackingPoints[0].GamePosition;

            TriggerOnDown(source);
		}
        		
		private static void ReceiveUp(InputSource source)
		{
			Console.WriteLine("input: up");
            TriggerOnUp(source);
		}
		
		private static void ReceiveClick(InputSource source)
		{
			Console.WriteLine("input: click");
            TriggerOnClick(source);
		}
		
		private static void ReceiveMove(InputSource source)
		{
			Console.WriteLine("input: move");
			MainPointerPosition = source.trackingPoints[0].GamePosition;
            TriggerOnMove(source);
		}

        public static bool IsPressed
        {
            get
            {
                return RegisteredSources[0].IsPressed;
            }
        }

        public static event InputHandler OnDown;
        private static void TriggerOnDown(InputSource source)
        {
            if (OnDown != null)
                OnDown(source);
        }

        public static event InputHandler OnUp;
        private static void TriggerOnUp(InputSource source)
        {
            if (OnUp != null)
                OnUp(source);
        }

        public static event InputHandler OnClick;
        private static void TriggerOnClick(InputSource source)
        {
            if (OnClick != null)
                OnClick(source);
        }

        public static event InputHandler OnMove;
        private static void TriggerOnMove(InputSource source)
        {
            if (OnMove != null)
                OnMove(source);
        }
    }
	
	
}

