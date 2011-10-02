using System;
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
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
using System.Drawing;
using osum.Graphics.Skins;
using osum.Audio;
using osum.GameModes;
using OpenTK.Graphics;
using OpenTK.Platform;
using osum.Helpers;

namespace osum.Support.iPhone
{
    [MonoTouch.Foundation.Register("HaxApplication")]
    public class HaxApplication : UIApplication
    {
        public override void SendEvent(UIEvent e)
        {
            if (e.Type == UIEventType.Touches && !AppDelegate.UsingViewController)
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
        public static AppDelegate Instance;

        public static EAGLView glView;
        public static GameBaseIphone game;
        static IGraphicsContext context;

        UIWindow window;

        private void RotationChanged(NSNotification notification)
        {
            UIInterfaceOrientation interfaceOrientation;
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

            if (ViewController == null)
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

            GameBase.NativeSize = new Size((int)(UIScreen.MainScreen.Bounds.Size.Height * GameBase.ScaleFactor),
                                    (int)(UIScreen.MainScreen.Bounds.Size.Width * GameBase.ScaleFactor));

#if !DIST
            Console.WriteLine("scale factor " + GameBase.ScaleFactor);
            Console.WriteLine("native size " + GameBase.NativeSize);
#endif
            GameBase.TriggerLayoutChanged();

            game.Initialize();
            glView.Run(game);

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, RotationChanged);

            return true;
        }

        public override void WillEnterForeground(UIApplication application)
        {
            if (Director.CurrentOsuMode == OsuMode.MainMenu)
                Director.ChangeMode(OsuMode.MainMenu, null);
        }

        public override void OnActivated(UIApplication app)
        {
            glView.StartAnimation();
        }

        public override void OnResignActivation(UIApplication app)
        {
            Player p = Director.CurrentMode as Player;
            if (p != null)
            {
                p.Pause();
                AudioEngine.Music.Stop(false);
            }

            glView.StopAnimation();
        }

        int lastCleanup;
        public override void ReceiveMemoryWarning(UIApplication application)
        {
#if !DIST
            Console.WriteLine("OSU MEMORY CLEANUP!");
#endif

            if (!Director.IsTransitioning)
            {
                if (Clock.Time - lastCleanup < 1000) return;

                lastCleanup = Clock.Time;
                TextureManager.PurgeUnusedTexture();
                GC.Collect();
            }
            else
            {
                GameBase.Scheduler.Add(delegate { ReceiveMemoryWarning(application); }, 500);
            }
        }

        public static UIViewController ViewController;

        public static bool UsingViewController;
        public static void SetUsingViewController(bool isUsing)
        {
                if (isUsing == UsingViewController) return;
                UsingViewController = isUsing;

                if (UsingViewController)
                {
                    if (ViewController == null)
                        ViewController = new GenericViewController();

                    Instance.window.AddSubview(ViewController.View);

                    InputSourceIphone source = InputManager.RegisteredSources[0] as InputSourceIphone;
                    source.ReleaseAllTouches();

                    glView.StopAnimation();
                }
                else
                {
                    ViewController.View.RemoveFromSuperview();
                    ViewController.Dispose();
                    ViewController = null;

                    glView.StartAnimation();
                }
        }
    }

    public class GenericViewController : UIViewController
    {
        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            switch (toInterfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                case UIInterfaceOrientation.LandscapeRight:
                    return toInterfaceOrientation == UIApplication.SharedApplication.StatusBarOrientation;
                    //only allow rotation on initial display, else all hell breaks loose.
                default:
                    return false;
            }
        }
    }
}

