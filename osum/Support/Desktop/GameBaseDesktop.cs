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
		}
		
		override public void MainLoop()
		{
			gameWindow = new GameWindowDesktop();
            WindowSize = gameWindow.ClientSize;
            gameWindow.Run();
		}

        protected override void InitializeBackgroundAudio()
        {
            backgroundAudioPlayer = new BackgroundAudioPlayerDesktop();
        }

        protected override void InitializeInput()
        {
            InputSourceMouse source = new InputSourceMouse(gameWindow.Mouse);
            InputManager.AddSource(source);
        }

        public override void SetupScreen()
        {
            GameBase.WindowSize = gameWindow.Size;

            GL.Viewport(GameBase.WindowSize);

            base.SetupScreen();
        }
    }
}

