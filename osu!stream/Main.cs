using osum.Graphics;
using osum.Support.Android;
#if ANDROID
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Views;
using Android.Content.PM;
using osum.GameModes;
using osum.GameModes.Play;
using Xamarin.Essentials;
using osum.Input;
using osum.Input.Sources;
#endif

#if !iOS && !ANDROID
using osum.Support.Desktop;
#endif

namespace osum
{
#if ANDROID
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.UserLandscape, MainLauncher = true)]
    public class Application : AppCompatActivity
#else
    public class Application
#endif
    {
#if ANDROID
        private static Activity activity;
#endif

        private static void Main(string[] args)
        {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#elif ANDROID
            GameBase.Instance = new GameBaseAndroid(activity);
            GameBase.Instance.Run();
#else
            GameBase game = new GameBaseDesktop();
            game.Run();
#endif
        }

#if ANDROID
        private bool pausedFromSuspend;

        public Application()
        {
            activity = this;
        }

        public override void OnBackPressed()
        {
            if (Director.IsTransitioning) return;

            switch (Director.CurrentOsuMode)
            {
                case OsuMode.Play when !((Player)Director.CurrentMode).IsPaused:
                    (Director.CurrentMode as Player)?.Pause();
                    break;
                case OsuMode.Play:
                {
                    if (!(bool)(Director.CurrentMode as Player)?.menu.Failed)
                        (Director.CurrentMode as Player).menu.MenuDisplayed = false;
                    break;
                }
                case OsuMode.MainMenu:
                    this.FinishAffinity();
                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Hide Status Bar, etc...
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.ImmersiveSticky);
                //This feature was only added in SDK version 28, so attempting to use it on older versions would result in a crash
                if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                    Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;

                Immersive = true;
            }

            Platform.Init(this, savedInstanceState);
        }

        protected override void OnPause()
        {
            base.OnPause();

            (Director.CurrentMode as Player)?.Pause();

            switch (Director.CurrentOsuMode)
            {
                case OsuMode.Play:
                    break;
                default:
                {
                    this.pausedFromSuspend = true;
                    AudioEngine.Music.Pause();

                    break;
                }
            }

            TextureManager.DisposeAll();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            GameBase.Config.SaveConfig();

            GameBaseAndroid.IsInitialized = false;
            GameBase.Instance = null;
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (GameBase.Instance == null) Main(null);
            else
            {
                if (this.pausedFromSuspend)
                {
                    AudioEngine.Music.Play();
                    this.pausedFromSuspend = false;
                }
            }


            GameBase.Instance.Run();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            InputSourceAndroid source = InputManager.RegisteredSources[0] as InputSourceAndroid;
            source.HandleTouches(e);

            return base.OnTouchEvent(e);
        }
#endif
    }
}
