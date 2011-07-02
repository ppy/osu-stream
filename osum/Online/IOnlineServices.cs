using System;
using osum.Helpers;

namespace osum.Online
{
    public interface IOnlineServices
    {
        void SubmitScore(string id, int score, VoidDelegate finished = null);
        void Authenticate(VoidDelegate finished = null);
        bool IsAuthenticated { get; }
        void ShowLeaderboard(string category = null, VoidDelegate finished = null);
    }
}