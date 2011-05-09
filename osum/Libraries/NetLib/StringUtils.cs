namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;

    public sealed class StringUtils
    {
        private StringUtils()
        {
        }

        public static void AssignStringCollection(StringCollection list, string theValue, bool withInit)
        {
            if (withInit)
            {
                list.Clear();
            }
            list.AddRange(GetStringArray(theValue));
        }

        public static string ExtractNumeric(string ASource, int AStartPos)
        {
            int num = AStartPos;
            while (((num <= ASource.Length) && (ASource[num] >= '0')) && (ASource[num] <= '9'))
            {
                num++;
            }
            return ASource.Substring(AStartPos, num - AStartPos);
        }

        public static string ExtractQuotedString(string S, char AQuoteBegin)
        {
            return ExtractQuotedString(S, AQuoteBegin, '\0');
        }

        public static string ExtractQuotedString(string S, char AQuoteBegin, char AQuoteEnd)
        {
            if (S.Length >= 2)
            {
                char ch = AQuoteEnd;
                if (AQuoteEnd == '\0')
                {
                    ch = AQuoteBegin;
                }
                if ((S[0] == AQuoteBegin) && (S[S.Length - 1] == ch))
                {
                    return S.Substring(1, S.Length - 2);
                }
            }
            return S;
        }

        public static string[] ExtractWords(string source, char[] delimiters)
        {
            ArrayList list = new ArrayList(source.Split(delimiters));
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (((string) list[i]).Length == 0)
                {
                    list.RemoveAt(i);
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        public static string GetDenormName(string name)
        {
            bool flag = false;
            string str = string.Empty;
            int num = 0;
            while (num < name.Length)
            {
                char ch = name[num];
                char ch2 = ch;
                if (ch2 != '"')
                {
                    if ((ch2 != '\\') || flag)
                    {
                        goto Label_0034;
                    }
                    flag = true;
                    num++;
                    continue;
                }
                if (!flag)
                {
                    ch = ' ';
                }
            Label_0034:
                flag = false;
                str = str + ch;
                num++;
            }
            return str;
        }

        public static string GetNormName(string name)
        {
            char[] chArray = new char[] { '\\', '"', '(', ')' };
            string str = string.Empty;
            foreach (char ch in name)
            {
                bool flag = false;
                foreach (char ch2 in chArray)
                {
                    if (ch2 == ch)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    str = str + '\\';
                }
                str = str + ch;
            }
            if (((str.IndexOf(' ') >= 0) || (str.IndexOf('\\') >= 0)) && ((str[0] != '"') && (str[str.Length - 1] != '"')))
            {
                str = "\"" + str + "\"";
            }
            return str;
        }

        public static string[] GetStringArray(string theValue)
        {
            return GetStringArray(theValue.Replace("\r\n", "\r"), '\r');
        }

        public static string[] GetStringArray(string theValue, char separator)
        {
            string[] sourceArray = theValue.Split(new char[] { separator });
            if ((sourceArray.Length > 0) && (sourceArray[sourceArray.Length - 1].Length == 0))
            {
                string[] destinationArray = new string[sourceArray.Length - 1];
                Array.Copy(sourceArray, destinationArray, (int) (sourceArray.Length - 1));
                return destinationArray;
            }
            return sourceArray;
        }

        public static string GetStringsAsString(DataStringCollection strings)
        {
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return GetStringsAsString(array);
        }

        public static string GetStringsAsString(string[] strings)
        {
            if (strings == null)
            {
                return string.Empty;
            }
            return (string.Join("\r\n", strings, 0, strings.Length) + ((strings.Length > 0) ? "\r\n" : string.Empty));
        }

        public static string GetStringsAsString(StringCollection strings)
        {
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return GetStringsAsString(array);
        }

        public static bool IsEmpty(string str)
        {
            if (str != null)
            {
                return (str.Length == 0);
            }
            return true;
        }

        public static long StrToInt64Def(string val, long defaultVal)
        {
            try
            {
                return Convert.ToInt64(val);
            }
            catch (Exception)
            {
            }
            return defaultVal;
        }

        public static int StrToIntDef(string val, int defaultVal)
        {
            try
            {
                return Convert.ToInt32(val);
            }
            catch (Exception)
            {
            }
            return defaultVal;
        }

        public static int StrToIntDef(string val, int fromBase, int defaultVal)
        {
            try
            {
                return Convert.ToInt32(val, fromBase);
            }
            catch (Exception)
            {
            }
            return defaultVal;
        }
    }
}

