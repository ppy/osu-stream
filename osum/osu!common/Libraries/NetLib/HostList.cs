namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;

    public class HostList : ICollection, IEnumerable
    {
        private ArrayList list = new ArrayList();

        protected internal void Add(HostInfo item)
        {
            this.list.Add(item);
        }

        protected internal void Clear()
        {
            this.list.Clear();
        }

        public void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        protected internal void Insert(int index, HostInfo item)
        {
            this.list.Insert(index, item);
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return this.list.IsSynchronized;
            }
        }

        public HostInfo this[int index]
        {
            get
            {
                return (HostInfo) this.list[index];
            }
        }

        public HostInfo this[string name]
        {
            get
            {
                foreach (HostInfo info in this)
                {
                    if (string.Compare(info.Name, name, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return info;
                    }
                }
                return null;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this.list.SyncRoot;
            }
        }
    }
}

