namespace osu_common.Libraries.NetLib
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    public class TextRequestItem : HttpRequestItem
    {
        private string textData;

        public TextRequestItem()
        {
            this.textData = string.Empty;
        }

        public TextRequestItem(string textData)
        {
            this.textData = textData;
        }

        protected internal override void AddData(byte[] data, int index, int length)
        {
            this.TextData = this.TextData + Encoding.ASCII.GetString(data, index, length);
        }

        protected internal override void AfterAddData()
        {
            if (base.Owner != null)
            {
                base.Owner.OnDataAdded(new DataAddedEventArgs(this, new StringStream(this.TextData)));
            }
        }

        public override Stream GetData()
        {
            if (!StringUtils.IsEmpty(this.TextData))
            {
                return new StringStream(this.TextData);
            }
            return Stream.Null;
        }

        [DefaultValue("")]
        public string TextData
        {
            get
            {
                return this.textData;
            }
            set
            {
                if (this.textData != value)
                {
                    this.textData = value;
                    base.Update();
                }
            }
        }
    }
}

