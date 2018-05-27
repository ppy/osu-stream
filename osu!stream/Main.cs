using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace osum
{
    public class Application
    {
        static void Main(string[] args)
        {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#else
            GameBase game = new GameBaseDesktop();
            game.Run();
#endif
        }
    }
}

