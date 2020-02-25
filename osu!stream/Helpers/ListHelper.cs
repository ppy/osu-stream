using System;
using System.Collections.Generic;

namespace osum.Helpers
{
    public static class ListHelper
    {
        public static T[] StableSort<T>(IList<T> values) where T : IComparable<T>
        {
            //TODO: can this be more efficient?
            KeyValuePair<int, T>[] keys = new KeyValuePair<int, T>[values.Count];
            T[] items = new T[values.Count];
            values.CopyTo(items, 0);

            for (int i = 0; i < values.Count; i++)
            {
                keys[i] = new KeyValuePair<int, T>(i, values[i]);
            }

            Array.Sort(keys, items, new StableComparer<T>());
            return items;
        }

        private sealed class StableComparer<T> : IComparer<KeyValuePair<int, T>> where T : IComparable<T>
        {
            public int Compare(KeyValuePair<int, T> a, KeyValuePair<int, T> b)
            {
                int result = a.Value.CompareTo(b.Value);
                return (result != 0) ? result : a.Key.CompareTo(b.Key);
            }
        }
    }
}