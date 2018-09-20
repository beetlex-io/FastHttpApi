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

        private string GetValue(string name)
        {
            string result = null;
            mItems.TryGetValue(name, out result);
            return result;
        }

        internal void Add(string name, string value)
        {
            mItems.Add(name, value);
        }
    }
}
