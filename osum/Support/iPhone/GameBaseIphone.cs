using System;
using MonoTouch.UIKit;
using osum.Helpers;

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
using System.Globalization;
using System.Threading;
using osum.GameModes;
using osum.Support.iPhone;

namespace osum
{
    public class GameBaseIphone : GameBase
    {
        public GameBaseIphone()
        {
        }

        override public void MainLoop()
        {
            AppDelegate.game = this;
            MonoTouch.UIKit.UIApplication.Main(new string[]{},"HaxApplication","AppDelegate");
        }

        bool disableDimming = false;
        internal override bool DisableDimming {
            get {
                return disableDimming;
            }
            set {
                if (value == disableDimming) return;
                disableDimming = value;
                UIApplication.SharedApplication.IdleTimerDisabled = value;
            }
        }

        public override void Initialize()
        {
            string culture = "en-US";
            switch (NSLocale.PreferredLanguages[0])
            {
                case "zh-Hans":
                    culture = "zh-CHS";
                    break;
                case "zh-Hant":
                    culture = "zh-CHT";
                    break;
                case "da":
                    culture = "da";
                    break;
                case "fr":
                    culture = "fr-FR";
                    break;
                case "it":
                    culture = "it-IT";
                    break;
                case "ja":
                    culture = "ja-JP";
                    break;
                case "ko":
                    culture = "ko-KR";
                    break;
                case "th":
                    culture = "th-TH";
                    break;
                    
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

#if !DIST
            Console.WriteLine("Running with culture " + culture + " " + System.Threading.Thread.CurrentThread.CurrentUICulture);
#endif

            switch (HardwareDetection.Version)
            {
                case HardwareVersion.iPad:
                case HardwareVersion.iPad2:
                    IsHandheld = false;
                    break;
                case HardwareVersion.iPhone:
                case HardwareVersion.iPhone3G:
                case HardwareVersion.iPod2G:
                case HardwareVersion.iPod1G:
                    IsSlowDevice = true;
                    break;
            }

            initialOrientation = UIApplication.SharedApplication.StatusBarOrientation;
            base.Initialize();
        }

        const UIInterfaceOrientation DEFAULT_ORIENTATION = UIInterfaceOrientation.LandscapeRight;
        UIInterfaceOrientation initialOrientation;

        public void HandleRotationChange(UIInterfaceOrientation orientation)
        {
            Player p = Director.CurrentMode as Player;

            if (p != null && !p.IsPaused)
                return; //don't rotate during gameplay.

            switch (orientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                case UIInterfaceOrientation.LandscapeRight:
                    break;
                default:
                    return;
            }

            if (initialOrientation == UIInterfaceOrientation.Portrait || initialOrientation == UIInterfaceOrientation.PortraitUpsideDown)
                initialOrientation = DEFAULT_ORIENTATION;

            FlipView = orientation != initialOrientation;
            UIApplication.SharedApplication.SetStatusBarOrientation(orientation, true);
        }

        public override void SetViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);

            GL.LoadIdentity();
            GL.Viewport(0, 0, GameBase.NativeSize.Height, GameBase.NativeSize.Width);
            GL.Ortho(0, GameBase.NativeSize.Height, GameBase.NativeSize.Width, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            float width = GameBase.NativeSize.Height;
            float height = GameBase.NativeSize.Width;

            float[] matrix;

            if (FlipView)
                matrix = new float[]{0, -1, 0, 0,
                              1, 0, 0, 0,
                              0, 0, 1, 0,
                              0, height, 0, 1};
            else
                matrix = new float[]{0, 1, 0, 0,
                              -1, 0, 0, 0,
                              0, 0, 1, 0,
                              width, 0, 0, 1};

            GL.LoadMatrix(ref matrix[0]);
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            //only initialise the first time (we may be here from a resume operation)
            return new BackgroundAudioPlayerIphone();
        }

        protected override void InitializeInput()
        {
            InputSource source = new InputSourceIphone();
            InputManager.AddSource(source);
        }

        public override string PathConfig {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/../Library/";
            }
        }

        string udidCached;
        public override string DeviceIdentifier {
            get {

                if (udidCached == null)
                {
#if SIMULATOR
                    udidCached = base.DeviceIdentifier;
#else
                    udidCached = UIDevice.CurrentDevice.UniqueIdentifier;
#endif
                }

                return udidCached;
            }
        }

        public override Thread RunInBackground(VoidDelegate task)
        {
            ParameterizedThreadStart pts = delegate {
                int ourTask = 0;
                UIApplication application = UIApplication.SharedApplication;

                if (UIDevice.CurrentDevice.IsMultitaskingSupported)
                {
                    application.BeginBackgroundTask(delegate {
                        //expired
                        if (ourTask != 0)
                        {
                            application.EndBackgroundTask(ourTask);
                            ourTask = 0;
                        }
                    });
                }

                task();

                if (ourTask != 0)
                {
                    application.BeginInvokeOnMainThread(delegate
                    {
                        if (ourTask != 0) //same as above
                        {
                            application.EndBackgroundTask(ourTask);
                            ourTask = 0;
                        }
                    });
                }
            };

            Thread t = new Thread (pts);
            t.Priority = ThreadPriority.Highest;
            t.IsBackground = true;
            t.Start();
            return t;
        }

        public override void ShowWebView(string url, string title = "", StringBoolDelegate checkFinished = null)
        {
            WebViewController webViewController = new WebViewController(url, title);
            if (checkFinished != null) webViewController.ShouldClose += checkFinished;
            UINavigationController nav = new UINavigationController(webViewController);
            nav.NavigationBar.TintColor = UIColor.DarkGray;

            AppDelegate.SetUsingViewController(true);
            AppDelegate.ViewController.PresentModalViewController(nav, true);
        }

        public override void OpenUrl(string url)
        {
            using (NSUrl nsUrl = new NSUrl(url))
                UIApplication.SharedApplication.OpenUrl(nsUrl);
        }
    }

    class WebViewController : UIViewController
    {
        string Url;
        string Title;

        UIWebView webView;

        public event StringBoolDelegate ShouldClose;

        public WebViewController(string url, string title)
        {
            Url = url;
            Title = title;
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        public override void LoadView()
        {
            base.LoadView();

            webView = new UIWebView();
            webView.Delegate = new FinishableWebViewDelegate(finished);
            webView.BackgroundColor = UIColor.Black;
            webView.ScalesPageToFit = true;
            webView.Opaque = false;
            webView.LoadRequest(NSUrlRequest.FromUrl(new NSUrl(Url)));
            View = webView;

            NavigationItem.Title = Title;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Close", UIBarButtonItemStyle.Done, delegate
            {
                Close();
            });
        }

        bool finished(string url)
        {
            Url = url;
            if (ShouldClose != null && ShouldClose(Url))
            {
                Close();
                return true;
            }

            return false;
        }

        public void Close()
        {
            DismissModalViewControllerAnimated(true);
            AppDelegate.SetUsingViewController(false);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewDidUnload()
        {
            webView.Dispose();
            base.ViewDidUnload();
        }
    }

    class FinishableWebViewDelegate : UIWebViewDelegate
    {
        StringBoolDelegate finishedDelegate;

        public FinishableWebViewDelegate(StringBoolDelegate finished)
        {
            finishedDelegate = finished;
        }

        public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            return !finishedDelegate(request.Url.AbsoluteString);
        }
    }
}

