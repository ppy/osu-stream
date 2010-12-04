namespace osu_common.Libraries.NetLib
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    [ListBindable(BindableSupport.No), Editor("Design.HttpRequestItemsEditor, Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
    public class HttpRequestItemList : CollectionBase
    {
        private HttpRequest owner;

        public HttpRequestItemList(HttpRequest owner)
        {
            this.owner = owner;
        }

        public void Add(HttpRequestItem item)
        {
            base.List.Add(item);
        }

        public BinaryRequestItem AddBinaryData()
        {
            BinaryRequestItem item = new BinaryRequestItem();
            this.Add(item);
            return item;
        }

        public FormFieldRequestItem AddFormField(string fieldName, string fieldValue)
        {
            FormFieldRequestItem item = new FormFieldRequestItem(fieldName, fieldValue);
            this.Add(item);
            return item;
        }

        public void AddRange(HttpRequestItem[] items)
        {
            foreach (HttpRequestItem item in items)
            {
                this.Add(item);
            }
        }

        public SubmitFileRequestItem AddSubmitFile(string fileName, string fieldName)
        {
            SubmitFileRequestItem item = new SubmitFileRequestItem(fileName, fieldName);
            this.Add(item);
            return item;
        }

        public TextRequestItem AddTextData(string textData)
        {
            TextRequestItem item = new TextRequestItem(textData);
            this.Add(item);
            return item;
        }

        public FormFieldRequestItem FormFieldByName(string fieldName)
        {
            foreach (HttpRequestItem item in this)
            {
                if ((item is FormFieldRequestItem) && (string.Compare(((FormFieldRequestItem) item).FieldName, fieldName, true, CultureInfo.InvariantCulture) == 0))
                {
                    return (FormFieldRequestItem) item;
                }
            }
            return null;
        }

        public void Move(int curIndex, int newIndex)
        {
            if (curIndex != newIndex)
            {
                object obj2 = base.InnerList[curIndex];
                if (newIndex < curIndex)
                {
                    base.InnerList.RemoveAt(curIndex);
                    base.InnerList.Insert(newIndex, obj2);
                }
                else
                {
                    base.InnerList.Insert(newIndex, obj2);
                    base.InnerList.RemoveAt(curIndex);
                }
                this.Update();
            }
        }

        protected override void OnClear()
        {
            base.OnClear();
            foreach (HttpRequestItem item in base.InnerList)
            {
                item.owner_ = null;
            }
        }

        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            if (this.Owner != null)
            {
                this.Owner.UpdateContentType();
            }
            this.Update();
        }

        protected override void OnInsertComplete(int index, object theValue)
        {
            base.OnInsertComplete(index, theValue);
            ((HttpRequestItem) theValue).owner_ = this.Owner;
            if (this.Owner != null)
            {
                this.Owner.UpdateContentType();
            }
            this.Update();
        }

        protected override void OnRemoveComplete(int index, object theValue)
        {
            base.OnRemoveComplete(index, theValue);
            ((HttpRequestItem) theValue).owner_ = null;
            if (this.Owner != null)
            {
                this.Owner.UpdateContentType();
            }
            this.Update();
        }

        public void Remove(HttpRequestItem item)
        {
            base.List.Remove(item);
        }

        public void Update()
        {
            if (this.Owner != null)
            {
                this.Owner.Update();
            }
        }

        public HttpRequestItem this[int index]
        {
            get
            {
                return (HttpRequestItem) base.InnerList[index];
            }
        }

        public HttpRequest Owner
        {
            get
            {
                return this.owner;
            }
        }
    }
}

