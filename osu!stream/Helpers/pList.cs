using System;
using System.Collections.Generic;

namespace osum.Helpers
{
    public class pList<T> : List<T> where T : IComparable<T>
    {
        private readonly bool forceSortOnAdd;
        internal bool UseBackwardsSearch;
        internal bool InsertAfterOnEqual;
        private readonly IComparer<T> comparer;

        public pList()
        {
        }

        public pList(int size)
            : base(size)
        {
        }

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
                {
                    base.Add(item);
                    index = 0;
                }
                else
                {
                    for (index = count - 1; index >= 0; index--)
                    {
                        int compare = base[index].CompareTo(item);
                        if (compare > 0) continue;

                        Insert((compare < 0 || InsertAfterOnEqual) ? ++index : index, item);
                        return index;
                    }

                    Insert(0, item);
                    index = 0;
                }
            }
            else
            {
                index = comparer != null ? BinarySearch(item, comparer) : BinarySearch(item);
                index = index < 0 ? ~index : (InsertAfterOnEqual ? index + 1 : index);
                Insert(index, item);
            }

            return index;
        }
    }
}