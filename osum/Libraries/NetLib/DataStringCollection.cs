namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public class DataStringCollection : IList, ICollection, IEnumerable
    {
        private string charSet;
        private ArrayList data;

        public DataStringCollection()
        {
            this.data = new ArrayList();
            this.charSet = "ASCII";
        }

        public DataStringCollection(string charSet)
        {
            this.data = new ArrayList();
            this.charSet = charSet;
        }

        public int Add(string value)
        {
            return this.Add(this.GetBytes(value));
        }

        public int Add(byte[] value)
        {
            return this.data.Add(value);
        }

        public void AddRange(string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            foreach (string str in value)
            {
                this.Add(str);
            }
        }

        public bool AddTextStr(byte[] bytes, bool addToLastString)
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                return false;
            }
            int index = 0;
            while (index < bytes.Length)
            {
                int sourceIndex = index;
                while (((index < bytes.Length) && (bytes[index] != 13)) && (bytes[index] != 10))
                {
                    index++;
                }
                if (addToLastString && (this.Count > 0))
                {
                    byte[] sourceArray = (byte[]) this.data[this.data.Count - 1];
                    byte[] destinationArray = new byte[(index - sourceIndex) + sourceArray.Length];
                    Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
                    Array.Copy(bytes, sourceIndex, destinationArray, sourceArray.Length, index - sourceIndex);
                    this.data[this.data.Count - 1] = destinationArray;
                    addToLastString = false;
                }
                else
                {
                    byte[] buffer3 = new byte[index - sourceIndex];
                    Array.Copy(bytes, sourceIndex, buffer3, 0, buffer3.Length);
                    this.Add(buffer3);
                }
                if ((index < bytes.Length) && (bytes[index] == 13))
                {
                    index++;
                }
                if ((index < bytes.Length) && (bytes[index] == 10))
                {
                    index++;
                }
            }
            return (((bytes.Length == 1) && (bytes[0] != 10)) || (((bytes.Length > 1) && (bytes[bytes.Length - 2] != 13)) && (bytes[bytes.Length - 1] != 10)));
        }

        private bool ArraysAreEqual(byte[] buf1, byte[] buf2)
        {
            if ((buf1 != null) || (buf2 != null))
            {
                if ((buf1 == null) || (buf2 == null))
                {
                    return false;
                }
                if (buf1.Length != buf2.Length)
                {
                    return false;
                }
                for (int i = 0; i < buf1.Length; i++)
                {
                    if (buf1[i] != buf2[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public bool Contains(byte[] value)
        {
            return (this.IndexOf(value) > -1);
        }

        public bool Contains(string value)
        {
            return this.Contains(this.GetBytes(value));
        }

        public bool Contains(string theValue, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return this.Contains(theValue);
            }
            return this.Exists(theValue);
        }

        public void CopyTo(StringCollection collection)
        {
            collection.Clear();
            foreach (string str in (IEnumerable) this)
            {
                collection.Add(str);
            }
        }

        public void CopyTo(string[] array, int index)
        {
            for (int i = 0; i < this.Count; i++)
            {
                array[i] = this[i];
            }
        }

        private bool Exists(string theValue)
        {
            foreach (string str in (IEnumerable) this)
            {
                if (string.Compare(str, theValue, true, CultureInfo.InvariantCulture) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private byte[] GetBytes(string text)
        {
            return Translator.GetBytes(text, this.CharSet);
        }

        public byte[] GetData(int index)
        {
            return (byte[]) this.data[index];
        }

        private string GetString(byte[] bytes)
        {
            return Translator.GetString(bytes, this.CharSet);
        }

        public int IndexOf(string value)
        {
            return this.IndexOf(this.GetBytes(value));
        }

        public int IndexOf(byte[] value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this.ArraysAreEqual((byte[]) this.data[i], value))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, string value)
        {
            this.Insert(index, this.GetBytes(value));
        }

        public void Insert(int index, byte[] value)
        {
            this.data.Insert(index, value);
        }

        public bool LoadFromStream(Stream stream)
        {
            return this.LoadFromStream(stream, 0L, false);
        }

        public bool LoadFromStream(Stream stream, long count, bool addToLastString)
        {
            if (count == 0L)
            {
                count = stream.Length;
            }
            if (count > 0L)
            {
                byte[] buffer = new byte[count];
                stream.Read(buffer, 0, (int) count);
                return this.AddTextStr(buffer, addToLastString);
            }
            return false;
        }

        public void Remove(byte[] value)
        {
            int index = this.IndexOf(value);
            if (index > -1)
            {
                this.RemoveAt(index);
            }
        }

        public void Remove(string value)
        {
            this.Remove(this.GetBytes(value));
        }

        public void RemoveAt(int index)
        {
            this.data.RemoveAt(index);
        }

        public void SaveToStream(Stream stream)
        {
            foreach (byte[] buffer in this.data)
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.WriteByte(13);
                stream.WriteByte(10);
            }
        }

        public void SetData(int index, byte[] value)
        {
            this.data[index] = value;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((string[]) array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataStringEnumerator(this);
        }

        int IList.Add(object value)
        {
            if (value is string)
            {
                return this.Add((string) value);
            }
            return this.Add((byte[]) value);
        }

        bool IList.Contains(object value)
        {
            if (value is string)
            {
                return this.Contains((string) value);
            }
            return this.Contains((byte[]) value);
        }

        int IList.IndexOf(object value)
        {
            if (value is string)
            {
                return this.IndexOf((string) value);
            }
            return this.IndexOf((byte[]) value);
        }

        void IList.Insert(int index, object value)
        {
            if (value is string)
            {
                this.Insert(index, (string) value);
            }
            this.Insert(index, (byte[]) value);
        }

        void IList.Remove(object value)
        {
            if (value is string)
            {
                this.Remove((string) value);
            }
            this.Remove((byte[]) value);
        }

        public string[] ToArray()
        {
            string[] array = new string[this.Count];
            this.CopyTo(array, 0);
            return array;
        }

        public override string ToString()
        {
            string[] array = new string[this.Count];
            this.CopyTo(array, 0);
            return (string.Join("\r\n", array) + "\r\n");
        }

        public string CharSet
        {
            get
            {
                return this.charSet;
            }
            set
            {
                this.charSet = value;
            }
        }

        public int Count
        {
            get
            {
                return this.data.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public string this[int index]
        {
            get
            {
                return this.GetString((byte[]) this.data[index]);
            }
            set
            {
                this.data[index] = this.GetBytes(value);
            }
        }

        public long StringsSize
        {
            get
            {
                long num = 0L;
                foreach (byte[] buffer in this.data)
                {
                    num += buffer.Length + "\r\n".Length;
                }
                return num;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (string) value;
            }
        }

        private class DataStringEnumerator : IEnumerator
        {
            private IEnumerator baseEnumerator;
            private DataStringCollection collection;

            public DataStringEnumerator(DataStringCollection collection)
            {
                this.collection = collection;
                this.baseEnumerator = collection.data.GetEnumerator();
            }

            bool IEnumerator.MoveNext()
            {
                return this.baseEnumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                this.baseEnumerator.Reset();
            }

            object IEnumerator.Current
            {
                get
                {
                    byte[] current = (byte[]) this.baseEnumerator.Current;
                    return this.collection.GetString(current);
                }
            }
        }
    }
}

