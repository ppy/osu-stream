using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace osum
{
    public class Application
    {
        static GameBase game;

        static void Main(string[] args)
        {
            game = new GameBaseIphone();
        }
    }
}

