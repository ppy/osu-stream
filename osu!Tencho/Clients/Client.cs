using System.Collections.Generic;
using System.Text;
using System.Threading;
using osu_Tencho.Helpers;
using osu_common.Tencho.Requests;
using osu_common.Tencho.Objects;
using osu_common.Tencho;
using osu_common;

namespace osu_Tencho.Clients
{
    internal enum MessageTypes
    {
        Private,
        Public,
        Notice
    }

    internal abstract class Client : SlimClient
    {
        internal bool AllowCity = false;

        internal bool isKilled;

        internal string lastChatLine;
        internal long lastChatLineTime;

        internal bool TranslateAllMessages;
        internal bool LanguageChannelInformed;

        protected long connectTime;

        protected string avatarFilename = null;

        internal Location Location;

        /// <summary>
        /// Hours offset from UTC.
        /// </summary>
        internal int TimeOffset;

        /// <summary>
        /// A byte-cached request containing the client's presence packet.
        /// </summary>
        internal Request UserPresenceRequest;

        internal Client()
        {
            connectTime = Tencho.CurrentTime;
        }

        /// <summary>
        /// Create our presence packet which will be used to identify ourselves to others.
        /// </summary>
        /// <param name="avatarFilename"></param>
        internal virtual void CreatePresence()
        {
        }

        protected virtual void WriteConsole(string s)
        {
            string id = Username ?? "unknown";
            Bacon.WriteLine("? " + id + " " + s);
        }

        protected bool killPending;
        protected string killPendingReason;

        internal void Kill()
        {
            Kill("send error");
        }

        internal void Kill(string reason)
        {
            if (killPending) return;

            killPending = true;
            killPendingReason = reason;
        }

        internal void KillInstant(string reason)
        {
            kill(killPending ? killPendingReason : reason);
        }

        protected object InternalLock = new object();
        protected int Rank;

        protected virtual void kill(string reason)
        {
            lock (InternalLock)
            {
                if (isKilled) return;
                isKilled = true;
            }

            try
            {
                if (Bacon.LoggingEnabled)
                    Bacon.WriteLine(username + " killed for " + reason);
                UserManager.UnregisterClient(this, reason);
            }
            catch
            {
                Bacon.WriteSystem(Username + " ------- KILL FAILED ------- ");
            }
        }

        /// <summary>
        /// Send a request to this client.  Invokes underlying calls to the end-user socket or bot's receiving method.
        /// </summary>
        internal abstract void SendRequest(Request req);

        internal void SendMessage(Client sender, string target, string message)
        {
            SendRequest(new Request(RequestType.Tencho_SendMessage, new bMessage(sender, target, message)));
            if (IsAway && RequireAwayMessage(sender))
                sender.SendAwayMessage(this);
        }

        protected virtual void SendAwayMessage(Client sender)
        {
        }

        internal virtual string AwayMessage { get { return null; } set { } }

        /// <summary>
        /// Changes the username of this client.  Handles updating all other clients.
        /// </summary>
        internal bool ChangeUsernameAccepted(string newUsername)
        {
            return UserManager.RenameUser(this, newUsername);
        }

        /// <summary>
        /// Handles the change of another client's username.
        /// </summary>
        internal abstract void HandleChangeUsername(Client other, string newUsername);
        
        public virtual bool IsAway { get { return false; } }

        internal virtual bool RequireAwayMessage(Client target) { return false; }

        internal virtual void InitializeClient()
        {
            if (!UserManager.RegisterClient(this, false))
                KillInstant("reg failure");
        }
    }
}