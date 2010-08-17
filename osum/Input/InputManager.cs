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
    		    
		    source.OnDown += OnDown;
			source.OnUp += OnUp;
			source.OnClick += OnClick;
			source.OnMove += OnMove;
		    
		    RegisteredSources.Add(source);
			
			return true;
	    }
		
		private static void OnDown(InputSource source)
		{
			Console.WriteLine("input: down");
			MainPointerPosition = source.trackingPoints[0].GamePosition;
		}
		
		private static void OnUp(InputSource source)
		{
			Console.WriteLine("input: up");
		}
		
		private static void OnClick(InputSource source)
		{
			Console.WriteLine("input: click");
		}
		
		private static void OnMove(InputSource source)
		{
			Console.WriteLine("input: move");
			MainPointerPosition = source.trackingPoints[0].GamePosition;
		}

        public static bool IsPressed
        {
            get
            {
                return RegisteredSources[0].IsPressed;
            }
        }
    }
	
	
}

