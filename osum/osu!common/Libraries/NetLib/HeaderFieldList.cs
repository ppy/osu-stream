namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;

    public class HeaderFieldList : ICollection, IEnumerable
    {
        private ArrayList list = new ArrayList();

        public void Add(HeaderField field)
        {
            this.list.Add(field);
        }

        public void Add(string name, string theValue)
        {
            this.Add(new HeaderField(name, theValue));
        }

        public static void AddHeaderArrayField(DataStringCollection source, string[] values, string name, string delimiter)
        {
            StringCollection strings = new StringCollection();
            strings.AddRange(values);
            AddHeaderMultiField(source, strings, name, delimiter);
        }

        public static void AddHeaderArrayField(StringCollection source, string[] values, string name, string delimiter)
        {
            StringCollection strings = new StringCollection();
            strings.AddRange(values);
            AddHeaderMultiField(source, strings, name, delimiter);
        }

        public static void AddHeaderField(DataStringCollection source, string name, string theValue)
        {
            if (!StringUtils.IsEmpty(theValue))
            {
                theValue = theValue.Replace("\r\n", "\r\n\t");
                if (theValue[theValue.Length - 1] == '\t')
                {
                    theValue = theValue.Substring(0, theValue.Length - 1);
                }
                source.AddRange(StringUtils.GetStringArray(string.Format("{0}: {1}", name, theValue)));
            }
        }

        public static void AddHeaderField(StringCollection source, string name, string theValue)
        {
            if (!StringUtils.IsEmpty(theValue))
            {
                theValue = theValue.Replace("\r\n", "\r\n\t");
                if (theValue[theValue.Length - 1] == '\t')
                {
                    theValue = theValue.Substring(0, theValue.Length - 1);
                }
                source.AddRange(StringUtils.GetStringArray(string.Format("{0}: {1}", name, theValue)));
            }
        }

        private static void AddHeaderMultiField(DataStringCollection source, StringCollection values, string name, string delimiter)
        {
            if (values.Count > 0)
            {
                AddHeaderField(source, name, values[0] + ((values.Count > 1) ? delimiter : string.Empty));
                for (int i = 1; i < values.Count; i++)
                {
                    source.Add('\t' + values[i] + ((i < (values.Count - 1)) ? delimiter : string.Empty));
                }
            }
        }

        private static void AddHeaderMultiField(StringCollection source, StringCollection values, string name, string delimiter)
        {
            if (values.Count > 0)
            {
                AddHeaderField(source, name, values[0] + ((values.Count > 1) ? delimiter : string.Empty));
                for (int i = 1; i < values.Count; i++)
                {
                    source.Add('\t' + values[i] + ((i < (values.Count - 1)) ? delimiter : string.Empty));
                }
            }
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool ContainsField(string name)
        {
            return (this.IndexOf(name) >= 0);
        }

        public void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        private static string GetDelimitedValue(string source, string lexem)
        {
            string str = InternalGetDelimitedValue(source, lexem).Trim();
            if ((str.Length == 0) || ((str[0] != '"') && (str[0] != '\'')))
            {
                return str;
            }
            if ((str[str.Length - 1] != '"') && (str[str.Length - 1] != '\''))
            {
                return str;
            }
            return str.Substring(1, str.Length - 2);
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public static int GetHeaderFieldList(int startFrom, IList source, HeaderFieldList fieldList)
        {
            if (source == null)
            {
                return startFrom;
            }
            for (int i = startFrom; i < source.Count; i++)
            {
                string str = (string) source[i];
                if (StringUtils.IsEmpty(str))
                {
                    return i;
                }
                if ((str[0] != '\t') && (str[0] != ' '))
                {
                    int index = str.IndexOf(":");
                    if (index >= 0)
                    {
                        fieldList.Add(str.Substring(0, index), i.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            return source.Count;
        }

        public static string GetHeaderFieldValue(IList source, HeaderField field)
        {
            string str = string.Empty;
            if ((field != null) && (field.Value != null))
            {
                int num = Convert.ToInt32(field.Value, CultureInfo.InvariantCulture);
                str = ((string) source[num]).Substring(field.Name.Length + ":".Length).TrimStart(new char[0]);
                for (int i = num + 1; i < source.Count; i++)
                {
                    string str3 = (string) source[i];
                    if (str3.Length == 0)
                    {
                        return str;
                    }
                    char ch = str3[0];
                    if (!ch.Equals('\t'))
                    {
                        char ch2 = str3[0];
                        if (!ch2.Equals(' '))
                        {
                            return str;
                        }
                    }
                    str = str + str3.Trim();
                }
            }
            return str;
        }

        public static string GetHeaderFieldValue(IList source, HeaderFieldList fieldList, string name)
        {
            return GetHeaderFieldValue(source, fieldList[name.ToLower(CultureInfo.InvariantCulture)]);
        }

        public static string GetHeaderFieldValueItem(string source, string itemName)
        {
            string str = GetDelimitedValue(source, itemName).Trim();
            if (StringUtils.IsEmpty(str) || ((str[0] != '\'') && (str[0] != '"')))
            {
                return str;
            }
            if ((str[str.Length - 1] != '\'') && (str[str.Length - 1] != '"'))
            {
                return str;
            }
            return str.Substring(1, str.Length - 1);
        }

        private int IndexOf(string name)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (string.Compare(name, ((HeaderField) this.list[i]).Name, true, CultureInfo.InvariantCulture) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void InsertHeaderFieldIfNeed(DataStringCollection source, string name, string theValue)
        {
            if (!StringUtils.IsEmpty(theValue))
            {
                HeaderFieldList fieldList = new HeaderFieldList();
                int index = GetHeaderFieldList(0, source, fieldList);
                if (!fieldList.ContainsField(name))
                {
                    if ((index < 0) || (index > source.Count))
                    {
                        index = source.Count;
                    }
                    theValue = theValue.Replace("\r\n", "\r\n\t");
                    if (theValue[theValue.Length - 1] == '\t')
                    {
                        theValue = theValue.Substring(0, theValue.Length - 1);
                    }
                    foreach (string str in StringUtils.GetStringArray(string.Format("{0}: {1}", name, theValue)))
                    {
                        source.Insert(index, str);
                        index++;
                    }
                }
            }
        }

        public static void InsertHeaderFieldIfNeed(ArrayList source, string name, string theValue)
        {
            if (!StringUtils.IsEmpty(theValue))
            {
                HeaderFieldList fieldList = new HeaderFieldList();
                int index = GetHeaderFieldList(0, source, fieldList);
                if (!fieldList.ContainsField(name))
                {
                    if ((index < 0) || (index > source.Count))
                    {
                        index = source.Count;
                    }
                    theValue = theValue.Replace("\r\n", "\r\n\t");
                    if (theValue[theValue.Length - 1] == '\t')
                    {
                        theValue = theValue.Substring(0, theValue.Length - 1);
                    }
                    source.InsertRange(index, StringUtils.GetStringArray(string.Format("{0}: {1}", name, theValue)));
                }
            }
        }

        public static void InsertHeaderFieldIfNeed(StringCollection source, string name, string theValue)
        {
            if (!StringUtils.IsEmpty(theValue))
            {
                HeaderFieldList fieldList = new HeaderFieldList();
                int index = GetHeaderFieldList(0, source, fieldList);
                if (!fieldList.ContainsField(name))
                {
                    if ((index < 0) || (index > source.Count))
                    {
                        index = source.Count;
                    }
                    theValue = theValue.Replace("\r\n", "\r\n\t");
                    if (theValue[theValue.Length - 1] == '\t')
                    {
                        theValue = theValue.Substring(0, theValue.Length - 1);
                    }
                    foreach (string str in StringUtils.GetStringArray(string.Format("{0}: {1}", name, theValue)))
                    {
                        source.Insert(index, str);
                        index++;
                    }
                }
            }
        }

        private static string InternalGetDelimitedValue(string source, string startLexem)
        {
            int index;
            string str = string.Empty;
            if ((startLexem.Length == 0) && (source.Length != 0))
            {
                index = 0;
            }
            else
            {
                index = source.ToLower(CultureInfo.InvariantCulture).IndexOf(startLexem);
            }
            if (index >= 0)
            {
                str = source.Substring(index + startLexem.Length);
                bool flag = false;
                string str2 = string.Empty;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str2.Length == 0)
                    {
                        char ch = str[i];
                        if (!ch.Equals('\''))
                        {
                            char ch2 = str[i];
                            if (!ch2.Equals('"'))
                            {
                                goto Label_00A2;
                            }
                        }
                        str2 = str[i].ToString(CultureInfo.InvariantCulture);
                        flag = !flag;
                        goto Label_00C9;
                    }
                Label_00A2:
                    if (str2.Length != 0)
                    {
                        char ch4 = str[i];
                        if (ch4.Equals(str2[0]))
                        {
                            flag = !flag;
                        }
                    }
                Label_00C9:
                    if (!flag)
                    {
                        char ch5 = str[i];
                        if (!ch5.Equals(';'))
                        {
                            char ch6 = str[i];
                            if (!ch6.Equals(','))
                            {
                                goto Label_0102;
                            }
                        }
                        return str.Substring(0, i);
                    }
                Label_0102:;
                }
            }
            return str;
        }

        public void Remove(HeaderField field)
        {
            this.list.Remove(field);
        }

        public static void RemoveHeaderField(DataStringCollection source, HeaderFieldList fieldList, string name)
        {
            HeaderField field = fieldList[name];
            if (field != null)
            {
                fieldList.Remove(field);
                int index = int.Parse(field.Value);
                source.RemoveAt(index);
                while (index < source.Count)
                {
                    string str = source[index];
                    if (StringUtils.IsEmpty(str) || ((str[0] != '\t') && (str[0] != ' ')))
                    {
                        break;
                    }
                    source.RemoveAt(index);
                }
            }
        }

        public static void RemoveHeaderField(ArrayList source, HeaderFieldList fieldList, string name)
        {
            HeaderField field = fieldList[name];
            if (field != null)
            {
                fieldList.Remove(field);
                int index = int.Parse(field.Value);
                source.RemoveAt(index);
                while (index < source.Count)
                {
                    string str = (string) source[index];
                    if (StringUtils.IsEmpty(str) || ((str[0] != '\t') && (str[0] != ' ')))
                    {
                        break;
                    }
                    source.RemoveAt(index);
                }
            }
        }

        public static void RemoveHeaderField(StringCollection source, HeaderFieldList fieldList, HeaderField field)
        {
            if (field != null)
            {
                fieldList.Remove(field);
                int index = int.Parse(field.Value);
                source.RemoveAt(index);
                while (index < source.Count)
                {
                    if (StringUtils.IsEmpty(source[index]) || ((source[index][0] != '\t') && (source[index][0] != ' ')))
                    {
                        break;
                    }
                    source.RemoveAt(index);
                }
            }
        }

        public static void RemoveHeaderField(StringCollection source, HeaderFieldList fieldList, string name)
        {
            RemoveHeaderField(source, fieldList, fieldList[name]);
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

        public HeaderField this[string name]
        {
            get
            {
                int index = this.IndexOf(name);
                if (index >= 0)
                {
                    return (HeaderField) this.list[index];
                }
                return null;
            }
        }

        public HeaderField this[int index]
        {
            get
            {
                return (HeaderField) this.list[index];
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

