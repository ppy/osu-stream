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
#if iOS
			game = new GameBaseIphone();
#else
			game = new GameBaseDesktop();
#endif			
		}
	}
}

