using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using osum.GameModes;
using osum.UI;

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

                fsw = new FileSystemWatcher(Environment.CurrentDirectory);
                fsw.EnableRaisingEvents = true;
                fsw.Changed += fsw_Changed;

                game.Run();
#endif
#if !DEBUG
            }
            catch (Exception e)
            {
                File.WriteAllText("error-mainthread.txt", e.ToString());
            }
#endif
        }

        static bool restartPending;
        private static FileSystemWatcher fsw;
        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.Contains("stream.exe"))
            {
                GameBase.Scheduler.Add(restart, 10000);
                fsw.EnableRaisingEvents = false;
            }
        }

        private static void restart()
        {
            if (restartPending) return;

            if (Director.CurrentOsuMode != OsuMode.Play && Director.CurrentOsuMode != OsuMode.Results && GameBase.Match != null)
            {
                GameBase.Scheduler.Add(restart, 2000);
                return;
            }

            GameBase.Notify(new Notification("Update available!", "osu!stream needs to briefly restart to update with fixes and new features.", NotificationStyle.Okay, delegate
            {
                Environment.Exit(-1);
            }));
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                File.WriteAllText("error-otherthread.txt", e.ToString());
            }
            catch { }
            Environment.Exit(-1);
        }
    }
}