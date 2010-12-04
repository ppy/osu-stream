namespace osu_common.Libraries.NetLib
{
    using System;

    public class CookieItem
    {
        private string cookieData;
        private string domain;
        private string expires;
        private string name;
        private string path;
        private bool secure;
        private string theValue;

        public CookieItem()
        {
        }

        public CookieItem(string name, string theValue)
        {
            this.name = name;
            this.theValue = theValue;
        }

        public string CookieData
        {
            get
            {
                return this.cookieData;
            }
            set
            {
                this.cookieData = value;
            }
        }

        public string Domain
        {
            get
            {
                return this.domain;
            }
            set
            {
                this.domain = value;
            }
        }

        public string Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                this.expires = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                this.path = value;
            }
        }

        public bool Secure
        {
            get
            {
                return this.secure;
            }
            set
            {
                this.secure = value;
            }
        }

        public string Value
        {
            get
            {
                return this.theValue;
            }
            set
            {
                this.theValue = value;
            }
        }
    }
}

