using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osum.Support;
using osum.Graphics;
using System.IO;
using osum.Graphics.Drawables;
#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

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
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif

namespace osum.GameModes
{
    class MainMenu : GameMode
    {
        pSprite osuLogo;
        pSprite osuLogoGloss;

        List<pSprite> explosions = new List<pSprite>();

        internal SpriteManager spriteManagerBehind = new SpriteManager();

        MenuState State = MenuState.Logo;

        static bool firstDisplay = true;

        public override void Initialize()
        {
            int initial_display = firstDisplay ? 2950 : 0;

            menuBackground =
                new pSprite(TextureManager.Load(OsuTexture.menu_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(255,255,255,255));
            menuBackground.ScaleScalar = 1.1f;
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
            osuLogoGloss = new pSprite(TextureManager.Load(OsuTexture.menu_gloss), FieldTypes.StandardSnapCentre, OriginTypes.Custom, ClockTypes.Mode, new Vector2(0, logo_stuff_v_offset), 0.91f, true, new Color4(255, 255, 255, 100));
            osuLogoGloss.Offset = new Vector2(255, 248);
            osuLogoGloss.Additive = true;
            osuLogoGloss.Transform(logoBounce);
            menuBackgroundNew.Add(osuLogoGloss);

            pSprite explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-110 * 0.625f, -110 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(252, 6, 127, 255));
            explosion.Transform(new TransformationBounce(initial_display + 50, initial_display + 2600, 1 * 0.625f * 0.625f, 1f, 7));
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(140 * 0.625f, 10 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(255, 212, 27, 255));
            explosion.Transform(new TransformationBounce(initial_display + 200, initial_display + 2900, 1.4f * 0.625f, 1.4f * 0.625f, 8));
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(-120 * 0.625f, 60 * 0.625f + logo_stuff_v_offset), 0.8f, true, new Color4(29, 209, 255, 255));
            explosion.Transform(new TransformationBounce(initial_display + 400, initial_display + 3200, 1.2f * 0.625f, 1.7f * 0.625f, 5));
            explosions.Add(explosion);
            menuBackgroundNew.Add(explosion);

            stream = new pSprite(TextureManager.Load(OsuTexture.menu_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 180), 0.95f, true, Color4.White);
            stream.Transform(new Transformation(TransformationType.Fade, 0, 1, initial_display + 900, initial_display + 1300));
            spriteManager.Add(stream);

            additiveStream = stream.Clone();
            additiveStream.Additive = true;
            additiveStream.DrawDepth = 0.96f;
            additiveStream.Alpha = 0;
            additiveStream.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display + 1300, initial_display + 2000));
            additiveStream.Transform(new Transformation(TransformationType.Fade, 0, 1, initial_display + 5000, initial_display + 5200) { Looping = true, LoopDelay = 5000 });
            additiveStream.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display + 5200, initial_display + 6000) { Looping = true, LoopDelay = 4400 });

            spriteManager.Add(additiveStream);


            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1, initial_display, initial_display);
            spriteManager.Sprites.ForEach(s => s.Transform(fadeIn));

            menuBackgroundNew.Transform(fadeIn);
            menuBackground.Transform(fadeIn);

            osuLogo.Transform(fadeIn);

            InitializeBgm();

            if (firstDisplay)
            {
                pDrawable whiteLayer = pSprite.FullscreenWhitePixel;
                whiteLayer.Clocking = ClockTypes.Mode;
                //whiteLayer.Additive = true;
                spriteManager.Add(whiteLayer);

                whiteLayer.Transform(new Transformation(TransformationType.Fade, 0, 0.125f, 800, initial_display - 200));
                whiteLayer.Transform(new Transformation(TransformationType.Fade, 0.125f, 1f, initial_display - 200, initial_display));
                whiteLayer.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display, initial_display + 1200, EasingTypes.In));

                pSprite headphones = new pSprite(TextureManager.Load(OsuTexture.menu_headphones), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Mode, new Vector2(0, 0), 0.9f, false, Color4.White);
                headphones.Additive = true;
                headphones.Transform(new Transformation(TransformationType.Fade, 0, 1, 50, 200));
                headphones.Transform(new Transformation(TransformationType.Fade, 1, 1, 1000, initial_display));
                spriteManager.Add(headphones);

                AudioEngine.PlaySample(OsuSamples.MainMenu_Intro);
                GameBase.Scheduler.Add(delegate { AudioEngine.Music.Play(); }, 2950); 
            }
            else
                AudioEngine.Music.Play();

            firstDisplay = false;
        }

        void osuLogo_OnClick(object sender, EventArgs e)
        {
            State = MenuState.Select;

            osuLogo.HandleInput = false;

            AudioEngine.PlaySample(OsuSamples.MenuHit);

            menuBackgroundNew.BeAwesome();

            osuLogo.Transformations.Clear();
            osuLogo.Transform(new Transformation(TransformationType.Scale, osuLogo.ScaleScalar, osuLogo.ScaleScalar * 2.4f, Clock.ModeTime, Clock.ModeTime + 1300, EasingTypes.InDouble));
            osuLogo.Transform(new Transformation(TransformationType.Rotation, osuLogo.Rotation, 0.35f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

            osuLogoGloss.Transformations.Clear();
            osuLogoGloss.FadeOut(100);
            osuLogoGloss.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));
            stream.FadeOut(150);
            additiveStream.FadeOut(150);

            osuLogo.FadeOut(800);

            explosions.ForEach(s => s.FadeOut(100));
        }


        
        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal static void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.m4a", true);
