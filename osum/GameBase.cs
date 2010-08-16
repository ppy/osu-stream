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


namespace osum
{
    public abstract class GameBase
    {
        public static GameBase Instance;

        /// <summary>
        /// Top-level sprite manager. Draws above everything else.
        /// </summary>
        private SpriteManager spriteManager = new SpriteManager();

        internal GameMode CurrentMode;

        internal static Size WindowSize;
		internal static Size StandardSize = new Size(1024,768);
		
		internal IBackgroundAudioPlayer backgroundAudioPlayer;


        internal bool ChangeMode(GameMode newMode, bool instant)
        {
            if (newMode == null) return false;

            if (CurrentMode != null)
            {
                CurrentMode.Dispose();
            }

            newMode.Initialize();
            CurrentMode = newMode;

            return true;
        }


        public GameBase()
        {
            Instance = this;
            MainLoop();
        }

        /// <summary>
        /// MainLoop runs, starts the main loop and calls Initialize when ready.
        /// </summary>
        public abstract void MainLoop();
		
		public virtual void SetupScreen()
		{
			StandardSize = new Size(1024,(int)(1024 * (float)WindowSize.Height/WindowSize.Width));

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

        public virtual void Initialize()
        {
			SetupScreen();
			
			ChangeMode(new MainMenu(),true);
			
            //Spinner h = new Spinner(1500, 6000, HitObjectSoundType.Normal);
            //spriteManager.Add(h);
        }

        public void Draw(FrameEventArgs e)
        {
            Clock.Update(e.Time);

            spriteManager.Update();
            CurrentMode.Update();

            GL.ClearColor(0, 0, 0, 1);

            CurrentMode.Draw();
            spriteManager.Draw();
        }
    }
}

