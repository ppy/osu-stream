using System;


namespace osum
{
	public class GameBaseMono : GameBase
	{
		public GameBaseMono()
		{
		}
		
		override public void MainLoop()
		{
			GameWindowMono window = new GameWindowMono();
            WindowSize = window.ClientSize;
            window.Run();
		}
	}
}

