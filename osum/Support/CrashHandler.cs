using System;
using System.Threading;
using System.IO;
using osum.UI;
using osu_common.Libraries.NetLib;

namespace osum.Support
{
    public static class CrashHandler
    {
        const string LOG_FILE = "error.log";

        static bool isInitialized;
        public static void Initialize()
        {
            if (isInitialized) return;

            if (File.Exists(LOG_FILE))
            {
                string contents = File.ReadAllText(LOG_FILE);
                File.Delete(LOG_FILE);

                GameBase.Scheduler.Add(delegate {
                    Notification notification = new Notification(
                            "Oops...",
                            "Looks like osu!stream encountered an error. Details will be sent to peppy (the guy who made this) so it can be fixed. Sorry for the trouble!",
                            NotificationStyle.Okay,
                            null);
                    GameBase.Notify(notification);
                },true);

                StringNetRequest nr = new StringNetRequest("http://www.osustream.com/admin/crash.php", "POST", contents);
                NetManager.AddRequest(nr);
            }

            AppDomain.CurrentDomain.UnhandledException += HandleException;

            isInitialized = true;
        }

        static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            string content = "exception=" + e.ExceptionObject.ToString();

#if iOS
            content += "&device=" + (int)osum.Support.iPhone.HardwareDetection.Version;
#endif

            File.WriteAllText(LOG_FILE, content);
        }
    }
}

