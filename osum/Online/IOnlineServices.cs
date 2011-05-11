using System;

namespace osum.Online
{
    public interface IOnlineServices
    {
        void Authenticate();
        bool IsAuthenticated { get; }
    }
}