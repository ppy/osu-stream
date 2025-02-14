using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Localisation;
using osum.UI;

#if iOS || ANDROID
using OpenTK.Graphics.ES11;
#if iOS
using Foundation;
using ObjCRuntime;
using OpenGLES;
#endif

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget = OpenTK.Graphics.ES11.All;

#else
#endif

namespace osum.GameModes.MainMenu
{
    internal partial class MainMenu : GameMode
    {
        private pSprite osuLogo;
        private pSprite osuLogoGloss;

#if iOS
        // the new version of the main menu theme is encoded in AAC, not HE-AAC.
        // without this offset, things get incorrect (especially after the first seek).
        public const int MAIN_MENU_OFFSET = -44;
#else
        public const int MAIN_MENU_OFFSET = 0;
#endif

        public const int MAIN_MENU_BEAT_LENGTH = 375;

        private readonly List<pSprite> explosions = new List<pSprite>();

        internal SpriteManager spriteManagerBehind = new SpriteManager();

        private MenuState State = MenuState.Logo;

        private static bool firstDisplay = true;

        public override void Initialize()
        {
            int initial_display = firstDisplay ? 2950 : 0;

            //spriteManagerBehind.Add(menuBackground);

            menuBackgroundNew = new MenuBackground();
            menuBackgroundNew.Clocking = ClockTypes.Mode;

            const int logo_stuff_v_offset = -20;

            Transformation logoBounce = new TransformationBounce(initial_display, initial_display + 2000, 0.625f, 0.4f, 2);

            osuLogo = new pSprite(TextureManager.Load(OsuTexture.menu_osu), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, logo_stuff_v_offset), 0.9f, true, Color4.White);
            osuLogo.Transform(logoBounce);
            osuLogo.OnClick += osuLogo_OnClick;
            menuBackgroundNew.Add(osuLogo);

            //gloss
            osuLogoGloss = new pSprite(TextureManager.Load(OsuTexture.menu_gloss), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, logo_stuff_v_offset), 0.91f, true, Color4.White);
            osuLogoGloss.Additive = true;
            menuBackgroundNew.Add(osuLogoGloss);

            Transformation explosionFade = new TransformationF(TransformationType.Fade, 0, 1, initial_display + 500, initial_display + 700);

            pSprite explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-90 * 0.625f, -90 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(112, 58, 144, 255));
            explosion.ScaleScalar = sizeForExplosion(0);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(170 * 0.625f, 10 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(242, 25, 138, 255));
            explosion.ScaleScalar = sizeForExplosion(1);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-130 * 0.625f, 88 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(254, 148, 4, 255));
            explosion.ScaleScalar = sizeForExplosion(2);
            explosion.Transform(explosionFade);
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            Transformation fadeIn = new TransformationF(TransformationType.Fade, 0, 1, initial_display, initial_display);
            spriteManager.Sprites.ForEach(s => s.Transform(fadeIn));

            stream = new pSprite(TextureManager.Load(OsuTexture.menu_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 180), 0.95f, true, Color4.White);
            stream.Transform(new TransformationF(TransformationType.Fade, 0, 1, initial_display + 900, initial_display + 1300));
            spriteManager.Add(stream);

            additiveStream = stream.Clone();
            additiveStream.Additive = true;
            additiveStream.DrawDepth = 0.96f;
            additiveStream.Transform(new TransformationF(TransformationType.Fade, 0, 1, initial_display + 1000, initial_display + 1200) { Looping = true, LoopDelay = 5000 });
            additiveStream.Transform(new TransformationF(TransformationType.Fade, 1, 0, initial_display + 1200, initial_display + 2000) { Looping = true, LoopDelay = 4400 });
            spriteManager.Add(additiveStream);

            osuLogoSmall = new pSprite(TextureManager.Load(OsuTexture.menu_logo), FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Mode, new Vector2(5, 5), 0.9f, true, Color4.White);
            osuLogoSmall.OnClick += delegate
            {
                if (State == MenuState.Select)
                    Director.ChangeMode(OsuMode.MainMenu);
            };
            osuLogoSmall.Alpha = 0;
            spriteManager.Add(osuLogoSmall);

            NewsButton = new NewsButton();
            spriteManager.Add(NewsButton);
            NewsButton.Alpha = 0;

            menuBackgroundNew.Transform(fadeIn);

            osuLogo.Transform(fadeIn);

            InitializeBgm();

            menuBackgroundNew.Transform(new TransformationBounce(initial_display, initial_display + 2000, menuBackgroundNew.ScaleScalar, 0.8f, 2));

            if (firstDisplay)
            {
                pDrawable whiteLayer = pSprite.FullscreenWhitePixel;
                whiteLayer.Alpha = 0;
                whiteLayer.Clocking = ClockTypes.Mode;
                spriteManager.Add(whiteLayer);

                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 0, 0.125f, 800, initial_display - 200));
                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 0.125f, 1f, initial_display - 200, initial_display));
                whiteLayer.Transform(new TransformationF(TransformationType.Fade, 1, 0, initial_display, initial_display + 1200, EasingTypes.In));

                pSprite headphones = new pSprite(TextureManager.Load(OsuTexture.menu_headphones), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.9f, false, Color4.White);
                headphones.Additive = true;
                headphones.Transform(new TransformationF(TransformationType.Fade, 0, 1, 50, 200));
                headphones.Transform(new TransformationF(TransformationType.Fade, 1, 1, 1000, initial_display));
                spriteManager.Add(headphones);

