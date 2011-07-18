using System;
using osum.Helpers;
using osum.UI;
using osum.Resources;
using osum.GameModes;

namespace osum.Online
{
    public static class OnlineHelper
    {
        /// <summary>
        /// Gets a value indicating whether online functionality is available.
        /// </summary>
        public static bool Available {
            get
            {
                return onlineServices != null && onlineServices.IsAuthenticated;
            }
        }

        static bool hasInitialised;

        public static bool Initialize(bool forceAuthentication = false)
        {
            hasInitialised = true;

            if (onlineServices == null)
            {
#if iOS
                onlineServices = new OnlineServicesIOS();
#endif
            }

            if (onlineServices != null)
            {
                if (!forceAuthentication && GameBase.Config.GetValue<bool>("GamecentreFailureAnnounced", false))
                    return false;
                onlineServices.Authenticate(authFinished);
            }
            else
                authFinished();

            return onlineServices != null;
        }

        static void authFinished()
        {
            if (onlineServices != null && onlineServices.IsAuthenticated)
            {
                //we succeeded, so reset the warning
                GameBase.Config.SetValue<bool>("GamecentreFailureAnnounced", false);
                if (Director.CurrentOsuMode == OsuMode.Options)
                    Director.ChangeMode(OsuMode.Options);
            }
            else
            {
                if (!GameBase.Config.GetValue<bool>("GamecentreFailureAnnounced", false))
                {
                    Notification n = new Notification(LocalisationManager.GetString(OsuString.GameCentreInactive), LocalisationManager.GetString(OsuString.GameCentreInactiveExplanation), NotificationStyle.Okay);
                    GameBase.Notify(n);
                    GameBase.Config.SetValue<bool>("GamecentreFailureAnnounced", true);
                }
            }

        }

        public static bool ShowRanking(string id, VoidDelegate finished = null)
        {
            if (!Initialize()) return false;

            onlineServices.ShowLeaderboard(id, finished);
            return true;
        }

        public static bool SubmitScore(string id, int score, VoidDelegate finished = null)
        {
            if (!Initialize()) return false;

            onlineServices.SubmitScore(id, score, finished);
            return true;
        }

        static IOnlineServices onlineServices;
    }
}

