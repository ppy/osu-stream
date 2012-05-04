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

        internal bool Scrollbar = true;
        internal bool AutomaticHeight = true;
        internal bool LockHorizontal = true;

        internal float EndStopLenience = 2;

        /// <summary>
        /// How much extra space at the start of all items to allow for headers.
        /// </summary>
        internal float StartBufferZone
        {
            get { return Position.Y; }
            set { Position.Y = value; }
        }

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
            scrollbar.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 800, Clock.ModeTime + 1400));
            nonDraggableManager.Add(scrollbar);
        }

        public override void Dispose()
        {
            nonDraggableManager.Dispose();
            base.Dispose();
        }

        internal override void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (Alpha == 0) return;

            base.HandleInputManagerOnMove(source, trackingPoint);

            if (LockHorizontal && movedX > 10 && movedY < 20)
                return;

            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null || InputManager.PrimaryTrackingPoint.HoveringObject is BackButton)
                return;

            float change = trackingPoint.WindowDelta.Y;

            movedY += Math.Abs(change);
            movedX += Math.Abs(trackingPoint.WindowDelta.X);

            ShowScrollbar();

            verticalDragOffset += change;
            velocity = change;
        }

        float movedX = 0;
        float movedY = 0;

        internal override void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            if (Alpha == 0) return;

            base.HandleInputManagerOnDown(source, trackingPoint);
        }

        internal override void HandleInputManagerOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            if (Alpha == 0) return;

            movedX = 0;
            movedY = 0;

            HideScrollbar();

            base.HandleInputManagerOnUp(source, trackingPoint);
        }

        internal void ShowScrollbar(bool pulse = false)
        {
            scrollbar.Transformations.Clear();
            if (scrollbar.Alpha != 1)
                scrollbar.FadeIn(200);
            if (pulse)
                HideScrollbar();
        }

        internal void HideScrollbar()
        {
            scrollbar.Transform(new TransformationF(TransformationType.Fade, scrollbar.Alpha, 0, Clock.ModeTime + 800, Clock.ModeTime + 1000));
        }

        internal float ScrollPosition { get { return verticalDragOffset; } }

        internal float ScrollPercentage { get { return (float)verticalDragOffset / offset_min; } }

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
                return Math.Min(offset_max + 1, Math.Max(offset_min - EndBufferZone, verticalDragOffset));
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
                sprite.Update();
                float newOffset = sprite.DisplayRectangle.Bottom;
                SetMaxHeight(newOffset);
            }
        }

        internal void SetMaxHeight(float newOffset)
        {
            offset_min = Math.Min(offset_min, -newOffset / GameBase.InputToFixedWidthAlign + ActualHeight);
            scrollbar.Scale.Y = (float)ActualHeight / (-offset_min + ActualHeight) * ActualHeight;
        }

        public override bool Draw()
        {
            base.Draw();
            nonDraggableManager.Draw();
            return true;
        }

        int ActualHeight
        {
            get
            {
                return (int)(GameBase.BaseSizeFixedWidth.Height - Position.Y);
            }
        }

        float lastFrameOffset;
        public override void Update()
        {
            float bound = offsetBound;

            if (aimOffset != null)
            {
                if (Math.Abs(aimOffset.Value - verticalDragOffset) < 2)
                    aimOffset = null;
                else
                    verticalDragOffset = verticalDragOffset * 0.8f + aimOffset.Value * 0.2f;
            }

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

            scrollbar.Position.Y = Position.Y + Offset.Y / (offset_min - ActualHeight) * ActualHeight;

            base.Update();
            nonDraggableManager.Update();
        }

        float? aimOffset;
        internal void ScrollTo(pDrawable sprite, float padding = 0)
        {
            ScrollTo(-(sprite.Position.Y - 50 - padding));
        }

        internal void ScrollTo(float scrollPoint)
        {
            aimOffset = Math.Max(offset_min, Math.Min(0, scrollPoint));
            ShowScrollbar(true); //pulse scrollbar display.
        }
    }
}