#if !DIST
                if (GameBase.Config.GetValue(@"MapperMode", true))
                {
                    GameBase.Mapper = true;
                    pText t = new pText("ENABLED MAPPER MODE", 24, new Vector2(0, 30), 1, false, Color4.Red)
                    {
                        Field = FieldTypes.StandardSnapTopCentre,
                        Origin = OriginTypes.Centre
                    };
                    t.FadeOutFromOne(1500);
                    spriteManager.Add(t);
                };
#endif

                pDrawable add = headphones.Clone();
                add.Additive = true;
                add.Transform(new TransformationF(TransformationType.Fade, 0, 0.1f, 50, 200));
                add.Transform(new TransformationF(TransformationType.Fade, 0.1f, 0.2f, 1000, initial_display));
                add.Transform(new TransformationF(TransformationType.Scale, 1, 1.12f, 1000, initial_display, EasingTypes.Out));
                spriteManager.Add(add);

                add = headphones.Clone();
                add.Additive = true;
                add.Transform(new TransformationF(TransformationType.Fade, 0, 0.1f, 50, 200));
                add.Transform(new TransformationF(TransformationType.Fade, 0.1f, 0.2f, 1000, initial_display));
                add.Transform(new TransformationF(TransformationType.Scale, 1, 1.04f, 1000, initial_display, EasingTypes.Out));
                spriteManager.Add(add);

                add = headphones.Clone();
                add.Additive = true;
                add.Transform(new TransformationF(TransformationType.Fade, 0, 0.1f, 50, 200));
                add.Transform(new TransformationF(TransformationType.Fade, 0.1f, 0.2f, 1000, initial_display));
                add.Transform(new TransformationF(TransformationType.Scale, 1, 1.08f, 1000, initial_display, EasingTypes.Out));
                spriteManager.Add(add);

                GameBase.Scheduler.Add(delegate
                {
                    AudioEngine.PlaySample(OsuSamples.MainMenu_Intro);
                    GameBase.Scheduler.Add(delegate
                    {
                        if (AudioEngine.Music != null) AudioEngine.Music.Play();
                    }, 2950);
                }, true);

                if (GameBase.Config.GetValue("firstrun", true))
                {
                    Notification notification = new Notification(
                        LocalisationManager.GetString(OsuString.FirstRunWelcome),
                        LocalisationManager.GetString(OsuString.FirstRunTutorial),
                        NotificationStyle.YesNo,
                        delegate(bool answer)
                        {
                            if (answer)
                            {
                                AudioEngine.PlaySample(OsuSamples.MenuHit);
                                Director.ChangeMode(OsuMode.Tutorial);
                            }

                            GameBase.Config.SetValue("firstrun", false);
                            GameBase.Config.SaveConfig();
                        });

                    GameBase.Scheduler.Add(delegate { GameBase.Notify(notification); }, initial_display + 1500);
                }
            }
            else
            {
                if (Director.LastOsuMode == OsuMode.Tutorial)
                    AudioEngine.Music.SeekTo(0);
                AudioEngine.Music.Play();
            }

            string username = GameBase.Config.GetValue<string>("username", null);

            bool hasAuth = GameBase.HasAuth;

            firstDisplay = false;
        }

        private void osuLogo_OnClick(object sender, EventArgs e)
        {
            State = MenuState.Select;

            osuLogo.HandleInput = false;
            osuLogo.Transformations.Clear();

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            menuBackgroundNew.BeAwesome();

            Transformation fadeIn = new TransformationF(TransformationType.Fade, 0, 0.98f, Clock.ModeTime + 1300, Clock.ModeTime + 1700);
            osuLogoSmall.Transform(fadeIn);

            Transformation move = new TransformationV(new Vector2(0, 50), Vector2.Zero, Clock.ModeTime + 500, Clock.ModeTime + 1000, EasingTypes.In);
            fadeIn = new TransformationF(TransformationType.Fade, 0, 0.98f, Clock.ModeTime + 500, Clock.ModeTime + 1000);
            NewsButton.Transform(fadeIn);
            NewsButton.Transform(move);

            osuLogo.Transformations.Clear();
            osuLogo.Transform(new TransformationF(TransformationType.Scale, osuLogo.ScaleScalar, osuLogo.ScaleScalar * 2.4f, Clock.ModeTime, Clock.ModeTime + 1300, EasingTypes.InDouble));
            osuLogo.Transform(new TransformationF(TransformationType.Rotation, osuLogo.Rotation, 0.35f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

            osuLogoGloss.Transformations.Clear();
            osuLogoGloss.FadeOut(200);
            osuLogoGloss.Transform(new TransformationF(TransformationType.Scale, osuLogoGloss.ScaleScalar, osuLogoGloss.ScaleScalar * 2.4f, Clock.ModeTime, Clock.ModeTime + 1300, EasingTypes.InDouble));
            stream.FadeOut(150);
            additiveStream.FadeOut(150);

            osuLogo.FadeOut(800);

            explosions.ForEach(s =>
            {
                s.Transformations.Clear();
                s.FadeOut(100);
            });
        }


        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal static bool InitializeBgm()
        {
            if (AudioEngine.Music == null) return false;

            //Start playing song select BGM.
#if iOS
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.m4a", true);
#else
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.mp3", true);
#endif

            return didLoad;
        }

        public override void Dispose()
        {
            //we will never use these textures again (the "intro" sheet) so get rid of them for good.
            TextureManager.Dispose(OsuTexture.menu_headphones);

            menuBackgroundNew.Dispose();
            spriteManagerBehind.Dispose();
            base.Dispose();
        }

        private double elapsedRotation;
        private pSprite stream;

        private int lastBgmBeat;
        private readonly float between_beats = MAIN_MENU_BEAT_LENGTH / 4f;

        private const int bar = 8;
        private pDrawable additiveStream;
        private MenuBackground menuBackgroundNew;
        private pSprite osuLogoSmall;
        public NewsButton NewsButton;

        public override void Update()
        {
            osuLogoGloss.Rotation = -menuBackgroundNew.Rotation;

            if (AudioEngine.Music != null && AudioEngine.Music.IsElapsing)
            {
                elapsedRotation += Clock.ElapsedMilliseconds;
                osuLogo.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * Clock.ElapsedMilliseconds);

                TransformationF tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Rotation) as TransformationF;

                float rCh = -(float)(Math.Cos((elapsedRotation + 500) / 3000f) * 0.00002 * Clock.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += rCh;
                else
                    menuBackgroundNew.Rotation += rCh;

                tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Scale) as TransformationF;

                float sCh = (float)(Math.Cos((elapsedRotation + 500) / 4000f) * 0.00002 * Clock.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += sCh;
                else
                    menuBackgroundNew.ScaleScalar += sCh;
            }

            if (State != MenuState.Select)
                updateBeat();

            base.Update();
            spriteManagerBehind.Update();
            menuBackgroundNew.Update();

            osuLogoGloss.ScaleScalar = osuLogo.ScaleScalar;
        }

        private int lastStrum;

        private void strum()
        {
            int strum = lastStrum;

            while (lastStrum == strum)
                strum = GameBase.Random.Next(0, menuBackgroundNew.lines.Count);

            menuBackgroundNew.lines[strum].FlashColour(Color4.White, 400);

            lastStrum = strum;
        }

        private void explode(int beat, float strength = 1)
        {
            pDrawable explosion = explosions[beat];

            if (explosion.Alpha == 0 && !menuBackgroundNew.IsAwesome && osuLogo.ScaleScalar >= 0.6f)
            {
                explosion.ScaleScalar *= 1.3f;
                explosion.FadeIn(100);
            }

            if (!menuBackgroundNew.IsAwesome)
            {
                float adjust = beat == 0 ? (1 - 0.1f * strength) : (beat == 1 ? (1 + 0.05f * strength) : 1);
                if (osuLogo.Transformations.Count != 0 && osuLogo.Transformations[0] is TransformationBounce)
                    ((TransformationBounce)osuLogo.Transformations[0]).EndFloat *= adjust;
                else
                {
                    osuLogo.ScaleScalar *= adjust;
                    osuLogo.ScaleTo(0.625f, 500, EasingTypes.In);
                }
            }

            explosion.FlashColour(ColourHelper.Lighten2(explosion.Colour, 0.4f * strength), 350);
            explosion.ScaleScalar *= 1 + (0.2f * strength);
            explosion.ScaleTo(sizeForExplosion(beat), 400, EasingTypes.In);
        }

        private float sizeForExplosion(int beat)
        {
            return 0.7f - beat * 0.05f;
        }

        public override bool Draw()
        {
            spriteManagerBehind.Draw();
            menuBackgroundNew.Draw();
            base.Draw();
            return true;
        }
    }

    internal enum MenuState
    {
        Logo,
        Select
    }
}
