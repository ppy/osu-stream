using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameModes;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Support;
using osum.Graphics.Renderers;
using osum.Graphics;
using osum.Online;

#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using DepthFunction = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
#else
using OpenTK.Graphics.OpenGL;
#endif


namespace osum
{
    public abstract class GameBase
    {
        public static GameBase Instance;

        public static Random Random = new Random();

        internal static Size BaseSizeFixedWidth = new Size(640, 426);
        internal static Size BaseSize = new Size(640, 426);
        internal static Size GamefieldBaseSize = new Size(512, 384);

        internal static int SpriteResolution;

        /// <summary>
        /// Ratio of sprite size compared to their default habitat (SpriteResolution)
        /// </summary>
        internal static float SpriteToBaseRatio;

        internal static float SpriteToNativeRatio;

        internal static float ScaleFactor = 1;
        internal static Size NativeSize;

        /// <summary>
        /// The ratio of actual-pixel window size in relation to the base resolution used internally.
        /// </summary>
        internal static float BaseToNativeRatio;

        /// <summary>
        /// The ratio of actual-pixel window size in relation to the base resolution used internally.
        /// Includes realignment for the range of widths where sprite ratio does not change.
        /// </summary>
        internal static float BaseToNativeRatioAligned;

        internal static Vector2 GamefieldOffsetVector1;

        internal static readonly NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public static double ElapsedMilliseconds = 1000 / 60f;

        internal static Scheduler Scheduler = new Scheduler();

        /// <summary>
        /// A list of components which get updated every frame.
        /// </summary>
        public static List<IUpdateable> Components = new List<IUpdateable>();

        /// <summary>
        /// Top-level sprite manager. Draws above everything else.
        /// </summary>
        internal static SpriteManager MainSpriteManager = new SpriteManager();

        /// <summary>
        /// May be set in the update loop to force the next frame to have an ElapsedMilliseconds of 0
        /// </summary>
        private bool ignoreNextFrameTime;

        public GameBase()
        {
            Instance = this;
            MainLoop();
        }

        internal static Size BaseSizeHalf
        {
            get { return new Size(BaseSizeFixedWidth.Width / 2, BaseSizeFixedWidth.Height / 2); }
        }

        internal static Vector2 GamefieldToStandard(Vector2 vec)
        {
            Vector2 newPosition = vec;
            GamefieldToStandard(ref newPosition);
            return newPosition;
        }

        internal static void GamefieldToStandard(ref Vector2 vec)
        {
            Vector2.Add(ref vec, ref GamefieldOffsetVector1, out vec);
        }

        internal static Vector2 StandardToGamefield(Vector2 vec)
        {
            Vector2 newPosition = vec;
            StandardToGamefield(ref newPosition);
            return newPosition;
        }

        internal static void StandardToGamefield(ref Vector2 vec)
        {
            Vector2.Subtract(ref vec, ref GamefieldOffsetVector1, out vec);
        }

        /// <summary>
        /// MainLoop runs, starts the main loop and calls Initialize when ready.
        /// </summary>
        public abstract void MainLoop();


        public static event VoidDelegate OnScreenLayoutChanged;

