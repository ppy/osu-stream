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
using OpenTK.Graphics;
using OpenTK.Platform;

namespace osum.Support.iPhone
{
    [MonoTouch.Foundation.Register("HaxApplication")]
    public class HaxApplication : UIApplication
    {
        public override void SendEvent(UIEvent e)
        {
            if (e.Type == UIEventType.Touches)
            {
                InputSourceIphone source = InputManager.RegisteredSources[0] as InputSourceIphone;
                source.HandleTouches(e.AllTouches);
                return;
            }

            base.SendEvent(e);
        }
    }

    [MonoTouch.Foundation.Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        static bool active;
        static bool firstActivation = true;

        public static AppDelegate Instance;

        public static EAGLView glView;
        public static GameBaseIphone game;
        static IGraphicsContext context;

        UIWindow window;

        private void RotationChanged(NSNotification notification)
        {
            UIInterfaceOrientation interfaceOrientation;
            Console.WriteLine("Received notification:"+UIDevice.CurrentDevice.Orientation);
            switch (UIDevice.CurrentDevice.Orientation)
            {
                case UIDeviceOrientation.LandscapeLeft:
                    interfaceOrientation = UIInterfaceOrientation.LandscapeRight;
                    break;
                case UIDeviceOrientation.LandscapeRight:
                    interfaceOrientation = UIInterfaceOrientation.LandscapeLeft;
                    break;
                default:
                    return;
            }
            game.HandleRotationChange(interfaceOrientation);
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launcOptions)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            window.MakeKeyAndVisible();

            UIApplication.SharedApplication.StatusBarHidden = true;

            Instance = this;

            HardwareVersion hardware = HardwareDetection.Version;

            context = Utilities.CreateGraphicsContext(EAGLRenderingAPI.OpenGLES1);

            glView = new EAGLView(window.Bounds);
            GameBase.ScaleFactor = UIScreen.MainScreen.Scale;

            window.AddSubview(glView);

#if !DIST
            Console.WriteLine("scale factor " + GameBase.ScaleFactor);
            GameBase.NativeSize = new Size((int)(UIScreen.MainScreen.Bounds.Size.Height * GameBase.ScaleFactor),
                                        (int)(UIScreen.MainScreen.Bounds.Size.Width * GameBase.ScaleFactor));
            Console.WriteLine("native size " + GameBase.NativeSize);
#endif
            GameBase.TriggerLayoutChanged();

            game.Initialize();
            glView.Run(game);

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, RotationChanged);

            return true;
        }

        public override void WillEnterForeground (UIApplication application)
        {
            if (Director.CurrentOsuMode == OsuMode.MainMenu)
                Director.ChangeMode(OsuMode.MainMenu, null);
        }

        public override void OnActivated(UIApplication app)
        {
            glView.StartAnimation();
            active = true;
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

            glView.StopAnimation();
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
#if !DIST
            Console.WriteLine("OSU MEMORY CLEANUP!");
#endif

            if (!Director.IsTransitioning)
            {
                TextureManager.PurgeUnusedTexture();
                GC.Collect();
            }
            else
            {
                GameBase.Scheduler.Add(delegate { ReceiveMemoryWarning(application); }, 500);
            }
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

                /*if (usingViewController)
                    Instance.window.AddSubview(Instance.viewController.View);
                else
                    Instance.viewController.View.RemoveFromSuperview();*/
            }
        }

        public static UIViewController ViewController {
            get {
                return null;//Instance.viewController;
            }
        }
    }
}

