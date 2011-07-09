using System;
using osum.GameModes;
using OpenTK;
using osum.Graphics.Drawables;
using OpenTK.Graphics;
using osum.Helpers;
using osum.GameModes.SongSelect;

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

        pRectangle scrollbar = new pRectangle(new Vector2(5, 0), new Vector2(4, 0), true, 1, new Color4(255, 255, 255, 255))
        {
            Field = FieldTypes.StandardSnapRight,
            Origin = OriginTypes.TopRight
        };

        public SpriteManagerDraggable()
        {
            CheckSpritesAreOnScreenBeforeRendering = true;
            scrollbar.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.ModeTime + 800, Clock.ModeTime + 1400));
            nonDraggableManager.Add(scrollbar);
        }

        public override void Dispose()
        {
            nonDraggableManager.Dispose();
            base.Dispose();
        }

        internal override void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            base.HandleInputManagerOnMove(source, trackingPoint);

            if (movedX > 10 && movedY < 30)
                return;

            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null || InputManager.PrimaryTrackingPoint.HoveringObject is BackButton)
                return;

            float change = trackingPoint.WindowDelta.Y;

            movedY += Math.Abs(change);
            movedX += Math.Abs(trackingPoint.WindowDelta.X);

            if (movedY < 10)
                return;

            if (scrollbar.Alpha != 1)
                scrollbar.FadeIn(200);

            verticalDragOffset += change;
            velocity = change;
        }

        float movedX = 0;
        float movedY = 0;

        internal override void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            base.HandleInputManagerOnDown(source, trackingPoint);
        }

        internal override void HandleInputManagerOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            movedX = 0;
            movedY = 0;

            scrollbar.Transformations.Clear();
            scrollbar.Transform(new Transformation(TransformationType.Fade, scrollbar.Alpha, 0, Clock.ModeTime + 800, Clock.ModeTime + 1000));

            base.HandleInputManagerOnUp(source, trackingPoint);
        }

        private float verticalDragOffset;
        private float offset_min = 0;
        private float offset_max = 0;
        private float velocity;

        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min - EndBufferZone, verticalDragOffset));
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
                float newOffset = -sprite.DisplayRectangle.Bottom + GameBase.BaseSizeFixedWidth.Height;
                if (newOffset < offset_min)
                {
                    offset_min = newOffset;
                    scrollbar.Scale.Y = (float)GameBase.BaseSizeFixedWidth.Height / (-offset_min + GameBase.BaseSizeFixedWidth.Height) * GameBase.BaseSizeFixedWidth.Height;
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
                verticalDragOffset = verticalDragOffset * 0.8f + bound * 0.2f + velocity;

                if (verticalDragOffset != bound)
                    velocity *= 0.7f;
                else
                    velocity *= 0.94f;
            }

            hasMovement = lastFrameOffset != verticalDragOffset;
            lastFrameOffset = verticalDragOffset;

            float stopLenience = EndStopLenience * 0.2f;
            float scaledBackOffset = bound * (1 - stopLenience) + verticalDragOffset * stopLenience;

            //change *= Math.Min(1, EndExcess / Math.Max(0.1f, Math.Abs(songSelectOffset - bound)));

            if (Director.PendingOsuMode == OsuMode.Unknown)
                Offset.Y = scaledBackOffset;

            scrollbar.Position.Y = Offset.Y / (offset_min - GameBase.BaseSizeFixedWidth.Height) * GameBase.BaseSizeFixedWidth.Height;
        }
    }
}

