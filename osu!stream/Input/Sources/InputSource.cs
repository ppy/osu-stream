using System.Collections.Generic;
using osum.Helpers;

namespace osum.Input.Sources
{
    public class InputSource
    {
        public List<TrackingPoint> trackingPoints = new List<TrackingPoint>();

        public bool IsPressed => PressedCount > 0;
        public int PressedCount;

        public event InputHandler OnDown;

        protected void TriggerOnDown(TrackingPoint trackingPoint)
        {
            PressedCount++;
            trackingPoints.Add(trackingPoint);

            trackingPoint.HoveringObjectConfirmed = false;

            GameBase.Scheduler.Add(delegate
            {
                OnDown?.Invoke(this, trackingPoint);
            });

            if (!trackingPoint.HoveringObjectConfirmed)
                trackingPoint.HoveringObject = null;
        }

        public event InputHandler OnUp;

        protected void TriggerOnUp(TrackingPoint trackingPoint)
        {
            PressedCount--;
            trackingPoints.Remove(trackingPoint);

            GameBase.Scheduler.Add(delegate
            {
                OnUp?.Invoke(this, trackingPoint);
            });

            if (!trackingPoint.HoveringObjectConfirmed)
                trackingPoint.HoveringObject = null;
        }

        public event InputHandler OnClick;

        protected void TriggerOnClick(TrackingPoint trackingPoint)
        {
            trackingPoint.HoveringObjectConfirmed = false;

            GameBase.Scheduler.Add(delegate
            {
                OnClick?.Invoke(this, trackingPoint);
            });

            if (!trackingPoint.HoveringObjectConfirmed)
                trackingPoint.HoveringObject = null;
        }

        public event InputHandler OnMove;

        protected void TriggerOnMove(TrackingPoint trackingPoint)
        {
            trackingPoint.HoveringObjectConfirmed = false;

            GameBase.Scheduler.Add(delegate
            {
                OnMove?.Invoke(this, trackingPoint);
            });
        }
    }
}