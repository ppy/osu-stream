namespace osu_common.Libraries.NetLib
{
    using System;

    public class HeaderField
    {
        private string name;
        private string theValue;

        public HeaderField(string name, string theValue)
        {
            this.name = name;
            this.theValue = theValue;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Value
        {
            get
            {
                return this.theValue;
            }
        }
    }
}

