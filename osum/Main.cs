using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace osum
{
    public class Application
    {
        static void Main(string[] args)
        {
#if !DEBUG
            try
            {
#if !iOS
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
#endif

#if iOS
                GameBase game = new GameBaseIphone();
                game.Run();
#else
                GameBase game = new GameBaseDesktop();

                FileSystemWatcher fsw = new FileSystemWatcher(Environment.CurrentDirectory);
                fsw.EnableRaisingEvents = true;
                fsw.Changed += fsw_Changed;
                

                game.Run();
#endif
#if !DEBUG
            }
            catch (Exception e)
            {
                File.WriteAllText("error.txt",e.ToString());
            }
#endif
        }

        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.Contains("stream.exe"))
            {
                Thread.Sleep(8000);
                Environment.Exit(-1);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("error.txt", e.ToString());
            Environment.Exit(-1);
        }
    }
}