        public virtual void SetViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

#if iOS
            GL.Ortho(0, GameBase.NativeSize.Height, GameBase.NativeSize.Width, 0, -1, 1);
            GL.Viewport(0, 0, GameBase.NativeSize.Height, GameBase.NativeSize.Width);
#else
            GL.Viewport(0, 0, NativeSize.Width, NativeSize.Height);
            GL.Ortho(0, NativeSize.Width, NativeSize.Height, 0, -1, 1);
#endif
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        /// <summary>
        /// Setup viewport and projection matrix. Should be called after a resolution/orientation change.
        /// </summary>
        public virtual void SetupScreen()
        {
            //Setup window...
            BaseSizeFixedWidth.Height = (int)(BaseSizeFixedWidth.Width * (float)NativeSize.Height / NativeSize.Width);

            GL.Disable(EnableCap.DepthTest);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.Disable(EnableCap.Lighting);
            GL.DepthMask(false);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            SetViewport();

            BaseToNativeRatio = (float)NativeSize.Width / BaseSizeFixedWidth.Width;

            GamefieldOffsetVector1 = new Vector2((float)(BaseSizeFixedWidth.Width - GamefieldBaseSize.Width) / 2,
                                                 (float)(BaseSizeFixedWidth.Height - GamefieldBaseSize.Height) / 4 * 3);


            int oldResolution = SpriteSheetResolution;

            //define any available sprite sheets here.
            if (NativeSize.Width < 720)
                SpriteSheetResolution = 480;
            else
                SpriteSheetResolution = 960;

            //if we are switching to a new sprite sheet (resizing window on PC) let's refresh our textures.
            if (SpriteSheetResolution != oldResolution && oldResolution > 0)
                TextureManager.ReloadAll(true);

            //calculations and internally, all textures are at 960-width-compatible sizes.
            const float base_sprite_res = 960;

            //handle lower resolution devices' aspect ratio band in a similar way with next to no extra effort.
            int testWidth = NativeSize.Width < 512 ? NativeSize.Width * 2 : NativeSize.Width;

            SpriteResolution = (int)(Math.Max(base_sprite_res, Math.Min(1024, testWidth)));
            //todo: this will fail if there's ever a device with width greater than 480 but less than 512 (ie. half of the range)
            //need to consider the WindowScaleFactor value here.

            BaseToNativeRatioAligned = BaseToNativeRatio * (base_sprite_res / GameBase.SpriteResolution);

            SpriteToBaseRatio = BaseSizeFixedWidth.Width / base_sprite_res;

            BaseSize = new Size((int)(NativeSize.Width / BaseToNativeRatioAligned), (int)(NativeSize.Height / BaseToNativeRatioAligned));

            SpriteToNativeRatio = (float)NativeSize.Width / SpriteResolution;
            //1024x = 1024/1024 = 1
            //960x  = 960/960   = 1
            //480x  = 480/960   = 0.5

#if DEBUG
            Console.WriteLine("Base Resolution is " + BaseSize + " (fixed: " + BaseSizeFixedWidth + ")");
#endif

            TriggerLayoutChanged();
        }

        /// <summary>
        /// This is where the magic happens.
        /// </summary>
        public virtual void Initialize()
        {
            SetupScreen();

            InitializeAssetManager();

            TextureManager.Initialize();

            InputManager.Initialize();

            InitializeInput();

            if (InputManager.RegisteredSources.Count == 0)
                throw new Exception("No input sources registered");

            BackgroundAudioPlayer music = InitializeBackgroundAudio();
            if (music == null)
                throw new Exception("No background audio manager registered");
            Clock.AudioTimeSource = music;
            Components.Add(music);

            SoundEffectPlayer effect = InitializeSoundEffects();
            if (effect == null)
                throw new Exception("No sound effect player registered");
            Components.Add(effect);

            AudioEngine.Initialize(effect, music);

#if !RELEASE
            DebugOverlay.Update();
#endif

            //Load the main menu initially.
            //Player.Beatmap = new GameplayElements.Beatmaps.Beatmap("Beatmaps/Lix - Phantom Ensemble -Ark Trance mix- v2 (James).osz2");
            //Player.Beatmap.BeatmapFilename = Player.Beatmap.Package.MapFiles[0];
            //AudioEngine.Music.Load(Player.Beatmap.GetFileBytes(Player.Beatmap.AudioFilename), false);
            Director.ChangeMode(OsuMode.MainMenu, null);

            OnlineHelper.Initialize();
        }

        /// <summary>
        /// Initializes the sound effects engine.
        /// </summary>
        protected virtual SoundEffectPlayer InitializeSoundEffects()
        {
            //currently openAL implementation is used across the board.
            return new SoundEffectPlayer();
        }

        /// <summary>
        /// Initializes the background audio playback engine.
        /// </summary>
        protected abstract BackgroundAudioPlayer InitializeBackgroundAudio();

        /// <summary>
        /// Initializes the input management subsystem.
        /// </summary>
        protected abstract void InitializeInput();


        /// <summary>
        /// Initializes the AssetManager. 
        /// Assets are skins, hitsounds, textures that come with the game.
        /// These are, depending on the platform, located in the executable itself.
        /// Maps are not included as assets to prevent oversized executables.
        /// </summary>
        /// <returns></returns>
        protected virtual NativeAssetManager InitializeAssetManager()
        {
            return new NativeAssetManager();
        }

