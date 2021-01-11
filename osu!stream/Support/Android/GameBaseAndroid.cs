using Android.App;
using Android.Views;
using OpenTK.Graphics.ES11;
using osum.Audio;
using osum.GameModes;
using osum.Input;
using osum.Input.Sources;
using Xamarin.Essentials;

namespace osum
{
    class GameBaseAndroid : GameBase
    {
        private Activity _activity;

        public GameWindowAndroid Window;

        public GameBaseAndroid(Activity activity, OsuMode mode = OsuMode.Unknown) : base(mode)
        {
            _activity = activity;
        }

        public override void Run()
        {
            Window = new GameWindowAndroid(_activity);
            Window.Run();

            _activity.SetContentView(Window);
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            return new BackgroundAudioPlayerAndroid();
        }

        protected override SoundEffectPlayer InitializeSoundEffects()
        {
            return new SoundEffectPlayerBass();
        }

        protected override void InitializeInput()
        {
            InputSource source = new InputSource();
            InputManager.AddSource(source);
        }

        public override void SetupScreen()
        {
            NativeSize = new System.Drawing.Size((int)DeviceDisplay.MainDisplayInfo.Width, (int)DeviceDisplay.MainDisplayInfo.Height);

            base.SetupScreen();
        }
    }
}