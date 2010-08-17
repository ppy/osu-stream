using System;
using System.Drawing;
using System.Collections.Generic;
namespace osum
{
	public class InputSource
	{
		public List<TrackingPoint> trackingPoints = new List<TrackingPoint>();
		
		public InputSource()
		{
			
		}

        public bool IsPressed { get { return PressedCount > 0; } }
        public int PressedCount;
		
		public event InputHandler OnDown;
		protected void TriggerOnDown()
		{
            PressedCount++;

            if (OnDown != null)
				OnDown(this);
		}
		
		public event InputHandler OnUp;
		protected void TriggerOnUp()
		{
            PressedCount--;

            if (OnUp != null)
				OnUp(this);
		}
		
		public event InputHandler OnClick;
		protected void TriggerOnClick()
		{
			if (OnClick != null)
				OnClick(this);
		}
		
		public event InputHandler OnMove;
		protected void TriggerOnMove()
		{
			if (OnMove != null)
				OnMove(this);
		}
	}
}

