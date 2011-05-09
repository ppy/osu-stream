namespace osu_common.Libraries.NetLib
{
    using System;

    public class TcpTextEventArgs : EventArgs
    {
        private string text;

        public TcpTextEventArgs(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }
    }
}

