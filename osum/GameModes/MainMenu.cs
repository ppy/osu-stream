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

        MenuState State = MenuState.Logo;

        const int initial_display = 2950;

        internal override void Initialize()
        {
            menuBackground =
                new pSprite(TextureManager.Load(OsuTexture.menu_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, Color.White);
            menuBackground.ScaleScalar = 1.07f;
            menuBackground.OnClick += new EventHandler(menuBackground_OnClick);
            spriteManager.Add(menuBackground);

            const int logo_stuff_v_offset = -20;

            pSprite headphones = new pSprite(TextureManager.Load(OsuTexture.menu_headphones), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(0, 0), 0.9f, false, Color4.White);
            headphones.Additive = true;
            headphones.Transform(new Transformation(TransformationType.Fade, 0, 1, 50, 100));
            headphones.Transform(new Transformation(TransformationType.Fade, 1, 1, 1000, initial_display));
            spriteManager.Add(headphones);

            osuLogo = new pSprite(TextureManager.Load(OsuTexture.menu_osu), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(0, logo_stuff_v_offset), 0.9f, true, Color4.White);
            osuLogo.Transform(new TransformationBounce(initial_display, initial_display + 2000, 1, 0.4f, 2));
            spriteManager.Add(osuLogo);

            //gloss
            osuLogoGloss = new pSprite(TextureManager.Load(OsuTexture.menu_gloss), FieldTypes.StandardSnapCentre, OriginTypes.Custom, ClockTypes.Audio, new Vector2(0, logo_stuff_v_offset), 0.91f, true, Color4.White);
#if MONO
            osuLogoGloss.Colour = new Color4(255, 255, 255, 100);
#endif
            osuLogoGloss.Offset = new Vector2(255, 248);
            osuLogoGloss.Additive = true;
            osuLogoGloss.Transform(new TransformationBounce(initial_display, initial_display + 2000, 1, 0.4f, 2));
            spriteManager.Add(osuLogoGloss);

            pSprite explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(-110, -110 + logo_stuff_v_offset), 0.8f, true, new Color4(252, 6, 127, 255));
            explosion.Transform(new TransformationBounce(initial_display + 50, initial_display + 2600, 1, 1f, 7));
            explosions.Add(explosion);
            spriteManager.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(140, 10 + logo_stuff_v_offset), 0.8f, true, new Color4(255, 212, 27, 255));
            explosion.Transform(new TransformationBounce(initial_display + 200, initial_display + 2900, 1.4f, 1.4f, 8));
            explosions.Add(explosion);
            spriteManager.Add(explosion);

            explosion = new pSprite(TextureManager.Load(OsuTexture.menu_circle), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(-120, 60 + logo_stuff_v_offset), 0.8f, true, new Color4(29, 209, 255, 255));
            explosion.Transform(new TransformationBounce(initial_display + 400, initial_display + 3200, 1.2f, 1.7f, 5));
            explosions.Add(explosion);
            spriteManager.Add(explosion);

            stream = new pSprite(TextureManager.Load(OsuTexture.menu_stream), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Audio, new Vector2(0, 186), 0.95f, true, Color4.White);
            stream.Transform(new Transformation(TransformationType.Fade, 0, 1, initial_display + 900, initial_display + 1300));
            stream.ExactCoordinates = true;
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

            menuOptions = new pSprite(TextureManager.Load(OsuTexture.menu_options), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                ClockTypes.Mode, Vector2.Zero, 0.1f, true, Color.White);
            menuOptions.Alpha = 0;
            menuOptions.OnClick += new EventHandler(menuOptions_OnClick);
            spriteManager.Add(menuOptions);

            if (firstDisplay)
            {
                pDrawable whiteLayer = pSprite.FullscreenWhitePixel;
                whiteLayer.Clocking = ClockTypes.Audio;
                //whiteLayer.Additive = true;
                spriteManager.Add(whiteLayer);

                whiteLayer.Transform(new Transformation(TransformationType.Fade, 0, 0.125f, 800, initial_display - 200));
                whiteLayer.Transform(new Transformation(TransformationType.Fade, 0.125f, 1f, initial_display - 200, initial_display));
                whiteLayer.Transform(new Transformation(TransformationType.Fade, 1, 0, initial_display, initial_display + 1200, EasingTypes.In));
            }

            InitializeBgm();
        }

        void menuOptions_OnClick(object sender, EventArgs e)
        {
            float yPos = InputManager.PrimaryTrackingPoint.BasePosition.Y;
            float relativePos = yPos / GameBase.BaseSize.Height;
            if (relativePos > 0.75f)
            {
                GameBase.Notify("No options yet!");
                //options
            }
            else if (relativePos > 0.5f)
            {
                Director.ChangeMode(OsuMode.Store);
                AudioEngine.PlaySample(OsuSamples.MenuHit);
            }
            else if (relativePos > 0.25f)
            {
                Director.ChangeMode(OsuMode.SongSelect);
                AudioEngine.PlaySample(OsuSamples.MenuHit);
            }
            else
            {
                Director.ChangeMode(OsuMode.Tutorial);
                AudioEngine.PlaySample(OsuSamples.MenuHit);
            }

            menuOptions.AdditiveFlash(300, 1).ScaleTo(1.1f,300);
        }

        void menuBackground_OnClick(object sender, EventArgs e)
        {
            State = MenuState.Select;

            AudioEngine.PlaySample(OsuSamples.MenuHit);

//#if MONO
//            Director.ChangeMode(OsuMode.SongSelect);
//#endif

            osuLogo.Transformations.Clear();
            osuLogo.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.AudioTime, Clock.AudioTime + 1000, EasingTypes.In));
            osuLogo.Transform(new Transformation(TransformationType.Rotation, osuLogo.Rotation, 1.4f, Clock.AudioTime, Clock.AudioTime + 1000, EasingTypes.In));

            osuLogoGloss.Transformations.Clear();
            osuLogoGloss.FadeOut(100);
            osuLogoGloss.Transform(new Transformation(TransformationType.Scale, 1, 4f, Clock.AudioTime, Clock.AudioTime + 1000, EasingTypes.In));

            stream.FadeOut(150);
            additiveStream.FadeOut(150);

            osuLogo.FadeOut(500);

            explosions.ForEach(s => s.FadeOut(100));

            menuOptions.FadeIn(200);

            menuBackground.FadeOut(100);
            menuBackground.HandleInput = false;
        }


        static bool firstDisplay = true;
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

            if (!firstDisplay && (didLoad || Clock.AudioTime < initial_display))
                AudioEngine.Music.SeekTo(initial_display);

            AudioEngine.Music.Play();

            firstDisplay = false;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        double elapsedRotation;
        private pSprite menuBackground;
        private pSprite menuOptions;
        private pSprite stream;

        int lastBgmBeat = 0;
        float between_beats = 375 / 2f;
        int offset = initial_display;
        const int bar = 8;
        private pDrawable additiveStream;

        public override void Update()
        {
            base.Update();

            if (Clock.AudioTime > initial_display)
            {
                elapsedRotation += GameBase.ElapsedMilliseconds;
                osuLogo.Rotation += (float)(Math.Cos((elapsedRotation) / 1000f) * 0.0001 * GameBase.ElapsedMilliseconds);

                menuOptions.Rotation = menuBackground.Rotation += -(float)(Math.Cos((elapsedRotation + 500) / 3000f) * 0.00002 * GameBase.ElapsedMilliseconds);
                menuOptions.ScaleScalar = menuBackground.ScaleScalar += -(float)(Math.Cos((elapsedRotation + 500) / 4000f) * 0.00002 * GameBase.ElapsedMilliseconds);

            }

            int newBeat = (int)((Clock.AudioTime - offset) / between_beats);

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

        private void explode(int beat)
        {
            if (explosions[beat].Transformations.Count > 0) return;

            explosions[beat].Transform(new TransformationBounce(Clock.AudioTime, Clock.AudioTime + (int)(between_beats * 2), explosions[beat].ScaleScalar, (1.3f - explosions[beat].ScaleScalar) * 0.5f, 3));
        }

        public override bool Draw()
        {
            if (!base.Draw()) return false;

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