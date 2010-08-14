using System;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

#if IPHONE
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
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using osum.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

using System.Drawing;

namespace osum
{
	public partial class GameWindowIphone : iPhoneOSGameView
	{
		[Export("layerClass")]
		static Class LayerClass ()
		{
			return iPhoneOSGameView.GetLayerClass();
		}

		[Export("initWithCoder:")]
		public GameWindowIphone (NSCoder coder) : base(coder)
		{
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGBA8;
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
		}

		protected override void ConfigureLayer (CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}
		
		private void SetViewport()
		{

            RectangleF bounds = UIScreen.MainScreen.Bounds;
            
		
			GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            
			GL.Ortho(0, GameBase.StandardSize.Height, GameBase.StandardSize.Width, 0, 0, 1);
            
			GL.MatrixMode(All.Modelview);
			GL.LoadIdentity();
			
			
			//Console.WriteLine("set viewport ({0}x{1}", Size.Width, Size.Height);
	    }
		
		protected override void OnLoad (EventArgs e)
		{
			GL.Disable(EnableCap.Lighting);
			GL.Enable(EnableCap.Blend);

			GameBase.Instance.Initialize();
			
			base.OnLoad(e);
		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize(e);
			
			SetViewport();
		}
		
		/*protected override void OnResize (EventArgs e)
		{
			base.OnResize(e);
			
			Console.WriteLine("resized to {0}x{1}",Size.Width,Size.Height);
			
            SetViewport();
		}*/
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);
		}
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			MakeCurrent();
			
			//GL.Rotate(5f, 0.0f, 0.0f, 1.0f);
			GL.Viewport(0,0,Size.Width,Size.Height);
			
			
			GL.PushMatrix();
			
			
			
			float width = GameBase.StandardSize.Height;
			float height = GameBase.StandardSize.Width;
			
			GL.LoadIdentity();
			GL.Translate(width / 2, height / 2, 0);
			GL.Rotate(90, 0, 0, 1);
			GL.Translate(-height / 2, -width / 2, 0);
			
			
			GL.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
			
			GameBase.Instance.Draw(e);
			
			GL.PopMatrix();
			
			SwapBuffers();
		}
	}
}

