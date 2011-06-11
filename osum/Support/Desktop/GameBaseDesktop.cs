using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;


namespace osum
{
    public class GameBaseDesktop : GameBase
    {
        GameWindowDesktop gameWindow;

        public GameBaseDesktop()
        {
            if (DateTime.Now > new DateTime(2011, 07, 07))
                Environment.Exit(-1);
        }
        
        override public void MainLoop()
        {
            gameWindow = new GameWindowDesktop();
            gameWindow.Run();
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
    }
}

