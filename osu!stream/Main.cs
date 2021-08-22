using osum.Support;
#if ANDROID
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Views;
using Android.Content.PM;
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
            return;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Hide Status Bar, etc...
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.HideNavigation | SystemUiFlags.Fullscreen |
                    SystemUiFlags.ImmersiveSticky | SystemUiFlags.Immersive);

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