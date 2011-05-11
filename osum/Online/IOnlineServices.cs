using System;

namespace osum.Online
{
    public interface IOnlineServices
    {
        void SubmitScore(string id, int score);
        void Authenticate();
        bool IsAuthenticated { get; }
        void ShowLeaderboard(string category);
    }
}