using System;
using System.Collections.Generic;
using System.Text;

namespace osu_common.Helpers
{
    public class pList<T> : List<T>
    {
        private readonly bool forceSortOnAdd;
        private IComparer<T> comparer;

        public pList()
        {}

        public pList(IComparer<T> comparer, bool forceSortOnAdd)
        {
            this.comparer = comparer;
            this.forceSortOnAdd = forceSortOnAdd;
        }

        public new void Add(T item)
        {
            if (forceSortOnAdd)
                AddInPlace(item);
            else
                base.Add(item);
        }

        public void AddInPlace(T item)
        {
            int index = comparer != null ? BinarySearch(item,comparer) : BinarySearch(item);
            Insert(index < 0 ? ~index : index,item);
        }
    }
}
