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

namespace osum
{
    public class GameBaseIphone : GameBase
    {
        GameWindowIphone gameWindow;

        public GameBaseIphone()
        {
        }

        override public void MainLoop()
        {
            MonoTouch.UIKit.UIApplication.Main(new string[]{});
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
                case "ja":
                    culture = "ja-JP";
                    break;
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

#if !DIST
            Console.WriteLine("Runningwith culture " + culture + " " + System.Threading.Thread.CurrentThread.CurrentUICulture);
#endif


            UIAccelerometer.SharedAccelerometer.UpdateInterval = 1;
            UIAccelerometer.SharedAccelerometer.Acceleration += HandleUIAccelerometerSharedAccelerometerAcceleration;

            gameWindow = GameWindowIphone.Instance;
            base.Initialize();
        }

        static float pi = (float)Math.PI;

        void HandleUIAccelerometerSharedAccelerometerAcceleration (object sender, UIAccelerometerEventArgs e)
        {
            Player p = Director.CurrentMode as Player;

            if (p != null && !p.IsPaused)
                return; //don't rotate during gameplay.

            float angle = (float)(Math.Atan2(e.Acceleration.X, e.Acceleration.Y) * 180/pi);

            if (Math.Abs(e.Acceleration.Z) < 0.6f)
            {
                if (angle > 45 && angle < 135)
                    FlipView = true;
                else if (angle < -45 && angle > -135)
                    FlipView = false;
            }
        }

        public override void SetViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0, GameBase.NativeSize.Height, GameBase.NativeSize.Width, 0, -1, 1);
            GL.Viewport(0, 0, GameBase.NativeSize.Height, GameBase.NativeSize.Width);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            float width = GameBase.NativeSize.Height;
            float height = GameBase.NativeSize.Width;
            GL.Translate(width / 2, height / 2, 0);
            GL.Rotate(FlipView ? 270 : 90, 0, 0, 1);
            GL.Translate(-height / 2, -width / 2, 0);
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            //only initialise the first time (we may be here from a resume operation)
            return new BackgroundAudioPlayerIphone();
        }

        protected override void InitializeInput()
        {
            InputSource source = new InputSourceIphone (gameWindow);
            gameWindow.SetInputHandler(source);
            InputManager.AddSource(source);
        }

        public override string PathConfig {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/../Library/";
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
    }
}

