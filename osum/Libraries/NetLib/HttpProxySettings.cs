using System.ComponentModel;

namespace osu_common.Libraries.NetLib
{
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class HttpProxySettings
    {
        private AuthenticationType authenticationType;
        private string password;
        private int port;
        private string server;
        private string userName;

        public HttpProxySettings()
        {
            Clear();
        }

        [DefaultValue(1)]
        public AuthenticationType AuthenticationType
        {
            get { return authenticationType; }
            set { authenticationType = value; }
        }

        [DefaultValue("")]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        [DefaultValue(0x1f90)]
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        [DefaultValue("")]
        public string Server
        {
            get { return server; }
            set { server = value; }
        }

        [DefaultValue("")]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public void Clear()
        {
            authenticationType = AuthenticationType.AutoDetect;
            userName = string.Empty;
            password = string.Empty;
            server = string.Empty;
            port = 0x1f90;
        }
    }
}