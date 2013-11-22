using System;
using MonoTouch.UIKit;
using osum.Helpers;
using MonoTouch.Security;
using osum.GameplayElements;

#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;

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
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using osum.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

using System.Drawing;
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

        override public void Run()
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
                default:
                    culture = NSLocale.PreferredLanguages[0];
                    break;

            }

            try {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

#if !DIST
                Console.WriteLine("Running with culture " + culture + " " + System.Threading.Thread.CurrentThread.CurrentUICulture);
#endif
            }
            catch {}

            switch (HardwareDetection.Version)
            {
                case HardwareVersion.iPad:
                    IsHandheld = false;
                    break;
                case HardwareVersion.iPhone:
                case HardwareVersion.iPhone3G:
                case HardwareVersion.iPod2G:
                case HardwareVersion.iPod1G:
                    IsSlowDevice = true;
                    break;
            }

            if (!HardwareDetection.RunningiOS6OrHigher)
                initialOrientation = UIApplication.SharedApplication.StatusBarOrientation;
            base.Initialize();
        }

        const UIInterfaceOrientation DEFAULT_ORIENTATION = UIInterfaceOrientation.LandscapeRight;
        UIInterfaceOrientation initialOrientation = DEFAULT_ORIENTATION;

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

        string identifier;
        public override string DeviceIdentifier {
            get {

                if (identifier == null)
                {
#if SIMULATOR
                    identifier = base.DeviceIdentifier;
#else
                    const string name = @"o!s";
                    const string account = @"soup";
                    SecStatusCode eCode;
                    // Query the record.
                    SecRecord oQueryRec = new SecRecord(SecKind.GenericPassword) { Service = name, Label = name, Account = account };
                    oQueryRec = SecKeyChain.QueryAsRecord(oQueryRec, out eCode);

                    // If found, try to get the identifier.
                    if (eCode == SecStatusCode.Success && oQueryRec != null && oQueryRec.Generic != null)
                    {
                        // Decode from UTF8.
                        return NSString.FromData(oQueryRec.Generic, NSStringEncoding.UTF8);
                    }

                    //we haven't yet stored a unique identifier. for old databases, let's migrate the udid across
                    //to make sure purchased content still works.

                    BeatmapDatabase.Initialize();
                    if (BeatmapDatabase.Version < 10 && BeatmapDatabase.BeatmapInfo.Find(b => b.Filename.Contains(".osp2")) != null)
                    {
                        identifier = UIDevice.CurrentDevice.UniqueIdentifier;
                    }
                    else
                    {
                        //create a new unique identifier
                        identifier = Guid.NewGuid().ToString();
                    }

                    //store to keychain
                    eCode = SecKeyChain.Add(new SecRecord(SecKind.GenericPassword)
                    {
                        Service = name,
                        Label = name,
                        Account = account,
                        Generic = NSData.FromString(identifier, NSStringEncoding.UTF8),
                        Accessible = SecAccessible.AfterFirstUnlockThisDeviceOnly
                    });
#endif
                }

                return identifier;
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
            nav.NavigationBar.TintColor = new UIColor(0,134/255f, 219/255f, 1);

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
        new string Title;

        UIWebView webView;
        UIActivityIndicatorView activity;

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

            activity = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);

            activity.Frame = new RectangleF(0, -20, 20, 20);
            activity.HidesWhenStopped = true;

            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(activity);

            NavigationItem.Title = Title;
            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Close", UIBarButtonItemStyle.Done, delegate
            {
                Close();
            });
        }

        bool finished(string url, bool done)
        {
            if (done)
                activity.StopAnimating();
            else
                activity.StartAnimating();

            if (done) return true;

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
            ShouldClose = null;

            webView.LoadRequest(NSUrlRequest.FromUrl(new NSUrl("about:blank")));
            webView.Delegate = null;
            webView = null;
            DismissModalViewControllerAnimated(true);
            AppDelegate.SetUsingViewController(false);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        public override void ViewDidUnload()
        {
            if (webView != null) webView.Dispose();
            base.ViewDidUnload();
        }
    }

    class FinishableWebViewDelegate : UIWebViewDelegate
    {
        StringBoolBoolDelegate finishedDelegate;

        public FinishableWebViewDelegate(StringBoolBoolDelegate finished)
        {
            finishedDelegate = finished;
        }

        public override bool ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            return !finishedDelegate(request.Url.AbsoluteString, false);
        }

        public override void LoadingFinished (UIWebView webView)
        {
            finishedDelegate(null, true);
        }
    }
}

