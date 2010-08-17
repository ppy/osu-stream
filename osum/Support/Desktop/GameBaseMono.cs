using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;


namespace osum
{
	public class GameBaseMono : GameBase
	{
        GameWindowMono gameWindow;

        public GameBaseMono()
		{
		}
		
		override public void MainLoop()
		{
			gameWindow = new GameWindowMono();
            WindowSize = gameWindow.ClientSize;
            gameWindow.Run();
		}

        protected override void InitializeBackgroundAudio()
        {
            backgroundAudioPlayer = new BackgroundAudioPlayerMono();
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

