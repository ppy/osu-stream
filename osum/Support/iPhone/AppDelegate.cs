using System;
using MonoTouch.UIKit;

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
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using osum.Graphics.Skins;

namespace osum
{
	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		public static AppDelegate Instance;
		
		// This method is invoked when the application has loaded its UI and is ready to run
		public override void FinishedLaunching (UIApplication app)
		{	
			UIApplication.SharedApplication.StatusBarHidden = true;
			UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.LandscapeRight,false);
			
			Console.WriteLine("+++FinishedLaunching");
			Instance = this;
		}
		
		public override void OnResignActivation (UIApplication app)
		{
			Console.WriteLine("+++ResignActivation");
			
			SkinManager.UnloadAll();
			
			if (glView.EAGLContext != null)
			    glView.Stop();
		}
		
		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication app)
		{
			Console.WriteLine("+++OnActivated");
			
			GameBase.WindowSize = new Size((int)glView.Bounds.Height, (int)glView.Bounds.Width);
			
			//start the run loop.
			glView.Run(60.0);
		}
	}
}

