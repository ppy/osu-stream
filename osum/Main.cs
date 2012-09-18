using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace osum
{
    public class Application
    {
        static void Main(string[] args)
        {
            try
            {
#if iOS
            GameBase game = new GameBaseIphone();
            game.Run();
#else
                GameBase game = new GameBaseDesktop();
                game.Run();
#endif
            }
            catch
            {
                Process.Start(Process.GetCurrentProcess().ProcessName);
            }
        }
    }
}

