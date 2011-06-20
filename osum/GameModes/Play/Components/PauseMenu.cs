using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;

namespace osum.GameModes.Play.Components
{
    class PauseMenu : GameComponent
    {

        private bool menuDisplayed;
        internal bool MenuDisplayed
        {
            get
            {
                return menuDisplayed;
            }

            set
            {
                menuDisplayed = value;

                if (menuDisplayed)
                {
                    Transformation move = new Transformation(TransformationType.MovementY, background.Position.Y, 0, Clock.ModeTime, Clock.ModeTime + 200);
                    Transformation fade = new Transformation(TransformationType.Fade, background.Alpha, 1, Clock.ModeTime, Clock.ModeTime + 200);

                    spriteManager.Sprites.ForEach(s =>
                    {
                        s.Transform(move);
                        s.Transform(fade);
                    });

                    pause();
                }
                else
                {
                    Transformation move = new Transformation(TransformationType.MovementY, background.Position.Y, offscreen_y, Clock.ModeTime, Clock.ModeTime + 200);
                    Transformation fade = new Transformation(TransformationType.Fade, background.Alpha, 0.4f, Clock.ModeTime, Clock.ModeTime + 200);

                    spriteManager.Sprites.ForEach(s =>
                    {
                        s.Transform(move);
                        s.Transform(fade);
                    });

                    Player p = Director.CurrentMode as Player;
                    if (p != null)
                        p.CountdownResume(Clock.AudioTime, 8);
                }
            }
        }

        internal bool Failed;

        internal void ShowFailMenu()
        {
            MenuDisplayed = true;
            Failed = true;

            buttonContinue.Transformations.Clear();
            buttonContinue.Alpha = 0;
            buttonContinue.AlwaysDraw = false;
        }

        public void ShowMenu()
        {
            MenuDisplayed = true;
        }

        private pSprite buttonContinue;
        private pSprite buttonRestart;
        private pSprite buttonQuit;

        TrackingPoint validPoint;
        private float validPointOffset;

        private pSprite background;

        const float offscreen_y = -160;
        private Color4 colourInactive = new Color4(200, 200, 200, 255);

        public override void Initialize()
        {
            base.Initialize();

            background = new pSprite(TextureManager.Load(OsuTexture.play_menu_background), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, Vector2.Zero, 0.8f, true, Color4.White);
            background.OnClick += Background_OnClick;
            spriteManager.Add(background);

            pSprite pullnotice = new pSprite(TextureManager.Load(OsuTexture.play_menu_pull), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, Vector2.Zero, 0.9f, false, Color4.White);
            pullnotice.Offset = new Vector2(0, 30);
            spriteManager.Add(pullnotice);

            Transformation move = new Transformation(TransformationType.MovementY, 0f, offscreen_y, 1000, 1500, EasingTypes.Out);
            Transformation fade = new Transformation(TransformationType.Fade, 1, 0.4f, 1000, 1500);

            spriteManager.Sprites.ForEach(s =>
            {
                s.Transform(move);
                s.Transform(fade);
            });

            buttonContinue = new pSprite(TextureManager.Load(OsuTexture.play_menu_continue), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(-210, 0) };
            buttonContinue.OnClick += ButtonContinue_OnClick;
            buttonContinue.OnHover += HandleButtonHover;
            buttonContinue.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonContinue);

            buttonRestart = new pSprite(TextureManager.Load(OsuTexture.play_menu_restart), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(0, 0) };
            buttonRestart.OnClick += ButtonRestart_OnClick;
            buttonRestart.OnHover += HandleButtonHover;
            buttonRestart.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonRestart);

            buttonQuit = new pSprite(TextureManager.Load(OsuTexture.play_menu_quit), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(210, 0) };
            buttonQuit.OnClick += ButtonQuit_OnClick;
            buttonQuit.OnHover += HandleButtonHover;
            buttonQuit.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonQuit);
        }

        void HandleButtonHover(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.FadeColour(Color4.White,100);
        }

        void HandleButtonHoverLost(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.FadeColour(colourInactive, 100);
        }

        void ButtonQuit_OnClick(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.AdditiveFlash(500,1);

            Director.ChangeMode(OsuMode.SongSelect);
            AudioEngine.PlaySample(OsuSamples.MenuBack);
        }

        void ButtonRestart_OnClick(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.AdditiveFlash(500,1);

            Director.ChangeMode(OsuMode.Play);
            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        void ButtonContinue_OnClick(object sender, EventArgs e)
        {
            MenuDisplayed = false;
            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        void Background_OnClick(object sender, EventArgs e)
        {
            if (validPoint == null)
            {
                //todo: this is lazy and wrong.
                validPoint = InputManager.PrimaryTrackingPoint;
                validPointOffset = validPoint.BasePosition.Y;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        internal void handleInput(InputSource source, TrackingPoint trackingPoint)
        {
            if (validPoint != null || MenuDisplayed) return;


            if (trackingPoint.BasePosition.Y < 30)
            {
                validPoint = trackingPoint;
                validPointOffset = validPoint.BasePosition.Y;
            }
        }

        public override void Update()
        {
            if (validPoint != null && Clock.ModeTime > 1600 && !Failed)
            {
                float pulledAmount = Math.Min(1, (validPoint.BasePosition.Y - validPointOffset + (MenuDisplayed ? -offscreen_y : 30)) / -offscreen_y);

                const float valid_pull = 0.7f;

                if (validPoint.Valid)
                {
                    spriteManager.Sprites.ForEach(s =>
                    {
                        s.Position.Y = offscreen_y * (1 - pulledAmount);
                        s.Alpha = 0.4f + 0.6f * (pulledAmount);
                    });

                    if (pulledAmount > valid_pull)
                        pause();
                }
                else
                {
                    MenuDisplayed = pulledAmount >= valid_pull;
                    validPoint = null;
                }
            }

            base.Update();
        }

        private void pause()
        {
            AudioEngine.Music.Pause();
            Clock.AbortLeadIn();

            Player p = Director.CurrentMode as Player;
            if (p != null)
                p.CountdownAbort();
        }
    }
}