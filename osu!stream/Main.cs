using osum.Support;
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
        private static Activity _this;
#endif

        private static void Main(string[] args)
        {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#elif ANDROID
            GameBase game = new GameBaseAndroid(_this);
            game.Run();
#else
            GameBase game = new GameBaseDesktop();
            game.Run();
#endif
        }

#if ANDROID
        public Application()
        {
            _this = this;
        }

        public override void OnBackPressed() {
            if (Director.IsTransitioning) return;
            
            switch (Director.CurrentOsuMode) {
                case OsuMode.Play when !((Player)Director.CurrentMode).IsPaused:
                    (Director.CurrentMode as Player)?.Pause();
                    break;
                case OsuMode.Play: {
                    if(!(bool)(Director.CurrentMode as Player)?.menu.Failed)
                        (Director.CurrentMode as Player).menu.MenuDisplayed = false;
                    break;
                }
                case OsuMode.MainMenu:
                    System.Environment.Exit(0);
                    break;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Hide Status Bar, etc...
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutStable | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutFullscreen | SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen | SystemUiFlags.ImmersiveSticky);
                Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
                
                Immersive = true;
            }

            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);

            Main(null);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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