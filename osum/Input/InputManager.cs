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

        /// <summary>
        /// Last standard window position of cursor.
        /// </summary>
        public static Vector2 MainPointerPosition;

        /// <summary>
        /// Active tracking point (first pressed). When more than one touches are present, will take the oldest still-valid touch.
        /// When using to track movement, check changes in reference to avoid sudden jumps between tracking points.
        /// </summary>
        public static TrackingPoint PrimaryTrackingPoint;

        public static List<TrackingPoint> TrackingPoints = new List<TrackingPoint>();

        public static bool IsTracking
        {
            get
            {
                if (RegisteredSources.Count == 0) return false;

                return RegisteredSources[0].trackingPoints.Count > 0;
            }
        }

        public static void Initialize()
        {

        }

        public static bool IsPressed
        {
            get
            {
                return RegisteredSources[0].IsPressed;
            }
        }

        #region Incoming Events

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

        internal static void Update()
        {
        }

        private static void UpdatePointerPosition(TrackingPoint point)
        {
            if (PrimaryTrackingPoint == point)
                MainPointerPosition = point.WindowPosition;

            TrackingPoints.Clear();
            foreach (InputSource source in RegisteredSources)
                TrackingPoints.AddRange(source.trackingPoints);
        }

        private static void ReceiveDown(InputSource source, TrackingPoint point)
        {
            //if (PrimaryTrackingPoint == null)
                PrimaryTrackingPoint = point;

            UpdatePointerPosition(point);
            TriggerOnDown(source, point);
        }

        private static void ReceiveUp(InputSource source, TrackingPoint point)
        {
            if (PrimaryTrackingPoint == point)
            {
                //find the next valid tracking point.
                PrimaryTrackingPoint = null;
                foreach (TrackingPoint p in TrackingPoints)
                {
                    if (p != point && p.Valid)
                    {
                        PrimaryTrackingPoint = p;
                        break;
                    }
                }
            }
			
            TriggerOnUp(source, point);
			UpdatePointerPosition(point);
        }

        private static void ReceiveClick(InputSource source, TrackingPoint point)
        {
            TriggerOnClick(source, point);
            UpdatePointerPosition(point);
        }

        private static void ReceiveMove(InputSource source, TrackingPoint point)
        {
			TriggerOnMove(source, point);
			UpdatePointerPosition(point);
        }

        #endregion

        #region Outgoing Events

        public static event InputHandler OnDown;
        private static void TriggerOnDown(InputSource source, TrackingPoint point)
        {
            point.IncreaseValidity();

            if (OnDown != null)
                OnDown(source, point);
        }

        public static event InputHandler OnUp;
        private static void TriggerOnUp(InputSource source, TrackingPoint point)
        {
            //tracking is no longer valid.
            point.DecreaseValidity();

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

        #endregion
    }


}

