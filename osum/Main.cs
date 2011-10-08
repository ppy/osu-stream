using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace osum
{
    public class Application
    {
        static void Main(string[] args)
        {
#if iOS
            new GameBaseIphone();
#else
            new GameBaseDesktop();
#endif
        }
    }
}

