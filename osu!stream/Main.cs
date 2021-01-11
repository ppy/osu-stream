using osum.Support;
#if ANDROID
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Views;
using Android.Content.PM;
using Xamarin.Essentials;
#endif
#if !iOS && !ANDROID
using osum.Support.Desktop;
#endif

namespace osum
{
#if ANDROID
    [Activity(Label = "@string/app_name", Theme = "@style/default", MainLauncher = true)]
    public class Application : AppCompatActivity
#else
    public class Application
#endif
    {
        private static void Main(string[] args)
        {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#elif ANDROID
#else
            GameBase game = new GameBaseDesktop();
            game.Run();
#endif
        }

#if ANDROID
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
#endif
    }
}