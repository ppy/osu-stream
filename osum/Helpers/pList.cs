using System;
using System.Collections.Generic;
using System.Text;

namespace osu_common.Helpers
{
    public class pList<T> : List<T> where T : IComparable<T>
    {
        private readonly bool forceSortOnAdd;
        internal bool UseBackwardsSearch;
        private IComparer<T> comparer;

        public pList()
        { }

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
            if (UseBackwardsSearch)
            {
                int count = Count;
                if (count == 0)
                    base.Add(item);
                else
                {
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (base[i].CompareTo(item) > 0)
                            continue;
                        base.Insert(i + 1, item);
                        return;
                    }
                    base.Insert(0, item);
                }
            }
            else
            {
                int index = comparer != null ? BinarySearch(item, comparer) : BinarySearch(item);
                Insert(index < 0 ? ~index : index, item);
            }
        }
    }
}
