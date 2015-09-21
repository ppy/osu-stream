using System;
using System.Threading;
using System.IO;
using osum.UI;
using osu_common.Libraries.NetLib;
using osum.Resources;

namespace osum.Support
{
    public static class CrashHandler
    {
        const string LOG_FILE = "error.log";
        static string LogFileFullPath { get { return GameBase.Instance.PathConfig + LOG_FILE; } }

        static bool isInitialized;
        public static void Initialize()
        {
            if (isInitialized) return;

#if MONO
            return;
#endif

            if (File.Exists(LogFileFullPath))
            {
                string contents = File.ReadAllText(LogFileFullPath);
                File.Delete(LogFileFullPath);

                GameBase.Scheduler.Add(delegate {
                    Notification notification = new Notification(
                            "Oops...",
                            LocalisationManager.GetString(OsuString.Crashed) ?? "A serious crash happened and has been reported",
                            NotificationStyle.Okay,
                            null);
                    GameBase.Notify(notification);
                },true);

                Report(contents);
            }

            AppDomain.CurrentDomain.UnhandledException += HandleException;

            isInitialized = true;
        }

        public static void Report(string contents)
        {
#if iOS
            contents += "&device=" + (int)osum.Support.iPhone.HardwareDetection.Version;
            contents += "&version=" + Foundation.NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
#endif
            StringNetRequest nr = new StringNetRequest("https://www.osustream.com/admin/crash.php", "POST", "exception=" + contents );
            NetManager.AddRequest(nr);
        }

        static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText(LogFileFullPath, e.ExceptionObject.ToString());
        }
    }
}

