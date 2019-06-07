using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class Cookies
    {
        private Dictionary<string, string> mItems = new Dictionary<string, string>();

        public string this[string name]
        {
            get
            {
                return GetValue(name);
            }
        }

        public void Clear()
        {
            mItems.Clear();
        }

        private string GetValue(string name)
        {
            string result = null;
            mItems.TryGetValue(name, out result);
            return result;
        }

        internal void Add(string name, string value)
        {
            name = System.Web.HttpUtility.UrlDecode(name);
            value = System.Web.HttpUtility.UrlDecode(value);
            mItems[name] = value;
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