        /// <summary>
        /// Main update cycle
        /// </summary>
        /// <returns>true if a draw should occur</returns>
        public bool Update(FrameEventArgs e)
        {
            double lastTime = Clock.TimeAccurate;
            double thisTime = 0;
            try { thisTime = e.Time; }
            catch { }
            //try-catch is precautionary after reading this http://xnatouch.codeplex.com/Thread/View.aspx?ThreadId=237507
            Clock.Update(thisTime);

            ElapsedMilliseconds = ignoreNextFrameTime ? 0 : Clock.TimeAccurate - lastTime;

            if (ElapsedMilliseconds > 1000)
                ElapsedMilliseconds = 0;

            ignoreNextFrameTime = false;

            Scheduler.Update();

#if !RELEASE
            DebugOverlay.Update();
#endif

#if DEBUG
            DebugOverlay.AddLine("GC: 0:" + GC.CollectionCount(0) + " 1:" + GC.CollectionCount(1) + " 2:" + GC.CollectionCount(2));
            DebugOverlay.AddLine("Window Size: " + NativeSize.Width + "x" + NativeSize.Height + " Sprite Resolution: " + SpriteResolution);
#endif

            TextureManager.Update();

            MainSpriteManager.Update();

            if (Director.Update())
            {
                ignoreNextFrameTime = true;
                //Mode change occurred; we don't need to do anything this frame.
                //We are on a blank screen and don't want to throw off timings, so let's cancel the draw.
                return false;
            }

            InputManager.Update();

            Components.ForEach(c => c.Update());

            return true;
        }

        /// <summary>
        /// Main draw cycle.
        /// </summary>
        public void Draw(FrameEventArgs e)
        {
            SpriteManager.Reset();

            GL.Clear(Constants.COLOR_BUFFER_BIT);
            //todo: Does clearing DEPTH as well here add a performance overhead?

            if (ignoreNextFrameTime)
                return;

            SpriteManager.Reset();

            Director.Draw();

            MainSpriteManager.Draw();

        }

        public static void TriggerLayoutChanged()
        {
            if (OnScreenLayoutChanged != null)
                OnScreenLayoutChanged();
        }

        internal static pSprite ActiveNotification;
        internal static int SpriteSheetResolution;

        internal static void Notify(string text, VoidDelegate action = null)
        {
            GameBase.Scheduler.Add(delegate
            {
                pSprite back = new pSprite(TextureManager.Load("notification"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 0.99f, false, Color4.White) { DimImmune = true };
                ActiveNotification = back;

                pText t = new pText(text, 36, Vector2.Zero, new Vector2(BaseSizeFixedWidth.Width * 0.9f, 0), 1, false, Color4.White, true) { Field = FieldTypes.StandardSnapCentre, Origin = OriginTypes.Centre, TextAlignment = TextAlignment.Centre, Clocking = ClockTypes.Game, DimImmune = true };

                Transformation bounce = new TransformationBounce(Clock.Time, Clock.Time + 800, 1, 0.1f, 8);
                Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1, Clock.Time, Clock.Time + 200);
                Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0, Clock.Time + 10000, Clock.Time + 10200);

                t.Transform(bounce, fadeIn, fadeOut);
                back.Transform(bounce, fadeIn, fadeOut);

                back.OnClick += delegate
                {
                    back.HandleInput = false;

                    Transformation bounce2 = new TransformationBounce(Clock.Time, Clock.Time + 300, 1.05f, 0.05f, 3);
                    Transformation fadeOut2 = new Transformation(TransformationType.Fade, 1, 0, Clock.Time, Clock.Time + 300);

                    back.Transformations.Clear();
                    t.Transformations.Clear();

                    back.Transform(bounce2, fadeOut2);
                    t.Transform(bounce2, fadeOut2);

                    if (action != null)
                        action();

                    ActiveNotification = null;
                    SpriteManager.UniversalDim = 0;
                };

                MainSpriteManager.Add(t);
                MainSpriteManager.Add(back);
            });
        }
    }
}