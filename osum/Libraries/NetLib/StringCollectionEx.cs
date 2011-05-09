namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;

    public class StringCollectionEx : StringCollection
    {
        public bool AddTextStr(string text, bool addToLastString)
        {
            foreach (string str in StringUtils.GetStringArray(text))
            {
                if (addToLastString && (base.Count > 0))
                {
                    StringCollection strings = this;
                    int num2 = base.Count - 1;
                    strings[num2] = strings[num2] + str;
                    addToLastString = false;
                }
                else
                {
                    base.Add(str);
                }
            }
            return (((text.Length == 1) && (text[0] != '\n')) || (((text.Length > 1) && (text[text.Length - 2] != '\r')) && (text[text.Length - 1] != '\n')));
        }

        public bool Contains(string theValue, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return base.Contains(theValue);
            }
            return this.Exists(theValue);
        }

        public void CopyTo(StringCollection collection)
        {
            collection.Clear();
            foreach (string str in this)
            {
                collection.Add(str);
            }
        }

        private bool Exists(string theValue)
        {
            StringEnumerator enumerator = base.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if (string.Compare(enumerator.Current, theValue, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return true;
                    }
                }
            return false;
        }

        public bool LoadFromStream(Stream stream, string charSet)
        {
            return this.LoadFromStream(stream, 0L, false, charSet);
        }

        public bool LoadFromStream(Stream stream, long count, bool addToLastString, string charSet)
        {
            if (count == 0L)
            {
                count = stream.Length;
            }
            if (count > 0L)
            {
                byte[] buffer = new byte[count];
                stream.Read(buffer, 0, (int) count);
                return this.AddTextStr(Translator.GetString(buffer, charSet), addToLastString);
            }
            return false;
        }

        public void SaveToStream(Stream stream, string charSet)
        {
            StringEnumerator enumerator = base.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    byte[] bytes = Translator.GetBytes(enumerator.Current, charSet);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.WriteByte(13);
                    stream.WriteByte(10);
                }
        }

        public string[] ToArray()
        {
            string[] array = new string[base.Count];
            base.CopyTo(array, 0);
            return array;
        }

        public override string ToString()
        {
            string[] array = new string[base.Count];
            base.CopyTo(array, 0);
            return (string.Join("\r\n", array) + "\r\n");
        }

        public long StringsSize
        {
            get
            {
                long num = 0L;
                foreach (string str in this)
                {
                    num += str.Length + "\r\n".Length;
                }
                return num;
            }
        }
    }
}

