namespace osu_common.Libraries.NetLib
{
    using System;

    public class HostInfo
    {
        internal string ipAddress;
        private string name;

        public HostInfo(string ipAddress, string name)
        {
            this.ipAddress = ipAddress;
            this.name = name;
        }

        public string IPAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }
    }
}

