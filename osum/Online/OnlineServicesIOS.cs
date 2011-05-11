using System;
using MonoTouch.GameKit;
using MonoTouch.Foundation;

namespace osum.Online
{
    public class OnlineServicesIOS : IOnlineServices
    {
        public OnlineServicesIOS()
        {
            localPlayer = GKLocalPlayer.LocalPlayer;
        }

        GKLocalPlayer localPlayer;

        public void Authenticate()
        {
            localPlayer.Authenticate(authenticationComplete);
        }

        void authenticationComplete(NSError error)
        {

        }

        public bool IsAuthenticated {
            get { return localPlayer.Authenticated; }
        }
    }
}

