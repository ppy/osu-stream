using System;
using MonoTouch.GameKit;
using MonoTouch.Foundation;
using osum.Support.iPhone;
using osum.Helpers;

namespace osum.Online
{
    public class OnlineServicesIOS : GKLeaderboardViewControllerDelegate, IOnlineServices
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

        static VoidDelegate finishedDelegate;

        /// <summary>
        /// Shows the leaderboard.
        /// </summary>
        /// <param name='category'>
        /// The ID of the leaderboard. If null, it will display the aggregate leaderboard (see http://developer.apple.com/library/ios/#documentation/GameKit/Reference/GKLeaderboardViewController_Ref/Reference/Reference.html)
        /// </param>
        public void ShowLeaderboard(string category = null, VoidDelegate finished = null)
        {
            AppDelegate.UsingViewController = true;
            GKLeaderboardViewController vc = new GKLeaderboardViewController();
            vc.Category = category;
            vc.Delegate = this;
            finishedDelegate = finished;
            AppDelegate.ViewController.PresentModalViewController(vc, true);
        }

        public override void DidFinish(GKLeaderboardViewController viewController)
        {
            if (finishedDelegate != null)
                finishedDelegate();
            finishedDelegate = null;

            AppDelegate.ViewController.DismissModalViewControllerAnimated(false);
            //if we want to animate, we need to delay the removal of the view, else it gets stuck.
            //for now let's just not animate!

            AppDelegate.UsingViewController = false;
        }

        public void SubmitScore(string id, int score)
        {
            GKScore gamekitScore = new GKScore(id);
            gamekitScore.Value = score;
            gamekitScore.ReportScore(delegate(NSError error) {
                if (error != null)
                {
                    //todo: handle this
                    Console.WriteLine("submission error: " + error.ToString());
                    Console.WriteLine("using id " + id + " score " + score);
                    return;
                }

                ShowLeaderboard(id);
                Console.WriteLine("score submitted");

            });
        }
    }
}

