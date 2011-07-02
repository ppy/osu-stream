using System;
using MonoTouch.UIKit;

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
using osum.Audio;
using System.Threading;
using osum.GameModes;

namespace osum.Support.iPhone
{
    // The name AppDelegate is referenced in the MainWindow.xib file.
    public partial class AppDelegate : UIApplicationDelegate
    {
        static bool active;
        static bool firstActivation = true;

        public static AppDelegate Instance;

        // This method is invoked when the application has loaded its UI and is ready to run
        public override void FinishedLaunching(UIApplication app)
        {    
            UIApplication.SharedApplication.StatusBarHidden = true;
            UIApplication.SharedApplication.SetStatusBarOrientation(UIInterfaceOrientation.LandscapeRight, false);

            Instance = this;

            glView.ContentScaleFactor = UIScreen.MainScreen.Scale;

            GameBase.ScaleFactor = glView.ContentScaleFactor;
            GameBase.NativeSize = new Size((int)(UIScreen.MainScreen.Bounds.Size.Height * GameBase.ScaleFactor),
                                        (int)(UIScreen.MainScreen.Bounds.Size.Width * GameBase.ScaleFactor));

            GameBase.TriggerLayoutChanged();

            int targetFps = 10000;

            switch (HardwareDetection.Version)
            {
                case HardwareVersion.iPhone:
                case HardwareVersion.iPhone3G:
                case HardwareVersion.iPod1G:
                case HardwareVersion.iPod2G:
                    targetFps = 40; //aim a bit lower with older devices.
                    break;
            }

            glView.Run(targetFps);
        }

        public override void WillEnterForeground (UIApplication application)
        {
            if (Director.CurrentOsuMode == OsuMode.MainMenu)
                Director.ChangeMode(OsuMode.MainMenu, null);
        }

        public override void OnActivated(UIApplication app)
        {
            active = true;
            if (memoryJettisoned)
                TextureManager.ReloadAll(false);
        }

        public override void OnResignActivation(UIApplication app)
        {
            active = false;

            Player p = Director.CurrentMode as Player;

            if (p != null)
            {
                p.Pause();
                AudioEngine.Music.Stop(false);
            }
        }

        bool memoryJettisoned;

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            //todo: implement this.
            /*if (!memoryJettisoned && !active)
            {
                TextureManager.UnloadAll();
                memoryJettisoned = true;
            }*/
        }

        public static bool Running { get { return active && !usingViewController; } }

        static bool usingViewController;

        public static bool UsingViewController {
            get { return usingViewController; }
            set
            {
                if (value == usingViewController)
                    return;
                usingViewController = value;

                if (usingViewController)
                    Instance.window.AddSubview(Instance.viewController.View);
                else
                    Instance.viewController.View.RemoveFromSuperview();
            }
        }

        public static UIViewController ViewController {
            get { return Instance.viewController; }
        }
    }
}

