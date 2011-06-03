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

        public pList(int size)
            : base(size)
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

        public int AddInPlace(T item)
        {
            return AddInPlace(item, UseBackwardsSearch);
        }
        
        public int AddInPlace(T item, bool useBackwardsSearch)
        {
            int index = -1;

            if (useBackwardsSearch)
            {
                int count = Count;
                if (count == 0)
                    base.Add(item);
                else
                {
                    for (index = count - 1; index >= 0; index--)
                    {
                        if (base[index].CompareTo(item) > 0)
                            continue;
                        base.Insert(index + 1, item);
                        return index;
                    }
                    base.Insert(0, item);
                }
            }
            else
            {
                index = comparer != null ? BinarySearch(item, comparer) : BinarySearch(item);
                index = index < 0 ? ~index : index;
                Insert(index, item);
            }

            return index;
        }
    }
}
