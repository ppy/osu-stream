using System;
using osum.Helpers;

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
                return Initialize() && onlineServices.IsAuthenticated;
            }
        }

        public static bool Initialize()
        {
            if (onlineServices == null)
            {
#if iOS
                onlineServices = new OnlineServicesIOS();
                onlineServices.Authenticate();
#endif
            }

            return onlineServices != null;
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

