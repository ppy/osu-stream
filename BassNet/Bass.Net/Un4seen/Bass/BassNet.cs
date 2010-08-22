using System.Security;

namespace Un4seen.Bass
{
    [SuppressUnmanagedCodeSecurity]
    public sealed class BassNet
    {
        internal static string _eMail = "yours";
        internal static string _registrationKey = "mine";

        private BassNet()
        {
        }

        internal static bool IsRegistered
        {
            get { return true; }
        }

        public static void Registration(string eMail, string registrationKey)
        {
            _eMail = eMail;
            _registrationKey = registrationKey;
        }
    }
}