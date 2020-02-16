using System.Collections.Generic;
using OpenTK;
using osum.Helpers;
using osum.Input.Sources;

namespace osum.Input
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
            //source.OnClick += ReceiveClick;
            source.OnMove += ReceiveMove;

            RegisteredSources.Add(source);

            return true;
        }

        internal static void Update()
        {
#if FULLER_DEBUG
            DebugOverlay.AddLine("Cursor Position: " + MainPointerPosition + " in window: " + GameBase.BaseSizeFixedWidth);
#endif
        }

        private static void ReceiveDown(InputSource source, TrackingPoint point)
        {
            //if (PrimaryTrackingPoint == null)
            PrimaryTrackingPoint = point;
            MainPointerPosition = point.BasePosition;
            TrackingPoints.Add(point);
            TriggerOnDown(source, point);

            /*pDrawable d = osum.Graphics.Sprites.pSprite.FullscreenWhitePixel;
            d.FadeOutFromOne(200);
            GameBase.MainSpriteManager.Add(d);
            AudioEngine.PlaySample(OsuSamples.HitClap,SampleSet.Soft);*/
        }

        private static void ReceiveUp(InputSource source, TrackingPoint point)
        {
            TrackingPoints.Remove(point);

            if (PrimaryTrackingPoint == point)
            {
                PrimaryTrackingPoint = null;

                //find the next valid tracking point.
                foreach (TrackingPoint p in TrackingPoints)
                {
                    if (p != point && p.Valid)
                    {
                        PrimaryTrackingPoint = p;
                        MainPointerPosition = point.BasePosition;
                        break;
                    }
                }
            }

            TriggerOnUp(source, point);
        }

        private static void ReceiveMove(InputSource source, TrackingPoint point)
        {
#if MONO
            if (PrimaryTrackingPoint == null)
                PrimaryTrackingPoint = point;
#endif
            if (PrimaryTrackingPoint == point)
                MainPointerPosition = point.BasePosition;
            TriggerOnMove(source, point);
        }

        #endregion

        #region Outgoing Events

        public static event InputHandler OnDown;
        private static void TriggerOnDown(InputSource source, TrackingPoint point)
        {
            point.IncreaseValidity();

            if (OnDown != null && !GameBase.GloballyDisableInput)
            {
                if (GameBase.ActiveNotification != null)
                    GameBase.ActiveNotification.HandleOnDown(source, point);
                else
                    OnDown(source, point);
            }
        }

        public static event InputHandler OnUp;
        private static void TriggerOnUp(InputSource source, TrackingPoint point)
        {
            //tracking is no longer valid.
            point.DecreaseValidity();

            if (OnUp != null && !GameBase.GloballyDisableInput)
            {
                if (GameBase.ActiveNotification != null)
                    GameBase.ActiveNotification.HandleOnUp(source, point);
                else
                    OnUp(source, point);
            }
        }

        public static event InputHandler OnMove;
        private static void TriggerOnMove(InputSource source, TrackingPoint point)
        {
            if (OnMove != null && !GameBase.GloballyDisableInput)
            {
                if (GameBase.ActiveNotification != null)
                    GameBase.ActiveNotification.HandleOnMove(source, point);
                else
                    OnMove(source, point);
            }
        }

        #endregion
    }


}

