using System;

using OpenTK.Platform;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

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


using osum.GameplayElements;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Graphics;
using osum.Helpers;



using System.Drawing;

	
namespace openglproject
{
	public abstract class GameBase
	{
		public static GameBase Instance;
		
		private SpriteManager spriteManager = new SpriteManager();
		
		public GameBase()
		{
			Instance = this;
            MainLoop();
        }
		
		/// <summary>
		/// MainLoop runs, starts the main loop and calls Initialize when ready.
		/// </summary>
        public abstract void MainLoop();

		private pSprite test2;
		
        public virtual void Initialize()
        {
            Spinner h = new Spinner(1500, 6000, HitObjectSoundType.Normal);
            spriteManager.Add(h);
			
            Console.WriteLine("initialize started");
            
            Console.WriteLine("initialize ended!");
        }
		
		public void Draw(FrameEventArgs e)
		{
            Clock.Update(e.Time);

            spriteManager.Update();
            
            GL.ClearColor(0,0,0,1);
			spriteManager.Draw();
		}
	}
}

