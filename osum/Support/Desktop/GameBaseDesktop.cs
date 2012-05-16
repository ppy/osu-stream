using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.GameModes.Play;


namespace osum
{
    public class GameBaseDesktop : GameBase
    {
        public GameWindowDesktop Window;

        public GameBaseDesktop(OsuMode mode = OsuMode.Unknown) : base(mode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        
        override public void Run()
        {
            Window = new GameWindowDesktop();
            Window.Run();
            Director.CurrentMode.Dispose();
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
            //InputManager.AddSource(new InputSourceMouse(Window.Mouse));
            InputManager.AddSource(new InputSourceBaanto());
        }

        public override void SetupScreen()
        {
            NativeSize = Window.ClientSize;

            base.SetupScreen();
        }

        public void Exit()
        {
            Window.Exit();
        }
    }
}

