namespace Un4seen.Bass.AddOn.Tags
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class Tag
    {
        private WMT_ATTR_DATATYPE _dataType;
        private int _index;
        private string _name;
        private object _value;

        public Tag(int index, string name, WMT_ATTR_DATATYPE type, object val)
        {
            _index = index;
            _name = name.TrimEnd(new char[1]);
            _dataType = type;
            switch (type)
            {
                case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
                    _value = Convert.ToUInt32(val);
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
                    _value = Convert.ToString(val).Trim();
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
                    _value = (byte[]) val;
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
                    _value = Convert.ToBoolean(val);
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
                    _value = Convert.ToUInt64(val);
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
                    _value = Convert.ToUInt16(val);
                    return;

                case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
                    _value = (Guid) val;
                    return;
            }
            throw new ArgumentException("Invalid data type", "type");
        }

        public static explicit operator byte[](Tag tag)
        {
            if (tag._dataType != WMT_ATTR_DATATYPE.WMT_TYPE_BINARY)
            {
                throw new InvalidCastException("Tag can not be converted to a byte array.");
            }
            return (byte[]) tag._value;
        }

        public static explicit operator string(Tag tag)
        {
            if (tag._dataType != WMT_ATTR_DATATYPE.WMT_TYPE_STRING)
            {
                throw new InvalidCastException("Tag can not be converted to a string.");
            }
            return (string) tag._value;
        }

        public static explicit operator bool(Tag tag)
        {
            if (tag._dataType != WMT_ATTR_DATATYPE.WMT_TYPE_BOOL)
            {
                throw new InvalidCastException("Tag can not be converted to a bool.");
            }
            return (bool) tag._value;
        }

        public static explicit operator Guid(Tag tag)
        {
            if (tag._dataType != WMT_ATTR_DATATYPE.WMT_TYPE_GUID)
            {
                throw new InvalidCastException("Tag can not be converted to a Guid.");
            }
            return (Guid) tag._value;
        }

        public static explicit operator int(Tag tag)
        {
            return (int) ((ulong) tag);
        }

        public static explicit operator long(Tag tag)
        {
            return (long) ((ulong) tag);
        }

        public static explicit operator ushort(Tag tag)
        {
            return (ushort) ((ulong) tag);
        }

        public static explicit operator uint(Tag tag)
        {
            return (uint) ((ulong) tag);
        }

        public static explicit operator ulong(Tag tag)
        {
            switch (tag._dataType)
            {
                case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
                case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
                case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
                    return (ulong) tag._value;
            }
            throw new InvalidCastException("Tag can not be converted to a number.");
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", _name, ValueAsString);
        }

        public WMT_ATTR_DATATYPE DataType
        {
            get
            {
                return _dataType;
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                switch (_dataType)
                {
                    case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
                        _value = (uint) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
                        _value = (string) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
                        _value = (byte[]) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
                        _value = (bool) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
                        _value = (ulong) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
                        _value = (ushort) value;
                        return;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
                        _value = (Guid) value;
                        return;
                }
            }
        }

        public string ValueAsString
        {
            get
            {
                string str = string.Empty;
                switch (_dataType)
                {
                    case WMT_ATTR_DATATYPE.WMT_TYPE_DWORD:
                    {
                        uint num = (uint) _value;
                        return num.ToString();
                    }
                    case WMT_ATTR_DATATYPE.WMT_TYPE_STRING:
                        return (string) _value;

                    case WMT_ATTR_DATATYPE.WMT_TYPE_BINARY:
                    {
                        int length = ((byte[]) _value).Length;
                        return ("[" + length.ToString() + " bytes]");
                    }
                    case WMT_ATTR_DATATYPE.WMT_TYPE_BOOL:
                    {
                        bool flag = (bool) _value;
                        return flag.ToString();
                    }
                    case WMT_ATTR_DATATYPE.WMT_TYPE_QWORD:
                    {
                        ulong num3 = (ulong) _value;
                        return num3.ToString();
                    }
                    case WMT_ATTR_DATATYPE.WMT_TYPE_WORD:
                    {
                        ushort num2 = (ushort) _value;
                        return num2.ToString();
                    }
                    case WMT_ATTR_DATATYPE.WMT_TYPE_GUID:
                    {
                        Guid guid = (Guid) _value;
                        return guid.ToString();
                    }
                }
                return str;
            }
        }
    }
}

