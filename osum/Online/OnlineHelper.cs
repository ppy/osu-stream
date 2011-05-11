using System;

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

        static bool initialized;

        public static bool Initialize()
        {
            if (!initialized)
            {
#if iOS
                onlineServices = new OnlineServicesIOS();
                onlineServices.Authenticate();
#endif
                initialized = true;
            }

            return onlineServices != null;
        }

        static IOnlineServices onlineServices;
    }
}

