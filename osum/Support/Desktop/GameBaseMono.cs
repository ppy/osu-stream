using System;


namespace openglproject
{
	public class GameBaseMono : GameBase
	{
		public GameBaseMono()
		{
		}
		
		override public void MainLoop()
		{
			GameWindowMono window = new GameWindowMono();
		}
	}
}

