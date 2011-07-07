using System;
using osum.GameModes;
using OpenTK;
using osum.Graphics.Drawables;
using OpenTK.Graphics;
using osum.Helpers;

namespace osum.Graphics.Sprites
{
    public class SpriteManagerDraggable : SpriteManager
    {
        SpriteManager nonDraggableManager = new SpriteManager();

        internal bool ShowScrollbar = true;
        internal bool AutomaticHeight = true;

        internal float EndStopLenience = 2;

        /// <summary>
        /// How much extra space at the end of all items to allow for the back button.
        /// </summary>
        internal float EndBufferZone = 60;

        pRectangle scrollbar = new pRectangle (new Vector2 (5,0), new Vector2 (4,0), true, 1, new Color4 (255,255,255,200))
        {
            Field = FieldTypes.StandardSnapRight,
            Origin = OriginTypes.TopRight
        };

        public SpriteManagerDraggable()
        {
            scrollbar.Transform(new Transformation (TransformationType.Fade, 1, 0, Clock.ModeTime + 800, Clock.ModeTime + 1400));
            nonDraggableManager.Add(scrollbar);
        }

        public override void Dispose ()
        {
            nonDraggableManager.Dispose();
            base.Dispose ();
        }

        internal override void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null)
                return;

            float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;

            songSelectOffset += change;
            velocity = change;


            base.HandleInputManagerOnMove(source, trackingPoint);
        }

        internal override void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            scrollbar.FadeIn(200);
            base.HandleInputManagerOnDown(source, trackingPoint);
        }

        internal override void HandleInputManagerOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            scrollbar.Transform(new Transformation (TransformationType.Fade, 1, 0, Clock.ModeTime + 800, Clock.ModeTime + 1000));
            base.HandleInputManagerOnUp(source, trackingPoint);
        }

        private float songSelectOffset;
        private float offset_min = 0;
        private float offset_max = 0;
        private float velocity;

        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min - EndBufferZone, songSelectOffset));
            }
        }

        internal void AddNonDraggable(pDrawable sprite)
        {
            nonDraggableManager.Add(sprite);
        }

        internal override void Add(pDrawable sprite)
        {
            base.Add(sprite);

            if (AutomaticHeight)
            {
                float newOffset = -sprite.DisplayRectangle.Bottom + GameBase.BaseSize.Height;
                if (newOffset < offset_min)
                {
                    offset_min = newOffset;
                    scrollbar.Scale.Y = (float)GameBase.BaseSize.Height / (-offset_min + GameBase.BaseSize.Height) * GameBase.BaseSize.Height;
                }
            }
        }

        public override bool Draw()
        {
            base.Draw();
            nonDraggableManager.Draw();
            return true;
        }


        float lastFrameOffset;
        public override void Update()
        {
            base.Update();

            nonDraggableManager.Update();

            float bound = offsetBound;

            if (!InputManager.IsPressed)
            {
                songSelectOffset = songSelectOffset * 0.8f + bound * 0.2f + velocity;

                if (songSelectOffset != bound)
                    velocity *= 0.7f;
                else
                    velocity *= 0.94f;
            }

            hasMovement = lastFrameOffset != songSelectOffset;
            lastFrameOffset = songSelectOffset;

            float stopLenience = EndStopLenience * 0.2f;
            float scaledBackOffset = bound * (1 - stopLenience) + songSelectOffset * stopLenience;

            //change *= Math.Min(1, EndExcess / Math.Max(0.1f, Math.Abs(songSelectOffset - bound)));

            if (Director.PendingOsuMode == OsuMode.Unknown)
                Offset.Y = scaledBackOffset;

            scrollbar.Position.Y = Offset.Y / (offset_min - GameBase.BaseSize.Height) * GameBase.BaseSize.Height;
        }
    }
}

