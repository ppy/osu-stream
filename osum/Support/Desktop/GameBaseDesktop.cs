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
        GameWindowDesktop gameWindow;

        
        public GameBaseDesktop(OsuMode mode = OsuMode.Unknown) : base(mode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        
        override public void Run()
        {
            gameWindow = new GameWindowDesktop();
            gameWindow.Run();
            Director.CurrentMode.Dispose();
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            return new BackgroundAudioPlayerDesktop();
        }

        protected override void InitializeInput()
        {
            InputSourceMouse source = new InputSourceMouse(gameWindow.Mouse);
            InputManager.AddSource(source);
        }

        public override void SetupScreen()
        {
            NativeSize = gameWindow.ClientSize;

            base.SetupScreen();
        }

        public void Exit()
        {
            gameWindow.Exit();
        }
    }
}

