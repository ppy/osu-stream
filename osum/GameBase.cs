using System;
using OpenTK.Platform;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using osum.GameplayElements;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Graphics;
using osum.Helpers;
using System.Drawing;

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
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif

using osum.GameModes;
using osum.Support;
using System.Collections.Generic;
using System.Globalization;
using osum.Audio;


namespace osum
{
    public abstract class GameBase
    {
        public static GameBase Instance;


        public static Random Random = new Random();

        /// <summary>
        /// Top-level sprite manager. Draws above everything else.
        /// </summary>
        private SpriteManager spriteManager = new SpriteManager();

        internal static Size WindowBaseSize = new Size(640, 480);
        internal static Size WindowBaseHalf { get { return new Size(WindowBaseSize.Width / 2, WindowBaseSize.Height / 2); } }
        internal static Size GamefieldBaseSize =  new Size(512,384);

        internal static int SpriteResolution = 1024;

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

        internal static readonly NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public static double ElapsedMilliseconds = 1000 / 60f;

        /// <summary>
        /// A list of components which get updated every frame.
        /// </summary>
        public static List<IUpdateable> Components = new List<IUpdateable>();

        public GameBase()
        {
            Instance = this;
            MainLoop();
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
            GL.Ortho(0, GameBase.WindowBaseSize.Height, GameBase.WindowBaseSize.Width, 0, -1, 1);
            GL.Viewport(0, 0, GameBase.WindowSize.Height, GameBase.WindowSize.Width);
#else
            GL.Viewport(0, 0, GameBase.WindowSize.Width, GameBase.WindowSize.Height);
            GL.Ortho(0, GameBase.WindowBaseSize.Width, GameBase.WindowBaseSize.Height, 0, -1, 1);
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
            WindowBaseSize.Height = (int)(WindowBaseSize.Width * (float)WindowSize.Height / WindowSize.Width);

            SetViewport();

            WindowRatio = (float)WindowSize.Height / WindowBaseSize.Height;

            //Setup gamefield...
            GamefieldSize = new Size(
                (int)Math.Round(GamefieldBaseSize.Width * WindowRatio),
                (int)Math.Round(GamefieldBaseSize.Height * WindowRatio)
            );

            GamefieldOffsetVector1 = new Vector2((float)(WindowBaseSize.Width - GamefieldBaseSize.Width) / 2,
                                                 (float)(WindowBaseSize.Height - GamefieldBaseSize.Height) / 4 * 3);

            GamefieldRatio = (float)GamefieldSize.Height / GamefieldBaseSize.Height;

            SpriteRatioToWindowBase = (float)WindowBaseSize.Width / SpriteResolution;

            SpriteRatioToWindow = (float)WindowSize.Width / SpriteResolution;

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
            Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(200,500));

            fpsDisplay = new pText("", 13, Vector2.Zero, new Vector2(256,40), 1, true, Color4.White, false);
            fpsDisplay.Field = FieldTypes.StandardSnapBottomRight;
            fpsDisplay.Origin = OriginTypes.BottomRight;
            spriteManager.Add(fpsDisplay);
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

        pText fpsDisplay;
        double weightedAverageFrameTime;

        /// <summary>
        /// Main update cycle.
        /// </summary>
        public void Update(FrameEventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);

            double lastTime = Clock.TimeAccurate;

            Clock.Update(e.Time);

            //todo: make more accurate
            ElapsedMilliseconds = Clock.TimeAccurate - lastTime;

            InputManager.Update();

            UpdateFpsOverlay();
            
            Director.Update();

            Components.ForEach(c => c.Update());

            spriteManager.Update();
        }

        private void UpdateFpsOverlay()
        {
            weightedAverageFrameTime = weightedAverageFrameTime * 0.98 + ElapsedMilliseconds * 0.02;
            double fps = (1000/weightedAverageFrameTime);

            if (Clock.Time < 1000) return;

            fpsDisplay.Colour = fps < 59 ? Color.OrangeRed : Color.GreenYellow;
            fpsDisplay.Text = String.Format("{0:0}fps {1} {2}", Math.Round(fps), Clock.Time, Clock.AudioTime);
        }

        /// <summary>
        /// Main draw cycle.
        /// </summary>
        public void Draw(FrameEventArgs e)
        {
            //todo: make update actually update on iphone and call from game architecture
            Update(e);

            Director.Draw();

            spriteManager.Draw();
        }
    }
}

