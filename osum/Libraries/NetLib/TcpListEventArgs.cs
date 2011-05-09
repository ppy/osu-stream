namespace osu_common.Libraries.NetLib
{
    using System;

    public class TcpListEventArgs : EventArgs
    {
        private DataStringCollection list;

        public TcpListEventArgs(DataStringCollection list)
        {
            this.list = list;
        }

        public DataStringCollection List
        {
            get
            {
                return this.list;
            }
        }
    }
}

