using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class QueryString
    {
        private Dictionary<string, string> mItems = new Dictionary<string, string>();



        public string this[string name]
        {
            get
            {
                return GetValue(name);
            }
        }

        private string GetValue(string name)
        {
            string result = null;
            mItems.TryGetValue(name, out result);
            return result;
        }

        internal void Add(string name, string value)
        {
            mItems[name] = System.Web.HttpUtility.UrlDecode(value);
        }

        public bool TryGetString(string name, out string value)
        {
            string result = GetValue(name);
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
            foreach (var item in mItems)
            {
                sb.AppendFormat("{0}={1}\r\n", item.Key, item.Value);
            }
            return sb.ToString();
        }
    }
}
