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
        pSprite arrow;

        SpriteManager sm = new SpriteManager();

        EventHandler Action;
        const float offset = 0;

        int colourIndex;
        private double elapsedRotation;

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
                Transform(new Transformation(positionAtDistance(50), fullyVisiblePosition, Clock.ModeTime, Clock.ModeTime + 150, EasingTypes.In));
                Transform(new Transformation(fullyVisiblePosition, hiddenPosition, Clock.ModeTime + 150, Clock.ModeTime + 400, EasingTypes.Out));
            }
            else
                Position = hiddenPosition;

            OnClick += OnBackgroundOnClick;
            OnHover += delegate
            {
                MoveTo(visiblePosition, 200, EasingTypes.InOut);
                dist = 0;
            };

            OnHoverLost += delegate
            {
                if (tp == null)
                    MoveTo(hiddenPosition, 200, EasingTypes.InDouble);
            };
            arrow = new pSprite(TextureManager.Load(OsuTexture.songselect_back_arrow), FieldTypes.StandardSnapBottomLeft, OriginTypes.Centre, ClockTypes.Mode, new Vector2(offset + 15, offset + 18), 1, true, Color4.White);

            InputManager.OnMove += new InputHandler(InputManager_OnMove);
            InputManager.OnUp += new InputHandler(InputManager_OnUp);

            Rotation = -(float)Math.PI / 4;

        }

        const int hit_minimum_distance = 20;
        const int hit_pull_distance = 70;
        const int pull_limit_distance = 110;
        bool minimumHitPossible;

        void InputManager_OnUp(InputSource source, TrackingPoint trackingPoint)
        {
            if (tp != null)
            {
                bool success = (minimumHitPossible && dist < hit_minimum_distance) || (dist > hit_pull_distance && dist < pull_limit_distance);

                Transformations.Clear();

                if (success)
                {

                    RotateTo(defaultRotation, 200, EasingTypes.In);
                    Transform(new Transformation(Position, fullyVisiblePosition, Clock.ModeTime, Clock.ModeTime + 150, EasingTypes.In));
                    Transform(new Transformation(fullyVisiblePosition, hiddenPosition, Clock.ModeTime + 150, Clock.ModeTime + 400, EasingTypes.Out));

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

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (tp != null)
            {
                Line l = new Line(downPoint, trackingPoint.BasePosition);

                //don't allow dragging "backwards"
                if (l.p1.X > l.p2.X && l.p1.Y < l.p2.Y)
                    l.p2 = l.p1;

                dist = 6f * (float)Math.Pow(Math.Abs(l.p2.X - l.p1.X) / 2 + Math.Abs(l.p2.Y - l.p1.Y) / 2, 0.49f);

                Vector2 subd = trackingPoint.BasePosition - new Vector2(hiddenPosition.X, GameBase.BaseSizeFixedWidth.Height - hiddenPosition.Y);

                if (dist > hit_pull_distance)
                    FadeColour(Color4.White, 100);
                else
                    FadeColour(Color4.Gray, 100);

                if (dist > hit_minimum_distance)
                    minimumHitPossible = false;

                if (dist > pull_limit_distance)
                {
                    RotateTo(defaultRotation, 200, EasingTypes.In);
                    MoveTo(hiddenPosition, 200, EasingTypes.In);
                }
                else
                {
                    float angle = (float)Math.Atan2(subd.X, subd.Y);
                    RotateTo(defaultRotation + (-angle + (float)Math.PI * 0.854f) / 2, 50, EasingTypes.In);
                    MoveTo(visiblePosition + new Vector2(dist, dist), 200, EasingTypes.In);
                }
            }
        }

        Vector2 downPoint;
        private TrackingPoint tp;
        private float dist;
        private float defaultRotation = -(float)Math.PI / 4;

        protected override bool checkHover(Vector2 position)
        {
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
            InputManager.OnMove -= InputManager_OnMove;
            InputManager.OnUp -= InputManager_OnUp;

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

            elapsedRotation += GameBase.ElapsedMilliseconds;

            //arrow.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * GameBase.ElapsedMilliseconds);

            //if (Transformations.Count == 0 && !IsHovering)
            //{
            //    colourIndex = (colourIndex + 1) % TextureManager.DefaultColours.Length;
            //    FadeColour(TextureManager.DefaultColours[colourIndex],10000);
            //}

            arrow.Alpha = this.Alpha;

            sm.Update();

            //Rotation += (float)GameBase.ElapsedMilliseconds * 0.0005f;
        }
    }
}
