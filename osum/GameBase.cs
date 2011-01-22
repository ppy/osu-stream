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
#if IPHONE
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

        internal static Size WindowBaseSize = new Size(640, 480);

        internal static Size GamefieldBaseSize = new Size(512, 384);

        internal static int SpriteResolution;

        /// <summary>
        /// Ratio of sprite size compared to their default habitat (SpriteResolution)
        /// </summary>
        internal static float SpriteRatioToWindowBase;

        internal static float SpriteRatioToWindow;

        internal static Size WindowSize;
        internal static Size GamefieldSize;

        /// <summary>
        /// The ratio of actual-pixel window size in relation to the base resolution used internally.
        /// </summary>
        internal static float WindowRatio;

        /// <summary>
        /// The ratio of the actual-pixel gamefield compared to the base resolution.
        /// </summary>
        internal static float GamefieldRatio;

        internal static Vector2 GamefieldOffsetVector1;


        internal static readonly NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public static double ElapsedMilliseconds = 1000/60f;

        /// <summary>
        /// A list of components which get updated every frame.
        /// </summary>
        public static List<IUpdateable> Components = new List<IUpdateable>();

        /// <summary>
        /// Top-level sprite manager. Draws above everything else.
        /// </summary>
        internal readonly SpriteManager SpriteManager = new SpriteManager();

        /// <summary>
        /// May be set in the update loop to force the next frame to have an ElapsedMilliseconds of 0
        /// </summary>
        private bool ignoreNextFrameTime;

        public GameBase()
        {
            Instance = this;
            MainLoop();
        }

        internal static Size WindowBaseHalf
        {
            get { return new Size(WindowBaseSize.Width/2, WindowBaseSize.Height/2); }
        }

        internal static Vector2 GamefieldToStandard(Vector2 vec)
        {
            Vector2 newPosition = vec;
            GamefieldToStandard(ref newPosition);
            return newPosition;
        }

        internal static void GamefieldToStandard(ref Vector2 vec)
        {
            //Vector2.Multiply(ref vec, GamefieldRatio, out vec);
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
            //Vector2.Multiply(ref vec, GamefieldRatio, out vec);
            Vector2.Subtract(ref vec, ref GamefieldOffsetVector1, out vec);
        }

        /// <summary>
        /// MainLoop runs, starts the main loop and calls Initialize when ready.
        /// </summary>
        public abstract void MainLoop();


        public static event VoidDelegate OnScreenLayoutChanged;

        public void SetViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

#if IPHONE
            GL.Ortho(0, GameBase.WindowSize.Height, GameBase.WindowSize.Width, 0, -1, 1);
            GL.Viewport(0, 0, GameBase.WindowSize.Height, GameBase.WindowSize.Width);
#else
            GL.Viewport(0, 0, WindowSize.Width, WindowSize.Height);
            GL.Ortho(0, WindowSize.Width, WindowSize.Height, 0, -1, 1);
#endif

            GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadIdentity();
        }

        /// <summary>
        /// Setup viewport and projection matrix. Should be called after a resolution/orientation change.
        /// </summary>
        public virtual void SetupScreen()
        {
            //Setup window...
            WindowBaseSize.Height = (int) (WindowBaseSize.Width*(float) WindowSize.Height/WindowSize.Width);

            SetViewport();

            WindowRatio = (float) WindowSize.Width/WindowBaseSize.Width;

            //Setup gamefield...
            GamefieldSize = new Size(
                (int) Math.Round(GamefieldBaseSize.Width*WindowRatio),
                (int) Math.Round(GamefieldBaseSize.Height*WindowRatio)
                );

            GamefieldOffsetVector1 = new Vector2((float) (WindowBaseSize.Width - GamefieldBaseSize.Width)/2,
                                                 (float) (WindowBaseSize.Height - GamefieldBaseSize.Height)/4*3);

            GamefieldRatio = (float) GamefieldSize.Height/GamefieldBaseSize.Height;

            SpriteResolution = Math.Max(960, Math.Min(1024, WindowSize.Width));

            SpriteRatioToWindowBase = (float) WindowBaseSize.Width/SpriteResolution;

            SpriteRatioToWindow = (float) WindowSize.Width/SpriteResolution;

            if (OnScreenLayoutChanged != null)
                OnScreenLayoutChanged();
        }

        /// <summary>
        /// This is where the magic happens.
        /// </summary>
        public virtual void Initialize()
        {
            SetupScreen();

            TextureManager.Initialize();

            InputManager.Initialize();

            InitializeInput();

            if (InputManager.RegisteredSources.Count == 0)
                throw new Exception("No input sources registered");

            IBackgroundAudioPlayer music = InitializeBackgroundAudio();
            if (music == null)
                throw new Exception("No background audio manager registered");
            Clock.AudioTimeSource = music;
            Components.Add(music);

            ISoundEffectPlayer effect = InitializeSoundEffects();
            if (effect == null)
                throw new Exception("No sound effect player registered");
            Components.Add(effect);

            AudioEngine.Initialize(effect, music);

            //Load the main menu initially.
            Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(0, 500));
        }

        /// <summary>
        /// Initializes the sound effects engine.
        /// </summary>
        protected virtual ISoundEffectPlayer InitializeSoundEffects()
        {
            //currently openAL implementation is used across the board.
            return new SoundEffectPlayer();
        }

        /// <summary>
        /// Initializes the background audio playback engine.
        /// </summary>
        protected abstract IBackgroundAudioPlayer InitializeBackgroundAudio();

        /// <summary>
        /// Initializes the input management subsystem.
        /// </summary>
        protected abstract void InitializeInput();

        /// <summary>
        /// Main update cycle
        /// </summary>
        /// <returns>true if a draw should occur</returns>
        public bool Update(FrameEventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);

            double lastTime = Clock.TimeAccurate;
            Clock.Update(e.Time);

            ElapsedMilliseconds = ignoreNextFrameTime ? 0 : Clock.TimeAccurate - lastTime;
            ignoreNextFrameTime = false;

            DebugOverlay.Update();

            TextureManager.Update();

            if (Director.Update())
            {
                ignoreNextFrameTime = true;
                //Mode change occurred; we don't need to do anything this frame.
                //We are on a blank screen and don't want to throw off timings, so let's cancel the draw.
                return false;
            }

            InputManager.Update();

            Components.ForEach(c => c.Update());

            SpriteManager.Update();

            return true;
        }

        /// <summary>
        /// Main draw cycle.
        /// </summary>
        public void Draw(FrameEventArgs e)
        {
            //todo: make update actually update on iphone and call from game architecture
            if (Update(e))
            {
                //todo: only clear when required
                if (Director.CurrentMode.RequireClear || Director.IsTransitioning)
#if IPHONE
					GL.Clear((int)ClearBufferMask.ColorBufferBit);
#else
                    GL.Clear(ClearBufferMask.ColorBufferBit);
#endif


                Director.Draw();

                SpriteManager.Draw();
            }
        }

        public static void TriggerLayoutChanged()
        {
            if (OnScreenLayoutChanged != null)
                OnScreenLayoutChanged();
        }
    }
}