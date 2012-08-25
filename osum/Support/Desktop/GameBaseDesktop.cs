using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.GameModes.Play;
using System.Runtime.InteropServices;


namespace osum
{
    public class GameBaseDesktop : GameBase
    {
        public GameWindowDesktop Window;

        LightingManager lighting;

        [DllImport("winmm.dll")]
        internal static extern uint timeBeginPeriod(uint period);

        [DllImport("winmm.dll")]
        internal static extern uint timeEndPeriod(uint period);

        public GameBaseDesktop(OsuMode mode = OsuMode.Unknown) : base(mode)
        {
        }

        public override void Initialize()
        {
            lighting = new LightingManager();
            base.Initialize();
        }
        
        override public void Run()
        {
            timeBeginPeriod(1);
            Window = new GameWindowDesktop();
            Window.Run();
            Director.CurrentMode.Dispose();
            timeEndPeriod(1);
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            return new BackgroundAudioPlayerDesktop();
        }

        protected override SoundEffectPlayer InitializeSoundEffects()
        {
            return new SoundEffectPlayerBass();
        }

        protected override void InitializeInput()
        {
            try
            {
                InputManager.AddSource(new InputSourceBaanto());
            }
            catch
            {
                InputManager.AddSource(new InputSourceMouse(Window.Mouse));
            }
        }

        public override void SetupScreen()
        {
            NativeSize = Window.ClientSize;

            base.SetupScreen();
        }

        public override bool Update()
        {
            lighting.Update();
            return base.Update();
        }

        public override void Dispose()
        {
            lighting.Dispose();
            base.Dispose();
        }


        public void Exit()
        {
            Window.Exit();
        }
    }
}

