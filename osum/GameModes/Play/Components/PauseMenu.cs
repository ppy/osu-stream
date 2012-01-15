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
using osum.Graphics.Renderers;
using osum.Resources;

namespace osum.GameModes.Play.Components
{
    class PauseMenu : GameComponent
    {
        pText menuText;

        private bool menuDisplayed;
        bool isPaused;
        internal bool MenuDisplayed
        {
            get
            {
                return menuDisplayed;
            }

            set
            {
                if (menuDisplayed == value) return;

                menuDisplayed = value;

                Player p = Director.CurrentMode as Player;

                if (menuDisplayed)
                {
                    if (GameBase.Instance != null) GameBase.Instance.DisableDimming = false;

                    Transformation move = new TransformationF(TransformationType.MovementY, background.Position.Y, 0, Clock.ModeTime, Clock.ModeTime + 200);
                    Transformation fade = new TransformationF(TransformationType.Fade, background.Alpha, 1, Clock.ModeTime, Clock.ModeTime + 200);

                    spriteManager.Sprites.ForEach(s =>
                    {
                        s.Transform(move);
                        s.Transform(fade);
                    });

                    if (menuText == null)
                    {
#if DIST
                        menuText = new pText(string.Format(LocalisationManager.GetString(OsuString.PauseInfo), Player.RestartCount, p != null ? Math.Round(p.Progress * 100) : 0, Clock.AudioTime), 24, new Vector2(0,80), 1, true, Color4.White)
#else
                        menuText = new pText(string.Format("{0} restarts\n{1}% completed\ncurrent time: {2}", Player.RestartCount, p != null ? Math.Round(p.Progress * 100) : 0, Clock.AudioTime), 24, new Vector2(0, 80), 1, true, Color4.White)
#endif
                        {
                            TextAlignment = TextAlignment.Centre,
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            TextShadow = true,
                            TagNumeric = -1,
                            Alpha = 0
                        };
    
                        menuText.FadeInFromZero(100);
                        spriteManager.Add(menuText);
                    }

                    if (p != null)
                    {
                        p.Pause();
                        isPaused = true;
                    }
                }
                else
                {
                    if (GameBase.Instance != null) GameBase.Instance.DisableDimming = true;

                    Transformation move = new TransformationF(TransformationType.MovementY, background.Position.Y, offscreen_y, Clock.ModeTime, Clock.ModeTime + 200);
                    Transformation fade = new TransformationF(TransformationType.Fade, background.Alpha, 0.4f, Clock.ModeTime, Clock.ModeTime + 200);

                    if (menuText != null)
                    {
                        menuText.AlwaysDraw = false;
                        menuText.Transformations.Clear();
                        menuText.Alpha = 0;
                        menuText = null;
                    }

                    spriteManager.Sprites.ForEach(s =>
                    {
                        s.Transform(move);
                        s.Transform(fade);
                    });

                    if (p != null && isPaused)
                    {
                        if (AudioEngine.Music != null)
                            AudioEngine.Music.Play();
                        isPaused = false;
                    }
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

        public void Toggle()
        {
            MenuDisplayed = !menuDisplayed;
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
        private pSprite pullnotice;
        internal bool FromBottom;

        public override void Initialize()
        {
            base.Initialize();

            FromBottom = false; // GameBase.Instance.FlipView;

            FieldTypes field = FromBottom ? FieldTypes.StandardSnapBottomCentre : FieldTypes.StandardSnapTopCentre;
            OriginTypes origin = FromBottom ? OriginTypes.BottomCentre : OriginTypes.TopCentre;

            background = new pSprite(TextureManager.Load(OsuTexture.play_menu_background), field, OriginTypes.TopCentre, ClockTypes.Mode, new Vector2(0, offscreen_y), 0.8f, true, Color4.White);
            background.Rotation = FromBottom ? (float)Math.PI : 0;
            spriteManager.Add(background);

            /*if (Director.LastOsuMode != OsuMode.Play)
            {
                pullnotice = new pSprite(TextureManager.Load(OsuTexture.play_menu_pull), field, origin, ClockTypes.Mode, Vector2.Zero, 0.9f, false, Color4.White);
                pullnotice.DrawHeight = 87;

                if (!FromBottom) pullnotice.DrawTop += 26;
                pullnotice.Offset = new Vector2(0, 30);
                spriteManager.Add(pullnotice);

                Transformation move = new TransformationF(TransformationType.MovementY, 0f, offscreen_y, 1000, 1500, EasingTypes.Out);
                Transformation fade = new TransformationF(TransformationType.Fade, 1, 0.4f, 1000, 1500);

                spriteManager.Sprites.ForEach(s =>
                {
                    s.Transform(move);
                    s.Transform(fade);
                });
            }
            else
            {
                background.Position.Y = offscreen_y;
            }*/

            buttonContinue = new pSprite(TextureManager.Load(OsuTexture.play_menu_continue), field, origin, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(-210, 3) };
            buttonContinue.OnClick += ButtonContinue_OnClick;
            buttonContinue.OnHover += HandleButtonHover;
            buttonContinue.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonContinue);

            buttonRestart = new pSprite(TextureManager.Load(OsuTexture.play_menu_restart), field, origin, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(0, 3) };
            buttonRestart.OnClick += ButtonRestart_OnClick;
            buttonRestart.OnHover += HandleButtonHover;
            buttonRestart.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonRestart);

            buttonQuit = new pSprite(TextureManager.Load(OsuTexture.play_menu_quit), field, origin, ClockTypes.Mode, Vector2.Zero, 0.85f, true, colourInactive) { Alpha = 0, Offset = new Vector2(210, 3) };
            buttonQuit.OnClick += ButtonQuit_OnClick;
            buttonQuit.OnHover += HandleButtonHover;
            buttonQuit.OnHoverLost += HandleButtonHoverLost;
            spriteManager.Add(buttonQuit);
        }

        void HandleButtonHover(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.FadeColour(Color4.White, 100);
        }

        void HandleButtonHoverLost(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.FadeColour(colourInactive, 100);
        }

        void ButtonQuit_OnClick(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.AdditiveFlash(500, 1);

            Director.ChangeMode(OsuMode.SongSelect);
            AudioEngine.PlaySample(OsuSamples.MenuBack);
        }

        void ButtonRestart_OnClick(object sender, EventArgs e)
        {
            pSprite s = sender as pSprite;
            s.AdditiveFlash(500, 1);

            Director.ChangeMode(OsuMode.Play);
            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        void ButtonContinue_OnClick(object sender, EventArgs e)
        {
            MenuDisplayed = false;
            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        public override void Dispose()
        {
            if (menuText != null)
                menuText.AlwaysDraw = false;

            base.Dispose();
        }

        internal void handleInput(InputSource source, TrackingPoint trackingPoint)
        {
            if (validPoint != null) return;

            float pos = getPos(trackingPoint);

            if (MenuDisplayed)
            {
                if (pos > 100 && pos < 180)
                {
                    validPoint = trackingPoint;
                    validPointOffset = getPos(validPoint);
                }
            }
            else
            {
                return;
                //disable pull behaviour for now.

                if (pos < (GameBase.IsHandheld ? 45 : 30))
                {
                    validPoint = trackingPoint;
                    validPointOffset = pos;
                }
            }
        }

        float getPos(TrackingPoint point)
        {
            return FromBottom ? GameBase.BaseSizeFixedWidth.Height - point.BasePosition.Y : point.BasePosition.Y;
        }

        public override void Update()
        {
           
            if (validPoint != null && !Failed)
            {
                if (pullnotice != null)
                {
                    spriteManager.Sprites.ForEach(s => s.Transformations.Clear());
                    pullnotice = null;
                }

                float pos = getPos(validPoint);

                float pulledAmount = Math.Min(1, (pos - validPointOffset + (MenuDisplayed ? -offscreen_y : 30)) / -offscreen_y);

                const float valid_pull = 0.7f;

                if (validPoint.Valid)
                {
                    spriteManager.Sprites.ForEach(s =>
                    {
                        if (s.TagNumeric == -1) return;
                        s.Position.Y = offscreen_y * (1 - pulledAmount);
                        s.Alpha = 0.4f + 0.6f * (pulledAmount);
                    });

                    if (pulledAmount > valid_pull)
                        if (AudioEngine.Music != null)
                        {
                            AudioEngine.Music.Pause();
                            isPaused = true;
                        }
                }
                else
                {
                    //force a switch here, so the animation resets.
                    menuDisplayed = !(pulledAmount >= valid_pull);
                    MenuDisplayed = pulledAmount >= valid_pull;

                    validPoint = null;
                }
            }

            base.Update();
        }

        /// <summary>
        /// returns true if a hitobject positioned at this location should override the menu
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        internal bool CheckHitObjectBlocksMenu(float pos)
        {
            return (FromBottom ? GameBase.BaseSizeFixedWidth.Height - pos : pos) < (GameBase.IsHandheld ? 45 : 30);
        }
    }
}