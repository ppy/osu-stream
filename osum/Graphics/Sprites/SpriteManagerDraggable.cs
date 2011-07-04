using System;
using osum.GameModes;
using OpenTK;

namespace osum.Graphics.Sprites
{
    public class SpriteManagerDraggable : SpriteManager
    {
        internal float EndExcess = 10;

        internal override void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null) return;

            float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
            float bound = offsetBound;

            if ((songSelectOffset - bound < 0 && change < 0) || (songSelectOffset - bound > 0 && change > 0))
                change *= Math.Min(1, EndExcess / Math.Max(0.1f, Math.Abs(songSelectOffset - bound)));
            songSelectOffset = songSelectOffset + change;
            velocity = change;


            base.HandleInputManagerOnMove (source, trackingPoint);
        }

        private float songSelectOffset;

        private float offset_min { get { return 0; } }
        private float offset_max = 0;
        private float velocity;

        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min, songSelectOffset));
            }
        }

        public override void Update()
        {
            base.Update();

            if (!InputManager.IsPressed)
            {
                float bound = offsetBound;

                float lastOffset = songSelectOffset;
                songSelectOffset = songSelectOffset * 0.8f + bound * 0.2f + velocity;

                if (songSelectOffset != lastOffset)
                    hasMovement = true;

                if (songSelectOffset != bound)
                    velocity *= 0.7f;
                else
                    velocity *= 0.94f;
            }
            else
            {
                hasMovement = true;
            }

            if (Director.PendingOsuMode == OsuMode.Unknown)
                Offset.Y = Offset.Y * 0.8f + songSelectOffset * 0.2f;
        }
    }
}

