using System;
using osum.Helpers;

namespace osum.Online
{
    public interface IOnlineServices
    {
        void SubmitScore(string id, int score);
        void Authenticate();
        bool IsAuthenticated { get; }
        void ShowLeaderboard(string category = null, VoidDelegate finished = null);
    }
}