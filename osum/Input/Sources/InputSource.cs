using System;
using System.Drawing;
using System.Collections.Generic;
using osum.Helpers;
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
        protected void TriggerOnDown(TrackingPoint trackingPoint)
        {
            PressedCount++;

            trackingPoint.HoveringObjectConfirmed = false;

            if (OnDown != null)
                OnDown(this, trackingPoint);
        }

        public event InputHandler OnUp;
        protected void TriggerOnUp(TrackingPoint trackingPoint)
        {
            PressedCount--;

            if (OnUp != null)
                OnUp(this, trackingPoint);
        }

        public event InputHandler OnClick;
        protected void TriggerOnClick(TrackingPoint trackingPoint)
        {
            trackingPoint.HoveringObjectConfirmed = false;

            if (OnClick != null)
                OnClick(this, trackingPoint);
        }

        public event InputHandler OnMove;
        protected void TriggerOnMove(TrackingPoint trackingPoint)
        {
            trackingPoint.HoveringObjectConfirmed = false;

            if (OnMove != null)
                OnMove(this, trackingPoint);
        }
    }
}

