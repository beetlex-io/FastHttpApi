using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class QueryString
    {
        private System.Collections.Specialized.NameValueCollection mItems = new System.Collections.Specialized.NameValueCollection();

        public System.Collections.Specialized.NameValueCollection Items { get { return mItems; } }

        public string this[string name]
        {
            get
            {
                return Items[name];
            }
        }

        private string GetValue(string name)
        {
            return mItems[name];
        }

        internal void Add(string name, string value)
        {
            mItems.Add(name, System.Web.HttpUtility.UrlDecode(value));
        }

        public bool TryGetString(string name, out string value)
        {
            string result = Items[name];
            if (!string.IsNullOrEmpty(result))
            {
                value = result;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetDateTime(string name, out DateTime value)
        {
            string result = GetValue(name);
            return DateTime.TryParse(result, out value);
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            string result = GetValue(name);
            return decimal.TryParse(result, out value);
        }

        public bool TryGetFloat(string name, out float value)
        {
            string result = GetValue(name);
            return float.TryParse(result, out value);
        }

        public bool TryGetDouble(string name, out double value)
        {
            string result = GetValue(name);
            return double.TryParse(result, out value);
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            string result = GetValue(name);
            return ushort.TryParse(result, out value);
        }

        public bool TryGetUInt(string name, out uint value)
        {
            string result = GetValue(name);
            return uint.TryParse(result, out value);
        }

        public bool TryGetULong(string name, out ulong value)
        {
            string result = GetValue(name);
            return ulong.TryParse(result, out value);
        }

        public bool TryGetInt(string name, out int value)
        {
            string result = GetValue(name);
            return int.TryParse(result, out value);
        }
        public bool TryGetLong(string name, out long value)
        {
            string result = GetValue(name);
            return long.TryParse(result, out value);
        }

        public bool TryGetShort(string name, out short value)
        {
            string result = GetValue(name);
            return short.TryParse(result, out value);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in Items.Keys)
            {
                sb.AppendFormat("{0}={1}\r\n", key, Items[key]);
            }
            return sb.ToString();
        }
    }
}
