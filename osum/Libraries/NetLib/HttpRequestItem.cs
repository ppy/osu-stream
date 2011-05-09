using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;

namespace osu_common.Libraries.NetLib
{
    [TypeConverter("Design.HttpRequestItemConverter, Design")]
    public abstract class HttpRequestItem
    {
        private static readonly byte[] UnsafeChars = Encoding.ASCII.GetBytes("+&*%<>\"#{}|\\^~[]'?!=/:$");
        private bool canonicalized = true;
        protected internal HttpRequest owner_;

        [DefaultValue(true)]
        public bool Canonicalized
        {
            get { return canonicalized; }
            set
            {
                if (canonicalized != value)
                {
                    canonicalized = value;
                    Update();
                }
            }
        }

        [Browsable(false)]
        public HttpRequest Owner
        {
            get { return owner_; }
        }

        [DefaultValue(0)]
        public int Tag { get; set; }

        protected internal abstract void AddData(byte[] data, int index, int length);
        protected internal abstract void AfterAddData();

        protected string GetCanonicalizedValue(string theValue)
        {
            if (!Canonicalized || ((Owner != null) && Owner.IsMultiPart()))
            {
                return theValue;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(theValue);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (IsUnsafeChar(bytes[i]) || (bytes[i] >= 0x80))
                {
                    builder.Append("%" +
                                   Convert.ToInt16(bytes[i], CultureInfo.InvariantCulture).ToString("X2",
                                                                                                    CultureInfo.
                                                                                                        InvariantCulture));
                }
                else if (bytes[i] == 0x20)
                {
                    builder.Append('+');
                }
                else
                {
                    builder.Append(Convert.ToChar(bytes[i], CultureInfo.InvariantCulture));
                }
            }
            return builder.ToString();
        }

        public abstract Stream GetData();

        private bool IsUnsafeChar(byte charCode)
        {
            for (int i = 0; i < UnsafeChars.Length; i++)
            {
                if (UnsafeChars[i] == charCode)
                {
                    return true;
                }
            }
            return false;
        }

        protected internal virtual void ParseHeader(IList header, HeaderFieldList fieldList)
        {
        }

        protected void Update()
        {
            if (Owner != null)
            {
                Owner.Update();
            }
        }
    }
}