#else
            bool didLoad = AudioEngine.Music.Load("Skins/Default/mainmenu.mp3", true);
#endif
        }

        public override void Dispose()
        {
            menuBackgroundNew.Dispose();
            spriteManagerBehind.Dispose();
            base.Dispose();
        }

        double elapsedRotation;
        private pSprite menuBackground;
        private pSprite menuOptions;
        private pSprite stream;

        int lastBgmBeat = 0;
        float between_beats = 375 / 2f;
        int offset = 0;
        const int bar = 8;
        private pDrawable additiveStream;
        private MenuBackground menuBackgroundNew;

        public override void Update()
        {
            base.Update();

            spriteManagerBehind.Update();
            menuBackgroundNew.Update();

            osuLogoGloss.Rotation = -menuBackgroundNew.Rotation;

            if (AudioEngine.Music.IsElapsing)
            {
                elapsedRotation += GameBase.ElapsedMilliseconds;
                osuLogo.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * GameBase.ElapsedMilliseconds);

                Transformation tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Rotation);

                float rCh = -(float)(Math.Cos((elapsedRotation + 500) / 3000f) * 0.00002 * GameBase.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += rCh;
                else
                    menuBackgroundNew.Rotation += rCh;

                tr = menuBackgroundNew.Transformations.Find(t => t.Type == TransformationType.Scale);

                float sCh = -(float)(Math.Cos((elapsedRotation + 500) / 4000f) * 0.00002 * GameBase.ElapsedMilliseconds);
                if (tr != null)
                    tr.EndFloat += sCh;
                else
                    menuBackgroundNew.ScaleScalar += sCh;
            }

            menuBackground.Rotation += -(float)(Math.Cos((elapsedRotation + 500) / 3000f) * 0.00003 * GameBase.ElapsedMilliseconds);
            menuBackground.ScaleScalar += -(float)(Math.Cos((elapsedRotation + 500) / 4000f) * 0.00001 * GameBase.ElapsedMilliseconds);

            int newBeat = (int)((Clock.AudioTime - offset) / between_beats);
            if (osuLogo.Transformations.Count == 0)
            {
                if (newBeat > 15)
                {
                    if (lastBgmBeat != newBeat)
                    {
                        switch (newBeat % 8)
                        {
                            case 0:
                                explode(0);
                                osuLogo.ScaleScalar -= 0.03f;
                                osuLogoGloss.ScaleScalar -= 0.03f;
                                break;
                            case 2:
                                explode(1);
                                osuLogo.ScaleScalar += 0.03f;
                                osuLogoGloss.ScaleScalar += 0.03f;
                                break;
                            case 5:
                                explode(0);
                                osuLogo.ScaleScalar -= 0.03f;
                                osuLogoGloss.ScaleScalar -= 0.03f;
                                break;
                            case 6:
                                explode(1);
                                osuLogo.ScaleScalar += 0.03f;
                                osuLogoGloss.ScaleScalar += 0.03f;
                                break;
                            case 7:
                                explode(2);
                                break;
                        }
    
                        lastBgmBeat = newBeat;
                    }
                }
            }
        }

        private void explode(int beat)
        {
            if (explosions[beat].Transformations.Count > 0) return;

            explosions[beat].Transform(new TransformationBounce(Clock.ModeTime, Clock.ModeTime + (int)(between_beats * 2), explosions[beat].ScaleScalar, (1.3f * 0.625f  - explosions[beat].ScaleScalar) * 0.5f, 3));
        }

        public override bool Draw()
        {
            spriteManagerBehind.Draw();
            menuBackgroundNew.Draw();

            base.Draw();

            

            //if (!Director.IsTransitioning)
            //	osuLogo.ScaleScalar = 1 + AudioEngine.Music.CurrentVolume/100;

            return true;
        }
    }

    enum MenuState
    {
        Logo,
        Select
    }
}