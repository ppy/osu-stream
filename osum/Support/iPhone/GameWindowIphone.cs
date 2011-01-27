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
using osum.Graphics.Skins;

namespace osum
{
	[MonoTouch.Foundation.Register("GameWindowIphone")]
	public partial class GameWindowIphone : iPhoneOSGameView
	{
		public static GameWindowIphone Instance;
		
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
			UserInteractionEnabled = true;
			ExclusiveTouch = true;
			
			Instance = this;
		}

		protected override void ConfigureLayer (CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}
		
		static bool firstLoad = true;
		protected override void OnLoad (EventArgs e)
		{
			GL.Disable(EnableCap.Lighting);
			GL.Enable(EnableCap.Blend);

			if (firstLoad)
			{
				GameBase.Instance.Initialize();
				firstLoad = false;
			}
			else
			{
				GameBase.Instance.SetupScreen();
				TextureManager.ReloadAll();	
			}
			
			base.OnLoad(e);
		}
		
		protected override void OnResize (EventArgs e)
		{
			base.OnResize(e);
		}
		
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
		}
		
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			MakeCurrent();
			
			GameBase.Instance.Draw(e);
			
			SwapBuffers();
		}
		
		
		InputSourceIphone inputHandler;
		
		public void SetInputHandler(InputSource source)
		{
			InputSourceIphone addableSource = source as InputSourceIphone;
			
			if (addableSource == null)
				return;
			
			inputHandler = addableSource;
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);
			
			if (inputHandler != null)
				inputHandler.HandleTouchesBegan(touches,evt);
			
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			
			if (inputHandler != null)
				inputHandler.HandleTouchesMoved(touches,evt);
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			
			if (inputHandler != null)
				inputHandler.HandleTouchesEnded(touches,evt);
		}
		
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			
			if (inputHandler != null)
				inputHandler.HandleTouchesCancelled(touches,evt);
		}
	}
}

