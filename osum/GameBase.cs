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


namespace osum
{
    public abstract class GameBase
    {
        public static GameBase Instance;

        /// <summary>
        /// Top-level sprite manager. Draws above everything else.
        /// </summary>
        private SpriteManager spriteManager = new SpriteManager();

        internal static Size WindowSize;
        internal static Size StandardSize = new Size(1024, 768);

        internal static Size StandardSizeHalf { get { return new Size(StandardSize.Width / 2, StandardSize.Height / 2); } }

        public static double ElapsedMilliseconds = 1000 / 60f;

        internal IBackgroundAudioPlayer backgroundAudioPlayer;
        internal SoundEffectPlayer soundEffectPlayer;

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

        /// <summary>
        /// Setup viewport and projection matrix. Should be called after a resolution/orientation change.
        /// </summary>
        public virtual void SetupScreen()
        {
            StandardSize = new Size(1024, (int)(1024 * (float)WindowSize.Height / WindowSize.Width));

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

#if IPHONE
			GL.Ortho(0, GameBase.StandardSize.Height, GameBase.StandardSize.Width, 0, 0, 1);
#else
            GL.Ortho(0, GameBase.StandardSize.Width, GameBase.StandardSize.Height, 0, 0, 1);
#endif

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        /// <summary>
        /// This is where the magic happens.
        /// </summary>
        public virtual void Initialize()
        {
            SetupScreen();

            InputManager.Initialize();
            InitializeInput();
            if (InputManager.RegisteredSources.Count == 0)
                throw new Exception("No input sources registered");

            InitializeBackgroundAudio();
            if (backgroundAudioPlayer == null)
                throw new Exception("No background audio manager registered");
            Clock.AudioTimeSource = backgroundAudioPlayer;
            Components.Add(backgroundAudioPlayer);

            InitializeSoundEffects();
            if (soundEffectPlayer == null)
                throw new Exception("No sound effect player registered");
            Components.Add(soundEffectPlayer);

            //Load the main menu initially.
            Director.ChangeMode(OsuMode.MainMenu, null);
        }

        /// <summary>
        /// Initializes the sound effects engine.
        /// </summary>
        protected virtual void InitializeSoundEffects()
        {
            soundEffectPlayer = new SoundEffectPlayer();
        }

        /// <summary>
        /// Initializes the background audio playback engine.
        /// </summary>
        protected abstract void InitializeBackgroundAudio();

        /// <summary>
        /// Initializes the input management subsystem.
        /// </summary>
        protected abstract void InitializeInput();

        int frameCount;
        double frameTime;

        /// <summary>
        /// Main update cycle.
        /// </summary>
        public void Update(FrameEventArgs e)
        {
            double lastTime = Clock.TimeAccurate;

            Clock.Update(e.Time);

            //todo: make more accurate
            ElapsedMilliseconds = Clock.TimeAccurate - lastTime;

            frameTime += ElapsedMilliseconds;
            frameCount++;

            if (frameTime > 1000)
            {
                Console.WriteLine(frameCount + " frames in " + frameTime + "ms");
                frameTime = 0;
                frameCount = 0;
            }

            Director.Update();

            Components.ForEach(c => c.Update());

            spriteManager.Update();
        }

        /// <summary>
        /// Main draw cycle.
        /// </summary>
        public void Draw(FrameEventArgs e)
        {
            //todo: make update actually update on iphone and call from game architecture
            Update(e);

            //not necessary when drawing background.


            Director.Draw();
            spriteManager.Draw();
        }
    }
}

