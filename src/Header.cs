using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class Header
    {

        #region header Type

        public const string ACCEPT = "Accept";

        public const string ACCEPT_ENCODING = "Accept-Encoding";

        public const string ACCEPT_LANGUAGE = "Accept-Language";

        public const string CACHE_CONTROL = "Cache-Control";

        public const string CONNECTION = "Connection";

        public const string COOKIE = "Cookie";

        public const string HOST = "Host";

        public const string REFERER = "Referer";

        public const string USER_AGENT = "User-Agent";

        public const string STATUS = "Status";

        public const string CONTENT_TYPE = "Content-Type";

        public const string CONTENT_LENGTH = "Content-Length";

        public const string CONTENT_ENCODING = "Content-Encoding";

        public const string TRANSFER_ENCODING = "Transfer-Encoding";

        public const string SERVER = "Server";


        #endregion

        private System.Collections.Specialized.NameValueCollection mItems = new System.Collections.Specialized.NameValueCollection();

        public System.Collections.Specialized.NameValueCollection Items { get { return mItems; } }

        public void Add(string name, string value)
        {
            mItems[name] = value;
        }

        public void Import(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                Tuple<string, string> result = HttpParse.AnalyzeHeader(line);
                if (result.Item1 != null)
                    Add(result.Item1, result.Item2);
            }
        }

        public string this[string name]
        {
            get
            {
                return mItems[name];
            }
            set
            {
                mItems[name] = value;
            }
        }

        public bool Read(PipeStream stream)
        {
            string line = null;
            while (stream.TryReadLine(out line))
            {
                if (string.IsNullOrEmpty(line))
                    return true;
                Import(line);
            }
            return false;
        }

        public void Write(PipeStream stream)
        {
            foreach (string key in mItems)
            {
                stream.WriteLine(key + ": " + mItems[key]);
            }
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
