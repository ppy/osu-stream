using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.Audio;
using osum.GameplayElements;
using osum.Graphics.Primitives;

namespace osum.GameModes.SongSelect
{
    class BackButton : pSprite
    {
        pAnimation arrow;

        EventHandler Action;
        const float offset = 0;

        SpriteManager sm = new SpriteManager();

        static Vector2 hiddenPosition = new Vector2(-80, -218);
        static Vector2 visiblePosition { get { return positionAtDistance(10); } }
        static Vector2 fullyVisiblePosition { get { return positionAtDistance(120); } }

        static Vector2 positionAtDistance(float distance)
        {
            return hiddenPosition + new Vector2(distance + 10, distance + 10);
        }

        public BackButton(EventHandler action, bool showIntroAnimation)
            : base(TextureManager.Load(OsuTexture.backbutton), FieldTypes.StandardSnapBottomLeft,
                OriginTypes.BottomLeft, ClockTypes.Mode, fullyVisiblePosition, 0.99f, true, Color4.White)
        {
            AlwaysDraw = true;
            Alpha = 1;
            Action = action;
            HandleInput = true;

            HandleClickOnUp = false;

            if (showIntroAnimation)
            {
                Transform(new TransformationV(positionAtDistance(50), fullyVisiblePosition, Clock.ModeTime, Clock.ModeTime + 150, EasingTypes.In));
                Transform(new TransformationV(fullyVisiblePosition, hiddenPosition, Clock.ModeTime + 150, Clock.ModeTime + 400, EasingTypes.Out));
            }
            else
                Position = hiddenPosition;

            OnClick += OnBackgroundOnClick;
            OnHover += delegate
            {
                MoveTo(visiblePosition, 200, EasingTypes.InOut);
                FadeColour(Color4.Gray, 200);
                dist = 0;
            };

            OnHoverLost += delegate
            {
                if (tp == null)
                {
                    MoveTo(hiddenPosition, 200, EasingTypes.InDouble);
                    FadeColour(Color4.White, 200);
                }
            };
            arrow = new pAnimation(TextureManager.LoadAnimation(OsuTexture.backbutton_arrows1, 2), FieldTypes.StandardSnapBottomLeft, OriginTypes.Custom, ClockTypes.Mode, new Vector2(offset + 15, offset + 18), 1, true, Color4.White);
            arrow.FrameDelay = 500;
            arrow.Offset = new Vector2(-330, 190);
            sm.Add(arrow);

            Rotation = -MathHelper.Pi / 4;

        }

        const int hit_minimum_distance = 20;
        const int hit_pull_distance = 60;
        const int pull_limit_distance = 100;
        bool minimumHitPossible;


        internal override void HandleOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            base.HandleOnUp(source, trackingPoint);

            if (tp != null)
            {
                bool success = (minimumHitPossible && dist < hit_minimum_distance) || (dist > hit_pull_distance && dist < pull_limit_distance);

                Transformations.Clear();

                if (success)
                {

                    RotateTo(defaultRotation, 200, EasingTypes.In);
                    Transform(new TransformationV(Position, fullyVisiblePosition, Clock.ModeTime, Clock.ModeTime + 150, EasingTypes.In));
                    Transform(new TransformationV(fullyVisiblePosition, hiddenPosition, Clock.ModeTime + 150, Clock.ModeTime + 400, EasingTypes.Out));

                    FadeColour(Color4.White, 0);
                    AudioEngine.PlaySample(OsuSamples.MenuBack);
                    Action(this, null);
                }
                else
                {
                    MoveTo(hiddenPosition, 200, EasingTypes.InDouble);
                    RotateTo(defaultRotation, 200, EasingTypes.In);
                    FadeColour(Color4.White, 200);
                }

                tp = null;
            }
        }


        internal override void HandleOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            base.HandleOnMove(source, trackingPoint);

            if (tp != null)
            {
                Line l = new Line(downPoint, trackingPoint.BasePosition);

                //don't allow dragging "backwards"
                if (l.p1.X > l.p2.X && l.p1.Y < l.p2.Y)
                    l.p2 = l.p1;

                dist = 6f * (float)Math.Pow(Math.Abs(l.p2.X - l.p1.X) / 2 + Math.Abs(l.p2.Y - l.p1.Y) / 2, 0.5f);

                Vector2 subd = trackingPoint.BasePosition - new Vector2(hiddenPosition.X, GameBase.BaseSizeFixedWidth.Height - hiddenPosition.Y);

                if (dist > hit_pull_distance)
                    FadeColour(Color4.White, 100);
                else
                    FadeColour(Color4.Gray, 100);

                if (dist > hit_minimum_distance)
                    minimumHitPossible = false;

                dist = Math.Min(pull_limit_distance, dist);

                /*if (dist > pull_limit_distance)
                {
                    RotateTo(defaultRotation, 100, EasingTypes.In);
                    MoveTo(hiddenPosition, 100, EasingTypes.In);
                }
                else*/
                {
                    float angle = (float)Math.Atan2(subd.X, subd.Y);
                    RotateTo(defaultRotation + (-angle + MathHelper.Pi * 0.854f) / 2, 50, EasingTypes.In);
                    MoveTo(visiblePosition + new Vector2(dist, dist), 200, EasingTypes.In);
                }
            }
        }

        Vector2 downPoint;
        private TrackingPoint tp;
        private float dist;
        private float defaultRotation = -MathHelper.Pi / 4;

        protected override bool checkHover(Vector2 position)
        {
            if (Alpha == 0) return false;

            return position.X < 100 && position.Y > GameBase.BaseSizeFixedWidth.Height - 100 || tp != null;
        }

        void OnBackgroundOnClick(object sender, EventArgs e)
        {
            tp = InputManager.TrackingPoints.Find(t => t.HoveringObject == this);
            if (tp != null)
            {
                downPoint = tp.BasePosition;
                minimumHitPossible = true;
            }
        }

        public override void Dispose()
        {
            sm.Dispose();
            base.Dispose();
        }

        public override bool Draw()
        {
            if (!base.Draw())
                return false;

            sm.Draw();

            return true;
        }

        internal override bool IsOnScreen
        {
            get
            {
                return true;
            }
        }

        public override void Update()
        {
            base.Update();

            arrow.Alpha = Alpha;
            arrow.Position = Position;
            arrow.Rotation = Rotation;
            arrow.Colour = Colour;

            sm.Update();
        }
    }
}
