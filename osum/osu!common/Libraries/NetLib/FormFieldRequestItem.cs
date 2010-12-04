namespace osu_common.Libraries.NetLib
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;

    public class FormFieldRequestItem : HttpRequestItem
    {
        private string fieldName;
        private string fieldValue;

        public FormFieldRequestItem()
        {
            this.fieldName = string.Empty;
            this.fieldValue = string.Empty;
        }

        public FormFieldRequestItem(string fieldName, string fieldValue)
        {
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
        }

        protected internal override void AddData(byte[] data, int index, int length)
        {
            this.FieldValue = this.FieldValue + Encoding.ASCII.GetString(data, index, length);
        }

        protected internal override void AfterAddData()
        {
            if (base.Owner != null)
            {
                base.Owner.OnDataAdded(new DataAddedEventArgs(this, new StringStream(this.FieldValue)));
            }
        }

        public override Stream GetData()
        {
            return new StringStream(this.GetRequest());
        }

        private string GetRequest()
        {
            if (base.Owner != null)
            {
                if (base.Owner.IsMultiPart())
                {
                    return string.Format("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}", base.GetCanonicalizedValue(this.FieldName), base.GetCanonicalizedValue(this.FieldValue));
                }
                if (base.Owner.IsForm())
                {
                    return string.Format("{0}={1}", base.GetCanonicalizedValue(this.FieldName), base.GetCanonicalizedValue(this.FieldValue));
                }
            }
            return string.Empty;
        }

        [DefaultValue("")]
        public string FieldName
        {
            get
            {
                return this.fieldName;
            }
            set
            {
                if (this.fieldName != value)
                {
                    this.fieldName = value;
                    base.Update();
                }
            }
        }

        [DefaultValue("")]
        public string FieldValue
        {
            get
            {
                return this.fieldValue;
            }
            set
            {
                if (this.fieldValue != value)
                {
                    this.fieldValue = value;
                    base.Update();
                }
            }
        }
    }
}

