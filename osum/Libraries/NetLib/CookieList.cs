namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Reflection;

    public class CookieList : CollectionBase
    {
        public void Add(CookieItem item)
        {
            base.List.Add(item);
        }

        public CookieItem Add(string name, string theValue)
        {
            CookieItem item = new CookieItem(name, theValue);
            this.Add(item);
            return item;
        }

        protected virtual string BuildRequestCookie(CookieItem cookie)
        {
            return (cookie.Name + "=" + cookie.Value);
        }

        private string GetExpires(string cookieData)
        {
            int index = cookieData.ToLower(CultureInfo.InvariantCulture).IndexOf("expires=");
            if (index > -1)
            {
                return HeaderFieldList.GetHeaderFieldValueItem(cookieData.Substring(index + "expires=".Length).Replace(",", "=="), string.Empty).Replace("==", ",");
            }
            return string.Empty;
        }

        public void GetResponseCookies(StringCollection responseHeader)
        {
            base.Clear();
            HeaderFieldList fieldList = new HeaderFieldList();
            HeaderFieldList.GetHeaderFieldList(0, responseHeader, fieldList);
            if (!StringUtils.IsEmpty(HeaderFieldList.GetHeaderFieldValue(responseHeader, fieldList, "set-cookie2")))
            {
                this.ProcessCookies(responseHeader, fieldList, "set-cookie2");
            }
            else
            {
                this.ProcessCookies(responseHeader, fieldList, "set-cookie");
            }
        }

        protected virtual void ParseResponseCookie(string cookieData)
        {
            if (!StringUtils.IsEmpty(cookieData))
            {
                string str = cookieData;
                string headerFieldValueItem = "";
                int index = cookieData.IndexOf("=");
                if (index > -1)
                {
                    str = cookieData.Substring(0, index);
                    headerFieldValueItem = HeaderFieldList.GetHeaderFieldValueItem(cookieData, str.ToLower(CultureInfo.InvariantCulture) + "=");
                }
                if (this[str] == null)
                {
                    CookieItem item = new CookieItem();
                    this.Add(item);
                    item.Name = str;
                    item.Value = headerFieldValueItem;
                    item.Expires = this.GetExpires(cookieData);
                    item.Domain = HeaderFieldList.GetHeaderFieldValueItem(cookieData, "domain=");
                    item.Path = HeaderFieldList.GetHeaderFieldValueItem(cookieData, "path=");
                    item.Secure = cookieData.ToLower(CultureInfo.InvariantCulture).IndexOf("secure") > -1;
                    item.CookieData = cookieData;
                }
            }
        }

        private void ProcessCookies(StringCollection responseHeader, HeaderFieldList fieldList, string fieldName)
        {
            foreach (HeaderField field in fieldList)
            {
                if (string.Compare(field.Name, fieldName, true, CultureInfo.InvariantCulture) == 0)
                {
                    this.ParseResponseCookie(HeaderFieldList.GetHeaderFieldValue(responseHeader, field));
                }
            }
        }

        public void Remove(CookieItem item)
        {
            base.List.Remove(item);
        }

        private void RemoveCookies(StringCollection requestHeader)
        {
            HeaderFieldList fieldList = new HeaderFieldList();
            HeaderFieldList.GetHeaderFieldList(0, requestHeader, fieldList);
            for (int i = fieldList.Count - 1; i >= 0; i--)
            {
                HeaderField field = fieldList[i];
                if (string.Compare(field.Name, "cookie", true, CultureInfo.InvariantCulture) == 0)
                {
                    HeaderFieldList.RemoveHeaderField(requestHeader, fieldList, field);
                }
            }
        }

        public void SetRequestCookies(StringCollection requestHeader)
        {
            this.RemoveCookies(requestHeader);
            string str = "";
            foreach (CookieItem item in this)
            {
                str = str + this.BuildRequestCookie(item) + "; ";
            }
            if (!StringUtils.IsEmpty(str))
            {
                str = str.Substring(0, str.Length - "; ".Length);
            }
            HeaderFieldList.AddHeaderField(requestHeader, "Cookie", str);
        }

        public CookieItem this[string name]
        {
            get
            {
                foreach (CookieItem item in this)
                {
                    if (string.Compare(item.Name, name, true, CultureInfo.InvariantCulture) == 0)
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        public CookieItem this[int index]
        {
            get
            {
                return (CookieItem) base.List[index];
            }
        }
    }
}

