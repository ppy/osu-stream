using System;
using System.Collections.Generic;
using System.Linq;

namespace osum
{
	public class Application
	{
		static GameBase game;
		
		static void Main (string[] args)
		{
#if IPHONE
			game = new GameBaseIphone();
#else
			game = new GameBaseMono();
#endif			
		}
	}
}

