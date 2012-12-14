using System;
using System.Collections.Generic;
using System.Text;
using osu_common.Tencho.Requests;

namespace osu_common.Tencho
{
    public class SlimClient
    {
        public SlimClient()
        {
        }

        public SlimClient(string username)
        {
            Username = username;
        }

        public RequestTarget RequestTargetType;

        private string ircFullName;
        public string IrcFullName
        {
            get { return ircFullName; }
            set
            {
                ircFullName = value;
                IrcFullNameCloaked = value.Remove(value.LastIndexOf('@')) + "@cho";
                IrcFullNameCleanCloaked = IrcFullNameCloaked.Replace("|osu", "");
            }
        }

        public string Address;
        public string IrcFullNameCloaked;
        public string IrcFullNameCleanCloaked;
        public string IrcRealname;
        public string IrcUser;

        public bool IsAdmin;
        public bool IsPrivileged; //can access priv'd channel

        public int UserId;

        protected string username;
        public string UsernameClean { get;  private set; }

        public string UsernameUnderscored;
        
        public virtual string Username
        {
            get { return username; }
            set
            {
                username = value;

                UsernameClean = username.Replace("|osu", "");
                UsernameUnderscored = Username.Replace(' ', '_');

                IrcFullName = UsernameUnderscored + "!" + IrcUser + "@" + Address;
            }
        }

        public string IrcPrefix
        {
            get
            {
                if (IsAdmin)
                    return "@";
                if (RequestTargetType == RequestTarget.Irc)
                    return "+";
                return string.Empty;
            }
        }

        public override string ToString()
        {
            return UsernameClean;
        }
    }
}
